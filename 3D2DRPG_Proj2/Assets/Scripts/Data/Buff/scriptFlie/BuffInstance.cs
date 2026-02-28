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
        //セットの際に継続が0あるかを確認
        //
        //
        //

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
