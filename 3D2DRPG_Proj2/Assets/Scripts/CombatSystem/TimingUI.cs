using UnityEngine;
using UnityEngine.UI;

public class TimingUI : MonoBehaviour
{
    [SerializeField] private Slider timingBar;
    [SerializeField] private Canvas canvas; // Canvas全体の参照
    [SerializeField] private float speed = 2.0f;
    [SerializeField] private KeyCode attackKey = KeyCode.Space;

    [Header("成功ゾーン設定")]
    [SerializeField] private RectTransform successZone; // SuccessZoneのImage
    [SerializeField, Range(0f, 1f)] private float successZoneCenter = 0.5f; // 中心位置（0～1）
    [SerializeField, Range(0f, 0.5f)] private float successZoneHalfWidth = 0.1f; // 半分の幅
    private float value = 0f;
    public bool isActive = false;

    private void Start()
    {
        // 起動時にSuccessZoneの位置を同期
        UpdateSuccessZonePosition();
        
        // Canvas を最初は非表示にしておく
        if (canvas != null)
        {
            canvas.gameObject.SetActive(false);
            Debug.Log("TimingUI: Canvas を初期状態で非アクティブにしました");
        }
    }

    public void Show(float _timingtime, float _timingWindowEnd)
    {
        Debug.Log("=== TimingUI.Show() 呼ばれました ===");
        
        // Canvas全体を表示
        if (canvas != null)
        {
            canvas.gameObject.SetActive(true);
            Debug.Log($"Canvas をアクティブにしました: {canvas.gameObject.activeSelf}");
        }
        else
        {
            Debug.LogWarning("TimingUI: canvas が null です。Inspectorで設定してください。");
        }
        
        // nullチェック
        if (timingBar == null)
        {
            Debug.LogError("TimingUI: timingBar が null です！Inspectorで設定してください。");
            return;
        }
        
        timingBar.gameObject.SetActive(true);
        value = 0f;
        isActive = true;
        
        Debug.Log($"TimingBar Active: {timingBar.gameObject.activeSelf}");
        
        // SuccessZoneを表示
        if (successZone != null)
        {
            successZone.gameObject.SetActive(true);
            Debug.Log($"SuccessZone Active: {successZone.gameObject.activeSelf}");
        }
        else
        {
            Debug.LogWarning("TimingUI: successZone が null です。Inspectorで設定するか、なくても動作します。");
        }
    }

    public void Hide()
    {
        Debug.Log("=== TimingUI.Hide() 呼ばれました ===");
        
        isActive = false;
        timingBar.gameObject.SetActive(false);
        
        // SuccessZoneを非表示
        if (successZone != null)
            successZone.gameObject.SetActive(false);
        
        // Canvas全体を非表示
        if (canvas != null)
        {
            canvas.gameObject.SetActive(false);
            Debug.Log("Canvas を非アクティブにしました");
        }
    }

    public bool IsTimingSuccess()
    {
        if (!isActive) return false;

        // バーを動かす
        value += Time.deltaTime * speed;
        timingBar.value = Mathf.PingPong(value, 1f);

        // 入力範囲でキーを押したら判定
        if (Input.GetKeyDown(attackKey) || Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log($"Timing Key Pressed at: {timingBar.value}");
            
            // 成功範囲の計算（UIのSuccessZoneと同じ範囲）
            float minSuccess = successZoneCenter - successZoneHalfWidth;
            float maxSuccess = successZoneCenter + successZoneHalfWidth;
            
            if (timingBar.value >= minSuccess && timingBar.value <= maxSuccess)
            {
                isActive = true; // 成功
                Debug.Log($"Timing Success! Range: {minSuccess:F2} ~ {maxSuccess:F2}, Pressed: {timingBar.value:F2}");
            }
            else
            {
                isActive = false; // 失敗
                Debug.Log($"Timing Failed! Range: {minSuccess:F2} ~ {maxSuccess:F2}, Pressed: {timingBar.value:F2}");
            }
            return true;// 押された
        }
        return false;// 押されていない
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
        
        Debug.Log($"SuccessZone更新: Width={zoneWidth}, Offset={offset}");
    }

    // Inspectorで値を変更した時に呼ばれる
    private void OnValidate()
    {
        // エディタでの変更時のみ実行
        if (Application.isPlaying) return;
        UpdateSuccessZonePosition();
    }
}
