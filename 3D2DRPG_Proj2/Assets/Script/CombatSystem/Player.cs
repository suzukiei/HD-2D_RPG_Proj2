using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Player : MonoBehaviour
{
    [SerializeField,Header("UIテスト")]
    UITest uITest;
    [SerializeField, Header("MAP管理")]
    MapManager mapManager;
    [SerializeField, Header("ターン管理")]
    TurnManager turnManager;
    [SerializeField, Header("Characterの格納先")]
    private List<CharacterData> characterData;
    [SerializeField]
    private List<Vector3> vector3s;
    private List<GameObject> gameObjects = new List<GameObject>();
    private List<CharacterData> EnemyDatas=new List<CharacterData>();
    public List<CharacterData> GetCharacterData() { return characterData; }
    //選択を管理
    public bool StartFlag = true;
    [SerializeField,Header("選択されたキャラクター")]
    private CharacterData character;
    private SkillData atk;
    private void Awake()
    {
        StartFlag = false;
        for (int i = 0; i < characterData.Count; i++)
        {
            //characterData[i] = new CharacterData();
            characterData[i].CharacterTransfrom = vector3s[i];
            var obj = Instantiate(characterData[i].CharacterObj, vector3s[i]*2, Quaternion.identity);
            obj.transform.parent = this.gameObject.transform;
            gameObjects.Add(obj);
        }
    }
    private void Update()
    {
        //処理中
        if (StartFlag)
        {
            //Debug.Log("StartFlag");
            //行動フェイズ
            if (character.StetasFlags== StetasFlag.move)
            {
                Debug.Log("move");
                character.StetasFlags = StetasFlag.none;
                UnityEvent<int> unityEvent =new UnityEvent<int>();
                unityEvent.AddListener(PlayerMove);
                uITest.Inputs(unityEvent, 4);
            }
            //行動フェイズ
            if (character.StetasFlags == StetasFlag.select)
            {
                Debug.Log("selectA");
                var enemyDatas = mapManager.GetCharacterDatas(character.CharacterTransfrom);
                if (enemyDatas.Count == 0)
                {
                    character.StetasFlags = StetasFlag.end;
                    StartFlag = true;
                    return;
                }
                Debug.Log("selectB");
                var characterAttackDatas = character.skills;
                //コールバック処理を実装
                UnityEvent<int> unityEvent = new UnityEvent<int>();
                unityEvent.AddListener(PlayerSelect);
                uITest.Inputs(unityEvent, characterAttackDatas.Length);
            }
            //行動フェイズ
            if (character.StetasFlags == StetasFlag.attack)
            {
                Debug.Log("attack");
                character.StetasFlags = StetasFlag.none;
                EnemyDatas = mapManager.GetCharacterDatas(character.CharacterTransfrom);
                //コールバック処理を実装
                UnityEvent<int> unityEvent = new UnityEvent<int>();
                unityEvent.AddListener(PlayerAttack);
                uITest.Inputs(unityEvent, EnemyDatas.Count);
                //character.StetasFlags = StetasFlag.none;
            }
            //行動フェイズ
            if (character.StetasFlags == StetasFlag.end)
            {
                turnManager.FlagChange();
                character.StetasFlags = StetasFlag.none;
                
            }

            StartFlag = false;
        }
    }
    //初期行動
    //移動判定処理
    public void PlayerMove(int movepoint)
    {
        //Debug.Log("PlayerMove");
       var vec = mapManager.PointMove(character.CharacterTransfrom, movepoint);
        //移動できているか判定
        if (vec.y == -1)
        {
            character.StetasFlags = StetasFlag.move;
            StartFlag = true;
            Debug.Log("Not");
        }
        else
        {
            character.StetasFlags = StetasFlag.select;
            character.CharacterTransfrom = vec;
            //Playerを移動
            var playerpos = gameObjects[characterData.IndexOf(character)].transform.position;
            gameObjects[characterData.IndexOf(character)].transform.position = new Vector3(vec.x*2,vec.y * 2,vec.z * 2);
            StartFlag = true;
            //Debug.Log("MovePlayerPoint");
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
    //移動判定処理
    public void PlayerController(CharacterData characterData)
    {
        character = characterData;
        character.StetasFlags = StetasFlag.move;
        //選択スタート
        StartFlag = true;
    }
    public void PlayerSelect(int movepoint)
    {
        Debug.Log("PlayerSelect");
        //攻撃方法を選択
        atk = character.skills[movepoint-1];
        character.StetasFlags = StetasFlag.attack;
        //選択スタート
        StartFlag = true;
    }
    //アタックのコールバック
    public void PlayerAttack(int movepoint)
    {
        Debug.Log("PlayerAttack");
        //キャンセルフラグ
        if (movepoint==-1)
        {
            character.StetasFlags = StetasFlag.attack;
            //選択スタート
            StartFlag = true;
            return;
        }
        //どの敵を倒すか選択する
        var enemyData = EnemyDatas[movepoint];
        //攻撃処理
        //ここは別のクラスで処理させる
        enemyData.hp -= atk.power;
        if(enemyData.hp<=0)
        {
           //エネミーが死んだときの処理を行う

        }
        character.StetasFlags = StetasFlag.end;
        //選択スタート
        StartFlag = true;

    }
}