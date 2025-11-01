using UnityEngine;

[CreateAssetMenu(menuName = "RPG/Buffs/TurnChange")]
public class TurnChangeBuff : BuffBase
{
    // 先頭か、後ろかのフラグ
    public bool isFront;
    //対象者削除フラグ
    public bool isBack;
    //先行か後攻かを変更する
    public override void Apply(Character target)
    {
        if (isBack)
            TurnManager.Instance.RemoveCharacterFromTurnList(target);
        else
            TurnManager.Instance.TurnChange(target, isFront ? 0 : TurnManager.Instance.turnList.Count - 1);
    }
    // バフ終了時に元の攻撃力に戻す
    public override void Remove() { }
}