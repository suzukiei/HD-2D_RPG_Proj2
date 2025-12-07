using UnityEngine;

/// <summary>
/// Timeline用の会話トリガー
/// SignalからCSVファイルを指定して会話を開始する
/// </summary>
public class TimelineConversationTrigger : MonoBehaviour
{
    [Header("会話UI")]
    [SerializeField] 
    private ConversationUI conversationUI;

    [Header("イベント001")]
    [SerializeField, Tooltip("イベント001のCSVファイル名")]
    private string event001CSV = "scenario01.csv";

    void Start()
    {
        // ConversationUIを自動検索
        if (conversationUI == null)
        {
            conversationUI = FindObjectOfType<ConversationUI>();
        }
    }

    /// <summary>
    /// イベント001の会話を開始（Timeline Signalから呼び出し）
    /// </summary>
    public void StartEvent001Conversation()
    {
        StartConversation(event001CSV);
    }

    /// <summary>
    /// 指定したCSVで会話を開始
    /// </summary>
    public void StartConversation(string csvFileName)
    {
        if (conversationUI == null)
        {
            Debug.LogError("ConversationUIが見つかりません！");
            return;
        }

        conversationUI.csvFileName = csvFileName;
        conversationUI.ReloadCSV();
        conversationUI.StartDialogue();
        
        Debug.Log($"Timeline会話開始: {csvFileName}");
    }
}

