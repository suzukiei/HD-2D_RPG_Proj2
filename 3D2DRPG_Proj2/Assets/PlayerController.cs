using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.SceneManagement;


public class PlayerController : MonoBehaviour
{

    [SerializeField,Header("歩きの速度"),Range(0,10)]
    private float Speed;

    [SerializeField, Header("ダッシュの速度"), Range(1, 10)]


//    [SerializeField, Header("・ｽ_・ｽb・ｽV・ｽ・ｽ・ｽﾌ托ｿｽ・ｽx"), Range(1, 5)]
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
        }
    }

    /// <summary>
    /// 謨ｵ縺ｨ縺ｮ陦晉ｪ∵､懃衍
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // GameManager繧帝壹＠縺ｦ繝舌ヨ繝ｫ髢句ｧ・
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartBattle(transform.position, collision.gameObject);
            }
            else
            {
                // GameManager縺悟ｭ伜惠縺励↑縺・ｴ蜷医・蠕捺擂縺ｮ譁ｹ豕輔〒繧ｷ繝ｼ繝ｳ驕ｷ遘ｻ
                Debug.LogWarning("GameManager縺瑚ｦ九▽縺九ｊ縺ｾ縺帙ｓ縲ら峩謗･繧ｷ繝ｼ繝ｳ驕ｷ遘ｻ縺励∪縺吶・");
                SceneManager.LoadScene("turnTestScene");
            }
        }
    }
}
