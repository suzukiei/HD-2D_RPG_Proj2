using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using DG.Tweening;

public class EnemyManager : MonoBehaviour
{
    [SerializeField]
    private List<CharacterData> enemyData;
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

    public List<GameObject> GetEnemyData() { return enemygameObjects; }
    private void Awake()
    {
        if (GameManager.Instance != null && GameManager.Instance.EnemyData.Count!=0)
        {
            enemyData.Clear();
            enemyData.AddRange(GameManager.Instance.EnemyData);
        }
        for (int i = 0; i < enemyData.Count; i++)
        {
            enemyData[i].CharacterTransfrom = vector3s[i];
            var obj = Instantiate(enemyData[i].CharacterObj, vector3s[i] * 2, Quaternion.identity);
            obj.AddComponent<Character>().init(enemyData[i]);
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
    /// Enemy の行動処理（優先度AI: スキルがあれば使用、なければプレイヤーを攻撃）
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

        // 初期位置を保存
        Vector3 originalPosition = actingEnemy.transform.position;
        Vector3 forwardPosition = originalPosition + Vector3.forward * forwardDistance;

        // プレイヤー候補の取得
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
            // ランダムに選択
            target = playerCandidates[UnityEngine.Random.Range(0, playerCandidates.Count)];
        }

        // 1. 前に出るアニメーション
        Tween forwardTween = actingEnemy.transform.DOMove(forwardPosition, forwardDuration)
            .SetEase(Ease.OutQuad);
        yield return forwardTween.WaitForCompletion();

        // 2. 考える時間を待つ
        yield return new WaitForSeconds(thinkingTime);

        // 3. 攻撃処理を実行
        ApplyAttack(target, chosenSkill, actingEnemy);

        // 4. 元の位置に戻るアニメーション
        Tween returnTween = actingEnemy.transform.DOMove(originalPosition, returnDuration)
            .SetEase(Ease.InQuad);
        yield return returnTween.WaitForCompletion();

        // 5. ステータスフラグを終了に
        actingEnemy.StatusFlag = StatusFlag.End;
        // 6. ターンを TurnManager に通知
        turnManager.FlagChange();
    }

    /// <summary>
    /// 攻撃処理（ダメージ計算と死亡判定）
    /// </summary>
    private void ApplyAttack(Character target, SkillData skill, Character attacker)
    {
        if (target == null) return;

        float power = 0;
        if (skill != null)
        {
            power = skill.power;
        }
        else
        {
            // スキルがない場合は通常攻撃を使用（CharacterData の atk を参照するが、必要に応じて）
            power = attacker != null ? attacker.atk : 1;
        }

        var targethp = target.hp - power;
        target.hp = (int)math.floor(targethp);
        Debug.Log($"{attacker.name} が {target.name} に {power} ダメージ。残りHP: {target.hp}");

        // ダメージエフェクトを表示（攻撃を受けたターゲットの位置の前に表示）
        // 注: 敵がプレイヤーを攻撃する場合、プレイヤーの位置にエフェクトが表示されます
        if (DamageEffectUI.Instance != null && target.CharacterObj != null)
        {
            DamageEffectUI.Instance.ShowDamageEffectOnEnemy(target.CharacterObj, power);
        }

        if (target.hp <= 0)
        {
            // プレイヤーが死亡した（HPを0に）
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
}
