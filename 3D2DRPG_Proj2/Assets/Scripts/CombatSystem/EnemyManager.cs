using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Animations;

public class EnemyManager : MonoBehaviour
{
    [SerializeField]
    public List<CharacterData> enemyData;
    [SerializeField, Header("敵の攻撃ランダムフラグ")]
    private bool AttackRandamFlag;
    [SerializeField]
    private TurnManager turnManager;
    [SerializeField]
    private List<Vector3> vector3s;
    [SerializeField, Header("前に出る距離")]
    private float forwardDistance = 2f;
    [SerializeField, Header("前に出る時間")]
    private float forwardDuration = 0.5f;
    [SerializeField, Header("考える時間")]
    private float thinkingTime = 1.5f;
    [SerializeField, Header("戻る時間")]
    private float returnDuration = 0.5f;
    private List<GameObject> enemygameObjects = new List<GameObject>();
    [SerializeField]
    public Animator enemyAnimator;

    public List<GameObject> GetEnemyData() { return enemygameObjects; }

    //public enum StatusEffect
    //{
    //    Poison,
    //    Stun,
    //    Burn,
    //    Freeze,
    //    Sleep,
    //    Silent,
    //    DamageUp,
    //    TurnChange,
    //    DefenceUp,
    //    SpdDown,
    //    SpdUp,
    //    MagicDamageDown,
    //    MagicCounter,
    //    Makituki,
    //    Zouen,
    //} //毒、スタン、やけど、凍結、眠り、魔封,ダメ増,ターンチェンジ,防御UP,スピードUP,スピードDN,マジックダメDN,反射,巻きつき,増援

    private void Awake()
    {
        if (GameManager.Instance != null && GameManager.Instance.EnemyData.Count != 0)
        {
            enemyData.Clear();
            enemyData.AddRange(GameManager.Instance.EnemyData);
        }
        for (int i = 0; i < enemyData.Count; i++)
        {
            enemyData[i].CharacterTransfrom = vector3s[i];
            var obj = Instantiate(enemyData[i].CharacterObj, vector3s[i] * 2, Quaternion.identity);
            obj.AddComponent<Character>().init(enemyData[i]);
            obj.transform.localRotation = Quaternion.Euler(0, -90, 0);
            obj.transform.parent = this.gameObject.transform;
            enemygameObjects.Add(obj);
        }
    }
    public void Test()
    {
        //EnemyChange
        turnManager.FlagChange();
    }

    /// <summary>
    /// Enemy の行動処理（簡単AI: スキルがあれば使用、なければプレイヤーを攻撃）
    /// TurnManager から呼び出される。プレイヤーの Character を攻撃する
    /// </summary>
    public void Test(Character actingEnemy)
    {
        // Nullチェック
        if (actingEnemy == null)
        {
            turnManager.FlagChange();
            return;
        }

        // エネミーのターン処理を開始（コルーチンで実行）
        StartCoroutine(EnemyTurnSequence(actingEnemy));
    }

