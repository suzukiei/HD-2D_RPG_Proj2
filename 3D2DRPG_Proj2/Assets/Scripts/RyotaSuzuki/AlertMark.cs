using UnityEngine;
using DG.Tweening;

/// <summary>
/// !マークを表示・アニメーションするスクリプト
/// DOTween使用版（より滑らかなアニメーション）
/// </summary>
public class AlertMark : MonoBehaviour
{
    [Header("アニメーション設定")]
    [SerializeField] private float popDuration = 0.3f;        // 出現アニメーション時間
    [SerializeField] private float bounceDuration = 0.5f;     // バウンスアニメーション時間
    [SerializeField] private float bounceHeight = 0.3f;       // バウンスの高さ
    [SerializeField] private int bounceCount = 2;             // バウンス回数
    
    private Vector3 originalScale;
    private Vector3 originalPosition;
    
    void Start()
    {
        originalScale = transform.localScale;
        originalPosition = transform.localPosition;
        
        // 出現アニメーション
        PlayAppearAnimation();
    }
    
    /// <summary>
    /// 出現アニメーション
    /// </summary>
    private void PlayAppearAnimation()
    {
        // 初期状態を設定（小さく）
        transform.localScale = Vector3.zero;
        
        // DOTweenを使ったアニメーション
        Sequence sequence = DOTween.Sequence();
        
        // ポップアップ
        sequence.Append(transform.DOScale(originalScale * 1.2f, popDuration * 0.7f).SetEase(Ease.OutBack));
        sequence.Append(transform.DOScale(originalScale, popDuration * 0.3f).SetEase(Ease.InOutQuad));
        
        // バウンスアニメーション
        if (bounceCount > 0)
        {
            sequence.Append(transform.DOLocalMoveY(originalPosition.y + bounceHeight, bounceDuration / 2f)
                .SetEase(Ease.OutQuad)
                .SetLoops(bounceCount * 2, LoopType.Yoyo));
        }
    }
    
    /// <summary>
    /// 消失アニメーション
    /// </summary>
    public void PlayDisappearAnimation(float duration = 0.2f)
    {
        transform.DOScale(Vector3.zero, duration).SetEase(Ease.InBack).OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }
    
    void OnDestroy()
    {
        // DOTweenのアニメーションをクリーンアップ
        transform.DOKill();
    }
}

