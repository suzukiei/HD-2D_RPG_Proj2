using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField, Header("ターン順UI")]
    private TurnUI turnUI;
    //シングルトンパターン
    private static UIManager instance;
    public static UIManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<UIManager>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("UIManager");
                    instance = obj.AddComponent<UIManager>();
                }
            }
            return instance;
        }
    }
    private void Awake()
    {
        // シングルトンパターンの実装
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // ターン順UIの更新
    public void UpdateTurnUI(List<GameObject> sortedTurnList, int turnNumber)
    {
        turnUI.UpdateTurnUI(sortedTurnList, turnNumber);
    }
    //ターンを進める
    public void NextTurn()
    {
        turnUI.AdvanceTurn();
    }

}
