using UnityEngine;
using static Unity.VisualScripting.Member;

[System.Serializable]
public class BuffInstance
{
    public BuffBase baseData;          // 元のデータ（ScriptableObject）
    [Header("共通情報")]
    public string buffName;
    [Header("残りターン数（独立管理）")]
    public int remainingTurns;
    [Header("バフ範囲")]
    public BuffRange buffRange;
    [Header("バフ説明")]
    public string description;
    [Header("バフを与えているキャラクター格納")]
    public Character sourceCharacter;

    public BuffInstance(BuffBase baseBuff)
    {
        baseData =baseBuff;
        buffName = baseData.buffName;
        remainingTurns = baseData.duration;
        buffRange = baseData.buffRange;
        sourceCharacter = baseData.sourceCharacter;
    }
    public void Apply(Character _sourceCharacter)
    {
        Debug.Log("BuffInstance Apply called for buff: " + _sourceCharacter);
        sourceCharacter = _sourceCharacter;
        // ScriptableObjectのApplyを呼ぶ（内部でtargetに効果を与える）
        baseData.sourceCharacter = _sourceCharacter;
        baseData.Apply(_sourceCharacter);
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
        baseData.Remove();
    }
}