    /// <summary>
    /// エネミーのターンシーケンス（前に出る→考える→攻撃→戻る）
    /// </summary>
    private IEnumerator EnemyTurnSequence(Character actingEnemy)
    {
        if (actingEnemy == null || actingEnemy.gameObject == null)
        {
            turnManager.FlagChange();
            yield break;
        }

        // 元の位置を保存
        Vector3 originalPosition = actingEnemy.transform.position;
        Vector3 forwardPosition = originalPosition + Vector3.forward * forwardDistance;

        // プレイヤーの取得
        List<Character> playerCandidates = new List<Character>();
        foreach (var playerObj in turnManager.players)
        {
            if (playerObj == null) continue;
            var ch = playerObj.GetComponent<Character>();
            if (ch != null) playerCandidates.Add(ch);
        }

        if (playerCandidates.Count == 0)
        {
            // プレイヤーがいない -> ターン終了
            turnManager.FlagChange();
            yield break;
        }

        // スキル選択（使用可能なスキルがあれば選択、なければnullでスキルなし）
        SkillData chosenSkill = null;
        if (actingEnemy.skills != null && actingEnemy.skills.Length > 0)
        {
            List<SkillData> avail = new List<SkillData>();
            foreach (var s in actingEnemy.skills)
            {
                if (s != null) avail.Add(s);
            }
            if (avail.Count > 0)
            {
                chosenSkill = avail[UnityEngine.Random.Range(0, avail.Count)];
            }
        }

        // ターゲット選択（優先: HPが低いプレイヤーを優先。ランダムフラグがtrueなら Random.Range を使用）
        Character target = null;
        int minHp = int.MaxValue;
        if (!AttackRandamFlag)
        {
            foreach (var p in playerCandidates)
            {
                if (p == null) continue;
                if (p.hp < minHp)
                {
                    minHp = p.hp;
                    target = p;
                }
            }
        }

        if (target == null)
        {
            if (chosenSkill.targetScope == TargetScope.Single)
            {
                // ランダムに選択
                target = playerCandidates[UnityEngine.Random.Range(0, playerCandidates.Count)];
            }

        }

        // 1. 前に出る、トゥイーンアニメーション
        Tween forwardTween = actingEnemy.transform.DOMove(forwardPosition, forwardDuration)
            .SetEase(Ease.OutQuad);
        yield return forwardTween.WaitForCompletion();
        // 攻撃アニメーション再生
        enemyAnimator = actingEnemy.EnemyAnimator;

        // 2. 考える時間を待つ
        yield return new WaitForSeconds(thinkingTime);
        if (enemyAnimator != null)
            enemyAnimator.SetTrigger("Attack");
        yield return new WaitForSeconds(0.5f);
        // 3. 攻撃処理を実行
        if (chosenSkill != null && chosenSkill.effectType == SkillEffectType.Buff)
        {
            // バフスキルの処理
            ApplyBuff(chosenSkill, actingEnemy);
        }
        else if (chosenSkill.targetScope == TargetScope.Single)
        {
            ApplyAttack(target, chosenSkill, actingEnemy);
        }
        else
        {
            ApplyAttack(playerCandidates, chosenSkill, actingEnemy);
        }
        // 4. 元の位置に戻る、トゥイーンアニメーション
        Tween returnTween = actingEnemy.transform.DOMove(originalPosition, returnDuration)
            .SetEase(Ease.InQuad);
        yield return returnTween.WaitForCompletion();

        // 5. ステータスフラグを終了に
        actingEnemy.StatusFlag = StatusFlag.End;
        // 6. ターンを TurnManager に通知
        turnManager.FlagChange();
    }

