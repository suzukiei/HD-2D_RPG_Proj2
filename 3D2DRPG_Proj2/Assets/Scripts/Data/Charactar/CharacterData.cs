using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum StatusFlag
{
    None = 0,
    Move,
    Select,
    Attack,
    Heal,
    Buff,
    End
}

[CreateAssetMenu(menuName = "CharacterData")]
public class CharacterData : ScriptableObject
{
    public string charactername;
    public Sprite characterIcon;
    public bool enemyCheckFlag;
    public int hp, mp, atk, def, spd;
    public int maxHp, maxMp;
    public int exp, level;
    public GameObject CharacterObj;
    public SkillData[] skills;
    public Vector3 CharacterTransfrom;
    public StatusFlag StatusFlag;
   
}

