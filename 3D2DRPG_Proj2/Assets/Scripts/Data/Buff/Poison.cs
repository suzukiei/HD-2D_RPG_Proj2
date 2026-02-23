using UnityEngine;

/// <summary>
/// 毒バフ
/// ターン終了時に固定ダメージを与える
/// </summary>
[CreateAssetMenu(menuName = "RPG/Buffs/Poison")]
public class Poison : BuffBase
{
    [Header("ターン毎のダメージ")]
    [Tooltip("毎ターン終了時に与えるダメージ量")]
    public int damagePerTurn = 10;

    private void OnEnable()
    {
        if (string.IsNullOrEmpty(buffId))
        {
            buffId = System.Guid.NewGuid().ToString();
        }
        if (string.IsNullOrEmpty(buffName))
        {
            buffName = "毒";
        }

        statusEffect = StatusEffect.Poison;
    }

    public override void Apply(Character target)
    {
        if (target == null)
        {
            Debug.LogWarning("PoisonBuff適用失敗: ターゲットがnullです");
            return;
        }

        sourceCharacter = target;
        Debug.Log($"{target.charactername} に毒（毎ターン{damagePerTurn}ダメージ）を適用しました");
    }

    public override void Remove()
    {
        if (sourceCharacter != null)
        {
            Debug.Log($"{sourceCharacter.charactername} から毒を解除しました");
        }
    }
}