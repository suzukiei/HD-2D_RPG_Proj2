using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character: MonoBehaviour
{
    private CharacterData characterData;
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
    public void init(CharacterData _characterData)
    {
        characterData = _characterData;
        name = characterData.name;
        enemyCheckFlag = characterData.enemyCheckFlag;
        hp = characterData.hp;
        mp = characterData.mp;
        atk = characterData.atk;
        def = characterData.def;
        spd = characterData.spd;
        maxHp = characterData.maxHp;
        maxMp = characterData.maxMp;
        exp = characterData.exp;
        level = characterData.level;
        CharacterObj = this.gameObject;
        skills = characterData.skills;
        statusEffects= characterData.statusEffects;
        CharacterTransfrom= characterData.CharacterTransfrom;
        StatusFlag= characterData.StatusFlag;
    }
    // ダメージ計算
    public int Attack(Character enemy,SkillData skillData)
    {
        int damage = Mathf.Max(0, (int)skillData.power - enemy.def);
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

}
