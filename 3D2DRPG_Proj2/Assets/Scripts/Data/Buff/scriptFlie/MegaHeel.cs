using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "RPG/Buffs/MegaHeel")]
public class MegaHeel : BuffBase
{
    [SerializeField]
    private int heelMax;
    [SerializeField]
    private int heelMin;
    [SerializeField]
    private int spdUp;
    public override void Apply(Character target)
    {
        int heel=Random.Range(heelMin, heelMax);
        target.hp += heel;
        if (target.hp >= target.maxHp)
            target.hp = target.maxHp;
        target.spd+=spdUp;

        // Remove時に元に戻せるように、適用した速度アップ値を保存
        if (currentInstance != null)
        {
            currentInstance.buffValue = spdUp;
        }
    }

    public override void Remove()
    {
        // バフ終了時に速度を元に戻す
        if (currentInstance != null && currentInstance.targetCharacter != null)
        {
            int spdToRemove = (int)currentInstance.buffValue;
            currentInstance.targetCharacter.spd -= spdToRemove;
            Debug.Log($"{currentInstance.targetCharacter.charactername} の速度が {spdToRemove} 低下（MegaHeelバフ解除）");
        }
    }
}
