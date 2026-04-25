using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Respawn : MonoBehaviour
{
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] int maxEnemies = 20;
    [SerializeField] float spawnRadius = 20f;
    [SerializeField] float respawnInterval = 2f;
    [SerializeField] float initialSpawnDelay = 2f;
    [SerializeField] float minDistanceFromPlayer = 5f;
    [SerializeField] bool enableDebugLog = true;
    
    private const string csvName = "エンカウントテーブル";
    
    private List<GameObject> enemyList = new List<GameObject>();
    private List<EnemySpawnGroup> encounterTable = new List<EnemySpawnGroup>();
    private Dictionary<string, CharacterData> characterDataCache = new Dictionary<string, CharacterData>();
    private float respawnTimer = 0f;
    private GameObject player;

    [System.Serializable]
    public class EnemySpawnGroup
    {
        public int no;
        public List<string> enemyNames = new List<string>();
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        LoadCharacterDataCache();
        LoadEncounterTable();
        StartCoroutine(InitialSpawn());
    }

    private void Update()
    {
        CleanupEnemyList();
        
        if (enemyList.Count < maxEnemies)
        {
            respawnTimer += Time.deltaTime;
            
            if (respawnTimer >= respawnInterval)
            {
                SpawnEnemy();
                respawnTimer = 0f;
            }
        }
    }

    private void LoadCharacterDataCache()
    {
        #if UNITY_EDITOR
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:CharacterData", new[] { "Assets/Scripts/Data/Charactar" });
        
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            CharacterData character = UnityEditor.AssetDatabase.LoadAssetAtPath<CharacterData>(path);
            
            if (character != null && !characterDataCache.ContainsKey(character.charactername))
            {
                characterDataCache[character.charactername] = character;
            }
        }
        #else
        CharacterData[] allCharacters = Resources.LoadAll<CharacterData>("CharacterData");
        
        foreach (var character in allCharacters)
        {
            if (character != null && !characterDataCache.ContainsKey(character.charactername))
            {
                characterDataCache[character.charactername] = character;
            }
        }
        #endif
        
        if (enableDebugLog)
        {
            Debug.Log($"[Respawn] CharacterDataキャッシュ読み込み完了: {characterDataCache.Count}件");
        }
    }

    private void LoadEncounterTable()
    {
        string csvPath = Path.Combine(Application.streamingAssetsPath, "DB", csvName + ".csv");
        
        if (!File.Exists(csvPath))
        {
            Debug.LogError($"[Respawn] CSVファイルが見つかりません: {csvPath}");
            return;
        }
        
        string csvText = File.ReadAllText(csvPath);
        string[] lines = csvText.Split('\n');
        
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;
            
            string[] values = line.Split(',');
            if (values.Length < 2) continue;
            
            if (!int.TryParse(values[0].Trim(), out int groupNo)) continue;
            
            List<string> enemyNames = new List<string>();
            for (int j = 1; j < values.Length; j++)
            {
                string enemyName = values[j].Trim();
                if (!string.IsNullOrEmpty(enemyName))
                {
                    enemyNames.Add(enemyName);
                }
            }
            
            if (enemyNames.Count > 0)
            {
                encounterTable.Add(new EnemySpawnGroup
                {
                    no = groupNo,
                    enemyNames = enemyNames
                });
            }
        }
        
        if (enableDebugLog)
        {
            Debug.Log($"[Respawn] エンカウントテーブル読み込み完了: {encounterTable.Count}グループ");
        }
    }

    private IEnumerator InitialSpawn()
    {
        Debug.Log("InitialSpawn呼ばれた");

        yield return new WaitForSeconds(initialSpawnDelay);
        
        if (enableDebugLog)
        {
            Debug.Log($"[Respawn] 初期スポーン開始: {maxEnemies}体");
        }
        
        for (int i = 0; i < maxEnemies; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(0.1f);
        }
        
        if (enableDebugLog)
        {
            Debug.Log($"[Respawn] 初期スポーン完了: {enemyList.Count}体");
        }
    }

    private void CleanupEnemyList()
    {
        enemyList.RemoveAll(enemy => enemy == null);
    }

    public Vector3 GetRandomNavMeshPosition(Vector3 center, float radius)
    {
        int maxAttempts = 30;
        
        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 randomPos = center + Random.insideUnitSphere * radius;
            randomPos.y = center.y;

            if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, 5.0f, NavMesh.AllAreas))
            {
                if (IsValidSpawnPosition(hit.position))
                {
                    return hit.position;
                }
            }
        }

        if (NavMesh.SamplePosition(center, out NavMeshHit centerHit, 5.0f, NavMesh.AllAreas))
        {
            if (IsValidSpawnPosition(centerHit.position))
            {
                return centerHit.position;
            }
        }
        
        Debug.LogWarning($"[Respawn] NavMesh上の有効な位置が見つかりませんでした");
        return center;
    }

    private bool IsValidSpawnPosition(Vector3 position)
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
        
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(position, player.transform.position);
            if (distanceToPlayer < minDistanceFromPlayer)
            {
                if (enableDebugLog)
                {
                    Debug.Log($"[Respawn] スポーン位置がプレイヤーに近すぎます: {distanceToPlayer}m");
                }
                return false;
            }
        }
        
        return true;
    }

    void SpawnEnemy()
    {
        if (encounterTable.Count == 0)
        {
            Debug.LogWarning("[Respawn] エンカウントテーブルが空です");
            return;
        }
        
        EnemySpawnGroup selectedGroup = encounterTable[Random.Range(0, encounterTable.Count)];
        Vector3 pos = GetRandomNavMeshPosition(transform.position, spawnRadius);
        GameObject enemy = Instantiate(enemyPrefab, pos, Quaternion.identity);
        
        EnemyWanderAI enemyAI = enemy.GetComponent<EnemyWanderAI>();
        if (enemyAI != null)
        {
            enemyAI.SetEncounterGroupId(selectedGroup.no);
            
            List<CharacterData> enemyDataList = new List<CharacterData>();
            foreach (string enemyName in selectedGroup.enemyNames)
            {
                if (characterDataCache.ContainsKey(enemyName))
                {
                    enemyDataList.Add(characterDataCache[enemyName]);
                }
                else
                {
                    Debug.LogWarning($"[Respawn] CharacterDataが見つかりません: {enemyName}");
                }
            }
            
            enemyAI.SetEnemyData(enemyDataList);
            
            if (enableDebugLog)
            {
                Debug.Log($"[Respawn] 敵スポーン: グループID={selectedGroup.no}, 敵数={enemyDataList.Count}");
            }
        }
        else
        {
            Debug.LogError("[Respawn] EnemyWanderAIコンポーネントが見つかりません");
        }
        
        enemyList.Add(enemy);
    }

    public void RemoveEnemy(GameObject enemy)
    {
        if (enemyList.Contains(enemy))
        {
            enemyList.Remove(enemy);
            
            if (enableDebugLog)
            {
                Debug.Log($"[Respawn] 敵を削除: 残り{enemyList.Count}体");
            }
        }
    }

    public int GetCurrentEnemyCount()
    {
        CleanupEnemyList();
        return enemyList.Count;
    }
}
