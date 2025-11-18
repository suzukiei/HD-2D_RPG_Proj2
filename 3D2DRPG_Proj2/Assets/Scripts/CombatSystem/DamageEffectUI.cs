using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// カメラから見てUIとして敵の位置の前らへんに攻撃を受けた時のダメージエフェクトを表示するスクリプト
/// </summary>
public class DamageEffectUI : MonoBehaviour
{
    [SerializeField, Header("メインカメラ")]
    private Camera mainCamera;
    
    [SerializeField, Header("UIキャンバス")]
    private Canvas uiCanvas;
    
    [SerializeField, Header("ダメージエフェクトプレハブ（3Dオブジェクト、任意）")]
    private GameObject damageEffectPrefab;
    [SerializeField, Header("ダメージエフェクトのオフセット（敵の前の位置調整、ワールド座標）")]
    private Vector3 damageEffectOffset = new Vector3(0, 1, -0.5f);
    
    [SerializeField, Header("ダメージテキストプレハブ（オプション）")]
    private GameObject damageTextPrefab;
    
    [SerializeField, Header("エフェクトのオフセット（敵の前の位置調整、ワールド座標）")]
    private Vector3 effectOffset = new Vector3(0, 1, -0.5f);
    
    [SerializeField, Header("エフェクトの表示時間")]
    private float effectDuration = 1.5f;
    
    [SerializeField, Header("エフェクトの親オブジェクト（ワールド空間、任意）")]
    private Transform effectParent;

    private static DamageEffectUI instance;
    public static DamageEffectUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<DamageEffectUI>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("DamageEffectUI");
                    instance = obj.AddComponent<DamageEffectUI>();
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        // シングルトンの初期化
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // カメラが設定されていない場合は自動取得
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindObjectOfType<Camera>();
            }
        }

        // UIキャンバスが設定されていない場合は自動取得（テキスト表示用）
        if (uiCanvas == null)
        {
            uiCanvas = FindObjectOfType<Canvas>();
        }
    }

    /// <summary>
    /// 敵の位置にダメージエフェクトを表示する
    /// </summary>
    /// <param name="enemyTransform">敵のTransform</param>
    /// <param name="damage">ダメージ値（テキスト表示用）</param>
    public void ShowDamageEffect(Transform enemyTransform, float damage = 0)
    {
        if (enemyTransform == null)
        {
            Debug.LogWarning("DamageEffectUI: 敵のTransformがnullです");
            return;
        }

        // 3Dエフェクトを表示（任意、ワールド空間）
        if (damageEffectPrefab != null)
        {
            // エフェクトの位置を計算（敵の前、ワールド座標）
            Vector3 effectPosition = enemyTransform.position + damageEffectOffset;

            // 3Dオブジェクトとしてエフェクトを生成
            GameObject effect = Instantiate(
                damageEffectPrefab, 
                effectPosition, 
                Quaternion.identity,
                effectParent != null ? effectParent : null
            );

            // エフェクトのアニメーション（フェードアウトなど）
            StartCoroutine(DestroyEffectAfterDelay(effect, effectDuration));
        }

        // ダメージテキストを表示（UI、オプション）
        if (damageTextPrefab != null && damage > 0 && uiCanvas != null)
        {
            // 敵のワールド座標をスクリーン座標に変換
            Vector3 screenPosition = mainCamera != null ? mainCamera.WorldToScreenPoint(enemyTransform.position) : Camera.main.WorldToScreenPoint(enemyTransform.position);
            
            // スクリーン座標をUIキャンバスのローカル座標に変換
            RectTransform canvasRect = uiCanvas.GetComponent<RectTransform>();
            Vector2 localPoint;
            Camera textCamera = uiCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : (mainCamera != null ? mainCamera : Camera.main);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, 
                screenPosition, 
                textCamera, 
                out localPoint
            );

            ShowDamageText(localPoint, damage);
        }
    }

    /// <summary>
    /// ダメージテキストを表示する（UI）
    /// </summary>
    private void ShowDamageText(Vector2 uiPosition, float damage)
    {
        if (uiCanvas == null) return;

        GameObject textObj = Instantiate(damageTextPrefab, uiCanvas.transform);
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        if (textRect != null)
        {
            textRect.anchoredPosition = uiPosition;
        }
        else
        {
            textObj.transform.position = uiPosition;
        }

        // TextMeshProUGUIまたはTextコンポーネントを取得してダメージ値を設定
        TextMeshProUGUI tmpText = textObj.GetComponent<TextMeshProUGUI>();
        if (tmpText != null)
        {
            tmpText.text = $"-{damage:F0}";
        }
        else
        {
            Text text = textObj.GetComponent<Text>();
            if (text != null)
            {
                text.text = $"-{damage:F0}";
            }
        }

        // テキストのアニメーション（上に移動しながらフェードアウト）
        if (textRect != null)
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(textRect.DOAnchorPosY(uiPosition.y + 100, effectDuration).SetEase(Ease.OutQuad));
            
            CanvasGroup canvasGroup = textObj.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = textObj.AddComponent<CanvasGroup>();
            }
            sequence.Join(canvasGroup.DOFade(0, effectDuration).SetEase(Ease.InQuad));
            
            sequence.OnComplete(() => Destroy(textObj));
        }
        else
        {
            StartCoroutine(DestroyEffectAfterDelay(textObj, effectDuration));
        }
    }

    /// <summary>
    /// 指定時間後にエフェクトを削除する
    /// </summary>
    private IEnumerator DestroyEffectAfterDelay(GameObject effect, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (effect != null)
        {
            Destroy(effect);
        }
    }

    /// <summary>
    /// 敵のGameObjectから直接エフェクトを表示する（便利メソッド）
    /// </summary>
    public void ShowDamageEffectOnEnemy(GameObject enemy, float damage = 0)
    {
        if (enemy != null)
        {
            ShowDamageEffect(enemy.transform, damage);
        }
    }
}
