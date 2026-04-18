using UnityEngine;

public sealed class FieldSpriteFaceCamera : MonoBehaviour
{
    [SerializeField] private Transform targetCamera;
    [SerializeField] private bool reverseForward = false;

    private void LateUpdate()
    {
        Transform cam = targetCamera != null
            ? targetCamera
            : (Camera.main != null ? Camera.main.transform : null);

        if (cam == null) return;

        Vector3 toCamera = cam.position - transform.position;

        // Yé≤âÒì]ÇæÇØÇ…ÇµÇΩÇ¢ÇÃÇ≈è„â∫ê¨ï™ÇÕñ≥éã
        toCamera.y = 0f;

        if (toCamera.sqrMagnitude < 0.0001f) return;

        Vector3 forward = reverseForward ? -toCamera.normalized : toCamera.normalized;
        transform.forward = forward;
    }
}