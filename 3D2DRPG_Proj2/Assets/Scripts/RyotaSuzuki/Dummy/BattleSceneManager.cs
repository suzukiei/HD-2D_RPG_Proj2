using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// バトルシーン専用のマネージャー
/// バトルの開始・終了・結果処理を管理
/// </summary>
public class BattleSceneManager : MonoBehaviour
{
    [Header("バトル設定")]
    [SerializeField] private bool autoStartBattle = true;
    [SerializeField] private float battleStartDelay = 1f;
    
    [Header("UnityEvents")]
    [SerializeField] private UnityEvent OnBattleSceneStart;
    [SerializeField] private UnityEvent OnBattleSceneEnd;
    
    [Header("デバッグ")]
    [SerializeField] private bool showDebugLog = true;
    
    private bool battleEnded = false;
    
    void Start()
    {
        if (showDebugLog)
        {
            Debug.Log("BattleSceneManager: バトルシーン開始");
        }
        
        // バトル開始イベントを発火
        OnBattleSceneStart?.Invoke();
        
        // 自動でバトル開始する場合
        if (autoStartBattle)
        {
            StartCoroutine(AutoStartBattle());
        }
    }
    
    void Update()
    {
        // テスト用：Escapeキーでバトル終了
        if (Input.GetKeyDown(KeyCode.Escape) && !battleEnded)
        {
            EndBattle(BattleResult.Escape);
        }
        
        // テスト用：Enterキーで勝利
        if (Input.GetKeyDown(KeyCode.Return) && !battleEnded)
        {
            EndBattle(BattleResult.Victory);
        }
        
        // テスト用：Backspaceキーで敗北
        if (Input.GetKeyDown(KeyCode.Backspace) && !battleEnded)
        {
            EndBattle(BattleResult.Defeat);
        }
    }
    
    /// <summary>
    /// 自動バトル開始
    /// </summary>
    private IEnumerator AutoStartBattle()
    {
        yield return new WaitForSeconds(battleStartDelay);
        
        if (showDebugLog)
        {
            Debug.Log("BattleSceneManager: バトル自動開始");
            Debug.Log("テスト操作: Enter=勝利, Backspace=敗北, Escape=逃走");
        }
    }
    
    /// <summary>
    /// バトル終了処理
    /// </summary>
    /// <param name="result">バトル結果</param>
    public void EndBattle(BattleResult result)
    {
        if (battleEnded)
        {
            if (showDebugLog)
            {
                Debug.Log("BattleSceneManager: 既にバトルは終了しています");
            }
            return;
        }
        
        battleEnded = true;
        
        if (showDebugLog)
        {
            Debug.Log($"BattleSceneManager: バトル終了 - 結果: {result}");
        }
        
        // バトル結果に応じた処理
        ProcessBattleResult(result);
        
        // バトル終了イベントを発火
        OnBattleSceneEnd?.Invoke();
        
        // GameManagerにバトル終了を通知
        StartCoroutine(NotifyBattleEnd());
    }
    
    /// <summary>
    /// バトル結果の処理
    /// </summary>
    private void ProcessBattleResult(BattleResult result)
    {
        switch (result)
        {
            case BattleResult.Victory:
                if (showDebugLog)
                {
                    Debug.Log("バトル勝利！経験値とアイテムを獲得");
                }
                // 経験値やアイテム獲得処理をここに追加
                break;
                
            case BattleResult.Defeat:
                if (showDebugLog)
                {
                    Debug.Log("バトル敗北...ゲームオーバー処理");
                }
                // ゲームオーバー処理をここに追加
                break;
                
            case BattleResult.Escape:
                if (showDebugLog)
                {
                    Debug.Log("バトルから逃走しました");
                }
                // 逃走処理をここに追加
                break;
        }
    }
    
    /// <summary>
    /// GameManagerにバトル終了を通知
    /// </summary>
    private IEnumerator NotifyBattleEnd()
    {
        // 少し待ってからフィールドに戻る
        yield return new WaitForSeconds(1f);
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EndBattle();
        }
        else
        {
            Debug.LogError("GameManagerが見つかりません！");
        }
    }
    
    /// <summary>
    /// 外部からバトル勝利を呼び出す
    /// </summary>
    public void TriggerVictory()
    {
        EndBattle(BattleResult.Victory);
    }
    
    /// <summary>
    /// 外部からバトル敗北を呼び出す
    /// </summary>
    public void TriggerDefeat()
    {
        EndBattle(BattleResult.Defeat);
    }
    
    /// <summary>
    /// 外部からバトル逃走を呼び出す
    /// </summary>
    public void TriggerEscape()
    {
        EndBattle(BattleResult.Escape);
    }
    
    /// <summary>
    /// バトルが終了しているかどうか
    /// </summary>
    public bool IsBattleEnded()
    {
        return battleEnded;
    }
}

/// <summary>
/// バトル結果を表す列挙型
/// </summary>
public enum BattleResult
{
    Victory,    // 勝利
    Defeat,     // 敗北
    Escape      // 逃走
} 