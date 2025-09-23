using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

namespace RyotaSuzuki
{
    /// <summary>
    /// フローティングボタンアニメーションのテストとデモを行うコンポーネント
    /// </summary>
    public class FloatingButtonTest : MonoBehaviour
    {
        [Header("デモ用UI要素")]
        [SerializeField, Header("上矢印ボタン")] 
        private Button upButton;
        
        [SerializeField, Header("下矢印ボタン")] 
        private Button downButton;
        
        [SerializeField, Header("設定パネル")] 
        private GameObject settingsPanel;
        
        [SerializeField, Header("距離スライダー")] 
        private Slider distanceSlider;
        
        [SerializeField, Header("時間スライダー")] 
        private Slider durationSlider;
        
        [SerializeField, Header("距離表示テキスト")] 
        private TextMeshProUGUI distanceText;
        
        [SerializeField, Header("時間表示テキスト")] 
        private TextMeshProUGUI durationText;

        [Header("デモ設定")]
        [SerializeField, Header("カウンターテキスト")] 
        private TextMeshProUGUI counterText;

        private FloatingButtonAnimation upButtonAnimation;
        private FloatingButtonAnimation downButtonAnimation;
        private int counter = 0;
        private bool settingsPanelVisible = false;

        void Start()
        {
            InitializeDemo();
            SetupUI();
        }

        void InitializeDemo()
        {
            // ▲ボタンの設定
            if (upButton != null)
            {
                upButtonAnimation = upButton.GetComponent<FloatingButtonAnimation>();
                if (upButtonAnimation == null)
                {
                    upButtonAnimation = upButton.gameObject.AddComponent<FloatingButtonAnimation>();
                }
                
                upButton.onClick.AddListener(() => {
                    counter++;
                    UpdateCounterDisplay();
                    Debug.Log("▲ボタンがクリックされました！");
                });
            }

            // ▼ボタンの設定
            if (downButton != null)
            {
                downButtonAnimation = downButton.GetComponent<FloatingButtonAnimation>();
                if (downButtonAnimation == null)
                {
                    downButtonAnimation = downButton.gameObject.AddComponent<FloatingButtonAnimation>();
                }
                
                downButton.onClick.AddListener(() => {
                    counter--;
                    UpdateCounterDisplay();
                    Debug.Log("▼ボタンがクリックされました！");
                });
            }

            // 初期カウンター表示
            UpdateCounterDisplay();
        }

        void SetupUI()
        {
            // 設定パネルを初期状態で非表示
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }

            // スライダーの設定
            if (distanceSlider != null)
            {
                distanceSlider.minValue = 5f;
                distanceSlider.maxValue = 50f;
                distanceSlider.value = 10f;
                distanceSlider.onValueChanged.AddListener(OnDistanceChanged);
            }

            if (durationSlider != null)
            {
                durationSlider.minValue = 0.5f;
                durationSlider.maxValue = 5f;
                durationSlider.value = 2f;
                durationSlider.onValueChanged.AddListener(OnDurationChanged);
            }

            // 初期値の表示を更新
            UpdateSliderDisplays();
        }

        void UpdateCounterDisplay()
        {
            if (counterText != null)
            {
                counterText.text = $"カウンター: {counter}";
            }
        }

        void OnDistanceChanged(float value)
        {
            if (distanceText != null)
            {
                distanceText.text = $"距離: {value:F1}px";
            }

            // 両方のボタンのアニメーション設定を更新
            UpdateAnimationSettings();
        }

        void OnDurationChanged(float value)
        {
            if (durationText != null)
            {
                durationText.text = $"時間: {value:F1}秒";
            }

            // 両方のボタンのアニメーション設定を更新
            UpdateAnimationSettings();
        }

        void UpdateAnimationSettings()
        {
            float distance = distanceSlider != null ? distanceSlider.value : 10f;
            float duration = durationSlider != null ? durationSlider.value : 2f;

            if (upButtonAnimation != null)
            {
                upButtonAnimation.UpdateAnimationSettings(distance, duration, Ease.InOutSine);
            }

            if (downButtonAnimation != null)
            {
                downButtonAnimation.UpdateAnimationSettings(distance, duration, Ease.InOutSine);
            }
        }

