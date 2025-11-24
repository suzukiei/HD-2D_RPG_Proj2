using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.SceneManagement;


public class PlayerController : MonoBehaviour
{

    [SerializeField,Header("歩く速度"),Range(0,10)]
    private float Speed;

    [SerializeField, Header("走る速度"), Range(1, 5)]


//    [SerializeField, Header("�E�_�E�b�E�V�E��E��E�̑��E�x"), Range(1, 5)]
    private float DashSpeed;

    private Rigidbody rb;

    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        
        // Rigidbodyの設定
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation; // 回転を固定
        }
    }

    // Update is called once per frame
    void Update()
    {
        // 移動方向を計算
        Vector3 moveDirection = Vector3.zero;
        bool isMoving = false;
        bool isDash = false;

        if (Input.GetKey(KeyCode.D))
        {
            moveDirection.z -= 1;
            animator.SetInteger("direction", 3);
            isMoving = true;
        }
        if (Input.GetKey(KeyCode.A))
        {
            moveDirection.z = 1;
            animator.SetInteger("direction", 2);
            isMoving = true;
        }
        if (Input.GetKey(KeyCode.W))
        {
            moveDirection.x += 1;
            animator.SetInteger("direction", 0);
            isMoving = true;
        }
        if (Input.GetKey(KeyCode.S))
        {
            moveDirection.x -= 1;
            animator.SetInteger("direction", 1);
            isMoving = true;
        }

        animator.SetBool("isMoving", isMoving);

        // ダッシュ判定
        float currentSpeed = Speed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed *= DashSpeed;
            isDash = true;
           
        }

        animator.SetBool("isDash", isDash);

        // Rigidbodyで移動
        if (rb != null && moveDirection != Vector3.zero)
        {
            Vector3 newPosition = rb.position + moveDirection.normalized * currentSpeed * Time.deltaTime;
            rb.MovePosition(newPosition);
        }
    }

    /// <summary>
    /// 敵との衝突検知
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // GameManagerを通してバトル開姁E
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartBattle(transform.position, collision.gameObject);
            }
            else
            {
                // GameManagerが存在しなぁE��合�E従来の方法でシーン遷移
                Debug.LogWarning("GameManagerが見つかりません。直接シーン遷移します、E");
                SceneManager.LoadScene("turnTestScene");
            }
        }
    }
}
