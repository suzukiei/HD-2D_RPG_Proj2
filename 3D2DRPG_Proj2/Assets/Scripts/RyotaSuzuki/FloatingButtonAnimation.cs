using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace RyotaSuzuki
{
    /// <summary>
    /// アニメーションの方向設定
    /// </summary>
    public enum AnimationDirection
    {
        Auto,       // 自動（画面位置に応じて決定）
        Up,         // 強制的に上方向
        Down,       // 強制的に下方向
        Manual      // 手動設定（moveUpwardの値を使用）
    }

    /// <summary>
    /// UIボタンにフローティングアニメーションを適用するコンポーネント
    /// ▲▼ボタンなどに上下にゆっくり動くアニメーションを提供します
    /// 画面境界を考慮して適切な方向にアニメーションします
    /// </summary>
    public class FloatingButtonAnimation : MonoBehaviour
    {
            [Header("フローティングアニメーション設定")]
    [SerializeField, Header("上下移動の距離（ピクセル）")] 
    private float floatDistance = 10f;
    
    [SerializeField, Header("アニメーションの継続時間（秒）")] 
    private float duration = 2f;
    
    [SerializeField, Header("イージングタイプ")] 
    private Ease easeType = Ease.InOutSine;
    
    [SerializeField, Header("自動開始する？")] 
    private bool autoStart = true;
    
    [SerializeField, Header("ボタンがクリックされた時にアニメーションを一時停止する？")] 
    private bool pauseOnClick = true;

    [Header("方向制御設定")]
    [SerializeField, Header("アニメーション方向")] 
    private AnimationDirection direction = AnimationDirection.Auto;
    
    [SerializeField, Header("画面境界からの最小距離（ピクセル）")] 
    private float screenBoundaryMargin = 50f;
    
    [SerializeField, Header("手動方向設定時の方向")] 
    private bool moveUpward = true;

        [Header("デバッグ設定")]
        [SerializeField, Header("デバッグログを表示する？")] 
        private bool showDebugLog = false;

        private RectTransform rectTransform;
        private Canvas parentCanvas;
        private Vector2 originalPosition;
        private Tween floatingTween;
        private Button button;
        private bool isAnimating = false;
        private bool shouldMoveUp = true; // 実際のアニメーション方向

        void Start()
        {
            InitializeComponents();
            
            if (autoStart)
            {
                StartFloatingAnimation();
            }
        }

        void InitializeComponents()
        {
            // RectTransformコンポーネントを取得
            rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                Debug.LogError($"[FloatingButtonAnimation] {gameObject.name}: RectTransformが見つかりません！");
                return;
            }

            // 親Canvasを取得
            parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas == null)
            {
                Debug.LogWarning($"[FloatingButtonAnimation] {gameObject.name}: 親Canvasが見つかりません。画面境界の検出ができません。");
            }

            // 元の位置を保存
            originalPosition = rectTransform.anchoredPosition;
            
            // アニメーション方向を決定
            DetermineAnimationDirection();
            
            // Buttonコンポーネントを取得（オプション）
            button = GetComponent<Button>();
            if (button != null && pauseOnClick)
            {
                // ボタンクリック時の処理を追加
                button.onClick.AddListener(OnButtonClicked);
            }

            if (showDebugLog)
            {
                Debug.Log($"[FloatingButtonAnimation] {gameObject.name}: 初期化完了 - 元の位置: {originalPosition}, 方向: {(shouldMoveUp ? "上" : "下")}");
            }
        }

        /// <summary>
        /// アニメーション方向を決定します
        /// </summary>
        private void DetermineAnimationDirection()
        {
            switch (direction)
            {
                case AnimationDirection.Auto:
                    shouldMoveUp = ShouldMoveUpBasedOnPosition();
                    break;
                case AnimationDirection.Up:
                    shouldMoveUp = true;
                    break;
                case AnimationDirection.Down:
                    shouldMoveUp = false;
                    break;
                case AnimationDirection.Manual:
                    shouldMoveUp = moveUpward;
                    break;
            }

            if (showDebugLog)
            {
                Debug.Log($"[FloatingButtonAnimation] {gameObject.name}: 方向決定 - {direction} -> {(shouldMoveUp ? "上" : "下")}");
            }
        }

        /// <summary>
        /// 画面位置に基づいてアニメーション方向を決定します
        /// </summary>
        private bool ShouldMoveUpBasedOnPosition()
        {
            if (parentCanvas == null || rectTransform == null) 
            {
                // デフォルトは下方向（安全側）
                return false;
            }

            try
            {
                // ワールド座標でのボタン位置を取得
                Vector3[] buttonCorners = new Vector3[4];
                rectTransform.GetWorldCorners(buttonCorners);
                
                // Canvasのワールド座標範囲を取得
                RectTransform canvasRect = parentCanvas.GetComponent<RectTransform>();
                Vector3[] canvasCorners = new Vector3[4];
                canvasRect.GetWorldCorners(canvasCorners);

                // ボタンの上端とCanvas上端までの距離
                float distanceToTop = canvasCorners[2].y - buttonCorners[2].y;
                // ボタンの下端とCanvas下端までの距離  
                float distanceToBottom = buttonCorners[0].y - canvasCorners[0].y;

                // 上方向に移動した場合の予想位置
                float topAfterAnimation = buttonCorners[2].y + floatDistance;
                // 下方向に移動した場合の予想位置
                float bottomAfterAnimation = buttonCorners[0].y - floatDistance;

                // 境界マージンを考慮して安全かどうか判定
                bool canMoveUp = (canvasCorners[2].y - topAfterAnimation) >= screenBoundaryMargin;
                bool canMoveDown = (bottomAfterAnimation - canvasCorners[0].y) >= screenBoundaryMargin;

                if (showDebugLog)
                {
                    Debug.Log($"[FloatingButtonAnimation] {gameObject.name}: 境界チェック - 上移動可能: {canMoveUp}, 下移動可能: {canMoveDown}");
                    Debug.Log($"[FloatingButtonAnimation] {gameObject.name}: 距離 - 上: {distanceToTop:F1}, 下: {distanceToBottom:F1}");
                }

                // 両方向とも安全な場合は、より距離のある方向を選択
                if (canMoveUp && canMoveDown)
                {
                    return distanceToTop > distanceToBottom;
                }
                // 上方向のみ安全
                else if (canMoveUp)
                {
                    return true;
                }
                // 下方向のみ安全、または両方とも危険な場合は下方向（安全側）
                else
                {
                    return false;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[FloatingButtonAnimation] {gameObject.name}: 境界検出エラー - {e.Message}");
                return false; // エラー時は安全側の下方向
            }
        }

        /// <summary>
        /// フローティングアニメーションを開始します
        /// </summary>
        public void StartFloatingAnimation()
        {
            if (rectTransform == null) return;

            // 既存のアニメーションを停止
            StopFloatingAnimation();

            // アニメーション方向を再決定（位置が変わっている可能性）
            if (direction == AnimationDirection.Auto)
            {
                DetermineAnimationDirection();
            }

            // 移動先の位置を計算
            float targetY = shouldMoveUp ? 
                originalPosition.y + floatDistance : 
                originalPosition.y - floatDistance;

            // 上下にループするアニメーションを作成
            floatingTween = rectTransform.DOAnchorPosY(targetY, duration)
                .SetEase(easeType)
                .SetLoops(-1, LoopType.Yoyo)  // 無限ループ、ヨーヨー効果
                .OnStart(() => {
                    isAnimating = true;
                    if (showDebugLog)
                        Debug.Log($"[FloatingButtonAnimation] {gameObject.name}: アニメーション開始 - 方向: {(shouldMoveUp ? "上" : "下")}");
                })
                .OnKill(() => {
                    isAnimating = false;
                    if (showDebugLog)
                        Debug.Log($"[FloatingButtonAnimation] {gameObject.name}: アニメーション停止");
                });
        }

        /// <summary>
        /// フローティングアニメーションを停止し、元の位置に戻します
        /// </summary>
        public void StopFloatingAnimation()
        {
            if (floatingTween != null && floatingTween.IsActive())
            {
                floatingTween.Kill();
                
                // 元の位置に戻る
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = originalPosition;
                }
            }
        }

        /// <summary>
        /// アニメーションを一時停止/再開します
        /// </summary>
        public void TogglePause()
        {
            if (floatingTween != null && floatingTween.IsActive())
            {
                if (floatingTween.IsPlaying())
                {
                    floatingTween.Pause();
                    if (showDebugLog)
                        Debug.Log($"[FloatingButtonAnimation] {gameObject.name}: アニメーション一時停止");
                }
                else
                {
                    floatingTween.Play();
                    if (showDebugLog)
                        Debug.Log($"[FloatingButtonAnimation] {gameObject.name}: アニメーション再開");
                }
            }
        }

        /// <summary>
        /// アニメーション設定を動的に変更します
        /// </summary>
        /// <param name="newDistance">新しい移動距離</param>
        /// <param name="newDuration">新しい継続時間</param>
        /// <param name="newEase">新しいイージング</param>
        public void UpdateAnimationSettings(float newDistance, float newDuration, Ease newEase)
        {
            floatDistance = newDistance;
            duration = newDuration;
            easeType = newEase;

            // 方向を再計算（距離が変わったため）
            if (direction == AnimationDirection.Auto)
            {
                DetermineAnimationDirection();
            }

            // アニメーション中の場合は再開始
            if (isAnimating)
            {
                StartFloatingAnimation();
            }

            if (showDebugLog)
            {
                Debug.Log($"[FloatingButtonAnimation] {gameObject.name}: 設定更新 - 距離: {newDistance}, 時間: {newDuration}, イージング: {newEase}, 方向: {(shouldMoveUp ? "上" : "下")}");
            }
        }

        /// <summary>
        /// アニメーション方向設定を変更します
        /// </summary>
        /// <param name="newDirection">新しい方向設定</param>
        /// <param name="manualUpward">Manual設定時の上方向フラグ</param>
        public void UpdateDirectionSetting(AnimationDirection newDirection, bool manualUpward = true)
        {
            direction = newDirection;
            moveUpward = manualUpward;
            
            // 方向を再決定
            DetermineAnimationDirection();

            // アニメーション中の場合は再開始
            if (isAnimating)
            {
                StartFloatingAnimation();
            }

            if (showDebugLog)
            {
                Debug.Log($"[FloatingButtonAnimation] {gameObject.name}: 方向設定更新 - {newDirection}, 結果: {(shouldMoveUp ? "上" : "下")}");
            }
        }

        /// <summary>
        /// ボタンがクリックされた時の処理
        /// </summary>
        private void OnButtonClicked()
        {
            if (pauseOnClick && isAnimating)
            {
                // 短時間アニメーションを停止してからリスタート（クリック感を演出）
                StopFloatingAnimation();
                
                // 少し遅延してからアニメーション再開
                DOVirtual.DelayedCall(0.1f, () => {
                    if (this != null && gameObject.activeInHierarchy)
                    {
                        StartFloatingAnimation();
                    }
                });

                if (showDebugLog)
                {
                    Debug.Log($"[FloatingButtonAnimation] {gameObject.name}: ボタンクリック - アニメーション一時停止後再開");
                }
            }
        }

        /// <summary>
        /// 現在アニメーション中かどうかを返します
        /// </summary>
        public bool IsAnimating => isAnimating;

        /// <summary>
        /// アニメーションの進行状況を0-1で返します
        /// </summary>
        public float AnimationProgress
        {
            get
            {
                if (floatingTween != null && floatingTween.IsActive())
                {
                    return floatingTween.ElapsedPercentage();
                }
                return 0f;
            }
        }

        void OnDestroy()
        {
            // オブジェクト破棄時にアニメーションを停止
            StopFloatingAnimation();
            
            // ボタンのイベントリスナーを解除
            if (button != null)
            {
                button.onClick.RemoveListener(OnButtonClicked);
            }
        }

        void OnDisable()
        {
            // オブジェクトが非アクティブになった時にアニメーションを停止
            StopFloatingAnimation();
        }

        void OnEnable()
        {
            // オブジェクトがアクティブになった時にアニメーションを再開
            if (autoStart && rectTransform != null)
            {
                // 少し遅延してからアニメーション開始（確実に初期化が終わってから）
                DOVirtual.DelayedCall(0.1f, () => {
                    if (this != null && gameObject.activeInHierarchy)
                    {
                        StartFloatingAnimation();
                    }
                });
            }
        }
    }
} 