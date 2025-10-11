using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StatusFlag
{
    None = 0,
    Move,
    Select,
    Attack,
    End
}

[CreateAssetMenu(menuName = "CharacterData")]
public class CharacterData : ScriptableObject
{
    public string name;
    public bool enemyCheckFlag;
    public int hp, mp, atk, def, spd;
    public int maxHp, maxMp;
    public int exp, level;
    public GameObject CharacterObj;
    public SkillData[] skills;
    public StatusEffectData statusEffects;
    public Vector3 CharacterTransfrom;
    public StatusFlag StatusFlag;

    // ダメージ計算
    public int Attack(CharacterData enemy)
    {
        int damage = Mathf.Max(0, atk - enemy.def);
        enemy.TakeDamage(damage);
        return damage;
    }

    // ダメージを受ける
    public void TakeDamage(int damage)
    {
        hp = Mathf.Max(0, hp - damage);
    }

    // HP/MP回復
    public void Heal(int amount)
    {
        hp = Mathf.Min(maxHp, hp + amount);
    }
    public void RestoreMp(int amount)
    {
        mp = Mathf.Min(maxMp, mp + amount);
    }

    // レベルアップ
    public void GainExp(int amount)
    {
        exp += amount;
        if (exp >= ExpToLevelUp())
        {
            LevelUp();
        }
    }
    private int ExpToLevelUp()
    {
        return level * 100; // 例: レベルごとに100ずつ必要
    }
    private void LevelUp()
    {
        level++;
        exp = 0;
        maxHp += 10;
        maxMp += 5;
        atk += 2;
        def += 2;
        spd += 1;
        hp = maxHp;
        mp = maxMp;
    }

    // ステータス異常付与
    public void AddStatusEffect(StatusEffectData effect)
    {
        statusEffects = effect;
    }

    public void useSkill(int skillNumber, CharacterData enemy)
    {
        if (skills != null && skillNumber >= 0 && skillNumber < skills.Length)
        {
            skills[skillNumber].Use(this, enemy);
        }
    }
}

