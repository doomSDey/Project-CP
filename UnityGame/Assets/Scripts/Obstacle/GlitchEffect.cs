using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GlitchEffect : MonoBehaviour
{
    [Header("Glitch Settings")]
    [SerializeField] private float glitchDuration = 5f; // How long the glitch will last
    [SerializeField] private float glitchIntensity = 0.1f; // Intensity of the camera shake
    [SerializeField] private float flickerInterval = 0.2f; // How often the screen flickers
    [SerializeField] private float distortionAmount = 0.5f; // Distortion effect intensity

    [Header("References")]
    [SerializeField] private Volume postProcessingVolume; // Reference to the Post Processing Volume
    [SerializeField] private Camera mainCamera; // Reference to the main camera

    private Vignette vignetteEffect;
    private LensDistortion lensDistortionEffect;
    private ChromaticAberration chromaticAberrationEffect;

    private bool isGlitching = false;

    private void Start()
    {
        // Find and cache the camera reference if not set
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // Cache the post-processing volume
        if (postProcessingVolume == null)
        {
            postProcessingVolume = FindObjectOfType<Volume>();
        }

        // Try to get the effects from the Post Processing Volume
        if (postProcessingVolume != null && postProcessingVolume.profile != null)
        {
            postProcessingVolume.profile.TryGet(out vignetteEffect);
            postProcessingVolume.profile.TryGet(out lensDistortionEffect);
            postProcessingVolume.profile.TryGet(out chromaticAberrationEffect);
        }
    }

    /// <summary>
    /// Triggers the glitch effect
    /// </summary>
    /// <param name="duration">How long the glitch will last</param>
    public void TriggerGlitch(float duration)
    {
        if (!isGlitching)
        {
            StartCoroutine(GlitchCoroutine(duration));
        }
    }

    /// <summary>
    /// Coroutine to manage the glitch effect over time
    /// </summary>
    private System.Collections.IEnumerator GlitchCoroutine(float duration)
    {
        isGlitching = true;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // Apply flicker, screen shake, and distortion
            FlickerScreen();
            ShakeCamera();
            DistortScreen();

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        ResetEffects();
        isGlitching = false;
    }

    /// <summary>
    /// Randomly flickers the screen color
    /// </summary>
    private void FlickerScreen()
    {
        if (Random.value < 0.1f) // 10% chance every frame
        {
            Color randomColor = Random.value > 0.5f ? Color.white : Color.red;
            mainCamera.backgroundColor = randomColor;
        }
        else
        {
            mainCamera.backgroundColor = Color.black;
        }
    }

    /// <summary>
    /// Shakes the camera position randomly
    /// </summary>
    private void ShakeCamera()
    {
        if (mainCamera != null)
        {
            Vector3 randomShake = Random.insideUnitSphere * glitchIntensity;
            randomShake.z = -10f; // Keep the camera's Z position fixed
            mainCamera.transform.localPosition = randomShake;
        }
    }

    /// <summary>
    /// Applies screen distortion (chromatic aberration, lens distortion, etc.)
    /// </summary>
    private void DistortScreen()
    {
        if (lensDistortionEffect != null)
        {
            lensDistortionEffect.intensity.value = Random.Range(-distortionAmount, distortionAmount);
        }

        if (chromaticAberrationEffect != null)
        {
            chromaticAberrationEffect.intensity.value = Random.Range(0.5f, 1f);
        }

        if (vignetteEffect != null)
        {
            vignetteEffect.intensity.value = Random.Range(0.4f, 0.6f);
        }
    }

    /// <summary>
    /// Resets all visual effects to normal
    /// </summary>
    private void ResetEffects()
    {
        if (mainCamera != null)
        {
            mainCamera.transform.localPosition = new Vector3(0f, 0f, -10f);
        }

        if (lensDistortionEffect != null)
        {
            lensDistortionEffect.intensity.value = 0f;
        }

        if (chromaticAberrationEffect != null)
        {
            chromaticAberrationEffect.intensity.value = 0f;
        }

        if (vignetteEffect != null)
        {
            vignetteEffect.intensity.value = 0.3f;
        }

        mainCamera.backgroundColor = Color.black;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.2f);
    }
}
