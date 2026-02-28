using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using Unity.VisualScripting;
using Unity.Mathematics;
using System;
using System.Collections;

//バフ効果
public enum buffEffect
{
    damegeDown,
    SkillnotUse,
}
//バフの効果を管理するクラス
[Serializable]
public class CharacterBuff
{
    public buffEffect effect;  // バフの効果の種類
    public BuffBase buffBase;  // バフの基本データ(使うか不明）
    public List<Character> target;　// バフの対象キャラクター
    public int remainingTurns;  // バフの残りターン数
}

/// <summary>
/// プレイヤーの戦闘行動を管理するクラス
/// </summary>
public class PlayerManager : MonoBehaviour
{
    #region 関数宣言
    [SerializeField, Header("UIテスト用")]
    private UITest uiTest;  // UIテスト用の参照
    [SerializeField, Header("ComboUI")]
    private ComboAttack comboUI;   // コンボ攻撃UIの参照
    [SerializeField, Header("選択用UI")]
    private SkillSelectionUI skillSelectionUI; // スキル選択UIの参照
    [SerializeField, Header("ターン管理")]
    private TurnManager turnManager; // ターン管理の参照
    [SerializeField, Header("プレイヤーキャラクターリスト")]
    private List<CharacterData> playerCharacters; // プレイヤーキャラクターデータのリスト
    [SerializeField, Header("キャラクターの生成配置座標")]
    private List<Vector3> spawnPositions; // キャラクターの生成配置座標のリスト
    [SerializeField, Header("プレイヤーステータスパネル")]
    private List<PlayerStatusPanel> playerStatusPanel; // プレイヤーステータスパネルのリスト

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
    //コンボ数のcount
    private int comboCount = 0;

    //Buffの管理用リスト
    public List<CharacterBuff>　characterBuffs = new List<CharacterBuff>();

    /// <summary>
    /// キャラクターデータ取得用
    /// </summary>
    public List<GameObject> GetPlayerCharacters() => characterObjects;

    #endregion

    #region 初期化と更新処理
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
    /// プレイヤーの行動開始（外部から呼び出される）
    /// </summary>
    public void StartPlayerAction(Character character)
    {

        selectedCharacter = character;
        selectedCharacter.StatusFlag = StatusFlag.Move;
        isActionPending = true;
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
        PlayerUpdate();
        // 行動処理のフラグをリセット
        isActionPending = false;
    }

    /// <summary>
    /// UIのプレイヤーステータスパネル更新
    /// </summary>
    private void PlayerUIUpdate()
    {
        for (int i = 0; i < characterObjects.Count; i++)
        {
            PlayerData playerData = new PlayerData(characterObjects[i].GetComponent<Character>());
            playerStatusPanel[i].UpdatePlayerStatus(playerData);
        }
    }

