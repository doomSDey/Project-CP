using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    private Vector3 originalPos;
    private float trauma = 0f;
    private float traumaReduction = 1f;
    private float frequency = 25f;

    private void Awake()
    {
        originalPos = transform.localPosition;
    }

    public void AddTrauma(float amount)
    {
        trauma = Mathf.Clamp01(trauma + amount);
    }

    private void Update()
    {
        if (trauma > 0)
        {
            float shake = trauma * trauma; // Square trauma for a more organic falloff

            float x = Mathf.PerlinNoise(Time.time * frequency, 0) * 2 - 1;
            float y = Mathf.PerlinNoise(0, Time.time * frequency) * 2 - 1;

            Vector3 offset = new Vector3(x, y, 0) * shake;
            transform.localPosition = originalPos + offset;

            trauma = Mathf.Clamp01(trauma - (traumaReduction * Time.deltaTime));
        }
        else
        {
            transform.localPosition = originalPos;
        }
    }
}
