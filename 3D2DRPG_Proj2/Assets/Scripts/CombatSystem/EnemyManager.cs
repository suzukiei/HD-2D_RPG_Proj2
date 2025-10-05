using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField]
    private List<CharacterData> enemyData;
    [SerializeField, Header("�U�����@�������_���ɂ���")]
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
    /// Enemy �̍s�������i�V���v��AI: �X�L���������_���I�����ăv���C���[���U���j
    /// TurnManager ����s������G�l�~�[�� Character ���󂯎��
    /// </summary>
    public void Test(Character actingEnemy)
    {
        // Null�`�F�b�N
        if (actingEnemy == null)
        {
            turnManager.FlagChange();
            return;
        }

        // �v���C���[�����擾
        List<Character> playerCandidates = new List<Character>();
        foreach (var playerObj in turnManager.players)
        {
            if (playerObj == null) continue;
            var ch = playerObj.GetComponent<Character>();
            if (ch != null) playerCandidates.Add(ch);
        }

        if (playerCandidates.Count == 0)
        {
            // �U���Ώۂ����Ȃ� -> �^�[���I��
            turnManager.FlagChange();
            return;
        }

        // �X�L���I���i�����_���Ŕ�null�ȃX�L����I�ԁj
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

        // �^�[�Q�b�g�I���i����: HP�ŏ��̃v���C���[��_���B�����_���ɂ������ꍇ�� Random.Range ���g���j
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
            // �ی��Ń����_��
            target = playerCandidates[Random.Range(0, playerCandidates.Count)];
        }

        // �U�����s
        ApplyAttack(target, chosenSkill, actingEnemy);

        // �s���I���t���O����
        actingEnemy.StatusFlag = StatusFlag.End;
        // �^�[���� TurnManager �ɕԂ�
        turnManager.FlagChange();
    }

    /// <summary>
    /// �U�������i�_���[�W�v�Z�E���S����j
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
            // �X�L����������Ί�{�U���͂��g�p�iCharacterData �� atk ���Q�Ƃ��铙�A�K�v�Ȃ璲���j
            power = attacker != null ? attacker.atk : 1;
        }

        target.hp -= power;
        Debug.Log($"{attacker.name} �� {target.name} �� {power} �_���[�W�B�c��HP: {target.hp}");

        if (target.hp <= 0)
        {
            // �G�l�~�[���S���̏����i�������j
            target.hp = 0;
            // ���X�g����폜
            if (turnManager.players.Contains(target.gameObject))
            {
                turnManager.players.Remove(target.gameObject);
            }
            if (turnManager.turnList.Contains(target.gameObject))
            {
                turnManager.turnList.Remove(target.gameObject);
            }
            // GameObject ��j��
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
