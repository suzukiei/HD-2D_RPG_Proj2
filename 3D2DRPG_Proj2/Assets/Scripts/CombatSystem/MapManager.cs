using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    //Inspector�ɕ����f�[�^��\�����邽�߂̃N���X
    [System.SerializableAttribute]
    public class ValueList
    {
        public List<CharacterData> List = new List<CharacterData>();

        public ValueList(List<CharacterData> list)
        {
            List = list;
        }
    }
    //Inspector�ɕ\�������
    [SerializeField, Header("�}�b�v��Character�ʒu��ێ�����")]
    private List<ValueList> _valueListList = new List<ValueList>();
    [SerializeField, Header("�}�b�v�̍ő���W�ێ�")]
    public Vector2 MaxRange;
    //Map��Character��z�u
    public void AppCharacter(Vector3 vector3, CharacterData characterData)
    {
        //Debug.Log(vector3);
        _valueListList[(int)vector3.x].List[(int)vector3.z] = characterData;
    }

    //�ړ��ł��邩�̔���F���W���m��
    //True:�ړ��ł���Έړ�������̍��W��Ԃ�
    //False:Y���W�Ƀ}�C�i�X������
    public Vector3 PointMove(Vector3 vector3, int movepoint)
    {
        CharacterData characterData = _valueListList[(int)vector3.x].List[(int)vector3.z];
        Vector3 move = new Vector3(0, 0, 0);
        switch (movepoint)
        {
            case 1:
                move = new Vector3(1, 0, 0);
                break;
            case 2:
                move = new Vector3(-1, 0, 0);
                break;
            case 3:
                move = new Vector3(0, 0, 1);
                break;
            case 4:
                move = new Vector3(0, 0, -1);
                break;
            default:
                return new Vector3(0, -1, 0);
                
        }
        //�}�b�v�͈͊O������
        //Debug.Log(vector3.x + move.x.ToString());
        if((int)(vector3.x + move.x) == _valueListList.Count||
           (int)(vector3.x + move.x) == -1||
           (int)(vector3.z + move.z) == -1||
           (int)(vector3.z + move.z) == _valueListList.Count)
            return new Vector3(0, -1, 0);

        //���̏ꏊ�ɂȂɂ����邩�𔻒�
        if (_valueListList[(int)(vector3.x + move.x)].List[(int)(vector3.z + move.z)] == null)
        {
            _valueListList[(int)vector3.x].List[(int)vector3.z] = null;
            _valueListList[(int)(vector3.x + move.x)].List[(int)(vector3.z + move.z)] = characterData;
            return new Vector3((int)(vector3.x + move.x), 0, (int)(vector3.z + move.z));
        }
        else
        {
            return new Vector3(0, -1, 0);
        }
    }
    //�}�b�v�Ŏ����̏ꏊ����G�����邩���m�F
    public List<CharacterData> GetCharacterDatas(Vector3 vector3)
    {
        Debug.Log("GetCharacterDatas");
        List <CharacterData> CharacterDatas = new List<CharacterData>();
        for (int i = -1; i <= 1; i++)
        {
            Debug.Log("A");
            for (int j = -1; j <= 1; j++)
            {
                Debug.Log("B");
                //�����̈ʒu�͊m�F�s�v
                if (i == 0 && j == 0)
                    continue;
                //�͈͊O�͊m�F���Ȃ�
                if ((int)(vector3.x + i) == _valueListList.Count ||
                    (int)(vector3.x + i) == -1 ||
                    (int)(vector3.z + j) == -1 ||
                    (int)(vector3.z + j) == _valueListList.Count)
                    continue;

                if (_valueListList[(int)vector3.x+i].List[(int)vector3.z+j] != null)
                {
                    Debug.Log(_valueListList[(int)vector3.x + i].List[(int)vector3.z + j].enemyCheckFalg);
                    if(_valueListList[(int)vector3.x + i].List[(int)vector3.z + j].enemyCheckFalg)
                    CharacterDatas.Add(_valueListList[(int)vector3.x + i].List[(int)vector3.z + j]);
                }
            }
        }
        Debug.Log(CharacterDatas.Count);

        return CharacterDatas;
    }
}
