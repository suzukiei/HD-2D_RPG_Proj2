using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{

<<<<<<< HEAD
    [SerializeField,Header("�����̑��x"),Range(0,10)]
    private float Speed;

    [SerializeField, Header("�_�b�V���̑��x"), Range(1, 5)]
=======
    [SerializeField,Header("�����̑��x"),Range(0,10)]
    private float Speed;

    [SerializeField, Header("�_�b�V���̑��x"), Range(1, 5)]
>>>>>>> 5d84a7cb4476bf2dc57c8ee6772f56d6eca276f8
    private float DashSpeed;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.D))
        {
            if (Input.GetKey(KeyCode.LeftShift))
                {
                this.transform.Translate(Speed * DashSpeed *Time.deltaTime, 0, 0);
                }
            this.transform.Translate(Speed*Time.deltaTime, 0, 0);
        }
        if (Input.GetKey(KeyCode.A))
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                this.transform.Translate(-(Speed * DashSpeed * Time.deltaTime), 0, 0);
            }
            this.transform.Translate(-(Speed * Time.deltaTime), 0, 0);
        }
        if (Input.GetKey(KeyCode.W))
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                this.transform.Translate(0, 0, Speed * DashSpeed * Time.deltaTime);
            }
            this.transform.Translate(0, 0, Speed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.S))
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                this.transform.Translate(0, 0, -(Speed * DashSpeed * Time.deltaTime));
            }
            this.transform.Translate(0, 0, -(Speed * Time.deltaTime));
<<<<<<< HEAD
=======
        }
    }

    /// <summary>
    /// 敵との衝突検知
    /// </summary>
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
>>>>>>> 5d84a7cb4476bf2dc57c8ee6772f56d6eca276f8
        }
    }
}
