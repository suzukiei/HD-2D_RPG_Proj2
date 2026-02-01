using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CutIn : MonoBehaviour
{
    //カットインキャンバス
    [SerializeField] CanvasGroup CutInCanvas;
    
    //カットインキャラの位置
    [SerializeField] Transform cutInUnitPos;

    //アニメーション
    //[SerializeField] Animator animator;

    //int CutInParamHash = Animator.StringToHash("CutIn");
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [ContextMenu("イベントを発火（テスト）")]
    public void SuguruCutIn()
    {
        //Live2Dなどで動かすCutInがあれば使う時が来るかもしれない
        //animator.SetTrigger(CutInParamHash);

        // 初期位置（画面外）
        Vector2 startPos = new Vector2(-800f, 0f);
        // 表示位置
        Vector2 showPos = new Vector2(0f, 0f);
        // 退場位置
        Vector2 endPos = new Vector2(800f, 0f);

        // 念のため初期化
        cutInUnitPos.localPosition = startPos;
        CutInCanvas.alpha = 0f;

        var sequence = DOTween.Sequence();

        sequence
            // スライドイン
            .Append(cutInUnitPos.DOLocalMove(showPos, 0.6f).SetEase(Ease.OutCubic)
                .SetEase(Ease.OutCubic))
            // フェードを途中から
            .Insert(0.1f, CutInCanvas.DOFade(1f, 0.6f).SetEase(Ease.OutQuad))

            .AppendInterval(1.5f)

            // 軽い溜め
            .Append(cutInUnitPos.DOPunchPosition(
                new Vector2(20f, 0f), 0.2f))

            // スライドアウト
            .Append(cutInUnitPos.DOLocalMove(endPos, 0.25f)
                .SetEase(Ease.InCubic))
            .Join(CutInCanvas.DOFade(0f, 0.2f))
            // 初期化
            .AppendCallback(() =>
            {
                cutInUnitPos.localPosition = startPos;
                CutInCanvas.alpha = 0f;
            });
    }
}
