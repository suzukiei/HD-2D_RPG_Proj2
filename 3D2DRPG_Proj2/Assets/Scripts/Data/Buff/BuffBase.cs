using System.Collections.Generic;
using UnityEngine;
//ハブ範囲
public enum BuffRange
{
    Self,       // 自分自身
    Ally,       // 味方
    Enemy,      // 敵
    AllAllies,  // 全味方
    AllEnemies  // 全敵
}

public abstract class BuffBase : ScriptableObject
{ 
    [Header("共通情報")]
    public string buffName;
    [Header("バフ効果ターン")]
    public int duration;
    [Header("バフ範囲")]
    public BuffRange buffRange;
    [Header("バフ説明")]
    public string description;
    [Header("バフを与えているキャラクター格納")]
    public Character sourceCharacter;

    // 実際にバフを適用する関数
    public abstract void Apply(Character target);

    // 終了時に呼ばれる
    public abstract void Remove();
}
