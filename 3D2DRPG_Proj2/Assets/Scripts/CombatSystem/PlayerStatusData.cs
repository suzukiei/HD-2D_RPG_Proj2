using UnityEngine;

[System.Serializable]
public class PlayerStatusData
{
    [Header("基本ステータス")]
    public string playerName = "主人公";
    public int level = 1;
    public int currentHP = 100;
    public int maxHP = 100;
    public int currentMP = 50;
    public int maxMP = 50;
    
    [Header("経験値")]
    public int currentExp = 0;
    public int expToNextLevel = 100;
    
    [Header("攻撃力・防御力")]
    public int attack = 25;
    public int defense = 15;
    public int magicAttack = 20;
    public int magicDefense = 18;
    
    [Header("速度・運")]
    public int speed = 12;
    public int luck = 10;
    
    /// <summary>
    /// HPの割合を取得（0.0～1.0）
    /// </summary>
    public float GetHPRatio()
    {
        if (maxHP <= 0) return 0f;
        return (float)currentHP / maxHP;
    }
    
    /// <summary>
    /// MPの割合を取得（0.0～1.0）
    /// </summary>
    public float GetMPRatio()
    {
        if (maxMP <= 0) return 0f;
        return (float)currentMP / maxMP;
    }
    
    /// <summary>
    /// 経験値の割合を取得（0.0～1.0）
    /// </summary>
    public float GetExpRatio()
    {
        if (expToNextLevel <= 0) return 1f;
        return (float)currentExp / expToNextLevel;
    }
    
    /// <summary>
    /// HPを回復
    /// </summary>
    public void HealHP(int amount)
    {
        currentHP = Mathf.Min(currentHP + amount, maxHP);
    }
    
    /// <summary>
    /// MPを回復
    /// </summary>
    public void RestoreMP(int amount)
    {
        currentMP = Mathf.Min(currentMP + amount, maxMP);
    }
    
    /// <summary>
    /// ダメージを受ける
    /// </summary>
    public void TakeDamage(int damage)
    {
        currentHP = Mathf.Max(currentHP - damage, 0);
    }
    
    /// <summary>
    /// MPを消費
    /// </summary>
    public bool ConsumeMP(int amount)
    {
        if (currentMP >= amount)
        {
            currentMP -= amount;
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// 戦闘不能かどうか
    /// </summary>
    public bool IsDead()
    {
        return currentHP <= 0;
    }
} 