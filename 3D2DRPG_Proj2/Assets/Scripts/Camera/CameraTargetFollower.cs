using UnityEngine;

public sealed class CameraTargetFollower : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 worldOffset;
    [SerializeField] private bool followRotation = false;

    private void LateUpdate()
    {
        if (target == null) return;

        transform.position = target.position + worldOffset;

        if (followRotation)
        {
            transform.rotation = target.rotation;
        }
    }
}