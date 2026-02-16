using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TurnManager : MonoBehaviour
{
    #region 変数宣言
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
    private bool turnFlag; // ターン開始していいかどうかのフラグ

    //シングルトンパターン
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
    #endregion
    
    #region 初期化
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
        // Spd 降順でソート（降順）
        //List<GameObject> sorted = turnList.OrderByDescending(c => c.GetComponent<Character>().Spd).ToList();
        // UIに設定
        // UIに現在のターン順の状態を表示する
        UIManager.Instance.UpdateTurnUI(sortedTurnList, turnNumber);
        // 状態のデータをUIに渡す
        // ターン処理をスタート
        StartCoroutine(TurnController());
    }

    // ターン管理
    // この処理はUpdateで実行する必要がある....
    private IEnumerator TurnController()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            // 次の処理を待つ
            if (turnFlag)
            {
                //ターン開始
                if (players.Count == 0 || enemys.Count == 0)
                {
                    EndTurnManager();
                    yield break;
                    //break;
                }
                //Debug.Log("ターン開始:" + turnNumber);
                // フラグを立てる
                turnFlag = false;
                // Turnリストを取得
                var nextCharacterStatus = sortedTurnList[turnNumber];
                currentTurnObject = nextCharacterStatus;
                // Characterのステータスを変更
                if (nextCharacterStatus == null)
                {
                    //Debug.Log("ターン対象が存在しません");
                    turnFlag = true;
                    turnNumber = (turnNumber + 1) % sortedTurnList.Count;
                    continue;
                }
                // True:Enemy False:Player                
                if (nextCharacterStatus.GetComponent<Character>().enemyCheckFlag)
                {
                    // Enemy処理
                    enemyManager.Test(nextCharacterStatus.GetComponent<Character>());
                    //Debug.Log("StartEnemy");
                }
                else
                {
                    // Player処理
                    nextCharacterStatus.GetComponent<Character>().StatusFlag = StatusFlag.Move;
                    playerManager.StartPlayerAction(nextCharacterStatus.GetComponent<Character>());
                    //Debug.Log("StartPlayer");
                }
                //現在のターンのリストから削除
                sortedTurnList[turnNumber] = null;
                // ターンカウンター
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
                //Debug.Log("ターン待機");
            }


        }
    }
    #endregion

    #region ターン順リスト操作(ターン処理）
    //ターン順リストの順序を変更
    public void TurnChange(Character character, int chageNum)
    {
        //ターン順リスト変更フラグを立てる
        turnChangeFlag = true;
        if (character == null)
            Debug.Log("ターン順リスト変更:対象キャラクターが存在しません");
        var changeobj = character.CharacterObj;

        Debug.Log("ターン順リスト変更:" + changeobj.name + "を" + chageNum + "番目に移動");
        var objectToMove = nextTurnList.FirstOrDefault(obj => obj == changeobj);
        if (objectToMove != null)
        {
            nextTurnList.Remove(objectToMove);
            //指定された位置に挿入
            nextTurnList.Insert(chageNum, objectToMove);
        }
    }
    //ターン順リストからキャラクターを削除
    public void RemoveCharacterFromTurnList(Character character)
    {
        var removeobj = character.CharacterObj;
        //ターン順リストから削除
        if (sortedTurnList.Contains(removeobj))
            sortedTurnList.Remove(removeobj);
        else if (nextTurnList.Contains(removeobj))
            nextTurnList.Remove(removeobj);
    }

    //ターン開始してフラグ
    public void FlagChange()
    {
        turnFlag = true;
    }
    #endregion

    #region 勝利・敗北処理
    //勝利、敗北時に呼び出し
    public void EndTurnManager()
    {
        //敗北処理
        if (players.Count == 0)
            DefeatProcess();
        //勝利処理
        if (enemys.Count == 0)
            VictoryProcess();

        //コルーチンを停止
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

        GameManager.Instance.EnemyData.AddRange(enemyManager.enemyData);

        // 倒した敵を記録し、経験値を計算
        int totalExp = 0;
        //if (GameManager.Instance != null && GameManager.Instance.EnemyData != null)
        //{
        foreach (var enemyData in GameManager.Instance.EnemyData)
        {
            Debug.Log($"倒した敵の確認: {enemyData.charactername}");
            if (enemyData != null)
            {
                GameManager.Instance.RecordEnemyDefeat(enemyData);
                //Debug.Log($"敵を倒した記録: {enemyData.charactername}");
                // 敵の経験値を合計
                totalExp += CalculateEnemyExp(enemyData);
            }
        }
        //}
        Debug.Log($"総獲得経験値: {totalExp}");
        // プレイヤーに経験値を配布
        if (totalExp > 0)
        {
            DistributeExperienceToPlayers(totalExp);
        }
       
        GameManager.Instance.EndBattle();
        //GameManager.Instance.EnemyDataClear();  
    }

    /// <summary>
    /// 敵から獲得できる経験値を計算
    /// </summary>
    /// <param name="enemyData">敵のデータ</param>
    /// <returns>獲得経験値</returns>
    private int CalculateEnemyExp(CharacterData enemyData)
    {
        if (enemyData == null) return 0;

        // 敵のレベルに基づいて経験値を計算
        // 基本経験値 = 敵レベル × 50
        int baseExp = enemyData.level * 50;

        // 敵の種類や強さに応じて調整可能
        // 例: ボス敵の場合は倍率を上げるなど

        return baseExp;
    }

    /// <summary>
    /// プレイヤーキャラクターに経験値を配布
    /// </summary>
    /// <param name="totalExp">配布する総経験値</param>
    private void DistributeExperienceToPlayers(int totalExp)
    {
        if (players == null || players.Count == 0)
        {
            Debug.LogWarning("プレイヤーキャラクターが見つかりません");
            return;
        }

        // 生存しているプレイヤーキャラクターに経験値を配布
        int alivePlayerCount = 0;
        foreach (var playerObj in players)
        {
            if (playerObj != null)
            {
                var character = playerObj.GetComponent<Character>();
                if (character != null && !character.enemyCheckFlag && character.hp > 0)
                {
                    alivePlayerCount++;
                }
            }
        }

        if (alivePlayerCount == 0)
        {
            Debug.LogWarning("生存しているプレイヤーキャラクターがいません");
            return;
        }

        // 各プレイヤーに均等に経験値を配布
        int expPerPlayer = totalExp / alivePlayerCount;

        Debug.Log($"経験値配布: 総経験値 {totalExp}, 生存プレイヤー数 {alivePlayerCount}, 1人あたり {expPerPlayer}");

        foreach (var playerObj in players)
        {
            if (playerObj != null)
            {
                var character = playerObj.GetComponent<Character>();
                if (character != null && !character.enemyCheckFlag && character.hp > 0)
                {
                    int oldLevel = character.level;
                    character.GainExp(expPerPlayer);

                    if (character.level > oldLevel)
                    {
                        Debug.Log($"{character.charactername} がレベル {oldLevel} から {character.level} にレベルアップしました！");
                    }
                }
            }
        }
        //プレイヤーデータをGameManagerに保存
        List<CharacterData> playerDataList = new List<CharacterData>();
        foreach (var playerObj in players)
        {
            if (playerObj != null)
            {
                var character = playerObj.GetComponent<Character>();
                if (character != null && !character.enemyCheckFlag)
                {
                    playerDataList.Add(character.GetCharacterData());
                }
            }
        }
        GameManager.Instance.PlayerDataSetStatus(playerDataList);
    }
    //End of TurnManager
    #endregion
}
