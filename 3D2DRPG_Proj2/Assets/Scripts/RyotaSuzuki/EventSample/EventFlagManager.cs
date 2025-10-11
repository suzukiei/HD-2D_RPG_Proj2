using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// イベントフラグを管理するシングルトンマネージャー
/// Dictionaryでシンプルに管理
/// </summary>
public class EventFlagManager : MonoBehaviour
{
    private static EventFlagManager instance;
    public static EventFlagManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("EventFlagManager");
                instance = go.AddComponent<EventFlagManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }
    
    [Header("デバッグ設定")]
    [SerializeField] private bool showDebugLog = true;
    
    // フラグを保存するDictionary
    private Dictionary<string, bool> flags = new Dictionary<string, bool>();
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// フラグを設定（true/false）
    /// </summary>
    public void SetFlag(string flagName, bool value)
    {
        if (flags.ContainsKey(flagName))
        {
            flags[flagName] = value;
        }
        else
        {
            flags.Add(flagName, value);
        }
        
        if (showDebugLog)
        {
            Debug.Log($"[EventFlag] {flagName} = {value}");
        }
    }
    
    /// <summary>
    /// フラグを取得（デフォルトはfalse）
    /// </summary>
    public bool GetFlag(string flagName)
    {
        if (flags.ContainsKey(flagName))
        {
            return flags[flagName];
        }
        return false; // 存在しないフラグはfalse
    }
    
    /// <summary>
    /// フラグが存在するかチェック
    /// </summary>
    public bool HasFlag(string flagName)
    {
        return flags.ContainsKey(flagName);
    }
    
    /// <summary>
    /// フラグを削除
    /// </summary>
    public void RemoveFlag(string flagName)
    {
        if (flags.ContainsKey(flagName))
        {
            flags.Remove(flagName);
            if (showDebugLog)
            {
                Debug.Log($"[EventFlag] {flagName} を削除しました");
            }
        }
    }
    
    /// <summary>
    /// すべてのフラグをクリア
    /// </summary>
    public void ClearAllFlags()
    {
        flags.Clear();
        if (showDebugLog)
        {
            Debug.Log("[EventFlag] すべてのフラグをクリアしました");
        }
    }
    
    /// <summary>
    /// 全フラグを表示（デバッグ用）
    /// </summary>
    [ContextMenu("全フラグを表示")]
    public void ShowAllFlags()
    {
        Debug.Log("=== イベントフラグ一覧 ===");
        if (flags.Count == 0)
        {
            Debug.Log("フラグはありません");
        }
        else
        {
            foreach (var flag in flags)
            {
                Debug.Log($"{flag.Key} = {flag.Value}");
            }
        }
        Debug.Log("=====================");
    }
    
    /// <summary>
    /// セーブデータ用：全フラグをJSON形式で取得
    /// </summary>
    public string GetFlagsAsJson()
    {
        FlagsSaveData saveData = new FlagsSaveData();
        saveData.flagKeys = new List<string>(flags.Keys);
        saveData.flagValues = new List<bool>(flags.Values);
        return JsonUtility.ToJson(saveData);
    }
    
    /// <summary>
    /// セーブデータ用：JSONから全フラグを復元
    /// </summary>
    public void LoadFlagsFromJson(string json)
    {
        FlagsSaveData saveData = JsonUtility.FromJson<FlagsSaveData>(json);
        flags.Clear();
        
        for (int i = 0; i < saveData.flagKeys.Count; i++)
        {
            flags.Add(saveData.flagKeys[i], saveData.flagValues[i]);
        }
        
        if (showDebugLog)
        {
            Debug.Log($"[EventFlag] {flags.Count}個のフラグを読み込みました");
        }
    }
}

/// <summary>
/// セーブ用のデータ構造
/// </summary>
[System.Serializable]
public class FlagsSaveData
{
    public List<string> flagKeys;
    public List<bool> flagValues;
}

