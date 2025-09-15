using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField]
    private List<CharacterData> enemyData;
    [SerializeField]
    private TurnManager turnManager;
    [SerializeField]
    private List<Vector3> vector3s;
    private List<GameObject> enemygameObjects = new List<GameObject>();

    public List<GameObject> GetEnemyData() { return enemygameObjects; }
    private void Awake()
    {
        for (int i = 0; i < enemyData.Count; i++)
        {
            enemyData[i].CharacterTransfrom = vector3s[i];
            var obj = Instantiate(enemyData[i].CharacterObj, vector3s[i] * 2, Quaternion.identity);
            obj.AddComponent<Character>().init(enemyData[i]);
            obj.transform.parent = this.gameObject.transform;
            enemygameObjects.Add(obj);
        }
    }
    public void Test()
    {
        //EnemyChange
        turnManager.FlagChange();
    }
}
