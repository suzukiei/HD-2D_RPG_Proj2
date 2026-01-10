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
    
    [Header("レベルアップ設定")]
    public int maxLevel = 99; // 最大レベル
    
    // レベルアップイベント（旧レベルを引数として渡す）
    public System.Action<int> OnLevelUp;
    
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
    
    /// <summary>
    /// 経験値を獲得し、レベルアップを処理
    /// </summary>
    /// <param name="amount">獲得経験値</param>
    /// <returns>レベルアップしたかどうか</returns>
    public bool GainExp(int amount)
    {
        if (level >= maxLevel)
        {
            currentExp = 0;
            return false;
        }
        
        currentExp += amount;
        bool leveledUp = false;
        
        // 連続レベルアップに対応
        while (currentExp >= expToNextLevel && level < maxLevel)
        {
            currentExp -= expToNextLevel;
            LevelUp();
            leveledUp = true;
        }
        
        return leveledUp;
    }
    
    /// <summary>
    /// レベルアップ処理
    /// </summary>
    private void LevelUp()
    {
        int oldLevel = level;
        level++;
        
        // 次のレベルに必要な経験値を計算
        CalculateExpToNextLevel();
        
        // ステータス上昇
        maxHP += 10;
        maxMP += 5;
        attack += 2;
        defense += 2;
        magicAttack += 2;
        magicDefense += 1;
        speed += 1;
        luck += 1;
        
        // HP/MP全回復
        currentHP = maxHP;
        currentMP = maxMP;
        
        // レベルアップイベント発火
        OnLevelUp?.Invoke(oldLevel);
    }
    
    /// <summary>
    /// 次のレベルに必要な経験値を計算
    /// </summary>
    private void CalculateExpToNextLevel()
    {
        // 二次関数的な成長曲線（Character.csと同じ計算式）
        expToNextLevel = 100 + (level - 1) * (level - 1) * 50;
    }
    
    /// <summary>
    /// 次のレベルに必要な経験値を取得（外部から参照可能）
    /// </summary>
    public int GetExpToNextLevel()
    {
        if (level >= maxLevel) return 0;
        return expToNextLevel;
    }
} 