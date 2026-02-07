using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// バフの適用範囲
/// </summary>
public enum BuffRange
{
    Self,       // 自分自身
    Ally,       // 味方
    Enemy,      // 敵
    AllAllies,  // 全味方
    AllEnemies  // 全敵
}

/// <summary>
/// バフの基底クラス
/// </summary>
public abstract class BuffBase : ScriptableObject
{ 
    [Header("基本情報")]
    [Tooltip("バフの一意の識別子（オプション）")]
    public string buffId;
    
    [Header("バフ名")]
    public string buffName;
    
    [Header("バフ説明")]
    [TextArea(3, 5)]
    public string description;
    
    [Header("バフアイコン")]
    [Tooltip("UI表示用のアイコン")]
    public Sprite icon;
    
    [Header("バフタイプ")]
    [Tooltip("カテゴリ分類（攻撃系/防御系/特殊系など）")]
    public BuffType buffType = BuffType.StatusEnhancement;
    
    [Header("持続時間")]
    [Tooltip("バフの持続ターン数")]
    public int duration;
    
    [Header("バフ範囲")]
    [Tooltip("バフが適用される対象範囲")]
    public BuffRange buffRange;
    
    [Header("バフを付与したキャラクター")]
    [Tooltip("このバフを付与したキャラクター（自動設定）")]
    public Character sourceCharacter;

    [Header("自分自身が対象")]
    [Tooltip("選択フェイズをなくして自分自身のみ対象にする")]
    public bool isSelfTarget = false;
    /// <summary>
    /// バフを適用する
    /// </summary>
    /// <param name="target">適用対象のキャラクター</param>
    public abstract void Apply(Character target);

    /// <summary>
    /// バフを解除する
    /// </summary>
    public abstract void Remove();
}
