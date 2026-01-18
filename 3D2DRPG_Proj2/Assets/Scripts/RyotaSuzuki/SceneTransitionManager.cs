using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

/// <summary>
/// シーン遷移トランジション効果を管理するマネージャー
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }
    
    [Header("トランジション設定")]
    [SerializeField] private float transitionDuration = 0.8f;
    [SerializeField] private Color transitionColor = Color.black;
    
    [Header("斜めワイプ設定")]
    [SerializeField] private float wipeAngle = 25f;  // ワイプの角度（小さいほど横に近い）
    
    private Canvas transitionCanvas;
    private RectTransform wipePanel;
    private Image wipeImage;
    private RectTransform borderLine;  // ボーダーライン
    private Image borderImage;
    private bool isTransitioning = false;
    
    [Header("デバッグ用ボーダー")]
    [SerializeField] private bool showBorder = true;
    [SerializeField] private Color borderColor = Color.white;
    [SerializeField] private float borderWidth = 10f;
    
    // 画面サイズ
    private float screenWidth;
    private float screenHeight;
    private float diagonalLength;
    
    void Awake()
    {
        // シングルトン設定
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetupTransitionUI();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// トランジション用UIを作成
    /// </summary>
    private void SetupTransitionUI()
    {
        screenWidth = 1920f;
        screenHeight = 1080f;
        diagonalLength = Mathf.Sqrt(screenWidth * screenWidth + screenHeight * screenHeight);
        
        // Canvas作成
        GameObject canvasObj = new GameObject("TransitionCanvas");
        canvasObj.transform.SetParent(transform);
        transitionCanvas = canvasObj.AddComponent<Canvas>();
        transitionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        transitionCanvas.sortingOrder = 10000;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // ワイプパネル作成
        GameObject panelObj = new GameObject("WipePanel");
        panelObj.transform.SetParent(canvasObj.transform, false);
        
        wipePanel = panelObj.AddComponent<RectTransform>();
        wipeImage = panelObj.AddComponent<Image>();
        wipeImage.color = transitionColor;
        
        // パネルサイズ：幅は画面幅の10倍、高さは回転しても隙間ができないよう大きめ
        wipePanel.sizeDelta = new Vector2(screenWidth * 10f, screenHeight * 16f);
        
        // ピボットを左端に設定 → 右端がワイプの「先端」になる
        wipePanel.pivot = new Vector2(0f, 0.5f);
        wipePanel.anchorMin = new Vector2(0.5f, 0.5f);
        wipePanel.anchorMax = new Vector2(0.5f, 0.5f);
        
        // ボーダーライン作成（パネル右端に配置）
        if (showBorder)
        {
            GameObject borderObj = new GameObject("BorderLine");
            borderObj.transform.SetParent(panelObj.transform, false);
            
            borderLine = borderObj.AddComponent<RectTransform>();
            borderImage = borderObj.AddComponent<Image>();
            borderImage.color = borderColor;
            
            // パネルの右端に細いラインを配置
            borderLine.anchorMin = new Vector2(1f, 0f);  // 右端
            borderLine.anchorMax = new Vector2(1f, 1f);  // 右端、高さ全体
            borderLine.pivot = new Vector2(1f, 0.5f);
            borderLine.sizeDelta = new Vector2(borderWidth, 0);  // 幅はborderWidth、高さは親に合わせる
            borderLine.anchoredPosition = Vector2.zero;
        }
        
        panelObj.SetActive(false);
        
        Debug.Log($"[SceneTransitionManager] トランジションUIを作成 - パネル幅: {screenWidth}, ボーダー: {showBorder}");
    }
    
    /// <summary>
    /// デバッグ用：パネルを強制表示（Inspectorから呼べる）
    /// </summary>
    [ContextMenu("パネルを強制表示（テスト）")]
    public void DebugShowPanel()
    {
        if (wipePanel == null)
        {
            Debug.LogError("wipePanelがnullです。Awakeが実行されていない可能性があります。");
            return;
        }
        
        wipePanel.gameObject.SetActive(true);
        wipePanel.localRotation = Quaternion.Euler(0, 0, -wipeAngle);
        wipeImage.color = new Color(transitionColor.r, transitionColor.g, transitionColor.b, 1f);
        
        // ワイプ途中の位置に配置（斜めのエッジが画面中央あたりに見える）
        wipePanel.anchoredPosition = new Vector2(0, 0);
        
        Debug.Log($"[SceneTransitionManager] パネルを強制表示しました。位置: {wipePanel.anchoredPosition}");
    }
    
    /// <summary>
    /// デバッグ用：パネルを非表示
    /// </summary>
    [ContextMenu("パネルを非表示")]
    public void DebugHidePanel()
    {
        if (wipePanel != null)
        {
            wipePanel.gameObject.SetActive(false);
            Debug.Log("[SceneTransitionManager] パネルを非表示にしました");
        }
    }
    
    /// <summary>
    /// 通常のフェードでシーン遷移
    /// </summary>
    public void LoadSceneWithFade(string sceneName)
    {
        if (isTransitioning) return;
        StartCoroutine(FadeTransition(sceneName));
    }
    
    /// <summary>
    /// 斜めフェードでシーン遷移
    /// </summary>
    public void LoadSceneWithDiagonalFade(string sceneName)
    {
        if (isTransitioning) return;
        StartCoroutine(DiagonalFadeTransition(sceneName));
    }
    
    /// <summary>
    /// シーン名に応じて適切なトランジションを選択
    /// </summary>
    public void LoadSceneWithAutoTransition(string sceneName)
    {
        Debug.Log($"[SceneTransitionManager] LoadSceneWithAutoTransition 呼び出し: '{sceneName}'");
        
        if (isTransitioning)
        {
            Debug.LogWarning("[SceneTransitionManager] 既にトランジション中のためスキップ");
            return;
        }
        
        // turnTestSceneの場合は斜めワイプ
        if (sceneName == "turnTestScene")
        {
            Debug.Log("[SceneTransitionManager] → 斜めワイプを使用");
            LoadSceneWithDiagonalFade(sceneName);
        }
        else
        {
            Debug.Log("[SceneTransitionManager] → 通常フェードを使用");
            LoadSceneWithFade(sceneName);
        }
    }
    
    /// <summary>
    /// 通常フェードトランジション
    /// </summary>
    private IEnumerator FadeTransition(string sceneName)
    {
        isTransitioning = true;
        
        // フェード用パネルを作成（通常用）
        GameObject fadePanel = new GameObject("FadePanel");
        fadePanel.transform.SetParent(transitionCanvas.transform, false);
        
        RectTransform fadeRect = fadePanel.AddComponent<RectTransform>();
        fadeRect.anchorMin = Vector2.zero;
        fadeRect.anchorMax = Vector2.one;
        fadeRect.sizeDelta = Vector2.zero;
        
        Image fadeImage = fadePanel.AddComponent<Image>();
        fadeImage.color = new Color(transitionColor.r, transitionColor.g, transitionColor.b, 0);
        
        // フェードイン（画面を暗く）
        yield return fadeImage.DOFade(1f, transitionDuration / 2f).WaitForCompletion();
        
        // シーン読み込み
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        // フェードアウト（画面を明るく）
        yield return fadeImage.DOFade(0f, transitionDuration / 2f).WaitForCompletion();
        
        Destroy(fadePanel);
        isTransitioning = false;
    }
    
    /// <summary>
    /// 斜めワイプトランジション（右から左へ斜めにスライド）
    /// </summary>
    private IEnumerator DiagonalFadeTransition(string sceneName)
    {
        isTransitioning = true;
        
        Debug.Log($"[SceneTransitionManager] 斜めワイプ開始: {sceneName}");
        
        // パネルを表示・回転
        wipePanel.gameObject.SetActive(true);
        wipePanel.localRotation = Quaternion.Euler(0, 0, -wipeAngle);
        
        // 色を確実に不透明に設定（Inspectorで半透明にしていても上書き）
        wipeImage.color = new Color(transitionColor.r, transitionColor.g, transitionColor.b, 1f);
        
        // ピボットが左端(0, 0.5)なので：
        // anchoredPosition.x = パネル左端の位置
        // パネル右端 = anchoredPosition.x + panelWidth
        
        float halfScreen = screenWidth / 2f;  // 960
        float panelWidth = wipePanel.sizeDelta.x;  // 1920
        
        // 回転角度を考慮して、斜めの端が角まで届く距離を計算
        float angleRad = wipeAngle * Mathf.Deg2Rad;
        float extraDistance = screenHeight * Mathf.Tan(angleRad);
        
        // 開始位置：パネルが完全に画面右外（見えない）
        Vector2 startPos = new Vector2(halfScreen + panelWidth + extraDistance, 0);
        
        // 覆う位置：斜めのエッジが左上の角を超えるまで移動
        // extraDistanceを追加して、斜めでも完全に覆う
        Vector2 coverPos = new Vector2(-halfScreen - extraDistance - 200f, 0);
        
        // 終了位置：パネルが完全に画面左外（見えない）
        Vector2 endPos = new Vector2(-halfScreen - panelWidth - extraDistance, 0);
        
        // 開始位置に配置
        wipePanel.anchoredPosition = startPos;
        
        Debug.Log($"[SceneTransitionManager] 開始: {startPos.x}（右外）, 覆う: {coverPos.x}（全画面）, 終了: {endPos.x}（左外）");
        Debug.Log($"[SceneTransitionManager] パネル左端: {startPos.x}, パネル右端: {startPos.x + panelWidth}");
        
        // フェーズ1: 右から中央へ（画面を覆う）
        // エッジが斜めにスライドして入ってくる
        yield return wipePanel.DOAnchorPos(coverPos, transitionDuration * 0.5f)
            .SetEase(Ease.Linear)  // 一定速度でスライド
            .WaitForCompletion();
        
        // シーン読み込み
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        // 少し待機
        yield return new WaitForSeconds(0.1f);
        
        // フェーズ2: 中央から左へ（画面を開く）
        // エッジが斜めにスライドして抜けていく
        yield return wipePanel.DOAnchorPos(endPos, transitionDuration * 0.5f)
            .SetEase(Ease.Linear)  // 一定速度でスライド
            .WaitForCompletion();
        
        // パネルを非表示
        wipePanel.gameObject.SetActive(false);
        
        isTransitioning = false;
        Debug.Log("[SceneTransitionManager] 斜めワイプ完了");
    }
    
    /// <summary>
    /// トランジション中かどうか
    /// </summary>
    public bool IsTransitioning()
    {
        return isTransitioning;
    }
}
