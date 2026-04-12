using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossObject : MonoBehaviour
{
    [Header("エネミー情報")]
    [SerializeField] private List<CharacterData> enemyData;    // エネミーデータリスト

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// エネミーデータを設定を取得
    /// </summary>
    public List<CharacterData> GetEnemyData()
    {
        return enemyData;
    }
}