    /// <summary>
    /// キャラクターの状態に応じて処理を分岐
    /// </summary>
    private void PlayerUpdate()
    {
        switch (selectedCharacter.StatusFlag)
        {
            case StatusFlag.Move:
                PlayerMove();
                break;
            case StatusFlag.Select:
                PlayerSelect();
                break;
            case StatusFlag.Attack:
                PlayerAttackSelect();
                break;
            case StatusFlag.Heal:
                PlayerHealSelect();
                break;
            case StatusFlag.Buff:
                PlayerBuff();
                break;
            case StatusFlag.End:
                PlayerEnd();
                break;
        }
    }
    /// <summary>
    /// 移動処理
    /// </summary>
    private void PlayerMove()
    {
        //開始位置を保存
        StartPosition = selectedCharacter.CharacterObj.transform.position;
        // キャラクターを行動位置に移動
        selectedCharacter.CharacterObj.transform.DOMove(ActionPosition, 1f).OnComplete(() =>
        {
            selectedCharacter.StatusFlag = StatusFlag.Select;
            isActionPending = true;
        }); ;
    }
    /// <summary>
    /// スキル選択処理
    /// </summary>
    private void PlayerSelect()
    {
        // スキル選択パネル
        List<SkillData> skills = new List<SkillData>();
        skills.AddRange(selectedCharacter.skills);
        // UnityEventを作成してコールバックを設定
        UnityEvent<int> callback = new UnityEvent<int>();
        //カリバフ効果適用
        callback.AddListener(OnSkillSelected);
        // スキル選択UIを表示
        skillSelectionUI.ShowSkillSelection(skills, callback);
    }
    /// <summary>
    /// 攻撃相手選択フェイズ
    /// </summary>
    private void PlayerAttackSelect()
    {
        // 攻撃対象選択パネル
        List<Character> enemies = getEnemy();
        var attackEvent = new UnityEvent<int>();
        attackEvent.AddListener((index) => OnAttackSelected(enemies, index));
        uiTest.Inputs(attackEvent, enemies.Count - 1, enemies);
    }
    /// <summary>
    /// 回復味方を選択するファイズ
    /// </summary>
    private void PlayerHealSelect()
    {
        // Heal対象選択パネル対象選択パネル
        List<Character> characters = getPlayer();
        var healEvent = new UnityEvent<int>();
        healEvent.AddListener((index) => OnHealSelected(characters, index));
        uiTest.Inputs(healEvent, characters.Count - 1, characters);
    }
    /// <summary>
    /// バフの処理
    /// </summary>
    private void PlayerBuff()
    {
        if (selectedSkill.buffEffect.Count > 0)
        {
            if (selectedCharacter.mp < selectedSkill.mpCost)
            {
                selectedCharacter.StatusFlag = StatusFlag.Select;
                isActionPending = true;
                return;
            }
            selectedCharacter.mp -= selectedSkill.mpCost;
            SetBuff();
        }
    }
    /// <summary>
    /// 最終処理
    /// </summary>
    private void PlayerEnd()
    {
        StartCoroutine(AttackEndTick());
    }
    private IEnumerator AttackEndTick()
    {
        yield return new WaitForSeconds(1f); // 攻撃エフェクトの表示時間に合わせて待機
        //攻撃後バフの設定
        //バフ効果の管理
        buffTurnManage();
        // キャラクターを開始位置に戻る
        selectedCharacter.CharacterObj.transform.DOMove(StartPosition, 1f).OnComplete(() =>
        {
            selectedCharacter.StatusFlag = StatusFlag.None;
            // ターン処理を終了
            turnManager.FlagChange();
        });
    }

    #endregion

    #region 行動処理のコールバック
    /// <summary>
    /// スキル選択時のコールバック
    /// </summary>
    private void OnSkillSelected(int index)
    {
        //callBackでかえってきた変数がスキルのリスト内の物選択しているか
        if (index < 0 || index >= selectedCharacter.skills.Length)
        {
            selectedCharacter.StatusFlag = StatusFlag.Select;
            isActionPending = true;
            return;
        }
        //スキルリストにセットされている確認
        if (selectedCharacter.skills[index] == null)// nullチェック追加
        {
            selectedCharacter.StatusFlag = StatusFlag.Select;
            isActionPending = true;
            return;
        }
        selectedSkill = selectedCharacter.skills[index];

        //選択したスキル名を表示させたい
        //
        //スキル呼び出し
        
        switch (selectedSkill.effectType)
        {
            case SkillEffectType.Attack:
                selectedCharacter.StatusFlag = StatusFlag.Attack;
                if (selectedSkill.targetScope == TargetScope.All||selectedCharacter.AllAttack)
                    OnAttackSelected(getEnemy(), 0);
                break;
            case SkillEffectType.Heal:
                selectedCharacter.StatusFlag = StatusFlag.Heal;
                if (selectedSkill.targetScope == TargetScope.All)
                    OnHealSelected(null, 0); 
                break;
            case SkillEffectType.Buff:
                selectedCharacter.StatusFlag = StatusFlag.Buff;
                if (selectedSkill.targetScope == TargetScope.All)
                    isActionPending = true;
                //    //仮バフ効果適用
                //    OnBuffSelected(null, 0, selectedSkill.buffEffect[0]);
                break;
        }
        if(selectedSkill.targetScope != TargetScope.All)
            isActionPending = true;
    }

