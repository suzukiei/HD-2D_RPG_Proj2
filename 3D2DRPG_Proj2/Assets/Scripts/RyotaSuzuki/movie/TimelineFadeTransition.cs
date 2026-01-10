using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Timeline Signal経由でフェードイン・フェードアウトを制御するクラス
/// </summary>
public class TimelineFadeTransition : MonoBehaviour
{
    [Header("フェード設定")]
    [SerializeField, Tooltip("フェード用Image")]
    private Image fadeImage;
    
    [SerializeField, Tooltip("フェード時間（秒）")]
    private float fadeDuration = 1f;
    
    [Header("色設定")]
    [SerializeField, Tooltip("フェード色")]
    private Color fadeColor = Color.black;
    
    private Canvas fadeCanvas;
    private bool isInitialized = false;
    
    void Awake()
    {
        Initialize();
    }
    
    /// <summary>
    /// 初期化処理
    /// </summary>
    private void Initialize()
    {
        if (isInitialized) return;
        
        // FadeImageが設定されていない場合は自動生成
        if (fadeImage == null)
        {
            CreateFadeUI();
        }
        else
        {
            // 手動設定の場合、最初は非表示
            fadeImage.gameObject.SetActive(false);
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0);
        }
        
        isInitialized = true;
    }
    
    /// <summary>
    /// フェード用UIを自動生成
    /// </summary>
    private void CreateFadeUI()
    {
        // Canvas作成
        GameObject canvasObj = new GameObject("FadeCanvas");
        fadeCanvas = canvasObj.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 9999; // 最前面
        
        canvasObj.AddComponent<GraphicRaycaster>();
        DontDestroyOnLoad(canvasObj); // シーン遷移しても残る
        
        // Image作成
        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(canvasObj.transform, false);
        
        fadeImage = imageObj.AddComponent<Image>();
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0);
        
        // 画面全体を覆う設定
        RectTransform rectTransform = imageObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;
        
        imageObj.SetActive(false);
        
        Debug.Log("[TimelineFadeTransition] フェード用UIを自動生成しました");
    }
    
    /// <summary>
    /// フェードアウト（画面を暗くする）
    /// Timeline Signalから呼び出す
    /// </summary>
    public void FadeOut()
    {
        Debug.Log("========== FadeOut が呼ばれました！ ==========");
        
        if (!isInitialized) Initialize();
        
        if (fadeImage == null)
        {
            Debug.LogError("[TimelineFadeTransition] FadeImageが設定されていません");
            return;
        }
        
        fadeImage.gameObject.SetActive(true);
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0);
        fadeImage.DOFade(1f, fadeDuration).SetUpdate(true);
        
        Debug.Log($"[TimelineFadeTransition] フェードアウト開始（{fadeDuration}秒）");
    }
    
    /// <summary>
    /// フェードイン（画面を明るくする）
    /// Timeline Signalから呼び出す
    /// </summary>
    public void FadeIn()
    {
        if (!isInitialized) Initialize();
        
        if (fadeImage == null)
        {
            Debug.LogError("[TimelineFadeTransition] FadeImageが設定されていません");
            return;
        }
        
        fadeImage.gameObject.SetActive(true);
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1);
        fadeImage.DOFade(0f, fadeDuration)
            .SetUpdate(true)
            .OnComplete(() => fadeImage.gameObject.SetActive(false));
        
        Debug.Log($"[TimelineFadeTransition] フェードイン開始（{fadeDuration}秒）");
    }
    
    /// <summary>
    /// 即座にフェードアウト（アニメーションなし）
    /// </summary>
    public void FadeOutImmediate()
    {
        if (!isInitialized) Initialize();
        
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1);
        }
    }
    
    /// <summary>
    /// 即座にフェードイン（アニメーションなし）
    /// </summary>
    public void FadeInImmediate()
    {
        if (!isInitialized) Initialize();
        
        if (fadeImage != null)
        {
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0);
            fadeImage.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// フェード時間を動的に変更
    /// </summary>
    public void SetFadeDuration(float duration)
    {
        fadeDuration = duration;
    }
    
    /// <summary>
    /// フェード色を動的に変更
    /// </summary>
    public void SetFadeColor(Color color)
    {
        fadeColor = color;
    }
    
    void OnDestroy()
    {
        // 自動生成したCanvasを削除
        if (fadeCanvas != null)
        {
            Destroy(fadeCanvas.gameObject);
        }
    }
}

