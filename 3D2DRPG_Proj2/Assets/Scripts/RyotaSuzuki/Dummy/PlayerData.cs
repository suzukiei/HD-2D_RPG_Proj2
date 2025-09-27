using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    [Header("基本情報")]
    public string playerName = "プレイヤー";
    public int level = 1;
    
    [Header("ステータス")]
    public int currentHP = 100;
    public int maxHP = 100;
    public int currentMP = 50;
    public int maxMP = 50;
    public int attack = 20;
    public int defense = 15;
    public int speed = 10;
    
    [Header("経験値")]
    public int currentEXP = 0;
    public int nextLevelEXP = 100;

    /// <summary>
    /// HP割合を取得（0.0～1.0）
    /// </summary>
    public float GetHPRatio()
    {
        return maxHP > 0 ? (float)currentHP / maxHP : 0f;
    }

    /// <summary>
    /// MP割合を取得（0.0～1.0）
    /// </summary>
    public float GetMPRatio()
    {
        return maxMP > 0 ? (float)currentMP / maxMP : 0f;
    }

    /// <summary>
    /// EXP割合を取得（0.0～1.0）
    /// </summary>
    public float GetEXPRatio()
    {
        return nextLevelEXP > 0 ? (float)currentEXP / nextLevelEXP : 0f;
    }
    /// <summary>
    /// コンストラクタ
    /// </summary>
    public PlayerData() { }
    /// <summary>
    /// 追加コンストラクタ（CharacterDataからの変換用）
    /// </summary>
    public PlayerData (Character characterData)
    {
        playerName = characterData.name;
        level = characterData.level;
        currentHP = characterData.hp;
        maxHP = characterData.maxHp;
        currentMP = characterData.mp;
        maxMP = characterData.maxMp;
        attack = characterData.atk;
        defense = characterData.def;
        speed = characterData.spd;
        currentEXP = characterData.exp;
        nextLevelEXP = characterData.level * 100; // 仮の計算式
    }
    /// <summary>
    /// ダミーデータを生成
    /// </summary>
    public static PlayerData CreateDummyData(int characterIndex)
    {
        PlayerData data = new PlayerData();
        
        switch (characterIndex)
        {
            case 0:
                data.playerName = "勇者";
                data.level = 12;
                data.currentHP = 180;
                data.maxHP = 200;
                data.currentMP = 45;
                data.maxMP = 60;
                data.attack = 35;
                data.defense = 28;
                data.speed = 22;
                data.currentEXP = 750;
                data.nextLevelEXP = 1000;
                break;
            case 1:
                data.playerName = "魔法使い";
                data.level = 10;
                data.currentHP = 95;
                data.maxHP = 120;
                data.currentMP = 85;
                data.maxMP = 100;
                data.attack = 18;
                data.defense = 12;
                data.speed = 25;
                data.currentEXP = 420;
                data.nextLevelEXP = 800;
                break;
            case 2:
                data.playerName = "戦士";
                data.level = 15;
                data.currentHP = 220;
                data.maxHP = 250;
                data.currentMP = 20;
                data.maxMP = 30;
                data.attack = 45;
                data.defense = 40;
                data.speed = 15;
                data.currentEXP = 1200;
                data.nextLevelEXP = 1500;
                break;
            default:
                data.playerName = $"キャラクター{characterIndex + 1}";
                break;
        }
        
        return data;
    }
}