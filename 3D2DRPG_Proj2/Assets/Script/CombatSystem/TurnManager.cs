using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    //仮でPlayerをPlayerManagerと読んでいるがPlayerManagerを後で作成して管理した方がよろしいかも
    [SerializeField, Header("マップマネージャー")]
    private MapManager mapManager;
    [SerializeField, Header("プレイヤーマネージャー")]
    private Player PlayerManager;
    [SerializeField, Header("プレイヤーのデータ")]
    private Enemy EnemyManager;
    [SerializeField, Header("プレイヤーのデータ")]
    private List<CharacterData> players;
    [SerializeField, Header("エネミーのデータ")]
    private List<CharacterData> enemys;
    [SerializeField, Header("")]
    private List<CharacterData> TurnList;
    private int TurnNamber = 0;
    public bool TurnFlag;

    private void Start()
    {
        //変数の初期化
        TurnFlag = true;
        TurnNamber = 0;

        //初期化
        Initialization();
    }
    private void Initialization()
    {
        //プレイヤーを取得
        players = PlayerManager.GetCharacterData();
        //エネミーを取得
        enemys = EnemyManager.GetEnemyData();
        //statusを貰って着て並び替える
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
        //UIに指示
        //順番のデータをUIに渡す
        //ターン処理スタート
        StartCoroutine(TurnController());
    }

    //ターン管理
    //この処理Updateでもいいかも....
    private IEnumerator TurnController()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            //次の処理を待つ
            if (TurnFlag)
            {
                Debug.Log("ターン処理中");
                TurnFlag = false;
                //Turnリストを取得
                var nextCharcterStatus = TurnList[TurnNamber];
                ///True:Enemy False:Player
                if(nextCharcterStatus.enemyCheckFalg)
                {
                    //Enemy処理



                }else
                {
                    //Player処理




                }

                //ターンの順番
                //ターンチェンジ
                if (TurnNamber < TurnList.Count)
                    TurnNamber++;
                else
                    TurnNamber = 0;
            } else
            {
                Debug.Log("ターン待ち");
            }

           
            //UIに現在のターン順次の順番を伝える
        }

    }
    
    public void FlagChange()
    {

    }


}
