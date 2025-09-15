using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMove : MonoBehaviour
{
    Rigidbody rigidbody;
    [SerializeField]float speed = 10;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = this.GetComponent<Rigidbody>();
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.D))
        {
            this.transform.Translate(0.01f, 0, 0);
        }
        if (Input.GetKey(KeyCode.A))
        {
            this.transform.Translate(-0.01f, 0, 0);
        }
        if (Input.GetKey(KeyCode.W))
        {
            this.transform.Translate(0, 0, 0.05f);
        }
        if (Input.GetKey(KeyCode.S))
        {
            this.transform.Translate(0, 0, -0.01f);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // GameManagerを通してバトル開始
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartBattle(transform.position, collision.gameObject);
            }
            else
            {
                // GameManagerが存在しない場合は従来の方法でシーン遷移
                Debug.LogWarning("GameManagerが見つかりません。直接シーン遷移します。");
                SceneManager.LoadScene("turnTestScene");
            }
        }
    }

}
