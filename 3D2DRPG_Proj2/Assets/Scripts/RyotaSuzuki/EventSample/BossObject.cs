using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossObject : MonoBehaviour
{
    [Header("エネミー情報")]
    [SerializeField] private List<CharacterData> enemyData;    // エネミーデータリスト
    [SerializeField] private List<CharacterData> midBossEnemyData;    // エネミーデータリスト

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// 最終ボスエネミーデータを設定を取得
    /// </summary>
    public List<CharacterData> GetEnemyData()
    {
        return enemyData;
    }

    /// <summary>
    /// 中間ボスエネミーデータを設定を取得
    /// </summary>
    public List<CharacterData> GetMidEnemyData()
    {
        return midBossEnemyData;
    }
}
