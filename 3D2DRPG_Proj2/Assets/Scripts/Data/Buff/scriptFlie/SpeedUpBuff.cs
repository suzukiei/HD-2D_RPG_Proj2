using UnityEngine;

/// <summary>
/// 速度上昇バフ
/// 直接ステータスを変更せず、CharacterBuffManagerを通じて効果を適用
/// </summary>
[CreateAssetMenu(menuName = "RPG/Buffs/SpeedUp")]
public class SpeedUpBuff : BuffBase
{
    private void OnEnable()
    {
        // デフォルト値の設定
        if (string.IsNullOrEmpty(buffId))
        {
            buffId = System.Guid.NewGuid().ToString();
        }
        if (buffType == BuffType.StatusEnhancement && string.IsNullOrEmpty(buffName))
        {
            buffName = "速度上昇";
        }
    }
    [Header("速度倍率")]
    [Tooltip("速度を何倍にするか（例: 1.5 = 50%上昇）")]
    public float speedMultiplier = 1.5f;

    /// <summary>
    /// バフ適用時の処理
    /// 注意: このメソッドは直接ステータスを変更しません
    /// CharacterBuffManagerがGetEffectiveSpeed()でこの倍率を適用します
    /// </summary>
    public override void Apply(Character target)
    {
        if (target == null)
        {
            Debug.LogWarning("SpeedUpBuff適用失敗: ターゲットがnullです");
            return;
        }
        
        sourceCharacter = target;
        
        // 直接ステータスを変更しない
        // CharacterBuffManagerのGetEffectiveSpeed()で倍率が適用される
        Debug.Log($"{target.charactername} に速度 {speedMultiplier}倍 のバフを適用しました");
    }

    /// <summary>
    /// バフ解除時の処理
    /// 注意: 直接ステータスを変更していないため、特別な処理は不要
    /// CharacterBuffManagerがバフリストから削除することで自動的に効果が無効化されます
    /// </summary>
    public override void Remove()
    {
        if (sourceCharacter != null)
        {
            Debug.Log($"{sourceCharacter.charactername} から速度バフを解除しました");
        }
        // 直接ステータスを変更していないため、復元処理は不要
    }
}
