using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// クイックタイムイベント戦闘UI
/// </summary>
public class QuickTimeCombatUI : MonoBehaviour
{
    [Header("UI設定")]
    [SerializeField] private GameObject quickTimePanel;
    [SerializeField] private Slider timingBar;
    [SerializeField] private Text instructionText;
    [SerializeField] private KeyCode attackKey = KeyCode.Space;
    
    [Header("成功ゾーン設定")]
    [SerializeField] private RectTransform successZone; // SuccessZoneのImage
    [SerializeField, Range(0f, 1f)] private float successZoneCenter = 0.5f; // 中心位置（0～1）
    [SerializeField, Range(0f, 0.5f)] private float successZoneHalfWidth = 0.15f; // 半分の幅（timingWindowと同期）
    
    [Header("タイミング設定")]
    [SerializeField] private float timingSpeed = 2.0f;
    [SerializeField] private float maxTime = 3.0f; // 最大待機時間
    
    private bool isActive = false;
    private float timer = 0f;
    private System.Action<bool> onCombatResult; // コールバック
    
    private void Start()
    {
        // 起動時にSuccessZoneの位置を同期
        UpdateSuccessZonePosition();
        
        // QuickTimePanelを最初は非表示にしておく
        if (quickTimePanel != null)
        {
            quickTimePanel.SetActive(false);
        }
    }
    
    private void Update()
    {
        if (!isActive) return;
        
        // タイマー更新
        timer += Time.deltaTime;
        
        // タイミングバーの更新（0.0～1.0の間で往復）
        float pingPongValue = Mathf.PingPong(timer * timingSpeed, 1f);
        timingBar.value = pingPongValue;
        
        // キー入力チェック
        if (Input.GetKeyDown(attackKey))
        {
            CheckTiming(pingPongValue);
        }
        
        // 時間切れ
        if (timer >= maxTime)
        {
            EndCombat(false);
        }
    }
    
    /// <summary>
    /// クイックタイム戦闘を開始
    /// </summary>
    public void StartQuickTimeCombat(System.Action<bool> resultCallback)
    {
        onCombatResult = resultCallback;
        isActive = true;
        timer = 0f;
        
        if (quickTimePanel != null)
        {
            quickTimePanel.SetActive(true);
        }
        
        if (timingBar != null)
        {
            timingBar.value = 0f;
        }
        
        if (instructionText != null)
        {
            instructionText.text = $"{attackKey}キーで攻撃！";
        }
        
        // SuccessZoneを表示
        if (successZone != null)
        {
            successZone.gameObject.SetActive(true);
        }
        
        Debug.Log("クイックタイム戦闘開始");
    }
    
    /// <summary>
    /// タイミング判定
    /// </summary>
    private void CheckTiming(float currentValue)
    {
        // 成功範囲の計算（UIのSuccessZoneと同じ範囲）
        float minSuccess = successZoneCenter - successZoneHalfWidth;
        float maxSuccess = successZoneCenter + successZoneHalfWidth;
        
        if (currentValue >= minSuccess && currentValue <= maxSuccess)
        {
            // 成功
            Debug.Log($"タイミング成功！範囲: {minSuccess:F2}～{maxSuccess:F2}, 押した位置: {currentValue:F2}");
            EndCombat(true);
        }
        else
        {
            // 失敗
            Debug.Log($"タイミング失敗。範囲: {minSuccess:F2}～{maxSuccess:F2}, 押した位置: {currentValue:F2}");
            EndCombat(false);
        }
    }
    
    /// <summary>
    /// 戦闘終了
    /// </summary>
    private void EndCombat(bool success)
    {
        isActive = false;
        
        if (quickTimePanel != null)
        {
            quickTimePanel.SetActive(false);
        }
        
        // SuccessZoneを非表示
        if (successZone != null)
        {
            successZone.gameObject.SetActive(false);
        }
        
        onCombatResult?.Invoke(success);
        onCombatResult = null;
        
        Debug.Log($"クイックタイム戦闘終了: {(success ? "成功" : "失敗")}");
    }
    
    /// <summary>
    /// タイミング速度を設定
    /// </summary>
    public void SetTimingSpeed(float speed)
    {
        timingSpeed = speed;
    }
    
    /// <summary>
    /// 最大時間を設定
    /// </summary>
    public void SetMaxTime(float time)
    {
        maxTime = time;
    }
    
    /// <summary>
    /// 成功ゾーンの範囲を設定
    /// </summary>
    public void SetSuccessZone(float center, float halfWidth)
    {
        successZoneCenter = Mathf.Clamp01(center);
        successZoneHalfWidth = Mathf.Clamp(halfWidth, 0f, 0.5f);
        UpdateSuccessZonePosition();
    }
    
    /// <summary>
    /// SuccessZoneの位置とサイズをパラメータに基づいて更新
    /// </summary>
    private void UpdateSuccessZonePosition()
    {
        if (successZone == null || timingBar == null) return;
        
        // Sliderの幅を取得
        RectTransform sliderRect = timingBar.GetComponent<RectTransform>();
        if (sliderRect == null) return;
        
        float sliderWidth = sliderRect.rect.width;
        
        // 幅が0の場合はスキップ（まだ初期化されていない）
        if (sliderWidth <= 0) return;

        // SuccessZoneのサイズを計算
        float zoneWidth = sliderWidth * (successZoneHalfWidth * 2);
        successZone.sizeDelta = new Vector2(zoneWidth, successZone.sizeDelta.y);

        // SuccessZoneの位置を計算（中心位置に基づく）
        float offset = (successZoneCenter - 0.5f) * sliderWidth;
        successZone.anchoredPosition = new Vector2(offset, successZone.anchoredPosition.y);
        
        Debug.Log($"QuickTimeCombat SuccessZone更新: Width={zoneWidth}, Offset={offset}");
    }

    // Inspectorで値を変更した時に呼ばれる
    private void OnValidate()
    {
        // エディタでの変更時のみ実行
        if (Application.isPlaying) return;
        UpdateSuccessZonePosition();
    }
}

