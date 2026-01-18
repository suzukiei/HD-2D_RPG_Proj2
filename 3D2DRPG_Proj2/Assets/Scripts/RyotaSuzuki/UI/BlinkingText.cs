using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using DG.Tweening;

/// <summary>
/// テキストをゆっくり点滅させるコンポーネント
/// TextMeshProまたは通常のUI Textに対応
/// </summary>
public class BlinkingText : MonoBehaviour
{
    [Header("点滅設定")]
    [Tooltip("点滅の速度（小さいほどゆっくり）")]
    [SerializeField] private float blinkSpeed = 1.0f;
    
    [Tooltip("最小アルファ値（0.0 = 完全透明）")]
    [SerializeField] private float minAlpha = 0.0f;
    
    [Tooltip("最大アルファ値（1.0 = 完全不透明）")]
    [SerializeField] private float maxAlpha = 1.0f;
    
    [Tooltip("開始時に自動的に点滅を開始")]
    [SerializeField] private bool autoStart = true;
    
    [Header("フェード設定")]
    [Tooltip("開始時にフェードインするか")]
    [SerializeField] private bool fadeInOnStart = false;
    
    [Tooltip("フェードイン時間")]
    [SerializeField] private float fadeInDuration = 1.0f;
    
    [Tooltip("フェードアウト時間")]
    [SerializeField] private float fadeOutDuration = 1.0f;

    [Header("画面フェード設定")]
    [Tooltip("シーン遷移時に画面全体をフェードするか")]
    [SerializeField] private bool fadeScreenOnSceneChange = true;
    
    [Tooltip("画面フェード用のCanvasがあれば指定（なければ自動生成）")]
    [SerializeField] private CanvasGroup screenFadeCanvas;

    private TextMeshProUGUI tmpText;
    private UnityEngine.UI.Text uiText;
    private bool isBlinking = false;
    private float currentAlpha;
    private bool fadingOut = true;
    private Tween currentTween;
    
    // 自動生成したフェード用オブジェクト
    private GameObject autoFadePanel;

    private void Awake()
    {
        // TextMeshProまたはUI Textコンポーネントを取得
        tmpText = GetComponent<TextMeshProUGUI>();
        uiText = GetComponent<UnityEngine.UI.Text>();

        if (tmpText == null && uiText == null)
        {
            Debug.LogWarning("BlinkingText: TextMeshProUGUIまたはUI Textコンポーネントが見つかりません。", this);
        }

        currentAlpha = maxAlpha;
    }

    private void Start()
    {
        if (fadeInOnStart)
        {
            // フェードインしてから点滅開始
            FadeIn(() => {
                if (autoStart)
                {
                    StartBlinking();
                }
            });
        }
        else if (autoStart)
        {
            StartBlinking();
        }
    }

    private void Update()
    {
        //いずれはゲームパッドのキーにも対応させなきゃいけない
        if(Input.GetKeyDown(KeyCode.Return))
        {
            FadeOutAndLoadScene("Opening");
        }

        if (!isBlinking) return;

        // アルファ値を更新
        if (fadingOut)
        {
            currentAlpha -= blinkSpeed * Time.deltaTime;
            if (currentAlpha <= minAlpha)
            {
                currentAlpha = minAlpha;
                fadingOut = false;
            }
        }
        else
        {
            currentAlpha += blinkSpeed * Time.deltaTime;
            if (currentAlpha >= maxAlpha)
            {
                currentAlpha = maxAlpha;
                fadingOut = true;
            }
        }

        // テキストのアルファ値を適用
        ApplyAlpha(currentAlpha);
    }

    /// <summary>
    /// テキストにアルファ値を適用
    /// </summary>
    private void ApplyAlpha(float alpha)
    {
        if (tmpText != null)
        {
            Color color = tmpText.color;
            color.a = alpha;
            tmpText.color = color;
        }
        else if (uiText != null)
        {
            Color color = uiText.color;
            color.a = alpha;
            uiText.color = color;
        }
    }

