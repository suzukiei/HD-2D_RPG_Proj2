using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UITest : MonoBehaviour
{
    private List<Character> character = new List<Character>();
    private int index = 0;
    private int maxIndex = 0;
    private bool inputFlag = false;
    [SerializeField, Header("矢印オブジェクト")]
    private GameObject EnemyAttakPointUI;
    public void Inputs(UnityEvent<int> unityEvent, int i, List<Character> Enemys)
    {
        index = 0;
        maxIndex = i;
        character.Clear();
        character.AddRange(Enemys);
        EnemyAttakPointUI.SetActive(true);
        EnemyAttakPointUI.transform.position = character[index].CharacterObj.transform.position + new Vector3(0, 2, 0);
        Debug.Log(i);
        inputFlag = true;
        StartCoroutine(EventCoroutines(unityEvent, i));
    }

    private IEnumerator EventCoroutines(UnityEvent<int> unityEvent, int i)
    {
        while (true)
        {
            yield return null;

            // iの値に応じて押せるキーを制御
            if (Input.GetKeyDown(KeyCode.W) && inputFlag)
            {
                inputFlag = false;
                ChengePoint(1);
            }
            if (Input.GetKeyDown(KeyCode.S) && inputFlag)
            {
                inputFlag = false;
                ChengePoint(-1);
            }
            if (Input.GetKeyDown(KeyCode.Space) && inputFlag)
            {
                inputFlag = false;
                unityEvent.Invoke(index);
                EnemyAttakPointUI.SetActive(false);
                break;
            }
        }
    }
    void ChengePoint(int movepoint)
    {
        index += movepoint;
        if (index == -1)
        {
            index = maxIndex;
        }
        else if (index == maxIndex+1)
        {
            index = 0;
        }
        EnemyAttakPointUI.transform.position = character[index].CharacterObj.transform.position + new Vector3(0, 2, 0);
        inputFlag = true;
    }
}
