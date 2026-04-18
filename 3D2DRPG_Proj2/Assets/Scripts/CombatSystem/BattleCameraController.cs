using System.Collections;
using UnityEngine;

public sealed class BattleCameraController : MonoBehaviour
{
    [Header("ç¿ïWån")]
    [SerializeField] private bool useLocalTransform = true;

    [Header("èÌéûÇÃÇ‰ÇÁÇ¨")]
    [SerializeField] private float swayHorizontal = 0.12f; // â°(Z)
    [SerializeField] private float swayVertical = 0.02f;   // èc(Y)
    [SerializeField] private float swaySpeedH = 0.35f;
    [SerializeField] private float swaySpeedV = 0.22f;
    [SerializeField] private float rollAmount = 0.5f;
    [SerializeField] private float rollSpeed = 0.18f;

    [Header("óhÇÍÇÃÉtÉFÅ[Éh")]
    [SerializeField] private float swayFadeOutDuration = 0.10f; // äÒÇÈéûÇ…óhÇÍÇé„ÇﬂÇÈ
    [SerializeField] private float swayFadeInDuration = 0.20f;  // ñﬂÇÈéûÇ…óhÇÍÇñﬂÇ∑

    [Header("ÉXÉLÉãî≠ìÆéûÇÃäÒÇËââèo")]
    [SerializeField] private float focusDepthAmount = 1.2f;
    [SerializeField] private float focusSideAmount = 0.35f;
    [SerializeField] private float focusVerticalAmount = 0.05f;

    [SerializeField] private float focusTiltZ = 1.2f;
    [SerializeField] private float lookAtWeightYaw = 6f;
    [SerializeField] private float lookAtWeightPitch = 2f;

    [SerializeField] private float focusInDuration = 0.12f;
    [SerializeField] private float focusHoldDuration = 0.10f;
    [SerializeField] private float focusOutDuration = 0.22f;

    [Header("ãììÆ")]
    [SerializeField] private bool restartFocusIfPlaying = true;

    private Vector3 _basePosition;
    private Quaternion _baseRotation;

    private Vector3 _eventPosOffset;
    private Vector3 _eventRotOffsetEuler;

    private float _timeOffset;
    private Coroutine _focusRoutine;

    // 0 = óhÇÍÇ»Çµ, 1 = í èÌóhÇÍ
    private float _swayBlend = 1f;

    private void Start()
    {
        CacheBaseTransform();
        _timeOffset = Random.Range(0f, 100f);
    }

    private void LateUpdate()
    {
        float t = Time.time + _timeOffset;

        float horizontal = Mathf.Sin(t * swaySpeedH) * swayHorizontal; // Z
        float vertical = Mathf.Sin(t * swaySpeedV) * swayVertical;     // Y
        float rotZ = Mathf.Sin(t * rollSpeed) * rollAmount;

        Vector3 rawSwayPos = new Vector3(0f, vertical, horizontal);
        Vector3 rawSwayRot = new Vector3(0f, 0f, rotZ);

        Vector3 swayPos = rawSwayPos * _swayBlend;
        Vector3 swayRot = rawSwayRot * _swayBlend;

        Vector3 finalPos = _basePosition + swayPos + _eventPosOffset;
        Quaternion finalRot = _baseRotation * Quaternion.Euler(swayRot + _eventRotOffsetEuler);

        if (UseLocalSpace())
        {
            transform.localPosition = finalPos;
            transform.localRotation = finalRot;
        }
        else
        {
            transform.position = finalPos;
            transform.rotation = finalRot;
        }
    }

    public void CacheBaseTransform()
    {
        if (UseLocalSpace())
        {
            _basePosition = transform.localPosition;
            _baseRotation = transform.localRotation;
        }
        else
        {
            _basePosition = transform.position;
            _baseRotation = transform.rotation;
        }
    }

    public void PlaySkillFocus(Vector3 targetWorldPos, float power = 1f)
    {
        if (_focusRoutine != null)
        {
            if (!restartFocusIfPlaying) return;
            StopCoroutine(_focusRoutine);
            _focusRoutine = null;
        }

        _focusRoutine = StartCoroutine(CoPlaySkillFocus(targetWorldPos, power));
    }

