using UnityEngine;

[System.Serializable]
public class BuffInstance
{
    public BuffBase baseData;          // ベースデータ（ScriptableObject）
    [Header("バフID")]
    public string buffId;
    [Header("バフ名")]
    public string buffName;
    [Header("残りターン数（ターン管理）")]
    public int remainingTurns;
    [Header("バフ範囲")]
    public BuffRange buffRange;
    [Header("バフ説明")]
    public string description;
    [Header("バフを付与したキャラクター")]
    public Character sourceCharacter;
    [Header("バフが適用されているキャラクター")]
    public Character targetCharacter;
    [Header("スキルが封じられているか？")]
    public int LockSkillIndex = -2; // -2=封じなし, -1=全スキル, 0移行は=プレイヤーの特定スキル(CharacterDataのスキルリスト順)
    [Header("バフの増減値")]
    public float buffValue = 0f; //バフの値、増減率など入れる
    [Header("ロックオン対象")]
    public Character lockedTarget = null; // LockIn用：攻撃対象を限定
    public enum StatusEffect
    {
        Poison,
        Stun,
        Burn,
        Freeze,
        Sleep,
        Silent,
        DamageUp,
        TurnChange,
        DefenceUp,
        SpdDown,
        SpdUp,
        MagicDamageDown,
        MagicCounter,
        Makituki,
        Zouen,
        MPRecovery,
        LockIn
    } //毒、スタン、やけど、凍結、眠り、魔封,ダメ増,ターンチェンジ,防御UP,スピードUP,スピードDN,マジックダメDN,反射,巻きつき,増援,MP回復,対象を絞る


    public BuffInstance(BuffBase baseBuff)
    {
        baseData = baseBuff;
        if (baseData != null)
        {
            buffId = baseData.buffId;
            buffName = baseData.buffName;
            remainingTurns = baseData.duration;
            buffRange = baseData.buffRange;
            description = baseData.description;
        }
    }
    
    /// <summary>
    /// バフを適用
    /// </summary>
    public void Apply(Character target)
    {
        if (target == null || baseData == null)
        {
            Debug.LogWarning("バフ適用失敗: ターゲットまたはベースデータがnullです");
            return;
        }
        
        targetCharacter = target;
        sourceCharacter = baseData.sourceCharacter;
        
        // ScriptableObjectのApplyを呼び出す（各バフクラスで実装）
        baseData.sourceCharacter = sourceCharacter;
        baseData.Apply(target);
        
        Debug.Log($"バフ '{buffName}' を {target.charactername} に適用しました");
    }

    public void TickTurn()
    {
        remainingTurns--;
    }

    public bool IsExpired()
    {
        return remainingTurns <= 0;
    }

    public void Remove()
    {
        if (baseData != null)
        {
            baseData.Remove();
        }
    }
}
