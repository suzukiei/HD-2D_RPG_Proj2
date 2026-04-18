using System.Collections;
using UnityEngine;

public enum CameraCardinalYaw
{
    Default = 0,
    Right = 1,   // +90
    Back = 2,    // +180
    Left = 3     // +270 (= -90)
}

public sealed class FieldCameraTurnController : MonoBehaviour
{
    [SerializeField] private float turnDuration = 0.25f;

    private Coroutine _turnRoutine;
    private float _defaultYaw;
    private CameraCardinalYaw _currentDirection = CameraCardinalYaw.Default;

    private void Start()
    {
        _defaultYaw = transform.eulerAngles.y;
    }

    public CameraCardinalYaw CurrentDirection => _currentDirection;

    public void SetDirection(CameraCardinalYaw direction)
    {
        _currentDirection = direction;
        float targetYaw = _defaultYaw + DirectionToYawOffset(direction);

        if (_turnRoutine != null)
        {
            StopCoroutine(_turnRoutine);
        }

        _turnRoutine = StartCoroutine(CoTurnTo(targetYaw));
    }

    public void ReturnToDefault()
    {
        SetDirection(CameraCardinalYaw.Default);
    }

    private float DirectionToYawOffset(CameraCardinalYaw direction)
    {
        switch (direction)
        {
            case CameraCardinalYaw.Right:
                return 90f;
            case CameraCardinalYaw.Back:
                return 180f;
            case CameraCardinalYaw.Left:
                return 270f;
            default:
                return 0f;
        }
    }

    private IEnumerator CoTurnTo(float targetYaw)
    {
        float startYaw = transform.eulerAngles.y;
        float time = 0f;

        while (time < turnDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / turnDuration);
            float eased = EaseInOutSine(t);

            float currentYaw = Mathf.LerpAngle(startYaw, targetYaw, eased);

            Vector3 euler = transform.eulerAngles;
            euler.y = currentYaw;
            transform.eulerAngles = euler;

            yield return null;
        }

        Vector3 finalEuler = transform.eulerAngles;
        finalEuler.y = targetYaw;
        transform.eulerAngles = finalEuler;

        _turnRoutine = null;
    }

    private float EaseInOutSine(float t)
    {
        return -(Mathf.Cos(Mathf.PI * t) - 1f) * 0.5f;
    }
}