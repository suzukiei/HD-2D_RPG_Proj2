using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Respawn : MonoBehaviour
{
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] int maxEnemies = 20;
    [SerializeField] float spawnRadius = 20f;
    //スポーン管理するオブジェクトリスト
    private List<GameObject> enemyList = new List<GameObject>();

    [System.Serializable]
    public class EnemySpawnGroup
    {
        public int no;
        public List<string> enemyNames = new List<string>();
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Update()
    {
        // 上限チェック
        if (enemyList.Count >= maxEnemies) return;

        SpawnEnemy();
    }



    public Vector3 GetRandomNavMeshPosition(Vector3 center, float radius)
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPos = center + Random.insideUnitSphere * radius;

            if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }

        return center; // 見つからなかった場合
    }

    void SpawnEnemy()
    {
        Vector3 pos = GetRandomNavMeshPosition(transform.position, spawnRadius);

        GameObject enemy = Instantiate(enemyPrefab, pos, Quaternion.identity);

        enemyList.Add(enemy);
    }
}
