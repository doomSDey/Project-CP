using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Camera))]
public class CameraBounds : MonoBehaviour
{
    [Header("Camera Target")]
    public Transform target;          // e.g. Player transform

    [Header("Tilemap for Level Bounds")]
    public Tilemap tilemap;

    private Camera mainCamera;
    private Vector3 minBounds;
    private Vector3 maxBounds;

    void Start()
    {
        mainCamera = GetComponent<Camera>();
        if (tilemap != null)
        {
            // Convert tilemap local bounds to world space
            Bounds localBounds = tilemap.localBounds;
            Vector3 worldMin = tilemap.transform.TransformPoint(localBounds.min);
            Vector3 worldMax = tilemap.transform.TransformPoint(localBounds.max);

            minBounds = worldMin;
            maxBounds = worldMax;
        }
        else
        {
            Debug.LogWarning("No tilemap assigned to CameraBounds. Clamping disabled.");
        }
    }

    void LateUpdate()
    {
        if (target == null || tilemap == null) return;

        float camHalfHeight = mainCamera.orthographicSize;
        float camHalfWidth = mainCamera.aspect * camHalfHeight;

        Vector3 targetPos = target.position;

        // If tilemap is smaller than camera, handle that scenario:
        float mapWidth = maxBounds.x - minBounds.x;
        float mapHeight = maxBounds.y - minBounds.y;

        float clampedX, clampedY;

        // Horizontal clamp
        if (mapWidth <= 2f * camHalfWidth)
        {
            // The camera is wider than (or equal to) the tilemap
            // Center the camera horizontally on the tilemap
            clampedX = (minBounds.x + maxBounds.x) / 2f;
        }
        else
        {
            clampedX = Mathf.Clamp(targetPos.x, minBounds.x + camHalfWidth, maxBounds.x - camHalfWidth);
        }

        // Vertical clamp
        if (mapHeight <= 2f * camHalfHeight)
        {
            // The camera is taller than (or equal to) the tilemap
            // Center the camera vertically on the tilemap
            clampedY = (minBounds.y + maxBounds.y) / 2f;
        }
        else
        {
            clampedY = Mathf.Clamp(targetPos.y, minBounds.y + camHalfHeight, maxBounds.y - camHalfHeight);
        }

        transform.position = new Vector3(clampedX, clampedY, transform.position.z);
    }
}
