using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Player : MonoBehaviour
{
    [SerializeField,Header("UI�e�X�g")]
    UITest uITest;
    [SerializeField, Header("UI�e�X�g")]
    MapManager mapManager;
    [SerializeField, Header("Character�̊i�[��")]
    private List<CharacterData> characterData;
    [SerializeField]
    private List<Vector3> vector3s;
    private List<GameObject> gameObjects = new List<GameObject>();
    public List<CharacterData> GetCharacterData() { return characterData; }
    //�I�����Ǘ�
    public bool StartFlag = true;
    [SerializeField,Header("�I�����ꂽ�L�����N�^�[")]
    private CharacterData character;
    private void Awake()
    {
        StartFlag = false;
        for (int i = 0; i < characterData.Count; i++)
        {
            //characterData[i] = new CharacterData();
            characterData[i].vector3 = vector3s[i];
            var obj = Instantiate(characterData[i].CharacterObj, vector3s[i]*2, Quaternion.identity);
            obj.transform.parent = this.gameObject.transform;
            gameObjects.Add(obj);
        }
    }
    private void Update()
    {
        //������
        if (StartFlag)
        {
            Debug.Log("StartFlag");
            //�s���t�F�C�Y
            if (character.StetasFlags== StetasFlag.move)
            {
                Debug.Log("move");
                character.StetasFlags = StetasFlag.none;
                UnityEvent<int> unityEvent =new UnityEvent<int>();
                unityEvent.AddListener(PlayerMove);
                uITest.Inputs(unityEvent, 4);
            }
            //�s���t�F�C�Y
            if (character.StetasFlags == StetasFlag.select)
            {
                character.StetasFlags = StetasFlag.none;
            }
            //�s���t�F�C�Y
            if (character.StetasFlags == StetasFlag.attack)
            {
                character.StetasFlags = StetasFlag.none;
            }
            //�s���t�F�C�Y
            if (character.StetasFlags == StetasFlag.end)
            {
                character.StetasFlags = StetasFlag.none;
            }

            StartFlag = false;
        }
    }

    public void PlayerMove(int movepoint)
    {
        Debug.Log("PlayerMove");
        var vec = mapManager.PointMove(character.vector3, movepoint);
        //�ړ��ł��Ă��邩����
        if (vec.y == -1)
        {
            character.StetasFlags = StetasFlag.move;
            StartFlag = true;
            Debug.Log("Not");
        }
        else
        {
            character.StetasFlags = StetasFlag.select;
            character.vector3 = vec;
            //Player���ړ�
            var playerpos = gameObjects[characterData.IndexOf(character)].transform.position;
            gameObjects[characterData.IndexOf(character)].transform.position = new Vector3(vec.x*2,vec.y * 2,vec.z * 2);
            StartFlag = true;
            Debug.Log("MovePlayerPoint");
        }
        switch (movepoint)
        {
            case 1:
                break;
            case 2:
                Debug.Log("2");
                break;
            case 3:
                Debug.Log("3");
                break;
            case 4:
                Debug.Log("4");
                break;
        }
    }
    public void PlayerController(CharacterData characterData)
    {
        character = characterData;
        character.StetasFlags = StetasFlag.move;
        //�I���X�^�[�g
        StartFlag = true;

    }
}
