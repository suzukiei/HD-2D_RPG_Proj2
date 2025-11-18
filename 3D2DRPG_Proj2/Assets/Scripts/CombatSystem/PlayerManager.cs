using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using Unity.VisualScripting;
using Unity.Mathematics;

/// <summary>
/// �v���C���[�̐퓬�s�����Ǘ�����N���X
/// </summary>
public class PlayerManager : MonoBehaviour
{
    [SerializeField, Header("UI�e�X�g�p")]
    private UITest uiTest;
    [SerializeField, Header("ComboUI")]
    private ComboAttack comboUI;
    [SerializeField, Header("�I��pUI")]
    private SkillSelectionUI skillSelectionUI;
    [SerializeField, Header("�^�[���Ǘ�")]
    private TurnManager turnManager;
    [SerializeField, Header("�v���C���[�L�����N�^�[�ꗗ")]
    private List<CharacterData> playerCharacters;
    [SerializeField, Header("�L�����N�^�[�����z�u���W")]
    private List<Vector3> spawnPositions;
    [SerializeField, Header("�v���C���[�X�e�[�^�X")]
    private List<PlayerStatusPanel> playerStatusPanel;

    [SerializeField, Header("�L�����N�^�[�퓬�J�n�ʒu")]
    private Vector3 ActionPosition;
    [SerializeField, Header("�L�����N�^�[�����ʒu")]
    private Vector3 StartPosition;
    // �L�����N�^�[��GameObject�i�[�p
    private List<GameObject> characterObjects = new List<GameObject>();

    // ���ݑI�𒆂̃L�����N�^�[
    private Character selectedCharacter;
    // ���ݑI�𒆂̃X�L��
    private SkillData selectedSkill;
    // �s���҂��t���O
    private bool isActionPending = false;
    //�I�����Ă���G�l�~�[
    private Character selectedEnemy;

    //�o�t�̌��ʂ��Ǘ�����ϐ�
    private List<BuffInstance> activeBuffs = new List<BuffInstance>();

    /// <summary>
    /// �L�����N�^�[�f�[�^�擾�p
    /// </summary>
    public List<GameObject> GetPlayerCharacters() => characterObjects;

    /// <summary>
    /// �����������i�L�����N�^�[�̔z�u�j
    /// </summary>
    private void Awake()
    {
        isActionPending = false;
        for (int i = 0; i < playerCharacters.Count; i++)
        {
            // �L�����N�^�[�̍��W�����Z�b�g
            playerCharacters[i].CharacterTransfrom = spawnPositions[i];
            // �L�����N�^�[��GameObject�𐶐�
            var obj = Instantiate(playerCharacters[i].CharacterObj, spawnPositions[i], Quaternion.identity);
            obj.AddComponent<Character>().init(playerCharacters[i]);
            obj.transform.parent = transform;
            characterObjects.Add(obj);
            // �^�[���Ǘ��ɃL�����N�^�[��o�^
            playerStatusPanel[i].gameObject.SetActive(true);
            PlayerData playerData = new PlayerData(characterObjects[i].GetComponent<Character>());
            playerStatusPanel[i].UpdatePlayerStatus(playerData);
        }
    }

    /// <summary>
    /// ���t���[���̏�ԊǗ��E�s������
    /// </summary>
    private void Update()
    {
        // UI�̃v���C���[�X�e�[�^�X�X�V
        PlayerUIUpdate();
        if (!isActionPending) return;

        // �L�����N�^�[�̏�Ԃɉ����ď����𕪊�
        switch (selectedCharacter.StatusFlag)
        {
            case StatusFlag.Move:
                //�����ʒu��ۑ�
                StartPosition = selectedCharacter.CharacterObj.transform.position;
                // �L�����N�^�[���s���ʒu�Ɉړ�
                selectedCharacter.CharacterObj.transform.DOMove(ActionPosition, 1f).OnComplete(() =>
                {
                    selectedCharacter.StatusFlag = StatusFlag.Select;
                    isActionPending = true;
                }); ;
                break;

            case StatusFlag.Select:
                // �X�L���I���t�F�[�Y
                List<SkillData> skills = new List<SkillData>();
                skills.AddRange(selectedCharacter.skills);
                // UnityEvent���쐬���ăR�[���o�b�N��ݒ�
                UnityEvent<int> callback = new UnityEvent<int>();
                callback.AddListener(OnSkillSelected);
                // �Z�I��UI��\��
                skillSelectionUI.ShowSkillSelection(skills, callback);
                break;

            case StatusFlag.Attack:
                // �U���ΏۑI���t�F�[�Y
                List<Character> enemies = getEnemy();
                var attackEvent = new UnityEvent<int>();
                attackEvent.AddListener((index) => OnAttackSelected(enemies, index));
                uiTest.Inputs(attackEvent, enemies.Count - 1, enemies);
                break;

            case StatusFlag.Heal:
                // Heel�Ώۃt�@�C�Y�ΏۑI���t�F�[�Y
                List<Character> characters = getPlayer();
                var healEvent = new UnityEvent<int>();
                healEvent.AddListener((index) => OnHealSelected(characters, index));
                uiTest.Inputs(healEvent, characters.Count - 1, characters);
                break;
            case StatusFlag.Buff:
                // Heel�Ώۃt�@�C�Y�ΏۑI���t�F�[�Y
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
                //�o�t���ʂ̊Ǘ�
                buffTurnManage();
                // �L�����N�^�[�������ʒu�ɖ߂�
                selectedCharacter.CharacterObj.transform.DOMove(StartPosition, 1f).OnComplete(() =>
                {
                    selectedCharacter.StatusFlag = StatusFlag.None;
                    // �^�[���I������
                    turnManager.FlagChange();
                }); ;
                break;
        }

        // �s��������t���O��������
        isActionPending = false;
    }
    /// <summary>
    /// UI�̃v���C���[�X�e�[�^�X�X�V
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
    /// �v���C���[�̍s���J�n�i�O������Ăяo���j
    /// </summary>
    public void StartPlayerAction(Character character)
    {

        selectedCharacter = character;
        selectedCharacter.StatusFlag = StatusFlag.Move;
        isActionPending = true;
    }

