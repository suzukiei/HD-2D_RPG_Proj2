using UnityEngine;

/// <summary>
/// 魔法反射バフ
/// </summary>
[CreateAssetMenu(menuName = "RPG/Buffs/MagicCounter")]
public class MagicCounter : BuffBase
{
    [Header("反射率")]
    [Tooltip("反射するダメージの割合（例: 1.0 = 100%反射）")]
    [Range(0f, 2f)]
    public float reflectRate = 1.0f;

    private void OnEnable()
    {
        if (string.IsNullOrEmpty(buffId))
        {
            buffId = System.Guid.NewGuid().ToString();
        }
        if (string.IsNullOrEmpty(buffName))
        {
            buffName = "マジックカウンター";
        }

        statusEffect = StatusEffect.MagicCounter;
    }

    public override void Apply(Character target)
    {
        if (target == null)
        {
            Debug.LogWarning("MagicCounterBuff適用失敗: ターゲットがnullです");
            return;
        }

        sourceCharacter = target;
        Debug.Log($"{target.charactername} にマジックカウンター（{reflectRate * 100}%反射）を適用しました");
    }

    public override void Remove()
    {
        if (sourceCharacter != null)
        {
            Debug.Log($"{sourceCharacter.charactername} からマジックカウンターを解除しました");
        }
    }
}