    /// <summary>
    /// 攻撃対象選択時のコールバック
    /// </summary>
    private void OnAttackSelected(List<Character> enemies, int index)
    {
        // 攻撃の範囲内かを確認
        if (index < 0 || index >= enemies.Count)
        {
            selectedCharacter.StatusFlag = StatusFlag.Attack;
            isActionPending = true;
            return;
        }
        //MPが足りるかを確認
        if (selectedCharacter.mp < selectedSkill.mpCost)
        {
            selectedCharacter.StatusFlag = StatusFlag.Select;
            isActionPending = true;
            return;
        }
        // 全の攻撃スキルの場合、すべての敵に攻撃を適用
        if (selectedSkill.targetScope == TargetScope.All||selectedCharacter.AllAttack)
        {
            selectedCharacter.mp -= selectedSkill.mpCost;
            foreach (var enemy in enemies)
            {
                bool EnemyDefeat = ApplyAttack(enemy, selectedSkill);
                if (!EnemyDefeat)
                {
                    Debug.Log($"{enemy}撃破！");
                }
            }
        } else
        {
            if (selectedSkill.canCombo)
            {
                selectedCharacter.mp -= selectedSkill.mpCost;
                comboCount = 0;
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
                selectedCharacter.mp -= selectedSkill.mpCost;
                bool EnemyDefeat = ApplyAttack(enemy, selectedSkill);
                if (!EnemyDefeat)
                {
                    Debug.Log($"{enemy}撃破！");
                }
            }
        }

        //コンボ以外
        if(!selectedSkill.canCombo)
        {
            //攻撃後バフの設定
            if (selectedCharacter.skills.Length > 0)
            {
                SetBuff();
            }
            selectedCharacter.StatusFlag = StatusFlag.End;
            isActionPending = true;
        }
    }

    /// <summary>
    /// コンボ時の攻撃処理
    /// </summary>
    public bool OnComboApplyAttack()
    {
        comboCount++;
        int attackdamege = 0;
        if (comboCount != 0)
            attackdamege =+ comboCount*selectedSkill.ComboDamage;
        var enemy = selectedEnemy;
        var enemysurvival =ApplyAttack(enemy, selectedSkill, attackdamege);
        return enemysurvival;
        //selectedCharacter.mp -= selectedSkill.mpCost;
    }

    /// <summary>
    /// コンボ攻撃後の処理
    /// </summary>
    private void OnComboEnd()
    {
        //攻撃後バフの設定
        if (selectedCharacter.skills.Length > 0)
        {
            SetBuff();
        }
        else
        {
            selectedCharacter.StatusFlag = StatusFlag.End;
            isActionPending = true;
        }
    }

    /// <summary>
    /// 攻撃対象選択時のコールバック
    /// </summary>
    private void OnHealSelected(List<Character> characters, int index)
    {
        // 対象の範囲内かを確認
        if (index < 0 || index >= characters.Count)
        {
            selectedCharacter.StatusFlag = StatusFlag.Heal;
            isActionPending = true;
            return;
        }
        // MPが存在するかを確認
        if (selectedCharacter.mp < selectedSkill.mpCost)
        {
            selectedCharacter.StatusFlag = StatusFlag.Select;
            isActionPending = true;
            return;
        }
        //すべての味方を対象
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
        //ヒール後にバフが必要かを確認
        //攻撃後バフの設定
        //if (selectedCharacter.skills.Length > 0)
        //{
        //    SetBuff();
        //}else
        //{
        //    selectedCharacter.StatusFlag = StatusFlag.End;
        //    isActionPending = true;
        //}

    }

    /// <summary>
    /// バフセレクト後の処理
    /// </summary>
    public void OnBuffSelected(List<Character> characters, int index,BuffBase buffBase)
    {
        if (characters == null)
        {
            if(buffBase.buffRange != BuffRange.AllAllies && buffBase.buffRange != BuffRange.AllEnemies)
            {
                Debug.LogWarning("バフの対象キャラクターが指定されていません。BuffRangeがAllAlliesまたはAllEnemiesの場合、charactersはnullであるべきです。");
                selectedCharacter.StatusFlag = StatusFlag.End;
                isActionPending = true;
                return;
            }
        } else if (index < 0 || index >= characters.Count)
        {
            selectedCharacter.StatusFlag = StatusFlag.Buff;
            isActionPending = true;
            return;
        }
        //通常スキルの処理
        var character = new Character();
        if (characters!=null)
        { character = characters[index]; }
        BuffInstance buff = new BuffInstance(buffBase);
        buff.baseData.sourceCharacter = selectedCharacter;
        buff.remainingTurns = buffBase.duration;
        buffApply(buff, character);

        selectedCharacter.StatusFlag = StatusFlag.End;
        isActionPending = true;
    }

    #endregion