    /// <summary>
    /// �X�L���I�����̃R�[���o�b�N
    /// </summary>
    private void OnSkillSelected(int index)
    {
        if (index < 0 || index >= selectedCharacter.skills.Length)
        {
            selectedCharacter.StatusFlag = StatusFlag.Select;
            isActionPending = true;
            return;
        }

        if (selectedCharacter.skills[index] == null)// null�`�F�b�N�ǉ�
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
    /// �U���ΏۑI�����̃R�[���o�b�N
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
        // �S�̍U���X�L���̏ꍇ�A���ׂĂ̓G�ɍU����K�p
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
            //�R���{�X�L���̏����i�������j
            var attackEvent = new UnityEvent<int>();
            attackEvent.AddListener((index) => OnComboApplyAttack());
            var attackEnd = new UnityEvent<int>();
            attackEnd.AddListener((index) => OnComboEnd());
            selectedEnemy = enemies[index];
            comboUI.Inputs(attackEvent, attackEnd, selectedSkill.maxcombo, selectedEnemy);
        }
        else
        {
            //�ʏ�X�L���̏���  
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
    /// �U�������i�_���[�W�v�Z�E���S����j
    /// </summary>
    private void ApplyAttack(Character enemy, SkillData skill)
    {
        if (enemy == null || skill == null) return; // null�`�F�b�N�ǉ�

        //�_���[�W����
        float random = UnityEngine.Random.Range(10, 20);
        random = random / 10;
        Debug.Log("����:" + random);
        //��b�_���[�W�v�Z
        var damage = selectedCharacter.atk* random;
        //�h��͌v�Z
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
            // �G�l�~�[���S���̏����i�������j
            //�G�l�~�[�̗̑͂�0�ɂ���
            enemy.hp = 0;
            turnManager.enemys.Remove(enemy.gameObject);
            turnManager.turnList.Remove(enemy.gameObject);
            //�G�l�~�[��GameObject��j�󂷂�
            Destroy(enemy.CharacterObj);

        }
    }

    private void OnComboEnd()
    {
        selectedCharacter.StatusFlag = StatusFlag.End;
        isActionPending = true;
    }
    /// <summary>
    /// �U���ΏۑI�����̃R�[���o�b�N
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
            //�S�̉񕜃X�L���̏���
            foreach (var getCharacter in characters)
            {
                ApplyHeal(getCharacter, selectedSkill);
            }
            selectedCharacter.mp -= selectedSkill.mpCost;
            selectedCharacter.StatusFlag = StatusFlag.End;
            isActionPending = true;
            return;
        }
        //�ʏ�X�L���̏���  
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
        //�ʏ�X�L���̏���
        var character = characters[index];
        BuffInstance buff = new BuffInstance (selectedSkill.buffEffect);
        buff.remainingTurns = selectedSkill.buffDuration;
        buffApply(buff, character);
        selectedCharacter.mp -= selectedSkill.mpCost;
        selectedCharacter.StatusFlag = StatusFlag.End;
        isActionPending = true;
    }
    /// <summary>
    /// �񕜏���
    /// </summary>
    private void ApplyHeal(Character character, SkillData skill)
    {
        if (character == null || skill == null) return; // null�`�F�b�N�ǉ�
        var hp = character.hp + skill.power;
        character.hp = (int)math.floor(hp);
        if (character.hp > character.maxHp)
        {
            character.hp = character.maxHp;
        }
    }
    /// <summary>
    /// �U���ΏۑI���t�F�[�Y�̓G�L�����N�^�[�擾
    /// </summary>
    private List<Character> getEnemy()
    {
        // �U���ΏۑI���t�F�[�Y
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
    /// �U���ΏۑI���t�F�[�Y�̖����L�����N�^�[�擾
    /// </summary>
    private List<Character> getPlayer()
    {
        // �U���ΏۑI���t�F�[�Y
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

    //�o�t���ʂ̓K�p
    private void buffApply(BuffInstance buff, Character target)
    {
        switch(buff.buffRange)
        {
            case BuffRange.Self:
                target = selectedCharacter;
                buff.Apply(target);
                activeBuffs.Add(buff);
                break;
            case BuffRange.Ally:
            case BuffRange.Enemy:
                //�P�̑I������(���̂Ƃ����target�őΉ�)
                buff.Apply(target);
                activeBuffs.Add(buff);
                break;
            case BuffRange.AllAllies:
                var players = getPlayer();
                foreach(var player in players)
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
    //�o�t���ʂ̉���
    private void buffRemove(BuffInstance buff)
    {
        buff.Remove();
        activeBuffs.Remove(buff);
    }
    //�o�t�̌��ʃ^�[���Ǘ�
    private void buffTurnManage()
    {
        //�o�t���ʃ^�[���Ȃ̂��𔻒�
        for (int activeBuffCount = activeBuffs.Count - 1; activeBuffCount >= 0; activeBuffCount--)
        {
            BuffInstance buff = activeBuffs[activeBuffCount];
            buff.TickTurn();
            if (buff.IsExpired())
            {
                //�o�t���ʏI��
                buffRemove(buff);
            }
        }
        //�o�t���ʃ^�[���I��
    }
}
