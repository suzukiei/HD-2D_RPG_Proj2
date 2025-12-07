using UnityEngine;

/// <summary>
/// マップを無限にスクロールさせる（オープニング演出用）
/// </summary>
public class InfiniteMapScroller : MonoBehaviour
{
    [Header("スクロール設定")]
    [SerializeField, Tooltip("スクロール速度")]
    private float scrollSpeed = 5f;

    [SerializeField, Tooltip("スクロール方向（通常は-Z方向）")]
    private Vector3 scrollDirection = Vector3.back;

    [Header("ループ設定")]
    [SerializeField, Tooltip("マップの長さ（Z軸）- ワープする距離")]
    private float mapLength = 100f;

    [Header("制御")]
    [SerializeField, Tooltip("自動でスクロールを開始するか")]
    private bool autoStart = true;

    private Vector3 startPosition;
    private bool isScrolling = false;

    void Start()
    {
        // 開始位置を記録
        startPosition = transform.position;

        if (autoStart)
        {
            StartScrolling();
        }
    }

    void Update()
    {
        if (!isScrolling) return;

        // マップをスクロール
        transform.position += scrollDirection.normalized * scrollSpeed * Time.deltaTime;

        // ループ処理（指定距離移動したら元の位置に戻す）
        float movedDistance = Vector3.Distance(startPosition, transform.position);
        
        if (movedDistance >= mapLength)
        {
            // 元の位置に戻す（ワープ）
            transform.position = startPosition;
        }
    }

    /// <summary>
    /// スクロール開始
    /// </summary>
    public void StartScrolling()
    {
        isScrolling = true;
    }

    /// <summary>
    /// スクロール停止
    /// </summary>
    public void StopScrolling()
    {
        isScrolling = false;
    }

    /// <summary>
    /// スクロール速度を変更
    /// </summary>
    public void SetScrollSpeed(float speed)
    {
        scrollSpeed = speed;
    }
}

