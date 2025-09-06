using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UITest : MonoBehaviour
{
    public void Inputs(UnityEvent<int> unityEvent,int i )
    {
        StartCoroutine(EventCoroutines(unityEvent,i));
    }
    private IEnumerator EventCoroutines(UnityEvent<int> unityEvent, int i)
    {

        while (true)
        {
            yield return null;
            //yield return new WaitForSeconds(0.1f);
            if (Input.GetKeyDown(KeyCode.W))
            {
                unityEvent.Invoke(1);
                break;
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                unityEvent.Invoke(2);
                break;
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                unityEvent.Invoke(3);
                break;
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                unityEvent.Invoke(4);
                break;
            }

        }
    }
}
