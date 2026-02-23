using UnityEngine;

/// <summary>
/// ロックオンバフ
/// 特定の対象へのクリティカル率を上昇させる
/// </summary>
[CreateAssetMenu(menuName = "RPG/Buffs/LockIn")]
public class LockIn : BuffBase
{
    [Header("クリティカル率上昇")]
    [Tooltip("ロックオン対象へのクリティカル率上昇値（例: 0.3 = +30%）")]
    [Range(0f, 1f)]
    public float criticalRateBonus = 0.3f;

    private void OnEnable()
    {
        if (string.IsNullOrEmpty(buffId))
        {
            buffId = System.Guid.NewGuid().ToString();
        }
        if (string.IsNullOrEmpty(buffName))
        {
            buffName = "ロックオン";
        }

        statusEffect = StatusEffect.LockIn;
    }

    public override void Apply(Character target)
    {
        if (target == null)
        {
            Debug.LogWarning("LockInBuff適用失敗: ターゲットがnullです");
            return;
        }

        sourceCharacter = target;
        Debug.Log($"{target.charactername} にロックオンバフを適用しました");
    }

    public override void Remove()
    {
        if (sourceCharacter != null)
        {
            Debug.Log($"{sourceCharacter.charactername} からロックオンバフを解除しました");
        }
    }
}