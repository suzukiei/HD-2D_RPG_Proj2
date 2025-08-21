using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private List<CharacterData> enemyData;
    [SerializeField]
    private List<Vector3> vector3s;
    private List<GameObject> gameObjects =new List<GameObject>();

    public List<CharacterData> GetEnemyData() { return enemyData; }
    private void Awake()
    {
        for (int i = 0; i < enemyData.Count; i++)
        {
            enemyData[i].vector3 = vector3s[i];
            var obj = Instantiate(enemyData[i].CharacterObj, vector3s[i] * 2, Quaternion.identity);
            obj.transform.parent = this.gameObject.transform;
            gameObjects.Add(obj);
        }
    }

}
