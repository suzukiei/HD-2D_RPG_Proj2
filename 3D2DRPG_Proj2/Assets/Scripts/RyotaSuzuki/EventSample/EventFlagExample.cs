using UnityEngine;

/// <summary>
/// EventFlagManagerの使用例
/// Dictionaryの使い方サンプル
/// </summary>
public class EventFlagExample : MonoBehaviour
{
    void Start()
    {
        DictionaryExample();
    }
    
    /// <summary>
    /// Dictionaryの基本的な使い方
    /// </summary>
    void DictionaryExample()
    {
        Debug.Log("=== Dictionary使用例 ===");
        
        // 1. フラグを設定
        EventFlagManager.Instance.SetFlag("tutorial_completed", true);
        EventFlagManager.Instance.SetFlag("boss_defeated", false);
        EventFlagManager.Instance.SetFlag("town_visited", true);
        
        // 2. フラグを取得
        bool isTutorialDone = EventFlagManager.Instance.GetFlag("tutorial_completed");
        Debug.Log($"チュートリアル完了: {isTutorialDone}");  // true
        
        // 3. フラグで条件分岐
        if (EventFlagManager.Instance.GetFlag("boss_defeated"))
        {
            Debug.Log("ボスを倒した！次のエリアへ");
        }
        else
        {
            Debug.Log("まだボスを倒していない");
        }
        
        // 4. 存在しないフラグはfalse
        bool unknownFlag = EventFlagManager.Instance.GetFlag("unknown_flag");
        Debug.Log($"存在しないフラグ: {unknownFlag}");  // false
        
        // 5. 複数フラグのチェック
        if (EventFlagManager.Instance.GetFlag("tutorial_completed") &&
            EventFlagManager.Instance.GetFlag("town_visited"))
        {
            Debug.Log("チュートリアル完了 かつ 街訪問済み");
        }
        
        Debug.Log("==================");
    }
    
    /// <summary>
    /// ゲーム進行の例
    /// </summary>
    void GameProgressExample()
    {
        // チャプター1開始
        EventFlagManager.Instance.SetFlag("chapter_1_started", true);
        
        // ボス戦開始
        EventFlagManager.Instance.SetFlag("boss_battle_started", true);
        
        // ボス撃破
        EventFlagManager.Instance.SetFlag("boss_defeated", true);
        EventFlagManager.Instance.SetFlag("boss_battle_started", false);
        
        // チャプター1クリア
        EventFlagManager.Instance.SetFlag("chapter_1_completed", true);
        EventFlagManager.Instance.SetFlag("chapter_2_started", true);
    }
    
    /// <summary>
    /// クエストシステムの例
    /// </summary>
    void QuestSystemExample()
    {
        // クエスト受注
        string questId = "quest_001";
        EventFlagManager.Instance.SetFlag($"{questId}_active", true);
        
        // 進行度をフラグで管理
        EventFlagManager.Instance.SetFlag($"{questId}_slime_defeated_1", true);
        EventFlagManager.Instance.SetFlag($"{questId}_slime_defeated_2", true);
        EventFlagManager.Instance.SetFlag($"{questId}_slime_defeated_3", true);
        
        // 全部倒したかチェック
        bool quest1 = EventFlagManager.Instance.GetFlag($"{questId}_slime_defeated_1");
        bool quest2 = EventFlagManager.Instance.GetFlag($"{questId}_slime_defeated_2");
        bool quest3 = EventFlagManager.Instance.GetFlag($"{questId}_slime_defeated_3");
        
        if (quest1 && quest2 && quest3)
        {
            // クエスト完了
            EventFlagManager.Instance.SetFlag($"{questId}_active", false);
            EventFlagManager.Instance.SetFlag($"{questId}_completed", true);
            Debug.Log("クエスト完了！");
        }
    }
    
    /// <summary>
    /// イベント分岐の例
    /// </summary>
    void EventBranchExample()
    {
        // プレイヤーの選択によって分岐
        bool helpedVillager = EventFlagManager.Instance.GetFlag("helped_villager");
        
        if (helpedVillager)
        {
            Debug.Log("村人を助けたルート");
            // 村人が仲間になるイベント
            EventFlagManager.Instance.SetFlag("villager_joined_party", true);
        }
        else
        {
            Debug.Log("村人を助けなかったルート");
            // 村人が敵対するイベント
            EventFlagManager.Instance.SetFlag("villager_hostile", true);
        }
    }
}
