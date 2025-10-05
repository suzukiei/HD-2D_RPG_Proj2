using UnityEngine;

/// <summary>
/// !マークを表示・アニメーションするスクリプト
/// DOTween不使用版（シンプルなアニメーション）
/// </summary>
public class AlertMarkSimple : MonoBehaviour
{
    [Header("アニメーション設定")]
    [SerializeField] private float popDuration = 0.3f;        // 出現アニメーション時間
    [SerializeField] private float bounceDuration = 0.5f;     // バウンスアニメーション時間
    [SerializeField] private float bounceHeight = 0.3f;       // バウンスの高さ
    [SerializeField] private int bounceCount = 2;             // バウンス回数
    
    private Vector3 originalScale;
    private Vector3 originalPosition;
    private float animationTimer = 0f;
    private AnimationState currentState = AnimationState.PopUp;
    private int currentBounce = 0;
    
    private enum AnimationState
    {
        PopUp,
        Bounce,
        Idle
    }
    
    void Start()
    {
        originalScale = transform.localScale;
        originalPosition = transform.localPosition;
        
        // 初期状態を設定（小さく）
        transform.localScale = Vector3.zero;
        currentState = AnimationState.PopUp;
        animationTimer = 0f;
    }
    
    void Update()
    {
        animationTimer += Time.deltaTime;
        
        switch (currentState)
        {
            case AnimationState.PopUp:
                UpdatePopUpAnimation();
                break;
            case AnimationState.Bounce:
                UpdateBounceAnimation();
                break;
            case AnimationState.Idle:
                // 待機中
                break;
        }
    }
    
    /// <summary>
    /// ポップアップアニメーション
    /// </summary>
    private void UpdatePopUpAnimation()
    {
        float progress = Mathf.Min(animationTimer / popDuration, 1f);
        
        // イージング（バックイーズ）
        float easedProgress = EaseOutBack(progress);
        
        // スケールを徐々に大きくする
        if (progress < 0.7f)
        {
            float scaleProgress = progress / 0.7f;
            transform.localScale = originalScale * 1.2f * EaseOutBack(scaleProgress);
        }
        else
        {
            float scaleProgress = (progress - 0.7f) / 0.3f;
            transform.localScale = Vector3.Lerp(originalScale * 1.2f, originalScale, scaleProgress);
        }
        
        // アニメーション完了
        if (progress >= 1f)
        {
            transform.localScale = originalScale;
            currentState = AnimationState.Bounce;
            animationTimer = 0f;
            currentBounce = 0;
        }
    }
    
    /// <summary>
    /// バウンスアニメーション
    /// </summary>
    private void UpdateBounceAnimation()
    {
        if (currentBounce >= bounceCount)
        {
            currentState = AnimationState.Idle;
            transform.localPosition = originalPosition;
            return;
        }
        
        float progress = animationTimer / bounceDuration;
        
        if (progress < 1f)
        {
            // サインカーブでバウンス
            float height = Mathf.Sin(progress * Mathf.PI) * bounceHeight;
            Vector3 newPos = originalPosition;
            newPos.y += height;
            transform.localPosition = newPos;
        }
        else
        {
            // 次のバウンス
            currentBounce++;
            animationTimer = 0f;
        }
    }
    
    /// <summary>
    /// イージング関数（バックイーズアウト）
    /// </summary>
    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
    
    /// <summary>
    /// 消失アニメーション
    /// </summary>
    public void PlayDisappearAnimation(float duration = 0.2f)
    {
        StartCoroutine(DisappearCoroutine(duration));
    }
    
    private System.Collections.IEnumerator DisappearCoroutine(float duration)
    {
        Vector3 startScale = transform.localScale;
        float timer = 0f;
        
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, progress);
            yield return null;
        }
        
        Destroy(gameObject);
    }
}
