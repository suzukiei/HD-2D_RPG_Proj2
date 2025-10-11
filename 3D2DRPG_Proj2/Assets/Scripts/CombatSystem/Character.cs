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
        CharacterObj= this.gameObject;
        skills = characterData.skills;
        statusEffects= characterData.statusEffects;
        CharacterTransfrom= characterData.CharacterTransfrom;
        StatusFlag= characterData.StatusFlag;
    }
}
