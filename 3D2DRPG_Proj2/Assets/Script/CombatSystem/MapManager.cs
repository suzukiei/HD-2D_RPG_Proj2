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
    [SerializeField]
    private List<ValueList> _valueListList = new List<ValueList>();

    //Map��Character��z�u
    public void appCharacter(Vector3 vector3, CharacterData characterData)
    {
        Debug.Log(vector3);
        _valueListList[(int)vector3.x].List[(int)vector3.z] = characterData;
    }
    //�}�b�v�Ŏ����̏ꏊ����G�����邩���m�F
    public List<CharacterData> GetCharacterDatas(Vector2 vector2)
    {
        List<CharacterData> CharacterDatas = new List<CharacterData>();
        for (int i = -1; i < 1; i++)
        {
            for (int j = -1; j < 1; j++)
            {
                if (i == 0 && j == 0)
                    continue;
                if (_valueListList[(int)vector2.x].List[(int)vector2.y] != null)
                {
                    CharacterDatas.Add(_valueListList[(int)vector2.x].List[(int)vector2.y]);
                }

            }
        }
        return CharacterDatas;
    }
}
