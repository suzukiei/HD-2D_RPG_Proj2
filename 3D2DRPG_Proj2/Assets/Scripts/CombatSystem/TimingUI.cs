using UnityEngine;
using UnityEngine.UI;

public class TimingUI : MonoBehaviour
{
    [SerializeField] private Slider timingBar;
    [SerializeField] private float speed = 2.0f;
    [SerializeField] private KeyCode attackKey = KeyCode.Space;
    [SerializeField] private float timingtime; // 差分
    [SerializeField] private float timingWindowEnd = 0.6f;   // タイミング受付終了
    private float value = 0f;
    public bool isActive = false;

    public void Show(float _timingtime, float _timingWindowEnd)
    {
        // タイミング差分とエンドタイムを設定
        //timingtime = _timingtime;
        timingtime = 1f / timingWindowEnd*_timingtime; // 正規化
        timingWindowEnd = _timingWindowEnd;
        Debug.Log("TimingUI Show called with timingtime: " + timingtime + ", timingWindowEnd: " + timingWindowEnd);
        //エンドタイムを使用してSliderのスビードを設定
        speed = 1f / timingWindowEnd;
        timingBar.gameObject.SetActive(true);
        value = 0f;
        isActive = true;
    }

    public void Hide()
    {
        isActive = false;
        timingBar.gameObject.SetActive(false);
    }

    public bool IsTimingSuccess()
    {
        if (!isActive) return false;

        // バーを動かす
        value += Time.deltaTime * speed;
        timingBar.value = Mathf.PingPong(value, 1f);

        // 一定範囲で押せたら成功（例：中央 ±0.1）
        if (Input.GetKeyDown(attackKey))
        {
            Debug.Log("Timing Key Pressed at: " + timingBar.value);
            float diff = Mathf.Abs(timingBar.value);
            if (diff > Mathf.Abs (timingtime-1f))
            {
                isActive = true;// 成功
                Debug.Log("Timing Success with diff: " + diff);
            }
            else
            {
                // タイミング受付終了
                isActive = false;// 失敗
                Debug.Log("Timing Failed with diff: " + diff);
            }
            return true;// 押された
        }
        return false;// 押されてない
    }
}
