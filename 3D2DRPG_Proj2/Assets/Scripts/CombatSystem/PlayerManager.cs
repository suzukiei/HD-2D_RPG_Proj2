using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using Unity.VisualScripting;
using Unity.Mathematics;

/// <summary>
/// プレイヤーの戦闘行動を管理するクラス
/// </summary>
public class PlayerManager : MonoBehaviour
{
    [SerializeField, Header("UIテスト用")]
    private UITest uiTest;
    [SerializeField, Header("ComboUI")]
    private ComboAttack comboUI;
    [SerializeField, Header("選択用UI")]
    private SkillSelectionUI skillSelectionUI;
    [SerializeField, Header("ターン管理")]
    private TurnManager turnManager;
    [SerializeField, Header("プレイヤーキャラクター一覧")]
    private List<CharacterData> playerCharacters;
    [SerializeField, Header("キャラクター初期配置座標")]
    private List<Vector3> spawnPositions;
    [SerializeField, Header("プレイヤーステータス")]
    private List<PlayerStatusPanel> playerStatusPanel;

    [SerializeField, Header("キャラクター戦闘開始位置")]
    private Vector3 ActionPosition;
    [SerializeField, Header("キャラクター初期位置")]
    private Vector3 StartPosition;
    // キャラクターのGameObject格納用
    private List<GameObject> characterObjects = new List<GameObject>();

