using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ボス戦中のイベントを管理するクラス
/// </summary>
public class BossEventTrigger : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private ConversationUI conversationUI;
    [SerializeField] private TurnManager turnManager;

    private Character bossCharacter;
    private List<GameManager.BattleMidEvent> events;

    void Start()
    {
        // GameManagerからボス戦情報を取得
        if (GameManager.Instance != null && GameManager.Instance.isBossBattle)
        {
            events = new List<GameManager.BattleMidEvent>(GameManager.Instance.battleMidEvents);

            // イベントをHP降順にソート（高い閾値から先にチェック）
            events.Sort((a, b) => b.hpThreshold.CompareTo(a.hpThreshold));

            Debug.Log($"[BossEventTrigger] ボス戦初期化: イベント{events.Count}個");
            foreach (var evt in events)
            {
                Debug.Log($"  - HP{evt.hpThreshold}%: {evt.dialogueCSV}");
            }
        }
        else
        {
            Debug.Log("[BossEventTrigger] 通常戦闘です");
        }
    }

    /// <summary>
    /// ボスキャラクターを設定
    /// </summary>
    public void Initialize(Character boss)
    {
        bossCharacter = boss;
        Debug.Log($"[BossEventTrigger] ボスキャラクター設定: {boss.charactername}");
    }

    /// <summary>
    /// HPをチェックしてイベントを発動
    /// </summary>
    public void CheckEvents()
    {
        if (!GameManager.Instance.isBossBattle || bossCharacter == null || events == null || events.Count == 0)
            return;

        float hpPercentage = (float)bossCharacter.hp / bossCharacter.maxHp * 100f;

        Debug.Log($"[BossEventTrigger] HP確認: {bossCharacter.hp}/{bossCharacter.maxHp} ({hpPercentage:F1}%)");

        // HP閾値に達したイベントをチェック
        for (int i = events.Count - 1; i >= 0; i--)
        {
            var evt = events[i];

            if (hpPercentage <= evt.hpThreshold)
            {
                TriggerEvent(evt);
                events.RemoveAt(i);  // 発動済みイベントを削除
                break;  // 1度に1つのイベントのみ発動
            }
        }
    }

    /// <summary>
    /// イベントを発動
    /// </summary>
    private void TriggerEvent(GameManager.BattleMidEvent evt)
    {
        Debug.Log($"[BossEventTrigger] ★イベント発動★ {evt.dialogueCSV} (HP{evt.hpThreshold}%以下)");

        // 戦闘を一時停止
        if (evt.pauseBattle && turnManager != null)
        {
            turnManager.PauseBattle();
        }

        // 会話イベント開始
        if (conversationUI != null)
        {
            conversationUI.StartDialogueWithCSV(evt.dialogueCSV);

            // 会話終了後に戦闘再開（一度だけ実行されるリスナー）
            if (evt.pauseBattle)
            {
                UnityEngine.Events.UnityAction resumeAction = null;
                resumeAction = () =>
                {
                    Debug.Log("[BossEventTrigger] 会話終了、戦闘を再開します");
                    if (turnManager != null)
                    {
                        turnManager.ResumeBattle();
                    }
                    // 一度実行したらリスナーを削除
                    conversationUI.onDialogueEnd.RemoveListener(resumeAction);
                };
                conversationUI.onDialogueEnd.AddListener(resumeAction);
            }
        }
        else
        {
            Debug.LogWarning("[BossEventTrigger] ConversationUIが見つかりません");
        }
    }
}
