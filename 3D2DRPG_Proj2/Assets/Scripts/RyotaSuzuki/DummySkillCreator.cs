using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// ダミーのスキルデータを作成するためのクラス
/// </summary>
public class DummySkillCreator : MonoBehaviour
{
    [Header("ダミースキル生成")]
    [SerializeField] private bool createDummySkills = false;
    
#if UNITY_EDITOR
    [ContextMenu("ダミースキルを作成")]
    public void CreateDummySkills()
    {
        // ダミースキルのデータ
        var dummySkillsData = new[]
        {
            new { name = "ファイアボール", mpCost = 10, power = 25, targetType = "敵単体" },
            new { name = "アイスランス", mpCost = 8, power = 20, targetType = "敵単体" },
            new { name = "ヒール", mpCost = 15, power = 30, targetType = "味方単体" },
            new { name = "ライトニング", mpCost = 20, power = 35, targetType = "敵単体" },
            new { name = "シールド", mpCost = 12, power = 0, targetType = "味方単体" },
            new { name = "メテオ", mpCost = 30, power = 50, targetType = "敵全体" },
            new { name = "リジェネ", mpCost = 18, power = 15, targetType = "味方単体" },
            new { name = "サンダーストーム", mpCost = 25, power = 40, targetType = "敵全体" }
        };
        
        // Assets/Scripts/Data/DummySkillsフォルダを作成
        string folderPath = "Assets/Scripts/Data/DummySkills";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets/Scripts/Data", "DummySkills");
        }
        
        // 各スキルのScriptableObjectを作成
        foreach (var skillData in dummySkillsData)
        {
            SkillData skill = ScriptableObject.CreateInstance<SkillData>();
            skill.name = skillData.name;
            skill.mpCost = skillData.mpCost;
            skill.power = skillData.power;
            skill.targetType = skillData.targetType;
            
            string assetPath = $"{folderPath}/{skillData.name}.asset";
            AssetDatabase.CreateAsset(skill, assetPath);
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"ダミースキル {dummySkillsData.Length}個を作成しました！");
    }
#endif
}

/// <summary>
/// テスト用のスキルリストを提供するクラス
/// </summary>
[System.Serializable]
public class DummySkillProvider : MonoBehaviour
{
    [Header("テスト用スキルリスト")]
    [SerializeField] private SkillData[] dummySkills;
    
    /// <summary>
    /// ダミースキルのリストを取得
    /// </summary>
    public SkillData[] GetDummySkills()
    {
        return dummySkills;
    }
    
    /// <summary>
    /// 指定した数のランダムスキルを取得
    /// </summary>
    public SkillData[] GetRandomSkills(int count)
    {
        if (dummySkills == null || dummySkills.Length == 0)
            return new SkillData[0];
            
        count = Mathf.Min(count, dummySkills.Length);
        SkillData[] result = new SkillData[count];
        
        // シャッフルして指定数を取得
        var shuffled = new System.Collections.Generic.List<SkillData>(dummySkills);
        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(i, shuffled.Count);
            result[i] = shuffled[randomIndex];
            shuffled[randomIndex] = shuffled[i];
        }
        
        return result;
    }
    
    /// <summary>
    /// キャラクタータイプに応じたスキルセットを取得
    /// </summary>
    public SkillData[] GetSkillsByCharacterType(string characterType)
    {
        switch (characterType.ToLower())
        {
            case "mage":
            case "魔法使い":
                return GetSkillsContaining(new[] { "ファイアボール", "アイスランス", "ライトニング", "メテオ" });
                
            case "healer":
            case "回復役":
                return GetSkillsContaining(new[] { "ヒール", "リジェネ", "シールド" });
                
            case "warrior":
            case "戦士":
                return GetRandomSkills(3); // 戦士用スキルがないので適当に
                
            default:
                return GetRandomSkills(4);
        }
    }
    
    /// <summary>
    /// 指定した名前のスキルを含むリストを取得
    /// </summary>
    private SkillData[] GetSkillsContaining(string[] skillNames)
    {
        var result = new System.Collections.Generic.List<SkillData>();
        
        foreach (string skillName in skillNames)
        {
            foreach (SkillData skill in dummySkills)
            {
                if (skill != null && skill.name.Contains(skillName))
                {
                    result.Add(skill);
                    break;
                }
            }
        }
        
        return result.ToArray();
    }
} 