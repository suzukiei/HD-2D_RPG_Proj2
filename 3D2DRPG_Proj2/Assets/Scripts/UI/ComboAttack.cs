using UnityEngine;
using UnityEngine.Events;
public class ComboAttack : MonoBehaviour
{
    //public Animator animator;
    [SerializeField,Header("ComboUI")]
    private TimingUI timingUI;
    [SerializeField] private float timingTime = 0.4f; // タイミング差分
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
            var testing = timingUI.IsTimingSuccess();
            // 攻撃入力
            if (testing)
            {
                TryAttack();
            }
            timer += Time.deltaTime;
            //Debug.Log(timer);
            if (timer > timingWindowEnd+timingTime)
            {
                EndCombo(); // タイミングを逃したら終了
            }
        }
    }
    // 攻撃入力を試みる
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
        Debug.Log("TryAttack");
        // タイミングよくクリックしたら次の攻撃へ
        if (timingUI.isActive)
        {
            Debug.Log(timingUI.isActive);
            NextAttack();
        }
        else
        {
            EndCombo();
        }
        //}
    }
    // コンボ攻撃開始
    private void StartAttack(int step)
    {
      
        comboStep = step;
        //animator.SetTrigger($"Attack{step}");
        canInput = true;
        timer = 0f;
        timingUI.Show(timingTime,timingWindowEnd);
    }
    // 次の攻撃へ
    private void NextAttack()
    {
        comboStep++;
        
        Debug.Log(comboStep);
        if (comboStep >= maxComboStep) // 3段コンボ上限など
        {
            EndCombo();
            return;
        }
        timingUI.Show(timingTime, timingWindowEnd);
        onComboAttack.Invoke(0);
        //animator.SetTrigger($"Attack{comboStep}");
        timer = 0f;
        canInput = true;
    }

    private void EndCombo()
    {
        timingUI.Hide();
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
