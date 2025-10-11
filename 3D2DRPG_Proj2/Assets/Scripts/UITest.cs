using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UITest : MonoBehaviour
{
    public void Inputs(UnityEvent<int> unityEvent, int i)
    {
        Debug.Log(i);
        StartCoroutine(EventCoroutines(unityEvent, i));
    }

    private IEnumerator EventCoroutines(UnityEvent<int> unityEvent, int i)
    {
        while (true)
        {
            yield return null;

            // i‚Ì’l‚É‰‚¶‚Ä‰Ÿ‚¹‚éƒL[‚ğ§Œä
            if (i <= 0 && Input.GetKeyDown(KeyCode.W))
            {
                unityEvent.Invoke(0);
                break;
            }
            if (i <= 1 && Input.GetKeyDown(KeyCode.S))
            {
                unityEvent.Invoke(1);
                break;
            }
            if (i <= 2 && Input.GetKeyDown(KeyCode.A))
            {
                unityEvent.Invoke(2);
                break;
            }
            if (i <= 3 && Input.GetKeyDown(KeyCode.D))
            {
                unityEvent.Invoke(3);
                break;
            }
        }
    }
}
