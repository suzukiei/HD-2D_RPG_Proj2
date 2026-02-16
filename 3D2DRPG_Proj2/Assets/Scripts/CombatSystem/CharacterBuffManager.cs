using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// キャラクターごとのバフ管理クラス
/// </summary>
public class CharacterBuffManager : MonoBehaviour
{
    [Header("バフ管理")]
    [SerializeField] private List<BuffInstance> activeBuffs = new List<BuffInstance>();
    
    private Character ownerCharacter;
    
    // ベースステータス（バフ適用前の元の値）
    private int baseAtk;
    private int baseDef;
    private int baseSpd;
    private int baseMaxHp;
    private int baseMaxMp;
    
    // バフ効果の合計値（計算用）
    private Dictionary<StatType, float> statModifiers = new Dictionary<StatType, float>();
    
    /// <summary>
    /// 初期化
    /// </summary>
    public void Initialize(Character character)
    {
        ownerCharacter = character;
        
        // ベースステータスを保存
        baseAtk = character.atk;
        baseDef = character.def;
        baseSpd = character.spd;
        baseMaxHp = character.maxHp;
        baseMaxMp = character.maxMp;
        
        // ステータス修正値の初期化
        InitializeStatModifiers();
    }
    
    /// <summary>
    /// ステータス修正値の初期化
    /// </summary>
    private void InitializeStatModifiers()
    {
        statModifiers[StatType.Attack] = 0f;
        statModifiers[StatType.Defense] = 0f;
        statModifiers[StatType.Speed] = 0f;
        statModifiers[StatType.MaxHp] = 0f;
        statModifiers[StatType.MaxMp] = 0f;
    }
    
    /// <summary>
    /// バフを適用
    /// </summary>
    public bool ApplyBuff(BuffInstance buffInstance, Character appliedBy)
    {
        //
        if (buffInstance == null || ownerCharacter == null)
        {
            Debug.LogWarning("バフ適用失敗: バフインスタンスまたはキャラクターがnullです");
            return false;
        }
        
        // 既に同じバフが適用されているかチェック
        // まずbuffIdでチェック、なければ型と名前でチェック
        var existingBuff = activeBuffs.FirstOrDefault(b => 
            b.baseData != null && 
            buffInstance.baseData != null && 
            (!string.IsNullOrEmpty(b.baseData.buffId) && !string.IsNullOrEmpty(buffInstance.baseData.buffId) && 
             b.baseData.buffId == buffInstance.baseData.buffId) ||
            (b.baseData.GetType() == buffInstance.baseData.GetType() &&
             b.baseData.buffName == buffInstance.baseData.buffName));
        
        if (existingBuff != null)
        {
            // スタック処理
            return HandleStack(existingBuff, buffInstance);
        }
        
        // 新しいバフを追加
        buffInstance.Apply(appliedBy);
        activeBuffs.Add(buffInstance);
        
        // ステータス修正値を再計算
        RecalculateStatModifiers();
        
        Debug.Log($"{ownerCharacter.charactername} にバフ '{buffInstance.buffName}' を適用しました");
        return true;
    }
    
    /// <summary>
    /// スタック処理
    /// </summary>
    private bool HandleStack(BuffInstance existingBuff, BuffInstance newBuff)
    {
        // 現在はスタック不可として、既存のバフを上書き（時間をリセット）
        existingBuff.remainingTurns = newBuff.remainingTurns;
        Debug.Log($"{ownerCharacter.charactername} のバフ '{existingBuff.buffName}' の持続時間をリセットしました");
        return true;
    }
    
