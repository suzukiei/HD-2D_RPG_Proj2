using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using Unity.VisualScripting;
using Unity.Mathematics;
using System;

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
    [SerializeField, Header("プレイヤーキャラクターリスト")]
    private List<CharacterData> playerCharacters;
    [SerializeField, Header("キャラクターの生成配置座標")]
    private List<Vector3> spawnPositions;
    [SerializeField, Header("プレイヤーステータスパネル")]
    private List<PlayerStatusPanel> playerStatusPanel;

    [SerializeField, Header("キャラクター戦闘開始位置")]
    private Vector3 ActionPosition;
    [SerializeField, Header("キャラクター開始位置")]
    private Vector3 StartPosition;
    // キャラクターのGameObject保存用
    private List<GameObject> characterObjects = new List<GameObject>();

    // 現在選択中のキャラクター
    private Character selectedCharacter;
    // 現在選択中のスキル
    private SkillData selectedSkill;
    // 行動待機のフラグ
    private bool isActionPending = false;
    //選択している敵
    private Character selectedEnemy;

    //バフの効果を管理する変数
    private List<BuffInstance> activeBuffs = new List<BuffInstance>();

    /// <summary>
    /// キャラクターデータ取得用
    /// </summary>
    public List<GameObject> GetPlayerCharacters() => characterObjects;

    /// <summary>
    /// 初期化処理（キャラクターの配置）
    /// </summary>
    private void Awake()
    {
        if (GameManager.Instance != null && GameManager.Instance.PlayerData.Count!=0)
        {
            playerCharacters.Clear();
            playerCharacters.AddRange(GameManager.Instance.PlayerData);
        }
        isActionPending = false;
        for (int i = 0; i < playerCharacters.Count; i++)
        {
            // キャラクターの座標をセット
            playerCharacters[i].CharacterTransfrom = spawnPositions[i];
            // キャラクターのGameObjectを作成
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
    /// ステータスマシンの状態管理・行動処理
    /// </summary>
    private void Update()
    {
        // UIのプレイヤーステータスパネル更新
        PlayerUIUpdate();
        if (!isActionPending) return;

        // キャラクターの状態に応じて処理を分岐
        switch (selectedCharacter.StatusFlag)
        {
            case StatusFlag.Move:
                //開始位置を保存
                StartPosition = selectedCharacter.CharacterObj.transform.position;
                // キャラクターを行動位置に移動
                selectedCharacter.CharacterObj.transform.DOMove(ActionPosition, 1f).OnComplete(() =>
                {
                    selectedCharacter.StatusFlag = StatusFlag.Select;
                    isActionPending = true;
                }); ;
                break;

            case StatusFlag.Select:
                // スキル選択パネル
                List<SkillData> skills = new List<SkillData>();
                skills.AddRange(selectedCharacter.skills);
                // UnityEventを作成してコールバックを設定
                UnityEvent<int> callback = new UnityEvent<int>();
                callback.AddListener(OnSkillSelected);
                // スキル選択UIを表示
                skillSelectionUI.ShowSkillSelection(skills, callback);
                break;

            case StatusFlag.Attack:
                // 攻撃対象選択パネル
                List<Character> enemies = getEnemy();
                var attackEvent = new UnityEvent<int>();
                attackEvent.AddListener((index) => OnAttackSelected(enemies, index));
                uiTest.Inputs(attackEvent, enemies.Count - 1, enemies);
                break;

            case StatusFlag.Heal:
                // Heal対象選択パネル対象選択パネル
                List<Character> characters = getPlayer();
                var healEvent = new UnityEvent<int>();
                healEvent.AddListener((index) => OnHealSelected(characters, index));
                uiTest.Inputs(healEvent, characters.Count - 1, characters);
                break;
            case StatusFlag.Buff:
                // Heal対象選択パネル対象選択パネル
                switch (selectedSkill.buffEffect.buffRange)
                {
                    case BuffRange.Self:
                        OnBuffSelected(null, 0);
                        break;
                    case BuffRange.AllAllies:
                    case BuffRange.AllEnemies:
                        OnBuffSelected(null, 0);
                        break;
                    case BuffRange.Ally:
                        List<Character> buffcharacters = getPlayer();
                        var buffEvent = new UnityEvent<int>();
                        buffEvent.AddListener((index) => OnBuffSelected(buffcharacters, index));
                        uiTest.Inputs(buffEvent, buffcharacters.Count - 1, buffcharacters);
                        break;
                    case BuffRange.Enemy:
                        List<Character> buffenemies = getEnemy();
                        var buffEvents = new UnityEvent<int>();
                        buffEvents.AddListener((index) => OnBuffSelected(buffenemies, index));
                        uiTest.Inputs(buffEvents, buffenemies.Count - 1, buffenemies);
                        break;
                }
                break;

            case StatusFlag.End:
                //バフ効果の管理
                buffTurnManage();
                // キャラクターを開始位置に戻る
                selectedCharacter.CharacterObj.transform.DOMove(StartPosition, 1f).OnComplete(() =>
                {
                    selectedCharacter.StatusFlag = StatusFlag.None;
                    // ターン処理を終了
                    turnManager.FlagChange();
                }); ;
                break;
        }

        // 行動処理のフラグをリセット
        isActionPending = false;
    }
    /// <summary>
    /// UIのプレイヤーステータスパネル更新
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
    /// プレイヤーの行動開始（外部から呼び出される）
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
                {
                    OnAttackSelected(getEnemy(), 0);
                }
                break;

            case SkillEffectType.Heal:
                selectedCharacter.StatusFlag = StatusFlag.Heal;
                if (selectedSkill.targetScope == TargetScope.All)
                    OnHealSelected(null, 0);
                break;
            case SkillEffectType.Buff:
                selectedCharacter.StatusFlag = StatusFlag.Buff;
                if (selectedSkill.targetScope == TargetScope.All)
                    OnBuffSelected(null, 0);
                break;
        }
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
        if (selectedCharacter.mp < selectedSkill.mpCost)
        {
            selectedCharacter.StatusFlag = StatusFlag.Select;
            isActionPending = true;
            return;
        }
        // 全の攻撃スキルの場合、すべての敵に攻撃を適用
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
        if (selectedSkill.canCombo)
        {
            selectedCharacter.mp -= selectedSkill.mpCost;
            //コンボスキルの処理（成功時）
            Func<int, bool> attackEvent;
            // 修正: OnComboApplyAttackメソッドをラムダ式でラップし、Func<int, bool>型にする
            attackEvent = (comboStep) => OnComboApplyAttack();
            var attackEnd = new UnityEvent<int>();
            attackEnd.AddListener((index) => OnComboEnd());
            selectedEnemy = enemies[index];
            comboUI.AttackTiming(selectedSkill.timingWindowStart, selectedSkill.timingWindowEnd);
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

    public bool OnComboApplyAttack()
    {
        var enemy = selectedEnemy;
        var enemysurvival =ApplyAttack(enemy, selectedSkill);
        return enemysurvival;
        //selectedCharacter.mp -= selectedSkill.mpCost;
    }

    /// <summary>
    /// 攻撃処理（ダメージ計算・撃破処理）
    /// true;敵が残っている、false:敵が撃破された
    /// </summary>
    private bool ApplyAttack(Character enemy, SkillData skill)
    {
        if (enemy == null || skill == null) return true; // nullチェック追加

        //ダメージ乱数
        float random = UnityEngine.Random.Range(10, 20);
        random = random / 10;
        Debug.Log("乱数:" + random);
        //基本ダメージ計算
        var damage = selectedCharacter.atk * random;
        //最終計算
        var finalDamage = damage * skill.power - enemy.def;
        var hp = enemy.hp - finalDamage;
        enemy.hp = (int)math.floor(hp);

        // ダメージエフェクトを表示（敵の位置の前に表示）
        if (DamageEffectUI.Instance != null && enemy.CharacterObj != null)
        {
            DamageEffectUI.Instance.ShowDamageEffectOnEnemy(enemy.CharacterObj, finalDamage);
        }

        if (enemy.hp <= 0)
        {
            // エネミーが撃破された処理（成功時）
            //エネミーの体力を0にする
            enemy.hp = 0;
            turnManager.enemys.Remove(enemy.gameObject);
            turnManager.turnList.Remove(enemy.gameObject);
            //エネミーのGameObjectを削除
            Destroy(enemy.CharacterObj);
            return false;
        }
        else
        {
            return true;
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
        if (selectedSkill.targetScope == TargetScope.All)
        {
            //全の回復スキルの処理
            foreach (var getCharacter in characters)
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
        if (index < 0 || index >= characters.Count)
        {
            selectedCharacter.StatusFlag = StatusFlag.Buff;
            isActionPending = true;
            return;
        }
        if (selectedCharacter.mp < selectedSkill.mpCost)
        {
            selectedCharacter.StatusFlag = StatusFlag.Select;
            isActionPending = true;
            return;
        }
        //通常スキルの処理
        var character = characters[index];
        BuffInstance buff = new BuffInstance(selectedSkill.buffEffect);
        buff.remainingTurns = selectedSkill.buffDuration;
        buffApply(buff, character);
        selectedCharacter.mp -= selectedSkill.mpCost;
        selectedCharacter.StatusFlag = StatusFlag.End;
        isActionPending = true;
    }
    /// <summary>
    /// 回復処理
    /// </summary>
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
    /// <summary>
    /// 攻撃対象選択パネルの敵キャラクター取得
    /// </summary>
    private List<Character> getEnemy()
    {
        // 攻撃対象選択パネル
        List<Character> enemies = new List<Character>();
        foreach (var enemyObj in turnManager.enemys)
        {
            var characterData = enemyObj.GetComponent<Character>();
            if (characterData != null)
            {
                enemies.Add(characterData);
            }
        }
        return enemies;
    }
    /// <summary>
    /// 攻撃対象選択パネルの味方キャラクター取得
    /// </summary>
    private List<Character> getPlayer()
    {
        // 攻撃対象選択パネル
        List<Character> players = new List<Character>();
        foreach (var playerObj in turnManager.players)
        {
            var characterData = playerObj.GetComponent<Character>();
            if (characterData != null)
            {
                players.Add(characterData);
            }
        }
        return players;
    }

    //バフ効果の適用
    private void buffApply(BuffInstance buff, Character target)
    {
        switch (buff.buffRange)
        {
            case BuffRange.Self:
                target = selectedCharacter;
                buff.Apply(target);
                activeBuffs.Add(buff);
                break;
            case BuffRange.Ally:
            case BuffRange.Enemy:
                //単一の選択対象(この場合はtargetに対応)
                buff.Apply(target);
                activeBuffs.Add(buff);
                break;
            case BuffRange.AllAllies:
                var players = getPlayer();
                foreach (var player in players)
                {
                    buff.Apply(player);
                    activeBuffs.Add(buff);
                }
                break;
            case BuffRange.AllEnemies:
                var enemies = getEnemy();
                foreach (var enemy in enemies)
                {
                    buff.Apply(enemy);
                    activeBuffs.Add(buff);
                }
                break;
        }

    }
    //バフ効果の解除
    private void buffRemove(BuffInstance buff)
    {
        buff.Remove();
        activeBuffs.Remove(buff);
    }
    //バフの効果ターン管理
    private void buffTurnManage()
    {
        //バフ効果ターンがあるかどうかを判定
        for (int activeBuffCount = activeBuffs.Count - 1; activeBuffCount >= 0; activeBuffCount--)
        {
            BuffInstance buff = activeBuffs[activeBuffCount];
            buff.TickTurn();
            if (buff.IsExpired())
            {
                //バフ効果終了
                buffRemove(buff);
            }
        }
        //バフ効果ターン処理終了
    }
}
