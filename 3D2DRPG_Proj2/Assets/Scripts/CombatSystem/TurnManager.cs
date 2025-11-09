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
    public List<GameObject> turnList = new List<GameObject>();// プレイヤーとエネミーをまとめたリスト
    [SerializeField]
    private List<GameObject> sortedTurnList = new List<GameObject>();// SPD順にソートされたリスト
    [SerializeField]
    private List<GameObject> nextTurnList = new List<GameObject>();// 次のターン用リスト
    //現在のターンオブジェクト
    public GameObject currentTurnObject;
    private bool turnChangeFlag = false; // ターン順変更フラグ
    private int turnNumber = 0; // 現在のターン数
    private bool turnFlag; // ターン処理中かどうかのフラグ

    //シリアライズフィールド
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
        // シングルトンパターンの実装
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

        // 変数の初期化
        turnFlag = true;
        turnNumber = 0;
        turnChangeFlag = false;
        // 初期化
        Initialization();
    }
    // 初期化処理
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
        // SPD順にソート
        turnList.Sort((a, b) => b.GetComponent<Character>().spd.CompareTo(a.GetComponent<Character>().spd)); // SPD降順でソート
        nextTurnList = new List<GameObject>(turnList);
        sortedTurnList = new List<GameObject>(turnList);
        // Spd が高い順（降順）
        //List<GameObject> sorted = turnList.OrderByDescending(c => c.GetComponent<Character>().Spd).ToList();
        // UIに指示
        // UIに現在のターン順次の順番を伝える
        UIManager.Instance.UpdateTurnUI(sortedTurnList, turnNumber);
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
                //ターン処理
                if (players.Count == 0 || enemys.Count == 0)
                {
                    EndTurnManager();
                    yield break;
                    //break;
                }
                Debug.Log("ターン処理中:" + turnNumber);
                // フラグを折る
                turnFlag = false;
                // Turnリストを取得
                var nextCharacterStatus = sortedTurnList[turnNumber];
                currentTurnObject = nextCharacterStatus;
                // Characterのステータスを変更
                if (nextCharacterStatus == null)
                {
                    Debug.Log("ターン対象が存在しません");
                    turnFlag = true;
                    turnNumber = (turnNumber + 1) % sortedTurnList.Count;
                    continue;
                }
                // True:Enemy False:Player
                if (nextCharacterStatus.GetComponent<Character>().enemyCheckFlag)
                {
                    // Enemy処理
                    enemyManager.Test(nextCharacterStatus.GetComponent<Character>());
                    Debug.Log("StartEnemy");
                }
                else
                {
                    // Player処理
                    nextCharacterStatus.GetComponent<Character>().StatusFlag = StatusFlag.Move;
                    playerManager.StartPlayerAction(nextCharacterStatus.GetComponent<Character>());
                    Debug.Log("StartPlayer");
                }
                //今のターンのリストから削除
                sortedTurnList[turnNumber] = null;
                // ターンチェンジ
                turnNumber++;
                if (turnNumber >= sortedTurnList.Count)
                {
                    turnNumber = 0;
                    if (turnChangeFlag)
                    {
                        turnChangeFlag = false;
                        // ターンリストを次のターン用リストで更新
                        sortedTurnList.Clear();
                        sortedTurnList.AddRange(nextTurnList);
                        nextTurnList.Clear();
                        nextTurnList.AddRange(turnList);
                    }
                    else
                    {
                        turnChangeFlag = false;
                        sortedTurnList.Clear();
                        // プレイヤーとエネミーをまとめてSPD順に並び替える
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
                Debug.Log("ターン待ち");
            }

          
        }
    }
    

    //ターンリストの順番を変更
    public void TurnChange(Character character, int chageNum)
    {
        //ターンリスト変更フラグを立てる
        turnChangeFlag = true;
        if(character==null)
            Debug.Log("ターンリスト変更:対象キャラクターが存在しません");
        var changeobj = character.CharacterObj;
      
            Debug.Log("ターンリスト変更:" + changeobj.name + "を" + chageNum + "番目に移動");
        var objectToMove = nextTurnList.FirstOrDefault(obj => obj == changeobj);
        if (objectToMove != null)
        {
            nextTurnList.Remove(objectToMove);
            //指定された位置に挿入
            nextTurnList.Insert(chageNum, objectToMove);
        }
    }
    //ターンリストからキャラクターを削除
    public void RemoveCharacterFromTurnList(Character character)
    {
        var removeobj = character.CharacterObj;
        //ターンリストから削除
        if (sortedTurnList.Contains(removeobj))
            sortedTurnList.Remove(removeobj);
        else if (nextTurnList.Contains(removeobj))
            nextTurnList.Remove(removeobj);
    }

    //ターン処理再開フラグ
    public void FlagChange()
    {

        turnFlag = true;
    }

    //勝利、敗北時に呼び出す
    public void EndTurnManager()
    {
        //敗北判定
        if (players.Count == 0)
            DefeatProcess();
        //敗北判定
        if (enemys.Count == 0)
            VictoryProcess();

        //コルーチン停止
        StopAllCoroutines();
    }

    //敗北処理
    private void DefeatProcess()
    {
        Debug.Log("敗北処理");
        GameManager.Instance.EndBattle();
    }

    //勝利処理
    private void VictoryProcess()
    {
        Debug.Log("勝利処理");
        GameManager.Instance.EndBattle();
    }
    //End of TurnManager
}
