using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "RPG/Buffs/SpecialDamageChance")]
public class SpecialDamageChance : BuffBase
{
    // このバフは、攻撃する効果を発動する確率を提供します。
    public BuffInstance BuffInstance;
    //攻撃する効果を発動する確率
    private List<Character> targetCharacters = new List<Character>();
    //発動する確率;
    [SerializeField,Range(0,100)]
    private int chance;
    public override void Apply(Character target)
    {
        if (target == null)
        {
            Debug.Log("ターゲットがいません");
            return;
            //for(int i = 0; i < TurnManager.Instance.enemys.Count; i++)
            //{
            //    targetCharacters.Add(TurnManager.Instance.enemys[i].GetComponent<Character>());
            //}
        }else
        {
            sourceCharacter= target;
        }
            //バフの効果を適用するロジックをここに実装
            int randamValue = Random.Range(0, 100);
        if (randamValue < chance)
        {
            // 効果が発動する場合の処理をここに実装
            Debug.Log("SpecialDamageChance buff activated!");
            target.hp -= 10; // 例: ダメージを与える
                                // ダメージエフェクトを表示（攻撃を受けたターゲットの位置の前に表示）
                                // 注: 敵がプレイヤーを攻撃する場合、プレイヤーの位置にエフェクトを表示します
            if (DamageEffectUI.Instance != null && target.CharacterObj != null)
            {
                DamageEffectUI.Instance.ShowDamageEffectOnEnemy(target.CharacterObj, 10);
            }
        }
        else
        {
            Remove();
        }
        //foreach (Character character in targetCharacters)
        //{
        //    if(character==null)
        //    {
        //        targetCharacters.Remove(character);
        //        continue;
        //    }

        //    // バフの効果を適用するロジックをここに実装
        //    int randamValue = Random.Range(0, 100);
        //    if (randamValue < chance)
        //    {
        //        // 効果が発動する場合の処理をここに実装
        //        Debug.Log("SpecialDamageChance buff activated!");
        //        character.hp -= 10; // 例: ダメージを与える
        //        // ダメージエフェクトを表示（攻撃を受けたターゲットの位置の前に表示）
        //        // 注: 敵がプレイヤーを攻撃する場合、プレイヤーの位置にエフェクトを表示します
        //        if (DamageEffectUI.Instance != null && character.CharacterObj != null)
        //        {
        //            DamageEffectUI.Instance.ShowDamageEffectOnEnemy(character.CharacterObj, 10);
        //        }
        //    }
        //    else
        //    {
        //        targetCharacters.Remove(character);
        //        Debug.Log("SpecialDamageChance buff did not activate.");
        //    }
        //}
    }
    // バフ終了時に元の攻撃力に戻す
    public override void Remove() {
        if(sourceCharacter != null)
        {
            Debug.Log("SpecialDamageChance buff removed.");
            // バフの効果を解除するロジックをここに実装
        }else
        {
            Debug.Log("SpecialDamageChance buff removed, but sourceCharacter is null.");
            CharacterBuffManager characterBuffManager= sourceCharacter.GetBuffManager();
           
            //characterBuffManager.RemoveBuff();


        }
    }
}