using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

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

    private TextMeshProUGUI tmpText;
    private UnityEngine.UI.Text uiText;
    private bool isBlinking = false;
    private float currentAlpha;
    private bool fadingOut = true;

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
        if (autoStart)
        {
            StartBlinking();
        }
    }

    private void Update()
    {
        //いずれはゲームパッドのキーにも対応させなきゃいけない
        if(Input.GetKeyDown(KeyCode.Return))
        {
            SceneManager.LoadScene("GameField");
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
}

