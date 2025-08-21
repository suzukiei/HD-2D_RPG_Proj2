using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    //����Player��PlayerManager�Ɠǂ�ł��邪PlayerManager����ō쐬���ĊǗ�����������낵������
    [SerializeField, Header("�}�b�v�}�l�[�W���[")]
    private MapManager mapManager;
    [SerializeField, Header("�v���C���[�}�l�[�W���[")]
    private Player PlayerManager;
    [SerializeField, Header("�v���C���[�̃f�[�^")]
    private Enemy EnemyManager;
    [SerializeField, Header("�v���C���[�̃f�[�^")]
    private List<CharacterData> players;
    [SerializeField, Header("�G�l�~�[�̃f�[�^")]
    private List<CharacterData> enemys;
    [SerializeField, Header("")]
    private List<CharacterData> TurnList;
    private int TurnNamber = 0;
    public bool TurnFlag;

    private void Start()
    {
        //�ϐ��̏�����
        TurnFlag = true;
        TurnNamber = 0;

        //������
        Initialization();
    }
    private void Initialization()
    {
        //�v���C���[���擾
        players = PlayerManager.GetCharacterData();
        //�G�l�~�[���擾
        enemys = EnemyManager.GetEnemyData();
        //status�����Ē��ĕ��ёւ���
        List<CharacterData> ListCollection = new List<CharacterData>();
        ListCollection.AddRange(players);
        ListCollection.AddRange(enemys);
        TurnList.Add(ListCollection[0]);
        mapManager.appCharacter(ListCollection[0].vector3, ListCollection[0]);
        for (int i = 1; i < ListCollection.Count; i++)
        {
            mapManager.appCharacter(ListCollection[i].vector3, ListCollection[i]);
            Debug.Log("1");
            for (int j = 0; j < TurnList.Count; j++)
            {
                if (ListCollection[i].spd > TurnList[j].spd)
                {
                    TurnList.Insert(j, ListCollection[i]);
                    break;
                }
                if (TurnList.Count - 1 == j)
                {
                    TurnList.Add(ListCollection[i]);
                    break;
                }
            }
        }
        //UI�Ɏw��
        //���Ԃ̃f�[�^��UI�ɓn��
        //�^�[�������X�^�[�g
        StartCoroutine(TurnController());
    }

    //�^�[���Ǘ�
    //���̏���Update�ł���������....
    private IEnumerator TurnController()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            //���̏�����҂�
            if (TurnFlag)
            {
                Debug.Log("�^�[��������");
                TurnFlag = false;
                //Turn���X�g���擾
                var nextCharcterStatus = TurnList[TurnNamber];
                ///True:Enemy False:Player
                if(nextCharcterStatus.enemyCheckFalg)
                {
                    //Enemy����



                }else
                {
                    //Player����




                }

                //�^�[���̏���
                //�^�[���`�F���W
                if (TurnNamber < TurnList.Count)
                    TurnNamber++;
                else
                    TurnNamber = 0;
            } else
            {
                Debug.Log("�^�[���҂�");
            }

           
            //UI�Ɍ��݂̃^�[�������̏��Ԃ�`����
        }

    }
    
    public void FlagChange()
    {

    }


}
