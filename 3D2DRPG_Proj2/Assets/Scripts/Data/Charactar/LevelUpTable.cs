using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LevelUpTable", menuName = "RPG/LevelUpTable")]
public class LevelUpTable : ScriptableObject
{
    [System.Serializable]
    public class StatGain
    {
        [Tooltip("ダイスの個数（2d6なら2）")]
        public int diceCount = 1;
        [Tooltip("ダイスの面数（1d6なら6）")]
        public int diceSides = 6;
        [Tooltip("固定値加算")]
        public int fixedBonus = 0;
        
        /// <summary>
        /// ダイスロールを実行
        /// 例: 1d6なら1～6のランダム、2d6なら2～12のランダム
        /// </summary>
        public int Roll()
        {
            int total = fixedBonus;
            for (int i = 0; i < diceCount; i++)
            {
                total += Random.Range(1, diceSides + 1); // 1～diceSides（両端含む）
            }
            return total;
        }
        
        /// <summary>
        /// 表示用の文字列（例: "1d6", "2d6+3"）
        /// </summary>
        public override string ToString()
        {
            if (diceCount == 0 && fixedBonus > 0)
                return fixedBonus.ToString();
            
            string result = $"{diceCount}d{diceSides}";
            if (fixedBonus > 0)
                result += $"+{fixedBonus}";
            return result;
        }
    }
    
    [System.Serializable]
    public class LevelUpData
    {
        [Header("レベル")]
        public int level;
        
        [Header("ステータス上昇（ダイスロール）")]
        public StatGain hpGain;
        public StatGain mpGain;
        public StatGain atkGain;  // STR（攻撃力）
        public StatGain defGain;  // DEF（防御力）
        public StatGain spdGain;  // SPD（素早さ）
        public StatGain intGain;  // INT（魔法攻撃力）
        
        [Header("スキル習得")]
        [Tooltip("このレベルで習得するスキル")]
        public SkillData learnedSkill;
    }
    
    [Header("キャラクター情報")]
    public string characterName;
    
    [Header("レベルアップデータ")]
    public List<LevelUpData> levelUpTable = new List<LevelUpData>();
    
    /// <summary>
    /// 指定レベルのデータを取得
    /// </summary>
    public LevelUpData GetLevelUpData(int level)
    {
        return levelUpTable.Find(x => x.level == level);
    }
}
