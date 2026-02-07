using UnityEngine;

/// <summary>
/// ステータスの種類
/// </summary>
public enum StatType
{
    Attack,     // 攻撃力 (atk)
    Defense,    // 防御力 (def)
    Speed,      // 速度 (spd)
    MaxHp,      // 最大HP
    MaxMp,      // 最大MP
    Hp,         // 現在HP
    Mp          // 現在MP
}

/// <summary>
/// バフ効果の種類
/// </summary>
public enum BuffEffectType
{
    FixedAdd,           // 固定値加算
    PercentageAdd,      // パーセンテージ加算（ベースステータスに対する割合）
    PercentageMultiply  // パーセンテージ乗算（現在のステータスに対する割合）
}