    /// <summary>
    /// 点滅を開始
    /// </summary>
    public void StartBlinking()
    {
        isBlinking = true;
        currentAlpha = maxAlpha;
        fadingOut = true;
    }

    /// <summary>
    /// 点滅を停止
    /// </summary>
    public void StopBlinking()
    {
        isBlinking = false;
        ApplyAlpha(maxAlpha); // 完全に表示した状態で停止
    }

    /// <summary>
    /// 点滅を一時停止（現在のアルファ値を保持）
    /// </summary>
    public void PauseBlinking()
    {
        isBlinking = false;
    }

    /// <summary>
    /// 点滅を再開
    /// </summary>
    public void ResumeBlinking()
    {
        isBlinking = true;
    }

    /// <summary>
    /// 点滅速度を変更
    /// </summary>
    public void SetBlinkSpeed(float speed)
    {
        blinkSpeed = speed;
    }

    /// <summary>
    /// アルファ値の範囲を変更
    /// </summary>
    public void SetAlphaRange(float min, float max)
    {
        minAlpha = Mathf.Clamp01(min);
        maxAlpha = Mathf.Clamp01(max);
    }
    
    /// <summary>
    /// フェードイン（透明から表示）
    /// </summary>
    public void FadeIn(System.Action onComplete = null)
    {
        // 現在のTweenをキャンセル
        currentTween?.Kill();
        
        // 透明からスタート
        ApplyAlpha(0f);
        
        if (tmpText != null)
        {
            currentTween = tmpText.DOFade(maxAlpha, fadeInDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => {
                    currentAlpha = maxAlpha;
                    onComplete?.Invoke();
                });
        }
        else if (uiText != null)
        {
            currentTween = uiText.DOFade(maxAlpha, fadeInDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => {
                    currentAlpha = maxAlpha;
                    onComplete?.Invoke();
                });
        }
    }
    
    /// <summary>
    /// フェードアウト（表示から透明）
    /// </summary>
    public void FadeOut(System.Action onComplete = null)
    {
        // 現在のTweenと点滅をキャンセル
        currentTween?.Kill();
        isBlinking = false;
        
        if (tmpText != null)
        {
            currentTween = tmpText.DOFade(0f, fadeOutDuration)
                .SetEase(Ease.InQuad)
                .OnComplete(() => {
                    currentAlpha = 0f;
                    onComplete?.Invoke();
                });
        }
        else if (uiText != null)
        {
            currentTween = uiText.DOFade(0f, fadeOutDuration)
                .SetEase(Ease.InQuad)
                .OnComplete(() => {
                    currentAlpha = 0f;
                    onComplete?.Invoke();
                });
        }
    }
    
    /// <summary>
    /// フェードアウトしてからシーン遷移
    /// </summary>
    public void FadeOutAndLoadScene(string sceneName)
    {
        StartCoroutine(FadeOutAndLoadSceneCoroutine(sceneName));
    }
    
    /// <summary>
    /// フェードアウトしてからシーン遷移（コルーチン版）
    /// </summary>
    private System.Collections.IEnumerator FadeOutAndLoadSceneCoroutine(string sceneName)
    {
        // 点滅を停止
        isBlinking = false;
        
        // テキストをフェードアウト
        float startAlpha = currentAlpha;
        float elapsed = 0f;
        
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeOutDuration;
            float alpha = Mathf.Lerp(startAlpha, 0f, t);
            ApplyAlpha(alpha);
            yield return null;
        }
        
        // 完全に透明に
        ApplyAlpha(0f);
        
        Debug.Log($"[BlinkingText] テキストフェードアウト完了、シーン遷移: {sceneName}");
        
        // SceneTransitionManagerがあれば使用（画面全体のフェード）
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadSceneWithFade(sceneName);
        }
        else
        {
            // なければ直接遷移
            SceneManager.LoadScene(sceneName);
        }
    }
    
    private void OnDestroy()
    {
        currentTween?.Kill();
    }
}

