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
    
    [Header("タイミング設定")]
    [SerializeField] private float timingWindow = 0.3f; // 成功判定の範囲（0.0～1.0の間）
    [SerializeField] private float timingSpeed = 2.0f;
    [SerializeField] private float maxTime = 3.0f; // 最大待機時間
    
    private bool isActive = false;
    private float timer = 0f;
    private float targetValue = 0.5f; // 目標値（中央）
    private System.Action<bool> onCombatResult; // コールバック
    
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
        
        Debug.Log("クイックタイム戦闘開始");
    }
    
    /// <summary>
    /// タイミング判定
    /// </summary>
    private void CheckTiming(float currentValue)
    {
        float diff = Mathf.Abs(currentValue - targetValue);
        
        if (diff <= timingWindow)
        {
            // 成功
            Debug.Log($"タイミング成功！差: {diff:F3}");
            EndCombat(true);
        }
        else
        {
            // 失敗
            Debug.Log($"タイミング失敗。差: {diff:F3}");
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
        
        onCombatResult?.Invoke(success);
        onCombatResult = null;
        
        Debug.Log($"クイックタイム戦闘終了: {(success ? "成功" : "失敗")}");
    }
    
    /// <summary>
    /// タイミングウィンドウを設定
    /// </summary>
    public void SetTimingWindow(float window)
    {
        timingWindow = Mathf.Clamp01(window);
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
}

