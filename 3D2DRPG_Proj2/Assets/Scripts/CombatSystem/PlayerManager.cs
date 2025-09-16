using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// プレイヤーの戦闘行動を管理するクラス
/// </summary>
public class PlayerManager : MonoBehaviour
{
    [SerializeField, Header("UIテスト用")]
    private UITest uiTest;
    [SerializeField, Header("ターン管理")]
    private TurnManager turnManager;
    [SerializeField, Header("プレイヤーキャラクター一覧")]
    private List<CharacterData> playerCharacters;
    [SerializeField, Header("キャラクター初期配置座標")]
    private List<Vector3> spawnPositions;

    // キャラクターのGameObject格納用
    private List<GameObject> characterObjects = new List<GameObject>();

    // 現在選択中のキャラクター
    private Character selectedCharacter;
    // 現在選択中のスキル
    private SkillData selectedSkill;
    // 行動待ちフラグ
    private bool isActionPending = false;

    /// <summary>
    /// キャラクターデータ取得用
    /// </summary>
    public List<GameObject> GetPlayerCharacters() => characterObjects;

    /// <summary>
    /// 初期化処理（キャラクターの配置）
    /// </summary>
    private void Awake()
    {
        isActionPending = false;
        for (int i = 0; i < playerCharacters.Count; i++)
        {
            // キャラクターの座標情報をセット
            playerCharacters[i].CharacterTransfrom = spawnPositions[i];
            // キャラクターのGameObjectを生成
            var obj = Instantiate(playerCharacters[i].CharacterObj, spawnPositions[i], Quaternion.identity);
            obj.AddComponent<Character>().init(playerCharacters[i]);
            obj.transform.parent = transform;
            characterObjects.Add(obj);
        }
    }

    /// <summary>
    /// 毎フレームの状態管理・行動処理
    /// </summary>
    private void Update()
    {
        if (!isActionPending) return;

        // キャラクターの状態に応じて処理を分岐
        switch (selectedCharacter.StatusFlag)
        {
            case StatusFlag.Move:
                // 移動アニメーション処理（未実装）

                break;

            case StatusFlag.Select:
                // スキル選択フェーズ
                var skills = selectedCharacter.skills;
                var selectEvent = new UnityEvent<int>();
                selectEvent.AddListener(OnSkillSelected);
                uiTest.Inputs(selectEvent, skills.Length-1);
                break;

            case StatusFlag.Attack:
                // 攻撃対象選択フェーズ
                List<Character> enemies = new List<Character>();
                foreach (var enemyObj in turnManager.enemys)
                {
                    var characterData = enemyObj.GetComponent<Character>();
                    if (characterData != null)
                    {
                        enemies.Add(characterData);
                    }
                }

                var attackEvent = new UnityEvent<int>();
                attackEvent.AddListener((index) => OnAttackSelected(enemies, index));
                uiTest.Inputs(attackEvent, enemies.Count-1);
                break;

            case StatusFlag.End:
                selectedCharacter.StatusFlag = StatusFlag.None;
                // ターン終了処理
                turnManager.FlagChange();
                break;
        }

        // 行動完了後フラグを下げる
        isActionPending = false;
        //テストコード
        if(selectedCharacter.StatusFlag == StatusFlag.Move)
        {
            selectedCharacter.StatusFlag = StatusFlag.Select;
            isActionPending = true;
        }
    }

    /// <summary>
    /// プレイヤーの行動開始（外部から呼び出し）
    /// </summary>
    public void StartPlayerAction(Character character)
    {
        selectedCharacter = character;
        selectedCharacter.StatusFlag = StatusFlag.Move;
        isActionPending = true;
    }

    /// <summary>
    /// スキル選択時のコールバック
    /// </summary>
    private void OnSkillSelected(int index)
    {
        if (index < 0 || index >= selectedCharacter.skills.Length)
        {
            selectedCharacter.StatusFlag = StatusFlag.Select;
            isActionPending = true;
            return;
        }

        if (selectedCharacter.skills[index] == null)// nullチェック追加
        {
            selectedCharacter.StatusFlag = StatusFlag.Select;
            isActionPending = true;
            return;
        } 
        selectedSkill = selectedCharacter.skills[index];
        selectedCharacter.StatusFlag = StatusFlag.Attack;
        isActionPending = true;
    }

    /// <summary>
    /// 攻撃対象選択時のコールバック
    /// </summary>
    private void OnAttackSelected(List<Character> enemies, int index)
    {
        if (index < 0 || index >= enemies.Count)
        {
            selectedCharacter.StatusFlag = StatusFlag.Attack;
            isActionPending = true;
            return;
        }
        var enemy = enemies[index];
        ApplyAttack(enemy, selectedSkill);
        selectedCharacter.StatusFlag = StatusFlag.End;
        isActionPending = true;
    }

    /// <summary>
    /// 攻撃処理（ダメージ計算・死亡判定）
    /// </summary>
    private void ApplyAttack(Character enemy, SkillData skill)
    {
        if (enemy == null || skill == null) return; // nullチェック追加

        enemy.hp -= skill.power;
        if (enemy.hp <= 0)
        {
            // エネミー死亡時の処理（未実装）
            //エネミーの体力を0にする
            enemy.hp = 0;
            turnManager.enemys.Remove(enemy.gameObject);
            turnManager.turnList.Remove(enemy.gameObject);
            //エネミーのGameObjectを破壊する
            Destroy(enemy.CharacterObj);

        }
    }
}