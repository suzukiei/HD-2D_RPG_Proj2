using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    //Inspectorに複数データを表示するためのクラス
    [System.SerializableAttribute]
    public class ValueList
    {
        public List<CharacterData> List = new List<CharacterData>();

        public ValueList(List<CharacterData> list)
        {
            List = list;
        }
    }
    //Inspectorに表示される
    [SerializeField]
    private List<ValueList> _valueListList = new List<ValueList>();

    //MapにCharacterを配置
    public void appCharacter(Vector3 vector3, CharacterData characterData)
    {
        Debug.Log(vector3);
        _valueListList[(int)vector3.x].List[(int)vector3.z] = characterData;
    }
    //マップで自分の場所から敵がいるかを確認
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
