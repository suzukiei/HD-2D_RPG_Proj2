using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// SkillSelectionUIのテスト用クラス
/// Zキーで技選択UIを表示してテストする
/// </summary>
public class SkillSelectionUITest : MonoBehaviour
{
    [Header("テスト設定")]
    [SerializeField] private SkillSelectionUI skillSelectionUI;
    [SerializeField] private DummySkillProvider dummySkillProvider;
    
    [Header("テスト用ダミーデータ")]
    [SerializeField] private List<SkillData> testSkills = new List<SkillData>();
    
    [Header("デバッグ情報")]
    [SerializeField] private bool showDebugLog = true;
    
    void Start()
    {
        // ダミースキルが設定されていない場合、コード内でダミーを作成
        if (testSkills.Count == 0)
        {
            CreateRuntimeDummySkills();
        }
        
        if (showDebugLog)
        {
            Debug.Log("SkillSelectionUITest: 準備完了！Zキーで技選択UIをテストできます。");
            Debug.Log($"テスト用スキル数: {testSkills.Count}");
        }
    }
    
    void Update()
    {
        // Zキーでテスト開始
        if (Input.GetKeyDown(KeyCode.Z))
        {
            StartSkillSelectionTest();
        }
        
        // デバッグ用：Xキーでスキル情報表示
        if (Input.GetKeyDown(KeyCode.X))
        {
            ShowSkillInfo();
        }
    }
    
    /// <summary>
    /// 技選択UIのテストを開始
    /// </summary>
    public void StartSkillSelectionTest()
    {
        if (skillSelectionUI == null)
        {
            Debug.LogError("SkillSelectionUI が設定されていません！");
            return;
        }
        
        if (testSkills.Count == 0)
        {
            Debug.LogError("テスト用スキルが設定されていません！");
            return;
        }
        
        if (showDebugLog)
        {
            Debug.Log("=== 技選択UIテスト開始 ===");
        }
        
        // UnityEventを作成してコールバックを設定
        UnityEvent<int> callback = new UnityEvent<int>();
        callback.AddListener(OnSkillSelected);
        
        // 技選択UIを表示
        skillSelectionUI.ShowSkillSelection(testSkills, callback);
    }
    
    /// <summary>
    /// 技が選択された時のコールバック
    /// </summary>
    /// <param name="skillIndex">選択されたスキルのインデックス（-1でキャンセル）</param>
    public void OnSkillSelected(int skillIndex)
    {
        if (skillIndex == -1)
        {
            Debug.Log("技選択がキャンセルされました");
        }
        else if (skillIndex >= 0 && skillIndex < testSkills.Count)
        {
            SkillData selectedSkill = testSkills[skillIndex];
            Debug.Log($"選択された技: {selectedSkill.name} (MP: {selectedSkill.mpCost}, 威力: {selectedSkill.power})");
            Debug.Log($"対象: {selectedSkill.targetType}");
        }
        else
        {
            Debug.LogError($"無効なスキルインデックス: {skillIndex}");
        }
        
        if (showDebugLog)
        {
            Debug.Log("=== 技選択UIテスト終了 ===");
        }
    }
    
    /// <summary>
    /// ランタイムでダミースキルを作成（ScriptableObjectが無い場合の代替）
    /// </summary>
    private void CreateRuntimeDummySkills()
    {
        var dummyData = new[]
        {
            new { name = "ファイアボール", mpCost = 10, power = 25, targetType = "敵単体" },
            new { name = "アイスランス", mpCost = 8, power = 20, targetType = "敵単体" },
            new { name = "ヒール", mpCost = 15, power = 30, targetType = "味方単体" },
            new { name = "ライトニング", mpCost = 20, power = 35, targetType = "敵単体" }
        };
        
        foreach (var data in dummyData)
        {
            SkillData skill = ScriptableObject.CreateInstance<SkillData>();
            skill.name = data.name;
            skill.mpCost = data.mpCost;
            skill.power = data.power;
            skill.targetType = data.targetType;
            testSkills.Add(skill);
        }
        
        if (showDebugLog)
        {
            Debug.Log("ランタイムダミースキルを作成しました");
        }
    }
    
    /// <summary>
    /// スキル情報をデバッグ表示
    /// </summary>
    private void ShowSkillInfo()
    {
        Debug.Log("=== テスト用スキル一覧 ===");
        for (int i = 0; i < testSkills.Count; i++)
        {
            var skill = testSkills[i];
            Debug.Log($"{i}: {skill.name} (MP:{skill.mpCost}, 威力:{skill.power}, 対象:{skill.targetType})");
        }
    }
    
    /// <summary>
    /// DummySkillProviderからスキルを取得してテストリストに設定
    /// </summary>
    [ContextMenu("DummySkillProviderからスキルを取得")]
    public void LoadSkillsFromProvider()
    {
        if (dummySkillProvider != null)
        {
            var skills = dummySkillProvider.GetRandomSkills(4);
            testSkills.Clear();
            testSkills.AddRange(skills);
            Debug.Log($"DummySkillProviderから {skills.Length} 個のスキルを読み込みました");
        }
        else
        {
            Debug.LogWarning("DummySkillProvider が設定されていません");
        }
    }
} 