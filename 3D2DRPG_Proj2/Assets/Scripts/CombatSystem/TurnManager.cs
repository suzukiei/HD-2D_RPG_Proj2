using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TurnManager : MonoBehaviour
{
    [SerializeField, Header("�v���C���[�}�l�[�W���[")]
    private PlayerManager playerManager;
    [SerializeField, Header("�G�l�~�[�}�l�[�W���[")]
    private EnemyManager enemyManager;
    [SerializeField, Header("�v���C���[�̃f�[�^")]
    public List<GameObject> players;
    [SerializeField, Header("�G�l�~�[�̃f�[�^")]
    public List<GameObject> enemys;
    [SerializeField, Header("�^�[�������X�g")]
    public List<GameObject> turnList = new List<GameObject>();
    private int turnNumber = 0;
    public bool turnFlag;

    private void Start()
    {
        // �ϐ��̏�����
        turnFlag = true;
        turnNumber = 0;

        // ������
        Initialization();
    }

    private void Initialization()
    {
        // �v���C���[���擾
        players = playerManager.GetPlayerCharacters();
        // �G�l�~�[���擾
        enemys = enemyManager.GetEnemyData();

        // �v���C���[�ƃG�l�~�[���܂Ƃ߂�SPD���ɕ��ёւ���
        turnList.Clear();
        turnList.AddRange(players);
        turnList.AddRange(enemys);

        turnList.Sort((a, b) => b.GetComponent<Character>().spd.CompareTo(a.GetComponent<Character>().spd)); // SPD�~���Ń\�[�g

        // Spd ���������i�~���j
        //List<GameObject> sorted = turnList.OrderByDescending(c => c.GetComponent<Character>().Spd).ToList();
        // UI�Ɏw��
        // ���Ԃ̃f�[�^��UI�ɓn��
        // �^�[�������X�^�[�g
        StartCoroutine(TurnController());
    }

    // �^�[���Ǘ�
    // ���̏���Update�ł���������....
    private IEnumerator TurnController()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            // ���̏�����҂�
            if (turnFlag)
            {
                //�^�[������
                if(players.Count == 0 || enemys.Count == 0)
                {
                    EndTurnManager();
                    yield break;
                    break;
                }
                Debug.Log("�^�[��������:" + turnNumber);
                // �t���O��܂�
                turnFlag = false;
                // Turn���X�g���擾
                var nextCharacterStatus = turnList[turnNumber];
                // Character�̃X�e�[�^�X��ύX
                if (nextCharacterStatus == null)
                {
                    Debug.Log("�^�[���Ώۂ����݂��܂���");
                    turnFlag = true;
                    turnNumber = (turnNumber + 1) % turnList.Count;
                    continue;
                }
                // True:Enemy False:Player
                if (nextCharacterStatus.GetComponent<Character>().enemyCheckFlag)
                {
                    // Enemy����
                    enemyManager.Test(nextCharacterStatus.GetComponent<Character>());
                    Debug.Log("StartEnemy");
                }
                else
                {
                    // Player����
                    nextCharacterStatus.GetComponent<Character>().StatusFlag = StatusFlag.Move;
                    playerManager.StartPlayerAction(nextCharacterStatus.GetComponent<Character>());
                    Debug.Log("StartPlayer");
                }

                // �^�[���̏���
                // �^�[���`�F���W
                turnNumber++;
                if (turnNumber >= turnList.Count)
                {
                    turnNumber = 0;
                }
                //turnNumber = (turnNumber + 1) % turnList.Count;
            }
            else
            {
                Debug.Log("�^�[���҂�");
            }

            // UI�Ɍ��݂̃^�[�������̏��Ԃ�`����
        }
    }

    public void FlagChange()
    {
        turnFlag = true;
    }

    //�����A�s�k���ɌĂяo��
    public void EndTurnManager()
    {
        //�s�k����
        if(players.Count == 0)
        {
            Debug.Log("�s�k");
            //�s�k����
        }
        //�s�k����
        if(enemys.Count == 0)
        {
            Debug.Log("����");
            //��������
        }
        //�R���[�`����~
        StopAllCoroutines();
    }

}
