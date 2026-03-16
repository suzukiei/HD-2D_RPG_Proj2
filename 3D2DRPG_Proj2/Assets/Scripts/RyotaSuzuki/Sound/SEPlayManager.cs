using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SEPlayManager : MonoBehaviour
{
    public static SEPlayManager Instance;

    /// <summary>
    /// 発火ソース
    /// </summary>
    public AudioSource seSource;

    /// <summary>
    /// SEソース
    /// </summary>

    public AudioClip select;

    public AudioClip enter;

    

    //都度追加
    public enum SE
    {
        Select,
        Enter
    }

    // Start is called before the first frame update
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    /// <summary>
    /// SEを鳴らす
    /// </summary>
    /// <param name="se"></param>
    
    public void PlaySE(SE se)
    {
        switch (se)
        {
            case SE.Select:
                seSource.PlayOneShot(select);
                break;

            case SE.Enter:
                seSource.PlayOneShot(enter);
                break;

        }
    }
}
