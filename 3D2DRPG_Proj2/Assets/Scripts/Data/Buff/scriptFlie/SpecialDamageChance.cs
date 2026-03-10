using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "RPG/Buffs/SpecialDamageChance")]
public class SpecialDamageChance : BuffBase
{
    // このバフは、一定確率で追加ダメージを与えるバフです。
    public BuffInstance BuffInstance;
    // 追加ダメージ判定を行う対象キャラクター一覧
    private List<Character> targetCharacters = new List<Character>();
    // 発生確率（％）
    [SerializeField,Range(0,100)]
    private int chance;
    public override void Apply(Character target)
    {
        if (target == null)
        {
            Debug.Log("ターゲットが設定されていません");
            return;
            //for(int i = 0; i < TurnManager.Instance.enemys.Count; i++)
            //{
            //    targetCharacters.Add(TurnManager.Instance.enemys[i].GetComponent<Character>());
            //}
        }else
        {
            sourceCharacter= target;
        }
            // バフ効果が発生するかをランダムで判定
            int randamValue = Random.Range(0, 100);
        if (randamValue < chance)
        {
            // 効果が発生した場合の処理
            Debug.Log("SpecialDamageChance buff activated!");
            target.hp -= 10; // 例: 固定値でダメージを与える
                                // ダメージエフェクトを表示（攻撃を受けたターゲットの手前に表示）
                                // 例: エネミー全体に付与する場合はプレイヤー側にもエフェクトを表示するなど
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

        //    // バフ効果が発生するかをランダムで判定
        //    int randamValue = Random.Range(0, 100);
        //    if (randamValue < chance)
        //    {
        //        // 効果が発生した場合の処理
        //        Debug.Log("SpecialDamageChance buff activated!");
        //        character.hp -= 10; // 例: 固定値でダメージを与える
        //        // ダメージエフェクトを表示（攻撃を受けたターゲットの手前に表示）
        //        // 例: エネミー全体に付与する場合はプレイヤー側にもエフェクトを表示するなど
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
    // バフ効果の終了時に呼ばれる
    public override void Remove() {
        if (sourceCharacter == null)
        {
            Debug.Log("SpecialDamageChance buff removed, but sourceCharacter is null.");
            return;
        }

        Debug.Log("SpecialDamageChance buff removed.");

        // CharacterBuffManager.RemoveBuff 経由の場合は既にリストから外すため、ここでは呼ばない
        if (isBeingRemoved)
            return;

        if (currentInstance == null)
        {
            Debug.LogWarning("SpecialDamageChance.Remove: currentInstance が null です。");
            return;
        }

        CharacterBuffManager characterBuffManager = sourceCharacter.GetBuffManager();
        if (characterBuffManager != null)
        {
            characterBuffManager.RemoveBuff(currentInstance);
        }
    }
}