using UnityEngine;

/// <summary>
/// 巻きつきバフ
/// ターン終了時に固定ダメージを与える
/// </summary>
[CreateAssetMenu(menuName = "RPG/Buffs/Makituki")]
public class Makituki : BuffBase
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
            buffName = "巻きつき";
        }

        statusEffect = StatusEffect.Makituki;
    }

    public override void Apply(Character target)
    {
        if (target == null)
        {
            Debug.LogWarning("MakitukiBuff適用失敗: ターゲットがnullです");
            return;
        }

        sourceCharacter = target;
        Debug.Log($"{target.charactername} に巻きつき（毎ターン{damagePerTurn}ダメージ）を適用しました");
    }

    public override void Remove()
    {
        if (sourceCharacter != null)
        {
            Debug.Log($"{sourceCharacter.charactername} から巻きつきを解除しました");
        }
    }
}