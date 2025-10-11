using UnityEngine;

/// <summary>
/// シンプルなイベントトリガー
/// 既存のConversationUIと連携して会話イベントを発生させる
/// </summary>
[RequireComponent(typeof(Collider))]
public class SimpleEventTrigger : MonoBehaviour
{
    [Header("イベント設定")]
    [SerializeField,Header("任意のイベントID")] private string eventId = "";
    [SerializeField,Header("任意のイベント名")] private string eventName = "";
    
    [Header("発動条件")]
    [SerializeField, Header("発動条件にタグを使用する")] private bool requirePlayerTag = true;
    [SerializeField, Header("タグ名")] private string playerTag = "Player";
    [SerializeField, Header("指定のフラグが全てtrueなら発動")] private string[] requiredFlags; // このフラグがtrueなら発動
    [SerializeField, Header("指定のフラグがtrueなら発動しない")] private string[] prohibitedFlags; // このフラグがtrueなら発動しない

    [Header("会話設定")]
    [SerializeField] private ConversationUI conversationUI;
    [SerializeField,Header("発動するイベントcsv")] private string csvFileName = "scenario01.csv";
    
    [Header("発動後の処理")]
    [SerializeField, Header("発動後、以下の処理を行うか")] private bool setFlagAfterEvent = true;
    [SerializeField, Header("処理を行うイベントID")] private string flagToSet = "";
    [SerializeField, Header("一度のみのフラグ立てにする？")] private bool oneTimeOnly = true; // 一度だけ発動
    [SerializeField, Header("イベント後フラグを削除する？")] private bool destroyAfterTrigger = false; // 発動後に削除
    
    [Header("発動方法")]
    [SerializeField] private TriggerType triggerType = TriggerType.AutoOnEnter;
    
    [Header("デバッグ")]
    [SerializeField] private bool showDebugLog = true;
    [SerializeField] private bool showGizmo = true;
    [SerializeField] private Color gizmoColor = Color.yellow;
    
    private bool hasTriggered = false;
    private bool playerInRange = false;
    
    public enum TriggerType
    {
        AutoOnEnter,    // 入った瞬間に自動発動
        ButtonPress     // 範囲内でボタン押したら発動
    }
    
    void Start()
    {
        // Colliderの設定確認
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        
        // ConversationUIを自動検索
        if (conversationUI == null)
        {
            conversationUI = FindObjectOfType<ConversationUI>();
            if (conversationUI == null && showDebugLog)
            {
                Debug.LogWarning($"[{eventName}] ConversationUIが見つかりません！");
            }
        }
    }
    
    void Update()
    {
        // ボタン押下タイプの場合
        if (triggerType == TriggerType.ButtonPress && playerInRange && !hasTriggered)
        {
            // Eキーまたはスペースで発動
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
            {
                TriggerEvent();
            }
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // プレイヤータグチェック
        if (requirePlayerTag && !other.CompareTag(playerTag))
            return;
        
        playerInRange = true;
        
        // 自動発動タイプの場合
        if (triggerType == TriggerType.AutoOnEnter)
        {
            TriggerEvent();
        }
        else if (showDebugLog)
        {
            Debug.Log($"[{eventName}] プレイヤーが範囲内に入りました（Eキーで発動）");
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (requirePlayerTag && !other.CompareTag(playerTag))
            return;
        
        playerInRange = false;
        
        if (showDebugLog && triggerType == TriggerType.ButtonPress)
        {
            Debug.Log($"[{eventName}] プレイヤーが範囲外に出ました");
        }
    }
    
    /// <summary>
    /// イベントを発動
    /// </summary>
    void TriggerEvent()
    {
        // 一度だけ発動の場合、チェック
        if (oneTimeOnly && hasTriggered)
        {
            if (showDebugLog)
            {
                Debug.Log($"[{eventName}] 既に発動済みです");
            }
            return;
        }
        
        // フラグチェック：必須フラグ
        if (requiredFlags != null && requiredFlags.Length > 0)
        {
            foreach (string flag in requiredFlags)
            {
                if (!EventFlagManager.Instance.GetFlag(flag))
                {
                    if (showDebugLog)
                    {
                        Debug.Log($"[{eventName}] 必須フラグ「{flag}」がfalseのため発動しません");
                    }
                    return;
                }
            }
        }
        
        // フラグチェック：禁止フラグ
        if (prohibitedFlags != null && prohibitedFlags.Length > 0)
        {
            foreach (string flag in prohibitedFlags)
            {
                if (EventFlagManager.Instance.GetFlag(flag))
                {
                    if (showDebugLog)
                    {
                        Debug.Log($"[{eventName}] 禁止フラグ「{flag}」がtrueのため発動しません");
                    }
                    return;
                }
            }
        }
        
        // イベント実行
        if (conversationUI != null)
        {
            // CSVファイル名を設定
            conversationUI.csvFileName = csvFileName;
            conversationUI.ReloadCSV();
            conversationUI.StartDialogue();
            
            if (showDebugLog)
            {
                Debug.Log($"[{eventName}] イベント発動！CSV: {csvFileName}");
            }
        }
        else
        {
            Debug.LogError($"[{eventName}] ConversationUIがnullです！");
        }
        
        // フラグを設定
        if (setFlagAfterEvent && !string.IsNullOrEmpty(flagToSet))
        {
            EventFlagManager.Instance.SetFlag(flagToSet, true);
        }
        
        hasTriggered = true;
        
        // 発動後に削除
        if (destroyAfterTrigger)
        {
            Destroy(gameObject, 0.5f);
        }
    }
    
    /// <summary>
    /// 外部から強制発動
    /// </summary>
    [ContextMenu("イベントを強制発動")]
    public void ForceTrigger()
    {
        hasTriggered = false;
        TriggerEvent();
    }
    
    /// <summary>
    /// リセット（再発動可能にする）
    /// </summary>
    [ContextMenu("イベントをリセット")]
    public void ResetTrigger()
    {
        hasTriggered = false;
        if (showDebugLog)
        {
            Debug.Log($"[{eventName}] イベントをリセットしました");
        }
    }
    
    /// <summary>
    /// ギズモ表示
    /// </summary>
    void OnDrawGizmos()
    {
        if (!showGizmo) return;
        
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = gizmoColor;
            Gizmos.matrix = transform.localToWorldMatrix;
            
            if (col is BoxCollider)
            {
                BoxCollider box = col as BoxCollider;
                Gizmos.DrawWireCube(box.center, box.size);
            }
            else if (col is SphereCollider)
            {
                SphereCollider sphere = col as SphereCollider;
                Gizmos.DrawWireSphere(sphere.center, sphere.radius);
            }
        }
    }
}
