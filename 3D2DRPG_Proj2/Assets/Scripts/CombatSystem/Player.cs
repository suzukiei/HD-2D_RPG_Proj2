using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Player : MonoBehaviour
{
    [SerializeField,Header("UI�e�X�g")]
    UITest uITest;
    [SerializeField, Header("MAP�Ǘ�")]
    MapManager mapManager;
    [SerializeField, Header("�^�[���Ǘ�")]
    TurnManager turnManager;
    [SerializeField, Header("Character�̊i�[��")]
    private List<CharacterData> characterData;
    [SerializeField]
    private List<Vector3> vector3s;
    private List<GameObject> gameObjects = new List<GameObject>();
    private List<CharacterData> EnemyDatas=new List<CharacterData>();
    public List<CharacterData> GetCharacterData() { return characterData; }
    //�I�����Ǘ�
    public bool StartFlag = true;
    [SerializeField,Header("�I�����ꂽ�L�����N�^�[")]
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
        //������
        if (StartFlag)
        {
            //Debug.Log("StartFlag");
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
                //�R�[���o�b�N����������
                UnityEvent<int> unityEvent = new UnityEvent<int>();
                unityEvent.AddListener(PlayerSelect);
                uITest.Inputs(unityEvent, characterAttackDatas.Length);
            }
            //�s���t�F�C�Y
            if (character.StetasFlags == StetasFlag.attack)
            {
                Debug.Log("attack");
                character.StetasFlags = StetasFlag.none;
                EnemyDatas = mapManager.GetCharacterDatas(character.CharacterTransfrom);
                //�R�[���o�b�N����������
                UnityEvent<int> unityEvent = new UnityEvent<int>();
                unityEvent.AddListener(PlayerAttack);
                uITest.Inputs(unityEvent, EnemyDatas.Count);
                //character.StetasFlags = StetasFlag.none;
            }
            //�s���t�F�C�Y
            if (character.StetasFlags == StetasFlag.end)
            {
                turnManager.FlagChange();
                character.StetasFlags = StetasFlag.none;
                
            }

            StartFlag = false;
        }
    }
    //�����s��
    //�ړ����菈��
    public void PlayerMove(int movepoint)
    {
        //Debug.Log("PlayerMove");
       var vec = mapManager.PointMove(character.CharacterTransfrom, movepoint);
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
            character.CharacterTransfrom = vec;
            //Player���ړ�
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
    //�ړ����菈��
    public void PlayerController(CharacterData characterData)
    {
        character = characterData;
        character.StetasFlags = StetasFlag.move;
        //�I���X�^�[�g
        StartFlag = true;
    }
    public void PlayerSelect(int movepoint)
    {
        Debug.Log("PlayerSelect");
        //�U�����@��I��
        atk = character.skills[movepoint-1];
        character.StetasFlags = StetasFlag.attack;
        //�I���X�^�[�g
        StartFlag = true;
    }
    //�A�^�b�N�̃R�[���o�b�N
    public void PlayerAttack(int movepoint)
    {
        Debug.Log("PlayerAttack");
        //�L�����Z���t���O
        if (movepoint==-1)
        {
            character.StetasFlags = StetasFlag.attack;
            //�I���X�^�[�g
            StartFlag = true;
            return;
        }
        //�ǂ̓G��|�����I������
        var enemyData = EnemyDatas[movepoint];
        //�U������
        //�����͕ʂ̃N���X�ŏ���������
        enemyData.hp -= atk.power;
        if(enemyData.hp<=0)
        {
           //�G�l�~�[�����񂾂Ƃ��̏������s��

        }
        character.StetasFlags = StetasFlag.end;
        //�I���X�^�[�g
        StartFlag = true;

    }
}