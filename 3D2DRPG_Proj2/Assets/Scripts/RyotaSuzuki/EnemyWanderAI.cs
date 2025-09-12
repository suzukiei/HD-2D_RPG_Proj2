using UnityEngine;
using UnityEngine.AI;
using System.Collections;

/// <summary>
/// NavMeshを使って徘徊する敵のAI
/// </summary>
public class EnemyWanderAI : MonoBehaviour
{
    [Header("徘徊設定")]
    [SerializeField] private float wanderRadius = 10f;          // 徘徊範囲
    [SerializeField] private float wanderTimer = 5f;           // 次の目標地点までの時間
    [SerializeField] private float minWaitTime = 1f;           // 最小待機時間
    [SerializeField] private float maxWaitTime = 3f;           // 最大待機時間
    
    [Header("デバッグ設定")]
    [SerializeField] private bool showDebugInfo = true;        // デバッグ情報表示
    [SerializeField] private bool showWanderRange = true;      // 徘徊範囲表示
    [SerializeField] private Color debugColor = Color.red;     // デバッグ色
    
    private NavMeshAgent navMeshAgent;
    private Vector3 initialPosition;
    private float timer;
    private bool isWaiting = false;
    
    void Start()
    {
        // NavMeshAgentを取得
        navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent == null)
        {
            Debug.LogError("NavMeshAgent が見つかりません！");
            enabled = false;
            return;
        }
        
        // 初期位置を記録
        initialPosition = transform.position;
        
        // 最初の目標地点を設定
        SetRandomDestination();
        
        if (showDebugInfo)
        {
            Debug.Log($"EnemyWanderAI: {gameObject.name} の徘徊開始");
            Debug.Log($"徘徊範囲: {wanderRadius}m, 移動間隔: {wanderTimer}秒");
        }
    }
    
    void Update()
    {
        // 待機中は何もしない
        if (isWaiting) return;
        
        // タイマー更新
        timer += Time.deltaTime;
        
        // 目標地点に到着したか、時間が経過したら新しい目標を設定
        if ((!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.5f) || timer >= wanderTimer)
        {
            StartCoroutine(WaitAndSetNewDestination());
        }
    }
    
    /// <summary>
    /// ランダムな目標地点を設定
    /// </summary>
    private void SetRandomDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += initialPosition;
        
        NavMeshHit hit;
        Vector3 finalPosition = Vector3.zero;
        
        // NavMesh上の有効な地点を探す
        if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, 1))
        {
            finalPosition = hit.position;
        }
        else
        {
            // 有効な地点が見つからない場合は初期位置周辺を再試行
            for (int i = 0; i < 10; i++)
            {
                Vector3 fallbackDirection = Random.insideUnitSphere * wanderRadius * 0.5f;
                fallbackDirection += initialPosition;
                
                if (NavMesh.SamplePosition(fallbackDirection, out hit, wanderRadius, 1))
                {
                    finalPosition = hit.position;
                    break;
                }
            }
        }
        
        // 有効な目標地点が見つかった場合のみ移動
        if (finalPosition != Vector3.zero)
        {
            navMeshAgent.SetDestination(finalPosition);
            timer = 0f;
            
            if (showDebugInfo)
            {
                Debug.Log($"{gameObject.name}: 新しい目標地点 {finalPosition}");
            }
        }
        else
        {
            if (showDebugInfo)
            {
                Debug.LogWarning($"{gameObject.name}: 有効な目標地点が見つかりませんでした");
            }
        }
    }
    
    /// <summary>
    /// 待機してから新しい目標地点を設定
    /// </summary>
    private IEnumerator WaitAndSetNewDestination()
    {
        isWaiting = true;
        navMeshAgent.ResetPath(); // 現在の移動を停止
        
        float waitTime = Random.Range(minWaitTime, maxWaitTime);
        
        if (showDebugInfo)
        {
            Debug.Log($"{gameObject.name}: {waitTime:F1}秒待機中...");
        }
        
        yield return new WaitForSeconds(waitTime);
        
        isWaiting = false;
        SetRandomDestination();
    }
    
    /// <summary>
    /// 徘徊を停止
    /// </summary>
    public void StopWandering()
    {
        if (navMeshAgent != null)
        {
            navMeshAgent.ResetPath();
        }
        
        StopAllCoroutines();
        isWaiting = false;
        enabled = false;
        
        if (showDebugInfo)
        {
            Debug.Log($"{gameObject.name}: 徘徊停止");
        }
    }
    
    /// <summary>
    /// 徘徊を再開
    /// </summary>
    public void ResumeWandering()
    {
        enabled = true;
        isWaiting = false;
        SetRandomDestination();
        
        if (showDebugInfo)
        {
            Debug.Log($"{gameObject.name}: 徘徊再開");
        }
    }
    
    /// <summary>
    /// 徘徊範囲を変更
    /// </summary>
    public void SetWanderRadius(float newRadius)
    {
        wanderRadius = newRadius;
        if (showDebugInfo)
        {
            Debug.Log($"{gameObject.name}: 徘徊範囲を {newRadius}m に変更");
        }
    }
    
    /// <summary>
    /// 移動速度を変更
    /// </summary>
    public void SetMoveSpeed(float speed)
    {
        if (navMeshAgent != null)
        {
            navMeshAgent.speed = speed;
            if (showDebugInfo)
            {
                Debug.Log($"{gameObject.name}: 移動速度を {speed} に変更");
            }
        }
    }
    
    /// <summary>
    /// 現在の状態を取得
    /// </summary>
    public bool IsWandering()
    {
        return enabled && !isWaiting;
    }
    
    /// <summary>
    /// デバッグ用ギズモ表示
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (showWanderRange)
        {
            Vector3 center = Application.isPlaying ? initialPosition : transform.position;
            
            // 徘徊範囲を表示
            Gizmos.color = debugColor;
            Gizmos.DrawWireSphere(center, wanderRadius);
            
            // 現在の目標地点を表示
            if (Application.isPlaying && navMeshAgent != null && navMeshAgent.hasPath)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(navMeshAgent.destination, 0.5f);
                Gizmos.DrawLine(transform.position, navMeshAgent.destination);
            }
        }
    }
} 