    private IEnumerator CoPlaySkillFocus(Vector3 targetWorldPos, float power)
    {
        Vector3 startPos = _eventPosOffset;
        Vector3 startRot = _eventRotOffsetEuler;
        float startSwayBlend = _swayBlend;

        try
        {
            Vector3 cameraBaseWorldPos = GetBaseWorldPosition();
            Quaternion cameraBaseWorldRot = GetBaseWorldRotation();

            Vector3 toTargetWorld = targetWorldPos - cameraBaseWorldPos;
            Vector3 toTargetLocal = WorldDirectionToReferenceLocal(toTargetWorld);

            float sideSign = Mathf.Sign(toTargetLocal.z);
            if (Mathf.Approximately(sideSign, 0f)) sideSign = 1f;

            float verticalSign = Mathf.Sign(toTargetLocal.y);

            Vector3 targetPos = new Vector3(
                focusDepthAmount * power,
                focusVerticalAmount * verticalSign * power,
                focusSideAmount * sideSign * power
            );

            Vector3 targetRot = CalculateLookRotationOffsetEuler(
                cameraBaseWorldPos,
                cameraBaseWorldRot,
                targetWorldPos,
                sideSign,
                power
            );

            // ç°ÇÃèÛë‘Ç©ÇÁÇªÇÃÇ‹Ç‹êVÉ^Å[ÉQÉbÉgÇ÷äÒÇÈ
            yield return LerpFocusState(
                fromPos: startPos,
                toPos: targetPos,
                fromRot: startRot,
                toRot: targetRot,
                fromSwayBlend: startSwayBlend,
                toSwayBlend: 0f,
                duration: Mathf.Max(focusInDuration, swayFadeOutDuration)
            );

            if (focusHoldDuration > 0f)
            {
                yield return new WaitForSeconds(focusHoldDuration);
            }

            // ñﬂÇËÇ¬Ç¬ÅAóhÇÍÇ‡èôÅXÇ…ñﬂÇ∑
            yield return LerpFocusState(
                fromPos: targetPos,
                toPos: Vector3.zero,
                fromRot: targetRot,
                toRot: Vector3.zero,
                fromSwayBlend: 0f,
                toSwayBlend: 1f,
                duration: Mathf.Max(focusOutDuration, swayFadeInDuration)
            );
        }
        finally
        {
            _eventPosOffset = Vector3.zero;
            _eventRotOffsetEuler = Vector3.zero;
            _swayBlend = 1f;
            _focusRoutine = null;
        }
    }

    private bool UseLocalSpace()
    {
        return useLocalTransform && transform.parent != null;
    }

    private Vector3 GetBaseWorldPosition()
    {
        if (UseLocalSpace())
        {
            return transform.parent.TransformPoint(_basePosition);
        }

        return _basePosition;
    }

    private Quaternion GetBaseWorldRotation()
    {
        if (UseLocalSpace())
        {
            return transform.parent.rotation * _baseRotation;
        }

        return _baseRotation;
    }

    private Vector3 WorldDirectionToReferenceLocal(Vector3 worldDirection)
    {
        if (UseLocalSpace())
        {
            return transform.parent.InverseTransformDirection(worldDirection);
        }

        return transform.InverseTransformDirection(worldDirection);
    }

    private Vector3 CalculateLookRotationOffsetEuler(
        Vector3 cameraBaseWorldPos,
        Quaternion cameraBaseWorldRot,
        Vector3 targetWorldPos,
        float sideSign,
        float power)
    {
        Vector3 dirWorld = targetWorldPos - cameraBaseWorldPos;
        if (dirWorld.sqrMagnitude <= 0.0001f)
        {
            return new Vector3(0f, 0f, focusTiltZ * -sideSign * power);
        }

        Quaternion lookRotWorld = Quaternion.LookRotation(dirWorld.normalized, Vector3.up);
        Quaternion deltaWorld = Quaternion.Inverse(cameraBaseWorldRot) * lookRotWorld;
        Vector3 deltaEuler = NormalizeEuler(deltaWorld.eulerAngles);

        float yaw = deltaEuler.y * lookAtWeightYaw / 100f;
        float pitch = -deltaEuler.x * lookAtWeightPitch / 100f;
        float roll = focusTiltZ * -sideSign * power;

        return new Vector3(pitch, yaw, roll);
    }

    private IEnumerator LerpFocusState(
        Vector3 fromPos,
        Vector3 toPos,
        Vector3 fromRot,
        Vector3 toRot,
        float fromSwayBlend,
        float toSwayBlend,
        float duration)
    {
        if (duration <= 0f)
        {
            _eventPosOffset = toPos;
            _eventRotOffsetEuler = toRot;
            _swayBlend = toSwayBlend;
            yield break;
        }

        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float n = Mathf.Clamp01(t / duration);

            // çsÇ´Ç∆ãAÇËÇ≈à·òaä¥Ç™èoÇ…Ç≠Ç¢ï‚ä‘
            float eased = EaseInOutSine(n);

            _eventPosOffset = Vector3.LerpUnclamped(fromPos, toPos, eased);
            _eventRotOffsetEuler = Vector3.LerpUnclamped(fromRot, toRot, eased);
            _swayBlend = Mathf.LerpUnclamped(fromSwayBlend, toSwayBlend, eased);

            yield return null;
        }

        _eventPosOffset = toPos;
        _eventRotOffsetEuler = toRot;
        _swayBlend = toSwayBlend;
    }

    private static Vector3 NormalizeEuler(Vector3 euler)
    {
        euler.x = NormalizeAngle(euler.x);
        euler.y = NormalizeAngle(euler.y);
        euler.z = NormalizeAngle(euler.z);
        return euler;
    }

    private static float NormalizeAngle(float angle)
    {
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }

    private static float EaseInOutSine(float t)
    {
        t = Mathf.Clamp01(t);
        return -(Mathf.Cos(Mathf.PI * t) - 1f) * 0.5f;
    }
}