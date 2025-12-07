using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.SceneManagement;


public class PlayerController : MonoBehaviour
{

    [SerializeField,Header("歩く速度"),Range(0,10)]
    private float Speed;

    [SerializeField, Header("走る速度"), Range(1, 5)]


//    [SerializeField, Header("�E�_�E�b�E�V�E��E��E�̑��E�x"), Range(1, 5)]
    private float DashSpeed;

    [Header("クイックタイム戦闘")]
    [SerializeField] private QuickTimeCombatUI quickTimeCombatUI;

    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if(GameManager.Instance != null)
        {
            if(GameManager.Instance.BattleWin)
            {
                
                GameManager.Instance.BattleWin = false;
                this.gameObject.transform.position=GameManager.Instance.PlayerBackPosition;
                Debug.Log("PlayerControllerがGameManagerを取得しました。");
            }
            Debug.LogWarning("GameManagerが見つかりません。");
        }
        // Rigidbodyの設定
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation; // 回転を固定
        }
    }

    // Update is called once per frame
    void Update()
    {
        // 移動方向を計算
        Vector3 moveDirection = Vector3.zero;

        if (Input.GetKey(KeyCode.D))
        {
            moveDirection.z -= 1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            moveDirection.z = 1;
        }
        if (Input.GetKey(KeyCode.W))
        {
            moveDirection.x += 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            moveDirection.x -= 1;
        }

        // ダッシュ判定
        float currentSpeed = Speed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed *= DashSpeed;
        }

        // Rigidbodyで移動
        if (rb != null && moveDirection != Vector3.zero)
        {
            Vector3 newPosition = rb.position + moveDirection.normalized * currentSpeed * Time.deltaTime;
            rb.MovePosition(newPosition);
        }
    }

    /// <summary>
    /// 敵との衝突検知
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.CompareTag("Enemy"))
        {
            // 敵と衝突した場合
            Debug.Log("Enemyと衝突");
            // GameManagerを通してバトル開始
            if (GameManager.Instance != null)
            {
                GetEnemyStetas(collision.gameObject);
            }
            else
            {
                // GameManagerが存在しなぁE��合�E従来の方法でシーン遷移
                Debug.LogWarning("GameManagerが見つかりません。直接シーン遷移します、E");
                SceneManager.LoadScene("turnTestScene");
            }
        }
    }
    /// <summary>
    /// 敵のステータスを取得
    /// </summary>
    private void GetEnemyStetas(GameObject enemy)
    {
        Debug.Log("敵のステータスを取得");
        if (enemy != null)
        {
            EnemyWanderAI enemyAI = enemy.GetComponent<EnemyWanderAI>();
            if (enemyAI != null)
            {
                var enemyDataList = enemyAI.GetEnemyData();
                
                if (enemyDataList != null && enemyDataList.Count > 0)
                {
                    // 戦ったことがあるかチェック（すべての敵を倒したことがある場合）
                    bool hasDefeatedAll = GameManager.Instance.HasDefeatedAllEnemies(enemyDataList);
                    
                    if (hasDefeatedAll && quickTimeCombatUI != null)
                    {
                        // クイックタイム戦闘を開始
                        StartQuickTimeCombat(enemy, enemyDataList);
                    }
                    else
                    {
                        // 通常の戦闘シーンに移動
                        StartNormalBattle(enemy, enemyDataList);
                    }
                }
                else
                {
                    // EnemyDataが設定されていない場合は従来の処理
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.StartBattle(transform.position, enemy);
                    }
                    else
                    {
                        Debug.LogWarning("GameManagerが見つかりません。直接シーン遷移します。");
                        SceneManager.LoadScene("turnTestScene");
                    }
                }
            }
            else
            {
                // EnemyWanderAIがない場合は従来の処理
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.StartBattle(transform.position, enemy);
                }
                else
                {
                    Debug.LogWarning("GameManagerが見つかりません。直接シーン遷移します。");
                    SceneManager.LoadScene("turnTestScene");
                }
            }
        }
    }

    /// <summary>
    /// クイックタイム戦闘を開始
    /// </summary>
    private void StartQuickTimeCombat(GameObject enemyObject, List<CharacterData> enemyDataList)
    {
        if (quickTimeCombatUI == null)
        {
            Debug.LogWarning("QuickTimeCombatUIが設定されていません。通常戦闘に移行します。");
            StartNormalBattle(enemyObject, enemyDataList);
            return;
        }
        
        // プレイヤーの移動を停止
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
        }
        
        // 敵のAIを一時停止
        EnemyWanderAI enemyAI = enemyObject.GetComponent<EnemyWanderAI>();
        if (enemyAI != null)
        {
            enemyAI.StopWandering();
        }
        
        // クイックタイム戦闘UIを開始
        quickTimeCombatUI.StartQuickTimeCombat((success) =>
        {
            if (success)
            {
                // タイミング成功：敵を倒す
                OnQuickTimeCombatSuccess(enemyObject, enemyDataList);
            }
            else
            {
                // タイミング失敗：通常戦闘に移行
                StartNormalBattle(enemyObject, enemyDataList);
            }
        });
    }

    /// <summary>
    /// クイックタイム戦闘成功時の処理
    /// </summary>
    private void OnQuickTimeCombatSuccess(GameObject enemyObject, List<CharacterData> enemyDataList)
    {
        // 敵を倒した記録
        if (enemyDataList != null)
        {
            foreach (var enemyData in enemyDataList)
            {
                if (enemyData != null)
                {
                    GameManager.Instance.RecordEnemyDefeat(enemyData);
                }
            }
        }
        
        // 敵を削除
        Destroy(enemyObject);
        
        Debug.Log($"クイックタイム戦闘成功！敵を倒しました。");
        
        // ここに経験値獲得などの処理を追加可能
    }

    /// <summary>
    /// 通常の戦闘を開始
    /// </summary>
    private void StartNormalBattle(GameObject enemyObject, List<CharacterData> enemyDataList)
    {
        if (GameManager.Instance != null)
        {
            // EnemyDataを設定
            GameManager.Instance.EnemyData = new List<CharacterData>();
            if (enemyDataList != null)
            {
                GameManager.Instance.EnemyData.AddRange(enemyDataList);
            }
            
            GameManager.Instance.StartBattle(transform.position, enemyObject);
        }
        else
        {
            Debug.LogWarning("GameManagerが見つかりません。直接シーン遷移します。");
            SceneManager.LoadScene("turnTestScene");
        }
    }
}
