using UnityEngine;

/// <summary>
/// スキル封じ（沈黙）バフ
/// ランダムに1つのスキルを使用不可にする
/// </summary>
[CreateAssetMenu(menuName = "RPG/Buffs/Silent")]
public class Silent : BuffBase
{
    private void OnEnable()
    {
        if (string.IsNullOrEmpty(buffId))
        {
            buffId = System.Guid.NewGuid().ToString();
        }
        if (string.IsNullOrEmpty(buffName))
        {
            buffName = "沈黙";
        }

        // statusEffectを設定
        statusEffect = StatusEffect.Silent;
    }

    /// <summary>
    /// バフ適用時の処理
    /// 実際のスキル選択処理はEnemyManager側で行う
    /// </summary>
    public override void Apply(Character target)
    {
        if (target == null)
        {
            Debug.LogWarning("SilentBuff適用失敗: ターゲットがnullです");
            return;
        }

        sourceCharacter = target;
        Debug.Log($"{target.charactername} に沈黙を適用しました");
    }

    /// <summary>
    /// バフ解除時の処理
    /// </summary>
    public override void Remove()
    {
        if (sourceCharacter != null)
        {
            Debug.Log($"{sourceCharacter.charactername} の沈黙が解除されました");
        }
    }
}