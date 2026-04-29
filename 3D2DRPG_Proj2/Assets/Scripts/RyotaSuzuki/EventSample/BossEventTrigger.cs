using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// ボス戦中のイベントを管理するクラス
/// </summary>
public class BossEventTrigger : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private ConversationUI conversationUI;
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private EnemyManager enemyManager;

    private List<Character> allEnemies = new List<Character>();
    private List<GameManager.BattleMidEvent> events;
    private HashSet<int> triggeredThresholds = new HashSet<int>(); // 発動済み閾値を記録

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
            
            // 戦闘開始時にHP100%のイベントをチェック
            CheckEvents();
        }
        else
        {
            Debug.Log("[BossEventTrigger] 通常戦闘です");
        }
    }

    /// <summary>
    /// 敵キャラクターリストを設定
    /// </summary>
    public void Initialize(Character boss)
    {
        // 互換性のため残す（1体のみの場合）
        Debug.Log($"[BossEventTrigger] ボスキャラクター設定: {boss.charactername}");
    }

    /// <summary>
    /// HPをチェックしてイベントを発動
    /// </summary>
    public void CheckEvents()
    {
        if (!GameManager.Instance.isBossBattle || events == null || events.Count == 0)
            return;
        
        // HP 0%のイベントは除外（全滅時に発動させる）
        var nonZeroEvents = events.Where(e => e.hpThreshold > 0).ToList();

        // EnemyManagerから全敵を取得
        if (enemyManager == null)
        {
            enemyManager = FindObjectOfType<EnemyManager>();
        }

        if (enemyManager == null)
        {
            Debug.LogWarning("[BossEventTrigger] EnemyManagerが見つかりません");
            return;
        }

        // 全敵のGameObjectを取得
        var enemyObjects = enemyManager.GetEnemyData();
        if (enemyObjects == null || enemyObjects.Count == 0)
        {
            return;
        }

        // 全敵のCharacterコンポーネントを取得
        allEnemies.Clear();
        foreach (var enemyObj in enemyObjects)
        {
            if (enemyObj != null)
            {
                var character = enemyObj.GetComponent<Character>();
                if (character != null)
                {
                    allEnemies.Add(character);
                }
            }
        }

        if (allEnemies.Count == 0)
        {
            return;
        }

        // 各敵のHP%を確認
        foreach (var enemy in allEnemies)
        {
            if (enemy == null || enemy.maxHp <= 0) continue;

            float hpPercentage = (float)enemy.hp / enemy.maxHp * 100f;

            // HP閾値に達したイベントをチェック（HP 0%を除く）
            for (int i = events.Count - 1; i >= 0; i--)
            {
                var evt = events[i];

                // HP 0%のイベントはスキップ（全滅時に発動させる）
                if (evt.hpThreshold == 0)
                {
                    continue;
                }

                // 既に発動済みの閾値はスキップ
                if (triggeredThresholds.Contains(evt.hpThreshold))
                {
                    continue;
                }

                // HP閾値に達しているかチェック（以下の場合に発動）
                if (hpPercentage <= evt.hpThreshold)
                {
                    Debug.Log($"[BossEventTrigger] {enemy.charactername} HP確認: {enemy.hp}/{enemy.maxHp} ({hpPercentage:F1}%) - 閾値{evt.hpThreshold}%に達しました");
                    
                    TriggerEvent(evt);
                    triggeredThresholds.Add(evt.hpThreshold); // 発動済みとしてマーク
                    events.RemoveAt(i);  // 発動済みイベントを削除
                    return;  // 1度に1つのイベントのみ発動
                }
            }
        }
    }

    /// <summary>
    /// 敵全滅時のイベントをチェック（HP 0%イベント）
    /// </summary>
    /// <param name="onEventEnd">イベント終了後のコールバック</param>
    /// <returns>0%イベントが発動したかどうか</returns>
    public bool CheckEnemyDefeatedEvent(System.Action onEventEnd = null)
    {
        if (!GameManager.Instance.isBossBattle || events == null || events.Count == 0)
            return false;

        // HP 0%のイベントを検索
        for (int i = events.Count - 1; i >= 0; i--)
        {
            var evt = events[i];

            if (evt.hpThreshold == 0)
            {
                // 既に発動済みかチェック
                if (triggeredThresholds.Contains(0))
                {
                    continue;
                }

                Debug.Log($"[BossEventTrigger] 敵全滅イベント発動: {evt.dialogueCSV}");
                
                TriggerEvent(evt, onEventEnd);
                triggeredThresholds.Add(0);
                events.RemoveAt(i);
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// イベントを発動
    /// </summary>
    /// <param name="evt">発動するイベント</param>
    /// <param name="onEventEnd">イベント終了後に実行する追加コールバック</param>
    private void TriggerEvent(GameManager.BattleMidEvent evt, System.Action onEventEnd = null)
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
                    // 追加のコールバックを実行
                    if (onEventEnd != null)
                    {
                        onEventEnd.Invoke();
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
