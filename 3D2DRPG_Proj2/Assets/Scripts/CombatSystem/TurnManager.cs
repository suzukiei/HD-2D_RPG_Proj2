using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TurnManager : MonoBehaviour
{
    [SerializeField, Header("プレイヤーマネージャー")]
    private PlayerManager playerManager;
    [SerializeField, Header("エネミーマネージャー")]
    private EnemyManager enemyManager;
    [SerializeField, Header("プレイヤーのデータ")]
    public List<GameObject> players;
    [SerializeField, Header("エネミーのデータ")]
    public List<GameObject> enemys;
    [SerializeField, Header("ターン順リスト")]
    private List<GameObject> turnList = new List<GameObject>();
    private int turnNumber = 0;
    public bool turnFlag;

    private void Start()
    {
        // 変数の初期化
        turnFlag = true;
        turnNumber = 0;

        // 初期化
        Initialization();
    }

    private void Initialization()
    {
        // プレイヤーを取得
        players = playerManager.GetPlayerCharacters();
        // エネミーを取得
        enemys = enemyManager.GetEnemyData();

        // プレイヤーとエネミーをまとめてSPD順に並び替える
        turnList.Clear();
        turnList.AddRange(players);
        turnList.AddRange(enemys);

        turnList.Sort((a, b) => b.GetComponent<Character>().spd.CompareTo(a.GetComponent<Character>().spd)); // SPD降順でソート

        // Spd が高い順（降順）
        //List<GameObject> sorted = turnList.OrderByDescending(c => c.GetComponent<Character>().Spd).ToList();
        // UIに指示
        // 順番のデータをUIに渡す
        // ターン処理スタート
        StartCoroutine(TurnController());
    }

    // ターン管理
    // この処理Updateでもいいかも....
    private IEnumerator TurnController()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            // 次の処理を待つ
            if (turnFlag)
            {
                Debug.Log("ターン処理中:" + turnNumber);
                turnFlag = false;
                // Turnリストを取得
                var nextCharacterStatus = turnList[turnNumber];
                // Characterのステータスを変更

                // True:Enemy False:Player
                if (nextCharacterStatus.GetComponent<Character>().enemyCheckFlag)
                {
                    // Enemy処理
                    enemyManager.Test();
                    Debug.Log("StartEnemy");
                }
                else
                {
                    // Player処理
                    nextCharacterStatus.GetComponent<Character>().StatusFlag = StatusFlag.Move;
                    playerManager.StartPlayerAction(nextCharacterStatus.GetComponent<Character>());
                    Debug.Log("StartPlayer");
                }

                // ターンの順番
                // ターンチェンジ
                turnNumber = (turnNumber + 1) % turnList.Count;
            }
            else
            {
                Debug.Log("ターン待ち");
            }

            // UIに現在のターン順次の順番を伝える
        }
    }

    public void FlagChange()
    {
        turnFlag = true;
    }
}
