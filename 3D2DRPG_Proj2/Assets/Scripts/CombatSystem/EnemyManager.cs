using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField]
    private List<CharacterData> enemyData;
    [SerializeField, Header("攻撃方法をランダムにする")]
    private bool AttackRandamFlag;
    [SerializeField]
    private TurnManager turnManager;
    [SerializeField]
    private List<Vector3> vector3s;
    private List<GameObject> enemygameObjects = new List<GameObject>();

    public List<GameObject> GetEnemyData() { return enemygameObjects; }
    private void Awake()
    {
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
    /// Enemy の行動処理（シンプルAI: スキルをランダム選択してプレイヤーを攻撃）
    /// TurnManager から行動するエネミーの Character を受け取る
    /// </summary>
    public void Test(Character actingEnemy)
    {
        // Nullチェック
        if (actingEnemy == null)
        {
            turnManager.FlagChange();
            return;
        }

        // プレイヤー候補を取得
        List<Character> playerCandidates = new List<Character>();
        foreach (var playerObj in turnManager.players)
        {
            if (playerObj == null) continue;
            var ch = playerObj.GetComponent<Character>();
            if (ch != null) playerCandidates.Add(ch);
        }

        if (playerCandidates.Count == 0)
        {
            // 攻撃対象がいない -> ターン終了
            turnManager.FlagChange();
            return;
        }

        // スキル選択（ランダムで非nullなスキルを選ぶ）
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
                chosenSkill = avail[Random.Range(0, avail.Count)];
            }
        }

        // ターゲット選択（現状: HP最小のプレイヤーを狙う。ランダムにしたい場合は Random.Range を使う）
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
            // 保険でランダム
            target = playerCandidates[Random.Range(0, playerCandidates.Count)];
        }

        // 攻撃実行
        ApplyAttack(target, chosenSkill, actingEnemy);

        // 行動終了フラグ処理
        actingEnemy.StatusFlag = StatusFlag.End;
        // ターンを TurnManager に返す
        turnManager.FlagChange();
    }

    /// <summary>
    /// 攻撃処理（ダメージ計算・死亡判定）
    /// </summary>
    private void ApplyAttack(Character target, SkillData skill, Character attacker)
    {
        if (target == null) return;

        int power = 0;
        if (skill != null)
        {
            power = skill.power;
        }
        else
        {
            // スキルが無ければ基本攻撃力を使用（CharacterData の atk を参照する等、必要なら調整）
            power = attacker != null ? attacker.atk : 1;
        }

        target.hp -= power;
        Debug.Log($"{attacker.name} が {target.name} に {power} ダメージ。残りHP: {target.hp}");

        if (target.hp <= 0)
        {
            // エネミー死亡時の処理（未実装）
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
            // GameObject を破壊
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
