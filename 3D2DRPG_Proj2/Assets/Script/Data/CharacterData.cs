using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CharacterData")]
public class CharacterData : ScriptableObject
{
    public string name;
    public bool enemyCheckFalg;
    public int hp, mp, atk, def,spd;
    public GameObject CharacterObj;
    public SkillData[] skills;
    public StatusEffectData statusEffects;
    public Vector3 vector3;
    
    public int attack(CharacterData enemy)
    {
        return 0;
    }

    public void useSkill(int skillNumber,CharacterData enemy)
    {
        //ƒXƒLƒ‹‚Ìˆ—‚ğ—¬‚·
    }
}

