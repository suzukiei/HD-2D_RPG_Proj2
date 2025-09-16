using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// �v���C���[�̐퓬�s�����Ǘ�����N���X
/// </summary>
public class PlayerManager : MonoBehaviour
{
    [SerializeField, Header("UI�e�X�g�p")]
    private UITest uiTest;
    [SerializeField, Header("�^�[���Ǘ�")]
    private TurnManager turnManager;
    [SerializeField, Header("�v���C���[�L�����N�^�[�ꗗ")]
    private List<CharacterData> playerCharacters;
    [SerializeField, Header("�L�����N�^�[�����z�u���W")]
    private List<Vector3> spawnPositions;

    // �L�����N�^�[��GameObject�i�[�p
    private List<GameObject> characterObjects = new List<GameObject>();

    // ���ݑI�𒆂̃L�����N�^�[
    private Character selectedCharacter;
    // ���ݑI�𒆂̃X�L��
    private SkillData selectedSkill;
    // �s���҂��t���O
    private bool isActionPending = false;

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
        }
    }

    /// <summary>
    /// ���t���[���̏�ԊǗ��E�s������
    /// </summary>
    private void Update()
    {
        if (!isActionPending) return;

        // �L�����N�^�[�̏�Ԃɉ����ď����𕪊�
        switch (selectedCharacter.StatusFlag)
        {
            case StatusFlag.Move:
                // �ړ��A�j���[�V���������i�������j

                break;

            case StatusFlag.Select:
                // �X�L���I���t�F�[�Y
                var skills = selectedCharacter.skills;
                var selectEvent = new UnityEvent<int>();
                selectEvent.AddListener(OnSkillSelected);
                uiTest.Inputs(selectEvent, skills.Length-1);
                break;

            case StatusFlag.Attack:
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

                var attackEvent = new UnityEvent<int>();
                attackEvent.AddListener((index) => OnAttackSelected(enemies, index));
                uiTest.Inputs(attackEvent, enemies.Count-1);
                break;

            case StatusFlag.End:
                selectedCharacter.StatusFlag = StatusFlag.None;
                // �^�[���I������
                turnManager.FlagChange();
                break;
        }

        // �s��������t���O��������
        isActionPending = false;
        //�e�X�g�R�[�h
        if(selectedCharacter.StatusFlag == StatusFlag.Move)
        {
            selectedCharacter.StatusFlag = StatusFlag.Select;
            isActionPending = true;
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
        selectedCharacter.StatusFlag = StatusFlag.Attack;
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
        var enemy = enemies[index];
        ApplyAttack(enemy, selectedSkill);
        selectedCharacter.StatusFlag = StatusFlag.End;
        isActionPending = true;
    }

    /// <summary>
    /// �U�������i�_���[�W�v�Z�E���S����j
    /// </summary>
    private void ApplyAttack(Character enemy, SkillData skill)
    {
        if (enemy == null || skill == null) return; // null�`�F�b�N�ǉ�

        enemy.hp -= skill.power;
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
}