    #region 行動処理の実装
    /// <summary>
    /// 攻撃処理（ダメージ計算・撃破処理）
    /// true;敵が残っている、false:敵が撃破された
    /// </summary>
    private bool ApplyAttack(Character enemy, SkillData skill, int Attackbuff = 0)
    {
        if (enemy == null || skill == null) return true; // nullチェック追加

        //ダメージ乱数
        float random = UnityEngine.Random.Range(10, 20);
        random = random / 10;
        Debug.Log("乱数:" + random);

        // バフ適用後の攻撃力と防御力を取得
        int effectiveAtk = selectedCharacter.GetEffectiveAttack();
        int effectiveDef = enemy.GetEffectiveDefense();

        //基本ダメージ計算（バフ適用後の攻撃力を使用）
        var damage = effectiveAtk * random;
        //最終計算（バフ適用後の防御力を使用）
        var finalDamage = damage * skill.power + Attackbuff - effectiveDef;
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
    /// バフ効果の適用
    /// 各キャラクターのCharacterBuffManagerに委譲
    /// </summary>
    private void buffApply(BuffInstance buff, Character target)
    {
        // || buff.baseData.duration==0
        if (buff == null)
        {
            Debug.LogWarning("バフ適用失敗: バフインスタンスがnullですまたは、そのターンのみの処理です。");
            return;
        }

        
        switch (buff.buffRange)
        {
            case BuffRange.Self:
                target = selectedCharacter;
                if (target != null)
                {
                    target.ApplyBuff(buff, selectedCharacter);
                }
                break;
            case BuffRange.Ally:
            case BuffRange.Enemy:
                //単一の選択対象(この場合はtargetに対応)
                if (target != null)
                {
                    target.ApplyBuff(buff,target);
                }
                break;
            case BuffRange.AllAllies:
                var players = getPlayer();
                foreach (var player in players)
                {
                    if (player != null)
                    {
                        // 各プレイヤー用に新しいバフインスタンスを作成
                        BuffInstance playerBuff = new BuffInstance(buff.baseData);
                        playerBuff.remainingTurns = buff.remainingTurns;
                        player.ApplyBuff(playerBuff, player);
                    }
                }
                break;
            case BuffRange.AllEnemies:
                var enemies = getEnemy();
                foreach (var enemy in enemies)
                {
                    if (enemy != null)
                    {
                        // 各敵用に新しいバフインスタンスを作成
                        BuffInstance enemyBuff = new BuffInstance(buff.baseData);
                        enemyBuff.remainingTurns = buff.remainingTurns;
                        enemy.ApplyBuff(enemyBuff, enemy);
                    }
                }
                break;
        }
    }
    
    /// <summary>
    /// バフの効果ターン管理
    /// 全てのキャラクターのバフを更新
    /// </summary>
    private void buffTurnManage()
    {
        // プレイヤーキャラクターのバフを更新
        var players = getPlayer();
        foreach (var player in players)
        {
            if (player != null)
            {
                player.TickBuffTurn();
            }
        }
        
        // 敵キャラクターのバフを更新
        var enemies = getEnemy();
        foreach (var enemy in enemies)
        {
            if (enemy != null)
            {
                enemy.TickBuffTurn();
            }
        }
    }

    /// <summary>
    /// バフをセットする
    /// </summary>
    private void SetBuff()
    {
        if (selectedSkill.buffEffect.Count > 0)
        {
            List<BuffBase> buffBase = selectedSkill.buffEffect;

            foreach (var buff in buffBase)
            {
                if (buff.isSelfTarget)
                {
                    OnBuffSelected(new List<Character>() { selectedCharacter }, 0, buff);
                    continue;
                }
                // Heal対象選択パネル対象選択パネル
                switch (buff.buffRange)
                {
                    case BuffRange.Self:
                        OnBuffSelected(new List<Character>() { selectedCharacter }, 0, buff);
                        break;
                    case BuffRange.AllAllies:
                    case BuffRange.AllEnemies:
                        OnBuffSelected(null, 0, buff);
                        break;
                    case BuffRange.Ally:
                        List<Character> buffcharacters = getPlayer();
                        var buffEvent = new UnityEvent<int>();
                        buffEvent.AddListener((index) => OnBuffSelected(buffcharacters, index, buff));
                        uiTest.Inputs(buffEvent, buffcharacters.Count - 1, buffcharacters);
                        break;
                    case BuffRange.Enemy:
                        if (buff.isSelfTarget)
                        {
                            OnBuffSelected(null, 0, buff);
                            break;
                        }
                        List<Character> buffenemies = getEnemy();
                        var buffEvents = new UnityEvent<int>();
                        buffEvents.AddListener((index) => OnBuffSelected(buffenemies, index, buff));
                        uiTest.Inputs(buffEvents, buffenemies.Count - 1, buffenemies);
                        break;
                }
            }
        }
        else
        {
            Debug.Log("バフ効果がありません");
            selectedCharacter.StatusFlag = StatusFlag.End;
            isActionPending = true;
            return;
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
    #endregion

}
