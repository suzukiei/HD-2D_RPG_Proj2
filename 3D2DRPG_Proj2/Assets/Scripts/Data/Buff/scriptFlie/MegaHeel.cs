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

    }

    public override void Remove()
    {
        throw new System.NotImplementedException();
    }
}
