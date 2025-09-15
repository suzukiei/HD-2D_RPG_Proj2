using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SkillData")]
public class SkillData : ScriptableObject
{
    public string name;
    public int mpCost;
    //public string effectType;
    public int power;
    public string targetType;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="applyID">0ならCharacter、1ならEnemy</param>
    /// <param name="character"></param>
    /// <param name="enemy"></param>
    public void apply(bool applyID, CharacterData characterData)
    {
        //プレイヤーかエネミーなのかを判断
        if(applyID)
        {
            
        }
    }
    public virtual void Use(CharacterData user, CharacterData target)
    {
        // スキルの効果処理
    }
}