    /// <summary>
    /// 個別攻撃処理（ダメージ計算と撃破処理）
    /// </summary>
    private void ApplyAttack(Character target, SkillData skill, Character attacker)
    {
        if (target == null) return;

        float power = 0;
        float AddDamageBonusPower = 0f;
        if (skill != null)
        {
            if (skill.DamageBonusFlg == true)
            {
                //ダメージボーナスの処理
                float rndDB = UnityEngine.Random.Range(1f, attacker.atk + 1);
                AddDamageBonusPower = skill.power + rndDB;

                power = AddDamageBonusPower;
            }
            else
            {
                power = skill.power;
            }

        }
        else
        {
            // スキルがない場合は通常攻撃を使用（CharacterData の atk を参照するが、必要に応じて）
            power = attacker != null ? attacker.atk : 1;
        }

        var targethp = target.hp - power;
        target.hp = (int)math.floor(targethp);

        //スキルにバフがあるなら適用させる。
        if (skill != null && skill.buffEffect != null && skill.buffEffect.Count > 0)
        {
            foreach (var buffEffect in skill.buffEffect)
            {
                if (buffEffect == null) continue;

                BuffInstance buff = new BuffInstance(buffEffect);
                buff.remainingTurns = skill.buffDuration;

                switch (buffEffect.statusEffect)
                {
                    case StatusEffect.Poison:
                        break;
                    case StatusEffect.Stun:
                        break;
                    case StatusEffect.Burn:
                        break;
                    case StatusEffect.Freeze:
                        break;
                    case StatusEffect.Sleep:
                        break;
                    //case StatusEffect.Silent:
                    //    // ランダムに1つスキルを選んで封じる
                    //    if (target.skills != null && target.skills.Length > 0)
                    //    {
                    //        List<int> availableSkills = new List<int>();
                    //        for (int i = 0; i < target.skills.Length; i++)
                    //        {
                    //            if (target.skills[i] != null) availableSkills.Add(i);
                    //        }

                    //        if (availableSkills.Count > 0)
                    //        {
                    //            buff.LockSkillIndex = availableSkills[UnityEngine.Random.Range(0, availableSkills.Count)];
                    //            Debug.Log($"{target.charactername}のスキル{buff.LockSkillIndex}番が封じられた");
                    //        }
                    //    }
                    //    break;
                    case StatusEffect.DamageUp:
                        break;
                    case StatusEffect.TurnChange:
                        break;
                    case StatusEffect.DefenceUp:
                        break;
                    case StatusEffect.SpdDown:
                        break;
                    case StatusEffect.SpdUp:
                        break;
                    case StatusEffect.MagicDamageDown:
                        break;
                    case StatusEffect.MagicCounter:
                        break;
                    case StatusEffect.Makituki:
                        break;
                    case StatusEffect.Zouen:
                        break;
                    default:
                        break;
                }
                target.ApplyBuff(buff, attacker);
            }
        }
        Debug.Log($"{attacker.name} が {target.name} に {power} ダメージ。残りHP: {target.hp}");

        // ダメージエフェクトを表示（攻撃を受けたターゲットの位置の前に表示）
        // 注: 敵がプレイヤーを攻撃する場合、プレイヤーの位置にエフェクトを表示します
        if (DamageEffectUI.Instance != null && target.CharacterObj != null)
        {
            DamageEffectUI.Instance.ShowDamageEffectOnEnemy(target.CharacterObj, power);
        }

        if (target.hp <= 0)
        {
            // プレイヤーが撃破された（HPが0に）
            target.hp = 0;
            // リストから削除
            if (turnManager.players.Contains(target.gameObject))
            {
                turnManager.players.Remove(target.gameObject);
            }
            if (turnManager.turnList.Contains(target.gameObject))
            {
                turnManager.turnList.Remove(target.gameObject);
            }
            // GameObject を削除
            if (target.CharacterObj != null)
            {
                Destroy(target.CharacterObj);
            }
            else
            {
                // fallback: Destroy target.gameObject
                Destroy(target.gameObject);
            }
        }
    }

    /// <summary>
    /// 全体攻撃処理（ダメージ計算と撃破処理）
    /// </summary>
    private void ApplyAttack(List<Character> target, SkillData skill, Character attacker)
    {
        if (target == null) return;

        float power = 0;
        float AddDamageBonusPower = 0f;
        if (skill != null)
        {
            if (skill.DamageBonusFlg == true)
            {
                //ダメージボーナスの処理
                float rndDB = UnityEngine.Random.Range(1f, attacker.atk + 1);
                AddDamageBonusPower = skill.power + rndDB;

                power = AddDamageBonusPower;
            }
            else
            {
                power = skill.power;
            }

        }

        //各キャラに全体攻撃、耐性を含んだ計算は未実装。
        foreach (Character chara in target)
        {
            var targethp = chara.hp - power;
            chara.hp = (int)math.floor(targethp);
            Debug.Log($"{attacker.name} が {chara.name} に {power} ダメージ。残りHP: {chara.hp}");

            // ダメージエフェクトを表示（攻撃を受けたターゲットの位置の前に表示）
            // 注: 敵がプレイヤーを攻撃する場合、プレイヤーの位置にエフェクトを表示します
            if (DamageEffectUI.Instance != null && chara.CharacterObj != null)
            {
                DamageEffectUI.Instance.ShowDamageEffectOnEnemy(chara.CharacterObj, power);
            }

            if (chara.hp <= 0)
            {
                // プレイヤーが撃破された（HPが0に）
                chara.hp = 0;
                // リストから削除
                if (turnManager.players.Contains(chara.gameObject))
                {
                    turnManager.players.Remove(chara.gameObject);
                }
                if (turnManager.turnList.Contains(chara.gameObject))
                {
                    turnManager.turnList.Remove(chara.gameObject);
                }
                // GameObject を削除
                if (chara.CharacterObj != null)
                {
                    Destroy(chara.CharacterObj);
                }
                else
                {
                    // fallback: Destroy target.gameObject
                    Destroy(chara.gameObject);
                }
            }
        }

    }