        void UpdateSliderDisplays()
        {
            if (distanceSlider != null)
            {
                OnDistanceChanged(distanceSlider.value);
            }

            if (durationSlider != null)
            {
                OnDurationChanged(durationSlider.value);
            }
        }

        void Update()
        {
            HandleInputs();
        }

        void HandleInputs()
        {
            // スペースキーで設定パネルの表示切り替え
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ToggleSettingsPanel();
            }

            // Rキーでカウンターリセット
            if (Input.GetKeyDown(KeyCode.R))
            {
                counter = 0;
                UpdateCounterDisplay();
                Debug.Log("カウンターをリセットしました");
            }

            // Pキーでアニメーションの一時停止/再開
            if (Input.GetKeyDown(KeyCode.P))
            {
                ToggleAnimations();
            }

            // 数字キーでイージングタイプを変更
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                ChangeEasing(Ease.InOutSine);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                ChangeEasing(Ease.InOutBounce);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                ChangeEasing(Ease.InOutElastic);
            }

            // 方向制御のテスト
            if (Input.GetKeyDown(KeyCode.A))
            {
                ChangeDirectionSetting(AnimationDirection.Auto);
            }
            else if (Input.GetKeyDown(KeyCode.U))
            {
                ChangeDirectionSetting(AnimationDirection.Up);
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                ChangeDirectionSetting(AnimationDirection.Down);
            }
        }

        public void ToggleSettingsPanel()
        {
            if (settingsPanel == null) return;

            settingsPanelVisible = !settingsPanelVisible;
            
            if (settingsPanelVisible)
            {
                settingsPanel.SetActive(true);
                
                // フェードインアニメーション
                CanvasGroup canvasGroup = settingsPanel.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = settingsPanel.AddComponent<CanvasGroup>();
                }
                
                canvasGroup.alpha = 0;
                canvasGroup.DOFade(1, 0.3f);
            }
            else
            {
                // フェードアウトアニメーション
                CanvasGroup canvasGroup = settingsPanel.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.DOFade(0, 0.3f)
                        .OnComplete(() => settingsPanel.SetActive(false));
                }
                else
                {
                    settingsPanel.SetActive(false);
                }
            }
        }

        public void ToggleAnimations()
        {
            if (upButtonAnimation != null)
            {
                upButtonAnimation.TogglePause();
            }

            if (downButtonAnimation != null)
            {
                downButtonAnimation.TogglePause();
            }

            Debug.Log("アニメーションの一時停止/再開を切り替えました");
        }

        public void ChangeEasing(Ease newEase)
        {
            float distance = distanceSlider != null ? distanceSlider.value : 10f;
            float duration = durationSlider != null ? durationSlider.value : 2f;

            if (upButtonAnimation != null)
            {
                upButtonAnimation.UpdateAnimationSettings(distance, duration, newEase);
            }

            if (downButtonAnimation != null)
            {
                downButtonAnimation.UpdateAnimationSettings(distance, duration, newEase);
            }

            Debug.Log($"イージングタイプを {newEase} に変更しました");
        }

        public void ChangeDirectionSetting(AnimationDirection newDirection)
        {
            if (upButtonAnimation != null)
            {
                upButtonAnimation.UpdateDirectionSetting(newDirection);
            }

            if (downButtonAnimation != null)
            {
                downButtonAnimation.UpdateDirectionSetting(newDirection);
            }

            Debug.Log($"アニメーション方向を {newDirection} に変更しました");
        }

        void OnGUI()
        {
            // デバッグ用GUI表示
            GUILayout.BeginArea(new Rect(10, 10, 300, 250));
            GUILayout.Label("=== フローティングボタンデモ ===");
            GUILayout.Label("操作方法:");
            GUILayout.Label("• Space: 設定パネル表示切り替え");
            GUILayout.Label("• R: カウンターリセット");
            GUILayout.Label("• P: アニメーション一時停止/再開");
            GUILayout.Label("• 1: InOutSine イージング");
            GUILayout.Label("• 2: InOutBounce イージング");
            GUILayout.Label("• 3: InOutElastic イージング");
            GUILayout.Label("• A: Auto方向（画面境界考慮）");
            GUILayout.Label("• U: 強制的に上方向");
            GUILayout.Label("• D: 強制的に下方向");
            GUILayout.EndArea();
        }
    }
} 