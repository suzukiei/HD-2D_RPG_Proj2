using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

/// <summary>
/// ゲーム全体を管理するシングルトンマネージャー
/// </summary>
public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    
    [Header("ゲーム状態")]
    [SerializeField] private GameState currentGameState = GameState.GameField;
    
    [Header("シーン管理")]
    [SerializeField] private string gameFieldSceneName = "GameField";
    [SerializeField] private string battleSceneName = "turnTestScene";
    [SerializeField] private string titleSceneName = "GameTitle";
    
    [Header("バトル設定")]
    [SerializeField] private bool enableBattleTransition = true;
    [SerializeField] private float battleTransitionDelay = 0.5f;
    
    [Header("UnityEvents")]
    [SerializeField] private UnityEvent OnBattleStart;
    [SerializeField] private UnityEvent OnBattleEnd;
    [SerializeField] private UnityEvent<GameState> OnGameStateChanged;
    
    [Header("デバッグ")]
    [SerializeField] private bool showDebugLog = true;
    public int isi = 1; // 既存の変数を保持
    
    // プロパティ
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                SetupInstance();
            }
            return instance;
        }
    }
    
    public GameState CurrentGameState => currentGameState;
    public bool IsBattleTransitionEnabled => enableBattleTransition;
    
    // 内部変数
    private Vector3 lastFieldPosition;
    private bool isTransitioning = false;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
            
            // シーン変更時のイベント登録
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            if (showDebugLog)
            {
                Debug.Log("GameManager: シングルトンインスタンスを作成しました");
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void OnDestroy()
    {
        // イベントの登録解除
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private static void SetupInstance()
    {
        instance = FindObjectOfType<GameManager>();

        if (instance == null)
        {
            GameObject gameObj = new GameObject();
            gameObj.name = "GameManager";
            instance = gameObj.AddComponent<GameManager>();
            DontDestroyOnLoad(gameObj);
        }
    }
    
    /// <summary>
    /// シーンロード時の処理
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (showDebugLog)
        {
            Debug.Log($"GameManager: シーンロード完了 - {scene.name}");
        }
        
        // シーン名に基づいてゲーム状態を更新
        UpdateGameStateFromScene(scene.name);
    }
    
    /// <summary>
    /// シーン名からゲーム状態を更新
    /// </summary>
    private void UpdateGameStateFromScene(string sceneName)
    {
        GameState newState = currentGameState;
        
        if (sceneName == battleSceneName)
        {
            newState = GameState.Battle;
        }
        else if (sceneName == gameFieldSceneName)
        {
            newState = GameState.GameField;
        }
        else if (sceneName == titleSceneName)
        {
            newState = GameState.Title;
        }
        
        if (newState != currentGameState)
        {
            SetGameState(newState);
        }
    }
    
    /// <summary>
    /// ゲーム状態を変更
    /// </summary>
    public void SetGameState(GameState newState)
    {
        if (currentGameState == newState) return;
        
        GameState previousState = currentGameState;
        currentGameState = newState;
        
        if (showDebugLog)
        {
            Debug.Log($"GameManager: ゲーム状態変更 {previousState} → {newState}");
        }
        
        // UnityEventを発火
        OnGameStateChanged?.Invoke(newState);
    }
    
    /// <summary>
    /// バトル開始（プレイヤーが敵に接触した時に呼び出される）
    /// </summary>
    public void StartBattle(Vector3 playerPosition, GameObject enemy = null)
    {
        if (isTransitioning)
        {
            if (showDebugLog)
            {
                Debug.Log("GameManager: シーン遷移中のため、バトル開始をスキップ");
            }
            return;
        }
        
        if (!enableBattleTransition)
        {
            if (showDebugLog)
            {
                Debug.Log("GameManager: バトル遷移が無効化されています");
            }
            return;
        }
        
        // フィールドでの最後の位置を記録
        lastFieldPosition = playerPosition;
        
        if (showDebugLog)
        {
            Debug.Log($"GameManager: バトル開始 - プレイヤー位置: {playerPosition}");
            if (enemy != null)
            {
                Debug.Log($"遭遇した敵: {enemy.name}");
            }
        }
        
        // バトル開始イベントを発火
        OnBattleStart?.Invoke();
        
        // バトルシーンに遷移
        StartCoroutine(TransitionToBattle());
    }
    
    /// <summary>
    /// バトル終了（バトルシーンから呼び出される）
    /// </summary>
    public void EndBattle()
    {
        if (showDebugLog)
        {
            Debug.Log("GameManager: バトル終了");
        }
        
        // バトル終了イベントを発火
        OnBattleEnd?.Invoke();
        
        // フィールドに戻る
        StartCoroutine(TransitionToGameField());
    }
    
    /// <summary>
    /// バトルシーンへの遷移
    /// </summary>
    private IEnumerator TransitionToBattle()
    {
        isTransitioning = true;
        
        // 遷移前の待機時間
        if (battleTransitionDelay > 0)
        {
            yield return new WaitForSeconds(battleTransitionDelay);
        }
        
        // シーン遷移
        SceneManager.LoadScene(battleSceneName);
        
        isTransitioning = false;
    }
    
    /// <summary>
    /// ゲームフィールドへの遷移
    /// </summary>
    private IEnumerator TransitionToGameField()
    {
        isTransitioning = true;
        
        // シーン遷移
        SceneManager.LoadScene(gameFieldSceneName);
        
        // シーン遷移完了まで待機
        yield return null;
        
        isTransitioning = false;
    }
    
    /// <summary>
    /// プレイヤーの最後のフィールド位置を取得
    /// </summary>
    public Vector3 GetLastFieldPosition()
    {
        return lastFieldPosition;
    }
    
    /// <summary>
    /// バトル遷移の有効/無効を切り替え
    /// </summary>
    public void SetBattleTransitionEnabled(bool enabled)
    {
        enableBattleTransition = enabled;
        if (showDebugLog)
        {
            Debug.Log($"GameManager: バトル遷移を{(enabled ? "有効" : "無効")}にしました");
        }
    }
    
    /// <summary>
    /// 指定シーンに遷移
    /// </summary>
    public void LoadScene(string sceneName)
    {
        if (isTransitioning)
        {
            if (showDebugLog)
            {
                Debug.Log("GameManager: 既にシーン遷移中です");
            }
            return;
        }
        
        if (showDebugLog)
        {
            Debug.Log($"GameManager: {sceneName} に遷移中...");
        }
        
        SceneManager.LoadScene(sceneName);
    }
    
    /// <summary>
    /// テスト用：強制的にバトル開始
    /// </summary>
    [ContextMenu("テスト用バトル開始")]
    public void TestStartBattle()
    {
        StartBattle(Vector3.zero);
    }
    
    /// <summary>
    /// テスト用：強制的にバトル終了
    /// </summary>
    [ContextMenu("テスト用バトル終了")]
    public void TestEndBattle()
    {
        EndBattle();
    }
}

/// <summary>
/// ゲームの状態を表す列挙型
/// </summary>
public enum GameState
{
    Title,      // タイトル画面
    GameField,  // フィールド画面
    Battle,     // バトル画面
    Menu,       // メニュー画面
    Pause       // ポーズ状態
}
