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
    public List<GameObject> turnList = new List<GameObject>();// �v���C���[�ƃG�l�~�[���܂Ƃ߂����X�g
    [SerializeField]
    private List<GameObject> sortedTurnList = new List<GameObject>();// SPD���Ƀ\�[�g���ꂽ���X�g
    [SerializeField]
    private List<GameObject> nextTurnList = new List<GameObject>();// ���̃^�[���p���X�g
    //���݂̃^�[���I�u�W�F�N�g
    public GameObject currentTurnObject;
    private bool turnChangeFlag = false; // �^�[�����ύX�t���O
    private int turnNumber = 0; // ���݂̃^�[����
    private bool turnFlag; // �^�[�����������ǂ����̃t���O

    //�V���A���C�Y�t�B�[���h
    private static TurnManager instance;
    public static TurnManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<TurnManager>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("TurnManager");
                    instance = obj.AddComponent<TurnManager>();
                }
            }
            return instance;
        }
    }
    private void Awake()
    {
        // �V���O���g���p�^�[���̎���
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {

        // �ϐ��̏�����
        turnFlag = true;
        turnNumber = 0;
        turnChangeFlag = false;
        // ������
        Initialization();
    }
    // ����������
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
        // SPD���Ƀ\�[�g
        turnList.Sort((a, b) => b.GetComponent<Character>().spd.CompareTo(a.GetComponent<Character>().spd)); // SPD�~���Ń\�[�g
        nextTurnList = new List<GameObject>(turnList);
        sortedTurnList = new List<GameObject>(turnList);
        // Spd ���������i�~���j
        //List<GameObject> sorted = turnList.OrderByDescending(c => c.GetComponent<Character>().Spd).ToList();
        // UI�Ɏw��
        // UI�Ɍ��݂̃^�[�������̏��Ԃ�`����
        UIManager.Instance.UpdateTurnUI(sortedTurnList, turnNumber);
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
                if (players.Count == 0 || enemys.Count == 0)
                {
                    EndTurnManager();
                    yield break;
                    //break;
                }
                Debug.Log("�^�[��������:" + turnNumber);
                // �t���O��܂�
                turnFlag = false;
                // Turn���X�g���擾
                var nextCharacterStatus = sortedTurnList[turnNumber];
                currentTurnObject = nextCharacterStatus;
                // Character�̃X�e�[�^�X��ύX
                if (nextCharacterStatus == null)
                {
                    Debug.Log("�^�[���Ώۂ����݂��܂���");
                    turnFlag = true;
                    turnNumber = (turnNumber + 1) % sortedTurnList.Count;
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
                //���̃^�[���̃��X�g����폜
                sortedTurnList[turnNumber] = null;
                // �^�[���`�F���W
                turnNumber++;
                if (turnNumber >= sortedTurnList.Count)
                {
                    turnNumber = 0;
                    if (turnChangeFlag)
                    {
                        turnChangeFlag = false;
                        // �^�[�����X�g�����̃^�[���p���X�g�ōX�V
                        sortedTurnList.Clear();
                        sortedTurnList.AddRange(nextTurnList);
                        nextTurnList.Clear();
                        nextTurnList.AddRange(turnList);
                    }
                    else
                    {
                        turnChangeFlag = false;
                        sortedTurnList.Clear();
                        // �v���C���[�ƃG�l�~�[���܂Ƃ߂�SPD���ɕ��ёւ���
                        sortedTurnList.AddRange(turnList);
                    }
                    UIManager.Instance.UpdateTurnUI(sortedTurnList, turnNumber);
                }
                else
                {
                    //UIManager.Instance.UpdateTurnUI(sortedTurnList, turnNumber);
                    UIManager.Instance.NextTurn();
                }

            }
            else
            {
                Debug.Log("�^�[���҂�");
            }

          
        }
    }
    

    //�^�[�����X�g�̏��Ԃ�ύX
    public void TurnChange(Character character, int chageNum)
    {
        //�^�[�����X�g�ύX�t���O�𗧂Ă�
        turnChangeFlag = true;
        if(character==null)
            Debug.Log("�^�[�����X�g�ύX:�ΏۃL�����N�^�[�����݂��܂���");
        var changeobj = character.CharacterObj;
      
            Debug.Log("�^�[�����X�g�ύX:" + changeobj.name + "��" + chageNum + "�ԖڂɈړ�");
        var objectToMove = nextTurnList.FirstOrDefault(obj => obj == changeobj);
        if (objectToMove != null)
        {
            nextTurnList.Remove(objectToMove);
            //�w�肳�ꂽ�ʒu�ɑ}��
            nextTurnList.Insert(chageNum, objectToMove);
        }
    }
    //�^�[�����X�g����L�����N�^�[���폜
    public void RemoveCharacterFromTurnList(Character character)
    {
        var removeobj = character.CharacterObj;
        //�^�[�����X�g����폜
        if (sortedTurnList.Contains(removeobj))
            sortedTurnList.Remove(removeobj);
        else if (nextTurnList.Contains(removeobj))
            nextTurnList.Remove(removeobj);
    }

    //�^�[�������ĊJ�t���O
    public void FlagChange()
    {

        turnFlag = true;
    }

    //�����A�s�k���ɌĂяo��
    public void EndTurnManager()
    {
        //�s�k����
        if (players.Count == 0)
            DefeatProcess();
        //�s�k����
        if (enemys.Count == 0)
            VictoryProcess();

        //�R���[�`����~
        StopAllCoroutines();
    }

    //�s�k����
    private void DefeatProcess()
    {
        Debug.Log("�s�k����");
        GameManager.Instance.EndBattle();
    }

    //��������
    private void VictoryProcess()
    {
        Debug.Log("��������");
        // 倒した敵を記録
        if (GameManager.Instance != null && GameManager.Instance.EnemyData != null)
        {
            foreach (var enemyData in GameManager.Instance.EnemyData)
            {
                if (enemyData != null)
                {
                    GameManager.Instance.RecordEnemyDefeat(enemyData);
                }
            }
        }
        
        GameManager.Instance.EndBattle();
    }
    //End of TurnManager
}
