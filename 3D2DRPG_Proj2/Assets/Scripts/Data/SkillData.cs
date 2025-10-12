using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DamageType { Physical, Magical, True }
public enum ElementType { None, Fire, Ice, Thunder, Wind, Light, Dark }
public enum TargetType { SingleEnemy, AllEnemies, Self, Ally, AllAllies }
public enum BuffType { None, AttackUp, DefenseUp, SpeedUp, AttackDown, DefenseDown }
public enum SkillEffectType { Attack, Heal, Buff, Debuff, Special }
[CreateAssetMenu(menuName = "SkillData")]
public class SkillData : ScriptableObject
{
    [Header("基本情報")]
    public string skillName;
    [TextArea] public string description;
    public Sprite icon;
    public AudioClip soundEffect;
    public GameObject vfxPrefab;
    public string animationName;
    //public string targetType;

    [Header("戦闘パラメータ")]
    public float power = 10f;
    public int mpCost = 0;
    public int spCost = 0;
    public float cooldown = 0f;
    public int hitCount = 1;
    [Range(0, 1)] public float criticalRate = 0.1f;
    [Range(0, 1)] public float accuracy = 1f;

    public DamageType damageType;
    public ElementType elementType;
    public TargetType targetType;

    [Header("コンボ設定")]
    public bool canCombo = false;
    public SkillData comboNextSkill;
    public float timingWindowStart = 0.3f;
    public float timingWindowEnd = 0.6f;
    public float comboDamageMultiplier = 1.2f;
    public int maxcombo= 3;
    public bool missCancel = true;

    [Header("状態異常・効果")]
    //public StatusEffect inflictStatus;
    public float statusChance = 0f;
    public BuffType buffType;
    public float buffValue = 0f;
    public int buffDuration = 0;

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

