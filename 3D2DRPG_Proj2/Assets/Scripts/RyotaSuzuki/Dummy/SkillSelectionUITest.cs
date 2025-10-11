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
            Debug.Log("SkillSelectionUITest: 準備完了！");
            Debug.Log("Zキー: 通常の技選択UIテスト");
            Debug.Log("Cキー: キャラクター情報付き技選択UIテスト");
            Debug.Log("1/2/3キー: キャラクター切り替えテスト");
            Debug.Log("Xキー: スキル情報表示");
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
        
        // Cキーでキャラクター付きテスト開始
        if (Input.GetKeyDown(KeyCode.C))
        {
            StartSkillSelectionWithCharacterTest();
        }
        
        // デバッグ用：Xキーでスキル情報表示
        if (Input.GetKeyDown(KeyCode.X))
        {
            ShowSkillInfo();
        }
        
        // 1, 2, 3キーでキャラクター切り替えテスト
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TestCharacterSwitch(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            TestCharacterSwitch(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            TestCharacterSwitch(2);
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
    /// キャラクター情報付きで技選択UIのテストを開始
    /// </summary>
    public void StartSkillSelectionWithCharacterTest()
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
            Debug.Log("=== キャラクター情報付き技選択UIテスト開始 ===");
        }
        
        // ランダムなキャラクターを選択
        int randomCharacterIndex = Random.Range(0, 3);
        PlayerData testCharacterData = PlayerData.CreateDummyData(randomCharacterIndex);
        
        // UnityEventを作成してコールバックを設定
        UnityEvent<int> callback = new UnityEvent<int>();
        callback.AddListener(OnSkillSelectedWithCharacter);
        
        // キャラクター情報付きで技選択UIを表示
        skillSelectionUI.ShowSkillSelection(testSkills, testCharacterData, callback);
        
        if (showDebugLog)
        {
            Debug.Log($"テストキャラクター: {testCharacterData.playerName} (Lv.{testCharacterData.level})");
        }
    }
    
    /// <summary>
    /// キャラクター情報付きテストでの技選択コールバック
    /// </summary>
    /// <param name="skillIndex">選択されたスキルのインデックス</param>
    public void OnSkillSelectedWithCharacter(int skillIndex)
    {
        PlayerData currentCharacter = skillSelectionUI.GetCurrentCharacterData();
        
        if (skillIndex == -1)
        {
            Debug.Log($"{currentCharacter?.playerName ?? "Unknown"} の技選択がキャンセルされました");
        }
        else if (skillIndex >= 0 && skillIndex < testSkills.Count)
        {
            SkillData selectedSkill = testSkills[skillIndex];
            Debug.Log($"{currentCharacter?.playerName ?? "Unknown"} が {selectedSkill.name} を選択しました");
            Debug.Log($"キャラクター情報: HP {currentCharacter?.currentHP}/{currentCharacter?.maxHP}, MP {currentCharacter?.currentMP}/{currentCharacter?.maxMP}");
        }
        else
        {
            Debug.LogError($"無効なスキルインデックス: {skillIndex}");
        }
        
        if (showDebugLog)
        {
            Debug.Log("=== キャラクター情報付き技選択UIテスト終了 ===");
        }
    }
    
    /// <summary>
    /// キャラクター切り替えテスト
    /// </summary>
    /// <param name="characterIndex">キャラクターインデックス（0-2）</param>
    public void TestCharacterSwitch(int characterIndex)
    {
        if (skillSelectionUI == null)
        {
            Debug.LogError("SkillSelectionUI が設定されていません！");
            return;
        }
        
        skillSelectionUI.SetCharacterByIndex(characterIndex);
        PlayerData characterData = PlayerData.CreateDummyData(characterIndex);
        
        if (showDebugLog)
        {
            Debug.Log($"キャラクター切り替え: {characterData.playerName} (Lv.{characterData.level})");
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