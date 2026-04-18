using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class CameraTurnZone : MonoBehaviour
{
    [Header("‚±‚Ìƒ][ƒ“‚Å‚ÌŒü‚«")]
    [SerializeField] private CameraCardinalYaw direction = CameraCardinalYaw.Right;

    [Header("—Dæ“xi‘å‚«‚¢‚Ù‚Ç—Dæj")]
    [SerializeField] private int priority = 0;

    [Header("‘ÎÛƒ^ƒO")]
    [SerializeField] private string targetTag = "Player";

    [Header("Resolver")]
    [SerializeField] private CameraTurnZoneResolver resolver;

    public CameraCardinalYaw Direction => direction;
    public int Priority => priority;

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(targetTag)) return;
        if (resolver == null) return;

        resolver.EnterZone(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(targetTag)) return;
        if (resolver == null) return;

        resolver.ExitZone(this);
    }
}