    private void ApplyBuff(SkillData skill, Character target)
    {
        if (skill == null || skill.buffEffect == null || skill.buffEffect.Count == 0)
            return;

        foreach (var buffBase in skill.buffEffect)
        {
            BuffInstance buff = new BuffInstance(buffBase);
            buff.remainingTurns = skill.buffDuration;
            buff.buffValue = skill.buffValue;

            // statusEffectで大きく分岐（状態異常の種類ごと）
            switch (buffBase.statusEffect)
            {   //魔法ダメ半減
                case StatusEffect.MagicDamageDown:

                    switch (buffBase.buffRange)
                    {
                        case BuffRange.Self:
                            // 自分自身にバフ
                            target.ApplyBuff(buff, target);
                            break;

                        case BuffRange.Ally:
                            // 味方1人にバフ（ランダムまたは条件選択）
                            var allies = GetAllies(); // 味方リスト取得
                            if (allies.Count > 0)
                            {
                                var ally = allies[UnityEngine.Random.Range(0, allies.Count)];
                                ally.ApplyBuff(buff, target);
                            }
                            break;

                        case BuffRange.AllAllies:
                            // 全味方にバフ
                            var allAllies = GetAllies();
                            foreach (var ally in allAllies)
                            {
                                BuffInstance allyBuff = new BuffInstance(buffBase);
                                allyBuff.remainingTurns = skill.buffDuration;
                                allyBuff.buffValue = skill.buffValue;
                                ally.ApplyBuff(allyBuff, target);
                            }
                            break;                     
                    }
                  break;

                case StatusEffect.Silent:
                    // 敵1人にデバフ
                    var enemies = GetPlayers(); // プレイヤーリスト取得
                    if (enemies.Count > 0)
                    {
                        var enemy = enemies[UnityEngine.Random.Range(0, enemies.Count)];

                        // スキル封じ処理
                        if (enemy.skills != null && enemy.skills.Length > 0)
                        {
                            var availableSkills = new List<int>();
                            for (int i = 0; i < enemy.skills.Length; i++)
                            {
                                if (enemy.skills[i] != null) availableSkills.Add(i);
                            }
                            if (availableSkills.Count > 0)
                            {
                                buff.LockSkillIndex = availableSkills[UnityEngine.Random.Range(0, availableSkills.Count)];
                            }
                        }

                        enemy.ApplyBuff(buff, target);
                    }
                    break;
            }
        }
    }

    //敵の味方リストを取得する。
    private List<Character> GetAllies()
    {
        List<Character> allies = new List<Character>();
        foreach (var enemyObj in turnManager.enemys)
        {
            if (enemyObj == null) continue;
            var ch = enemyObj.GetComponent<Character>();
            if (ch != null) allies.Add(ch);
        }
        return allies;
    }

    //味方サイドの味方リストを取得する。
    private List<Character> GetPlayers()
    {
        List<Character> players = new List<Character>();
        foreach (var playerObj in turnManager.players)
        {
            if (playerObj == null) continue;
            var ch = playerObj.GetComponent<Character>();
            if (ch != null) players.Add(ch);
        }
        return players;
    }
}
