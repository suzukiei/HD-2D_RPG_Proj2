using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private List<CharacterData> characterData;
    [SerializeField]
    private List<Vector3> vector3s;
    private List<GameObject> gameObjects = new List<GameObject>();
    public List<CharacterData> GetCharacterData() { return characterData; }

    private void Awake()
    {
        for (int i = 0; i < characterData.Count; i++)
        {
            //characterData[i] = new CharacterData();
            characterData[i].vector3 = vector3s[i];
            var obj = Instantiate(characterData[i].CharacterObj, vector3s[i]*2, Quaternion.identity);
            obj.transform.parent = this.gameObject.transform;
            gameObjects.Add(obj);
        }
    }

}
