using UnityEngine;
using DG.Tweening; // DOTween library

namespace platform_and_gems.gem_animation
{
    public class gem_animation : MonoBehaviour
    {
        [Header("Up Down Settings")]
        public bool CanMove = true;
        public float MoveDistance = 1f; // Up-down movement distance
        public float MoveDuration = 1f; // Up-down movement Duration
        public Ease MoveEase = Ease.Linear; // Ease type for up-down movement
        private float m_MoveDistance = 0f;

        [Header("Rotation Settings")]
        public bool CanRotation = true;
        public Vector3 RotationAxis = Vector3.up; // Rotation axis 
        public float RotationDuration = 2f; // Duration of a complete rotation
        public Ease RotationEase = Ease.Linear; // Ease type for rotation

        private Tween moveTween;
        private Tween rotateTween;

        void Start()
        {
            if (CanMove)
            {
                StartMovement();
            }
            if (CanRotation) 
            {
                StartRotation();
            }
        }

        public void StartMovement()
        {
            m_MoveDistance =transform.localPosition.y + MoveDistance;
            moveTween?.Kill();
            moveTween = transform.DOLocalMoveY(m_MoveDistance, MoveDuration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(MoveEase);
        }

        public void StartRotation()
        {
            rotateTween?.Kill();
            rotateTween = transform.DOLocalRotate(RotationAxis * 360f, RotationDuration, RotateMode.FastBeyond360)
                .SetLoops(-1 ,LoopType.Restart)
                .SetEase(RotationEase);
        }


        void OnDestroy()
        {
            moveTween?.Kill();
            rotateTween?.Kill();
        }
    }
}