    /// <summary>
    /// バフを解除
    /// </summary>
    public bool RemoveBuff(BuffInstance buffInstance)
    {
        if (buffInstance == null)
        {
            return false;
        }
        
        if (activeBuffs.Contains(buffInstance))
        {
            buffInstance.Remove();
            activeBuffs.Remove(buffInstance);
            
            // ステータス修正値を再計算
            RecalculateStatModifiers();
            
            Debug.Log($"{ownerCharacter.charactername} からバフ '{buffInstance.buffName}' を解除しました");
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// ターン経過処理
    /// </summary>
    public void TickTurn()
    {
        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            var buff = activeBuffs[i];
            buff.TickTurn();
            
            if (buff.IsExpired())
            {
                RemoveBuff(buff);
            }
        }
    }
    
    /// <summary>
    /// ステータス修正値を再計算
    /// </summary>
    private void RecalculateStatModifiers()
    {
        InitializeStatModifiers();
        
        foreach (var buff in activeBuffs)
        {
            if (buff.baseData == null) continue;
            
            // 各バフの効果を適用
            // この部分は各バフクラスで実装する必要があります
            // 現在はAttackUpBuffなどの具体的な実装に依存
        }
    }
    
    /// <summary>
    /// バフ適用後の攻撃力を取得
    /// </summary>
    public int GetEffectiveAttack()
    {
        if (ownerCharacter == null)
        {
            return 0;
        }
        
        // 現在のベース攻撃力を取得（レベルアップなどで変更されている可能性があるため）
        float effectiveAtk = ownerCharacter.atk;
        
        // バフ効果を適用
        foreach (var buff in activeBuffs)
        {
            if (buff.baseData is AttackUpBuff attackBuff)
            {
                effectiveAtk *= attackBuff.attackMultiplier;
            }
        }
        
        return Mathf.RoundToInt(effectiveAtk);
    }
    
    /// <summary>
    /// バフ適用後の防御力を取得
    /// </summary>
    public int GetEffectiveDefense()
    {
        if (ownerCharacter == null)
        {
            return 0;
        }
        
        // 現在のベース防御力を取得
        float effectiveDef = ownerCharacter.def;
        
        // 防御力バフの効果を適用
        foreach (var buff in activeBuffs)
        {
            if (buff.baseData is DefenseUpBuff defenseBuff)
            {
                effectiveDef *= defenseBuff.defenseMultiplier;
            }
        }
        
        return Mathf.RoundToInt(effectiveDef);
    }
    
    /// <summary>
    /// バフ適用後の速度を取得
    /// </summary>
    public int GetEffectiveSpeed()
    {
        if (ownerCharacter == null)
        {
            return 0;
        }
        
        // 現在のベース速度を取得
        float effectiveSpd = ownerCharacter.spd;
        
        // 速度バフの効果を適用
        foreach (var buff in activeBuffs)
        {
            if (buff.baseData is SpeedUpBuff speedBuff)
            {
                effectiveSpd *= speedBuff.speedMultiplier;
            }
        }
        
        return Mathf.RoundToInt(effectiveSpd);
    }
    
    /// <summary>
    /// バフ適用後の魔法攻撃力を取得
    /// </summary>
    public int GetEffectiveMagicAttack()
    {
        if (ownerCharacter == null)
        {
            return 0;
        }
        
        // 現在のベース攻撃力を取得（魔法攻撃力はINTとして扱う）
        // 注意: CharacterクラスにINTフィールドがない場合は、atkを代用
        float effectiveMagicAtk = ownerCharacter.atk; // 将来的にINTフィールドが追加されたら変更
        
        // 魔法攻撃力バフの効果を適用
        foreach (var buff in activeBuffs)
        {
            if (buff.baseData is MagicAttackUpBuff magicBuff)
            {
                effectiveMagicAtk *= magicBuff.magicAttackMultiplier;
            }
        }
        
        return Mathf.RoundToInt(effectiveMagicAtk);
    }
    
    /// <summary>
    /// 指定されたステータスタイプのバフ適用後の値を取得
    /// </summary>
    public float GetEffectiveStat(StatType statType, float baseValue)
    {
        float effectiveValue = baseValue;
        
        // バフ効果を適用
        foreach (var buff in activeBuffs)
        {
            if (buff.baseData == null) continue;
            
            // 各バフタイプに応じた処理
            switch (statType)
            {
                case StatType.Attack:
                    if (buff.baseData is AttackUpBuff attackBuff)
                    {
                        effectiveValue *= attackBuff.attackMultiplier;
                    }
                    break;
                case StatType.Defense:
                    if (buff.baseData is DefenseUpBuff defenseBuff)
                    {
                        effectiveValue *= defenseBuff.defenseMultiplier;
                    }
                    break;
                case StatType.Speed:
                    if (buff.baseData is SpeedUpBuff speedBuff)
                    {
                        effectiveValue *= speedBuff.speedMultiplier;
                    }
                    break;
                // 他のステータスタイプも同様に処理
            }
        }
        
        return effectiveValue;
    }
    
    /// <summary>
    /// 指定されたバフが適用されているかチェック（型で検索）
    /// </summary>
    public bool HasBuff(System.Type buffType)
    {
        return activeBuffs.Any(b => b.baseData != null && b.baseData.GetType() == buffType);
    }
    
    /// <summary>
    /// 指定されたバフIDのバフが適用されているかチェック
    /// </summary>
    public bool HasBuff(string buffId)
    {
        if (string.IsNullOrEmpty(buffId))
        {
            return false;
        }
        return activeBuffs.Any(b => b.baseData != null && b.baseData.buffId == buffId);
    }
    
    /// <summary>
    /// 指定されたバフIDのバフインスタンスを取得
    /// </summary>
    public BuffInstance GetBuff(string buffId)
    {
        if (string.IsNullOrEmpty(buffId))
        {
            return null;
        }
        return activeBuffs.FirstOrDefault(b => b.baseData != null && b.baseData.buffId == buffId);
    }
    
    /// <summary>
    /// アクティブなバフのリストを取得
    /// </summary>
    public List<BuffInstance> GetActiveBuffs()
    {
        return new List<BuffInstance>(activeBuffs);
    }
    
    /// <summary>
    /// 全てのバフを解除
    /// </summary>
    public void ClearAllBuffs()
    {
        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            RemoveBuff(activeBuffs[i]);
        }
    }
    
    /// <summary>
    /// ベースステータスを更新（レベルアップ時など）
    /// </summary>
    public void UpdateBaseStats(int newAtk, int newDef, int newSpd, int newMaxHp, int newMaxMp)
    {
        baseAtk = newAtk;
        baseDef = newDef;
        baseSpd = newSpd;
        baseMaxHp = newMaxHp;
        baseMaxMp = newMaxMp;
    }
}
