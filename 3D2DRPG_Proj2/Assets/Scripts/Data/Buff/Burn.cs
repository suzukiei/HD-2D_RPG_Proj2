using UnityEngine;

/// <summary>
/// やけどバフ
/// ターン終了時に固定ダメージを与える
/// </summary>
[CreateAssetMenu(menuName = "RPG/Buffs/Burn")]
public class Burn : BuffBase
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
            buffName = "やけど";
        }

        statusEffect = StatusEffect.Burn;
    }

    public override void Apply(Character target)
    {
        if (target == null)
        {
            Debug.LogWarning("BurnBuff適用失敗: ターゲットがnullです");
            return;
        }

        sourceCharacter = target;
        Debug.Log($"{target.charactername} にやけど（毎ターン{damagePerTurn}ダメージ）を適用しました");
    }

    public override void Remove()
    {
        if (sourceCharacter != null)
        {
            Debug.Log($"{sourceCharacter.charactername} からやけどを解除しました");
        }
    }
}