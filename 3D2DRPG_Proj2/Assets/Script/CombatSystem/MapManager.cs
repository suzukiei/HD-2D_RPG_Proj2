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
    [SerializeField, Header("マップのCharacter位置を保持する")]
    private List<ValueList> _valueListList = new List<ValueList>();
    [SerializeField, Header("マップの最大座標保持")]
    public Vector2 MaxRange;
    //MapにCharacterを配置
    public void AppCharacter(Vector3 vector3, CharacterData characterData)
    {
        //Debug.Log(vector3);
        _valueListList[(int)vector3.x].List[(int)vector3.z] = characterData;
    }

    //移動できるかの判定：座標を確定
    //True:移動できれば移動した後の座標を返す
    //False:Y座標にマイナスが入る
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
        //マップ範囲外か判定
        Debug.Log(vector3.x + move.x.ToString());
        if((int)(vector3.x + move.x) == _valueListList.Count||
           (int)(vector3.x + move.x) == -1||
           (int)(vector3.z + move.z) == -1||
           (int)(vector3.z + move.z) == _valueListList.Count)
            return new Vector3(0, -1, 0);

        //今の場所になにかあるかを判定
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