    // 現在選択中のキャラクター
    private Character selectedCharacter;
    // 現在選択中のスキル
    private SkillData selectedSkill;
    // 行動待ちフラグ
    private bool isActionPending = false;
    //選択しているエネミー
    private Character selectedEnemy;

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
            // ターン管理にキャラクターを登録
            playerStatusPanel[i].gameObject.SetActive(true);
            PlayerData playerData = new PlayerData(characterObjects[i].GetComponent<Character>());
            playerStatusPanel[i].UpdatePlayerStatus(playerData);
        }
    }

    /// <summary>
    /// 毎フレームの状態管理・行動処理
    /// </summary>
    private void Update()
    {
        // UIのプレイヤーステータス更新s
        PlayerUIUpdate();
        if (!isActionPending) return;

        // キャラクターの状態に応じて処理を分岐
        switch (selectedCharacter.StatusFlag)
        {
            case StatusFlag.Move:
                //初期位置を保存
                StartPosition = selectedCharacter.CharacterObj.transform.position;
                // キャラクターを行動位置に移動
                selectedCharacter.CharacterObj.transform.DOMove(ActionPosition, 1f).OnComplete(() =>
                {
                    selectedCharacter.StatusFlag = StatusFlag.Select;
                    isActionPending = true;
                }); ;
                break;

            case StatusFlag.Select:
                // スキル選択フェーズ
                List<SkillData> skills = new List<SkillData>();
                skills.AddRange(selectedCharacter.skills);
                // UnityEventを作成してコールバックを設定
                UnityEvent<int> callback = new UnityEvent<int>();
                callback.AddListener(OnSkillSelected);
                // 技選択UIを表示
                skillSelectionUI.ShowSkillSelection(skills, callback);
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
                uiTest.Inputs(attackEvent, enemies.Count - 1, enemies);
                break;

            case StatusFlag.Heal:
                // Heel対象ファイズ対象選択フェーズ
                List<Character> characters = new List<Character>();
                foreach (var characterObj in turnManager.players)
                {
                    var characterData = characterObj.GetComponent<Character>();
                    if (characterData != null)
                    {
                        characters.Add(characterData);
                    }
                }

                var healEvent = new UnityEvent<int>();
                healEvent.AddListener((index) => OnHealSelected(characters, index));
                uiTest.Inputs(healEvent, characters.Count - 1, characters);
                break;
            case StatusFlag.Buff:
                // Heel対象ファイズ対象選択フェーズ
                List<Character> buffcharacters = new List<Character>();
                foreach (var characterObj in turnManager.players)
                {
                    var characterData = characterObj.GetComponent<Character>();
                    if (characterData != null)
                    {
                        buffcharacters.Add(characterData);
                    }
                }

                var buffhealEvent = new UnityEvent<int>();
                buffhealEvent.AddListener((index) => OnHealSelected(buffcharacters, index));
                uiTest.Inputs(buffhealEvent, buffcharacters.Count - 1, buffcharacters);
                break;

            case StatusFlag.End:
                selectedCharacter.CharacterObj.transform.DOMove(StartPosition, 1f).OnComplete(() =>
                {
                    selectedCharacter.StatusFlag = StatusFlag.None;
                    // ターン終了処理
                    turnManager.FlagChange();
                }); ;
                break;
        }

        // 行動完了後フラグを下げる
        isActionPending = false;
        //テストコード
        //if(selectedCharacter.StatusFlag == StatusFlag.Move)
        //{
        //    selectedCharacter.StatusFlag = StatusFlag.Select;
        //    isActionPending = true;
        //}
    }
    /// <summary>
    /// UIのプレイヤーステータス更新
    /// </summary>
    public void PlayerUIUpdate()
    {

        for (int i = 0; i < characterObjects.Count; i++)
        {
            PlayerData playerData = new PlayerData(characterObjects[i].GetComponent<Character>());
            playerStatusPanel[i].UpdatePlayerStatus(playerData);
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
        switch (selectedSkill.effectType)
        {
            case SkillEffectType.Attack:
                selectedCharacter.StatusFlag = StatusFlag.Attack;
                if (selectedSkill.targetScope == TargetScope.All)
                    OnAttackSelected(null, 0);
                    break;

            case SkillEffectType.Heal:
                selectedCharacter.StatusFlag = StatusFlag.Heal;
                if (selectedSkill.targetScope == TargetScope.All)
                    OnHealSelected(null, 0);
                break;
            case SkillEffectType.Buff:
                //バフ処理未実装
                selectedCharacter.StatusFlag = StatusFlag.Buff;
                if (selectedSkill.targetScope == TargetScope.All)
                    OnBuffSelected(null,0);
                break;
        }
        isActionPending = true;
    }

    /// <summary>
    /// 攻撃対象選択時のコールバック
    /// </summary>
    private void OnAttackSelected(List<Character> enemies, int index)
    {
        // 全体攻撃スキルの場合、すべての敵に攻撃を適用
        if (selectedSkill.targetScope == TargetScope.All)
        {
            if (selectedCharacter.mp < selectedSkill.mpCost)
            {
                selectedCharacter.StatusFlag = StatusFlag.Select;
                isActionPending = true;
                return;
            }
            foreach (var enemy in enemies)
            {
                ApplyAttack(enemy, selectedSkill);
            }
            selectedCharacter.mp -= selectedSkill.mpCost;
            selectedCharacter.StatusFlag = StatusFlag.End;
            isActionPending = true;
            return;
        }
        if (index < 0 || index >= enemies.Count)
        {
            selectedCharacter.StatusFlag = StatusFlag.Attack;
            isActionPending = true;
            return;
        }
        if (selectedCharacter.mp < selectedSkill.mpCost)
        {
            selectedCharacter.StatusFlag = StatusFlag.Select;
            isActionPending = true;
            return;
        }

        if (selectedSkill.canCombo)
        {
            selectedCharacter.mp -= selectedSkill.mpCost;
            //コンボスキルの処理（未実装）
            var attackEvent = new UnityEvent<int>();
            attackEvent.AddListener((index) => OnComboApplyAttack());
            var attackEnd = new UnityEvent<int>();
            attackEnd.AddListener((index) => OnComboEnd());
            selectedEnemy = enemies[index];
            comboUI.Inputs(attackEvent, attackEnd, selectedSkill.maxcombo, selectedEnemy);
        }
        else
        {
            //通常スキルの処理  
            var enemy = enemies[index];
            ApplyAttack(enemy, selectedSkill);
            selectedCharacter.mp -= selectedSkill.mpCost;
            selectedCharacter.StatusFlag = StatusFlag.End;
            isActionPending = true;
        }

    }

    public void OnComboApplyAttack()
    {
        var enemy = selectedEnemy;
        ApplyAttack(enemy, selectedSkill);
        //selectedCharacter.mp -= selectedSkill.mpCost;
    }

    /// <summary>
    /// 攻撃処理（ダメージ計算・死亡判定）
    /// </summary>
    private void ApplyAttack(Character enemy, SkillData skill)
    {
        if (enemy == null || skill == null) return; // nullチェック追加

        var hp = enemy.hp - skill.power;
        enemy.hp = (int)math.floor(hp);
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

    private void OnComboEnd()
    {
        selectedCharacter.StatusFlag = StatusFlag.End;
        isActionPending = true;
    }
    /// <summary>
    /// 攻撃対象選択時のコールバック
    /// </summary>
    private void OnHealSelected(List<Character> characters, int index)
    {
        if (index < 0 || index >= characters.Count)
        {
            selectedCharacter.StatusFlag = StatusFlag.Heal;
            isActionPending = true;
            return;
        }
        if (selectedCharacter.mp < selectedSkill.mpCost)
        {
            selectedCharacter.StatusFlag = StatusFlag.Select;
            isActionPending = true;
            return;
        }
        if(selectedSkill.targetScope == TargetScope.All)
        {
            //全体回復スキルの処理
            foreach(var getCharacter in characters)
            {
                ApplyHeal(getCharacter, selectedSkill);
            }
            selectedCharacter.mp -= selectedSkill.mpCost;
            selectedCharacter.StatusFlag = StatusFlag.End;
            isActionPending = true;
            return;
        }
        //通常スキルの処理  
        var character = characters[index];
        ApplyHeal(character, selectedSkill);
        selectedCharacter.mp -= selectedSkill.mpCost;
        selectedCharacter.StatusFlag = StatusFlag.End;
        isActionPending = true;
    }
    public void OnBuffSelected(List<Character> characters, int index)
    {
        selectedCharacter.StatusFlag = StatusFlag.End;
        isActionPending = true;
    }

    private void ApplyHeal(Character character, SkillData skill)
    {
        if (character == null || skill == null) return; // nullチェック追加
        var hp = character.hp + skill.power;
        character.hp = (int)math.floor(hp);
        if (character.hp > character.maxHp)
        {
            character.hp = character.maxHp;
        }
    }
}