using UnityEngine;
using UnityEngine.Events;
public class ComboAttack : MonoBehaviour
{
    //public Animator animator;

    [SerializeField] private float timingWindowStart = 0.4f; // 攻撃中のタイミング受付開始
    [SerializeField] private float timingWindowEnd = 0.6f;   // 攻撃中のタイミング受付終了

    private UnityEvent<int> onComboEnd;
    private UnityEvent<int> onComboAttack;
    private Character enemy;
    private int comboStep = 0;
    private int maxComboStep = 3; // 最大コンボ数
    private bool canInput = false;
    private float timer = 0f;

    private void Update()
    {
        // 攻撃中の時間経過を管理
        if (canInput)
        {
            // 攻撃入力
            if (Input.GetMouseButtonDown(0))
            {
                TryAttack();
            }
            timer += Time.deltaTime;
            Debug.Log(timer);
            if (timer > timingWindowEnd)
            {
                EndCombo(); // タイミングを逃したら終了
            }
        }
    }

    private void TryAttack()
    {
        canInput = false;
        //if (!canInput && comboStep == 0)
        //{
        //    // 最初の攻撃
        //    StartAttack(1);
        //}  else 
        //if (canInput)
        //{
        // タイミングよくクリックしたら次の攻撃へ
        if (timer >= timingWindowStart && timer <= timingWindowEnd)
        {
            NextAttack();
        }
        else
        {
            EndCombo();
        }
        //}
    }

    private void StartAttack(int step)
    {
        comboStep = step;
        //animator.SetTrigger($"Attack{step}");
        canInput = true;
        timer = 0f;
    }

    private void NextAttack()
    {
        comboStep++;
        
        Debug.Log(comboStep);
        if (comboStep > maxComboStep) // 3段コンボ上限など
        {
            EndCombo();
            return;
        }
        onComboAttack.Invoke(0);
        //animator.SetTrigger($"Attack{comboStep}");
        timer = 0f;
        canInput = true;
    }

    private void EndCombo()
    {
        comboStep = 1;
        onComboEnd.Invoke(0);
        canInput = false;
        timer = 0f;
    }
    public void Inputs(UnityEvent<int> _attackEvent, UnityEvent<int> _attackEnd, int _maxcombo, Character enemies)
    {
        onComboEnd = _attackEnd;
        onComboAttack = _attackEvent;
        enemy = enemies;
        maxComboStep = _maxcombo;
        StartAttack(0);
    }
}
