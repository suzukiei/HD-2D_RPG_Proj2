using UnityEngine;

[CreateAssetMenu(menuName = "RPG/Buffs/AttackUp")]
public class AttackUpBuff : BuffBase
{
    // 攻撃力倍率
    public float attackMultiplier = 1.5f;
    // バフ前の攻撃力を保存する変数
    public int originalAttack;

    // バフ適用時に攻撃力を増加
    public override void Apply(Character target)
    {
        //キャラクターステータスを取得
        sourceCharacter = target;
        // 元の攻撃力を保存
        originalAttack = target.atk;
        // 攻撃力を増加
        target.atk = (int)(target.atk * attackMultiplier);
    }

    // バフ終了時に元の攻撃力に戻す
    public override void Remove()
    {
        // キャラクターの攻撃力を元に戻す
        sourceCharacter.atk = originalAttack;
    }
}
