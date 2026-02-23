using UnityEngine;

/// <summary>
/// 魔法ダメージ軽減バフ
/// </summary>
[CreateAssetMenu(menuName = "RPG/Buffs/MagicDamageDown")]
public class MagicDamageDown : BuffBase
{
    [Header("魔法ダメージ軽減率")]
    [Tooltip("軽減率（例: 0.5 = 50%軽減）")]
    [Range(0f, 1f)]
    public float damageReductionRate = 0.5f;

    private void OnEnable()
    {
        if (string.IsNullOrEmpty(buffId))
        {
            buffId = System.Guid.NewGuid().ToString();
        }
        if (string.IsNullOrEmpty(buffName))
        {
            buffName = "マジックバリア";
        }

        statusEffect = StatusEffect.MagicDamageDown;
    }

    public override void Apply(Character target)
    {
        if (target == null)
        {
            Debug.LogWarning("MagicDamageDownBuff適用失敗: ターゲットがnullです");
            return;
        }

        sourceCharacter = target;
        Debug.Log($"{target.charactername} にマジックバリア（{damageReductionRate * 100}%軽減）を適用しました");
    }

    public override void Remove()
    {
        if (sourceCharacter != null)
        {
            Debug.Log($"{sourceCharacter.charactername} からマジックバリアを解除しました");
        }
    }
}