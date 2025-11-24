using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour
{

    [SerializeField,Header("歩く速度"),Range(0,10)]
    private float Speed;

    [SerializeField, Header("走る速度"), Range(1, 5)]


//    [SerializeField, Header("�E�_�E�b�E�V�E��E��E�̑��E�x"), Range(1, 5)]
    private float DashSpeed;

    private Rigidbody rb;

    private Animator animator;
    [SerializeField] private InputActionAsset inputActions;
    private InputAction moveAction;
    private InputAction dashAction;
    private InputAction selectAction;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        var controllers = Input.GetJoystickNames();
        // Rigidbodyの設定
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation; // 回転を固定
        }
        if (inputActions != null)
        {
            var playerActionMap = inputActions.FindActionMap("Player");
            moveAction = playerActionMap.FindAction("Move");
            dashAction = playerActionMap.FindAction("Dash");
            selectAction = playerActionMap.FindAction("Select");

            moveAction.Enable();
            dashAction.Enable();
            selectAction.Enable();

            Debug.Log("inputActionsを正常に取り込んだ");
            foreach(var device in InputSystem.devices)
            {
                Debug.Log(device.name + ":" + device.GetType().Name);
            }

        }

    }

    // Update is called once per frame
    void Update()
    {

        Vector2 inputVector = moveAction.ReadValue<Vector2>();
        // 移動方向を計算
        Vector3 moveDirection = Vector3.zero;
        bool isMoving = false;
        bool isDash = false;
        PadTest(inputVector);

        if (Input.GetKey(KeyCode.D) || inputVector.x > 0.5f)
        {
            moveDirection.z -= 1;
            animator.SetInteger("direction", 3);
            isMoving = true;
        }
        if (Input.GetKey(KeyCode.A) || inputVector.x < -0.5f)
        {
            moveDirection.z += 1;
            animator.SetInteger("direction", 2);
            isMoving = true;
        }
        if (Input.GetKey(KeyCode.W) || inputVector.y > 0.5f)
        {
            moveDirection.x += 1;
            animator.SetInteger("direction", 0);
            isMoving = true;
        }
        if (Input.GetKey(KeyCode.S) || inputVector.y < -0.5f)
        {
            moveDirection.x -= 1;
            animator.SetInteger("direction", 1);
            isMoving = true;
        }

        animator.SetBool("isMoving", isMoving);

        // ダッシュ判定
        float currentSpeed = Speed;
        if (Input.GetKey(KeyCode.LeftShift) || dashAction.IsPressed())
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

    void OnEnable()
    {
        moveAction?.Enable();
        dashAction?.Enable();
        selectAction?.Enable();
    }

    void OnDisable()
    {
        moveAction?.Disable();
        dashAction?.Disable();
        selectAction?.Disable();
    }

    void PadTest(Vector2 vector)
    {
        Debug.Log("x" + vector.x);
        Debug.Log("y" + vector.y);
         
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
                // GameManagerが存在しない従来の方法でシーン遷移
                Debug.LogWarning("GameManagerが見つかりません。直接シーン遷移します、E");
                SceneManager.LoadScene("turnTestScene");
            }
        }
    }
}
