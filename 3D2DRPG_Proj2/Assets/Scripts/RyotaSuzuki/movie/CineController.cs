using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// アニメーション制御方法
/// </summary>
public enum AnimationControlType
{
    None,           // アニメーション制御しない
    DirectState,    // 状態名を直接指定（animator.Play）
    Parameters      // パラメータで制御（IsMoving, direction など）
}

/// <summary>
/// 複数キャラクターの位置を管理するムービーコントローラー
/// </summary>
[System.Serializable]
public class CharacterMovieData
{
    [Tooltip("移動させるキャラクター")]
    public GameObject character;
    
    [Tooltip("ムービー中の位置（nullの場合は移動しない）")]
    public Transform moviePosition;
    
    [Tooltip("元の位置に戻すか")]
    public bool returnToOriginal = true;
    
    [Tooltip("回転も元に戻すか（falseなら位置だけ）")]
    public bool returnRotation = false;
    
    [Header("アニメーション設定")]
    [Tooltip("アニメーション制御方法")]
    public AnimationControlType animationControlType = AnimationControlType.DirectState;
    
    [Tooltip("直接指定する状態名（DirectState使用時）")]
    public string movieAnimationState = "Idle";
    
    [Tooltip("IsMovingパラメータの値（Parameters使用時）")]
    public bool setIsMoving = false;
    
    [Tooltip("directionパラメータの値（Parameters使用時、0-4）")]
    public int setDirection = 0;
    
    [Tooltip("IsDashパラメータの値（Parameters使用時）")]
    public bool setIsDash = false;
    
    [Tooltip("最初にパラメータをリセットするか")]
    public bool resetAnimatorParameters = true;
    
    // 保存用
    [HideInInspector] public Vector3 savedPosition;
    [HideInInspector] public Quaternion savedRotation;
}

public class CineController : MonoBehaviour
{
    [Header("ムービー設定")]
    [SerializeField, Tooltip("再生するPlayableDirector")] 
    private PlayableDirector playableDirector;
    
    [SerializeField, Tooltip("一度だけ再生するか")]
    private bool playOnce = true;
    
    [Header("キャラクター設定")]
    [SerializeField, Tooltip("管理するキャラクター一覧")]
    private List<CharacterMovieData> characters = new List<CharacterMovieData>();
    
    [Header("制御設定")]
    [SerializeField, Tooltip("ムービー中プレイヤー操作を無効化するか")]
    private bool disablePlayerControl = true;
    
    [SerializeField, Tooltip("フェードイン・アウトを使用するか")]
    private bool useFade = false;
    
    [SerializeField, Tooltip("フェード時間")]
    private float fadeDuration = 1f;
    
    [SerializeField, Tooltip("フェード用Image（nullの場合は自動生成）")]
    private Image fadeImage;
    
    private bool isPlaying = false;
    private bool hasPlayed = false;
    private Canvas fadeCanvas;
    private GameObject fadePanel;
    
    void Start()
    {
        Debug.Log($"[CineController] Start実行 - このGameObjectはアクティブ: {gameObject.name}");
        
        // PlayableDirectorのイベント登録
        if (playableDirector != null)
        {
            playableDirector.played += OnMovieStarted;
            playableDirector.stopped += OnMovieStopped;
        }
        
        // フェード機能を使用する場合、フェード用UIを準備
        if (useFade)
        {
            SetupFadeUI();
        }
    }
    
    void OnEnable()
    {
        Debug.Log($"[CineController] OnEnable - {gameObject.name}がアクティブになりました");
    }
    
    void OnDisable()
    {
        Debug.Log($"[CineController] OnDisable - {gameObject.name}が非アクティブになりました");
    }
    
    /// <summary>
    /// ムービーを再生（外部から呼び出し用）
    /// </summary>
    public void PlayMovie()
    {
        if (isPlaying)
        {
            Debug.LogWarning("既にムービーが再生中です");
            return;
        }
        
        if (playableDirector == null)
        {
            Debug.LogError("PlayableDirectorが設定されていません");
            return;
        }
        
        StartCoroutine(PlayMovieCoroutine());
    }
    
    /// <summary>
    /// ムービー再生のコルーチン
    /// </summary>
    private IEnumerator PlayMovieCoroutine()
    {
        isPlaying = true;
        hasPlayed = true;
        
        // フェードアウト
        if (useFade)
        {
            yield return StartCoroutine(FadeOut());
        }
        
        // キャラクターの位置を保存して移動
        foreach (var charData in characters)
        {
            if (charData.character == null) continue;
            
            // 元の位置を保存
            charData.savedPosition = charData.character.transform.position;
            charData.savedRotation = charData.character.transform.rotation;
            
            // ムービー位置に移動
            if (charData.moviePosition != null)
            {
                charData.character.transform.position = charData.moviePosition.position;
                charData.character.transform.rotation = charData.moviePosition.rotation;
                Debug.Log($"{charData.character.name} をムービー位置に移動しました");
            }
            
            // アニメーション制御
            SetMovieAnimation(charData);
            
            // プレイヤー操作を無効化
            if (disablePlayerControl)
            {
                DisableControl(charData.character);
            }
        }
        
        // フェードイン
        if (useFade)
        {
            yield return StartCoroutine(FadeIn());
        }
        
        // ムービー再生
        playableDirector.Play();
        Debug.Log("ムービー再生開始");
    }
    
    /// <summary>
    /// ムービー開始時のコールバック
    /// </summary>
    private void OnMovieStarted(PlayableDirector director)
    {
        Debug.Log("ムービー再生中...");
    }
    
    /// <summary>
    /// ムービー終了時のコールバック
    /// </summary>
    private void OnMovieStopped(PlayableDirector director)
    {
        if (!isPlaying) return;
        
        StartCoroutine(StopMovieCoroutine());
    }
    
    /// <summary>
    /// ムービー終了時の処理
    /// </summary>
    private IEnumerator StopMovieCoroutine()
    {
        // フェードアウト
        if (useFade)
        {
            yield return StartCoroutine(FadeOut());
        }
        
        // キャラクターを元の位置に戻す
        foreach (var charData in characters)
        {
            if (charData.character == null) continue;
            
            // 元の位置に戻す
            if (charData.returnToOriginal)
            {
                charData.character.transform.position = charData.savedPosition;
                
                // 回転も戻す設定の場合のみ
                if (charData.returnRotation)
                {
                    charData.character.transform.rotation = charData.savedRotation;
                    Debug.Log($"{charData.character.name} を元の位置・回転に戻しました");
                }
                else
                {
                    Debug.Log($"{charData.character.name} を元の位置に戻しました（回転は維持）");
                }
            }
            
            // プレイヤー操作を有効化
            if (disablePlayerControl)
            {
                EnableControl(charData.character);
            }
        }
        
        // フェードイン
        if (useFade)
        {
            yield return StartCoroutine(FadeIn());
        }
        
        isPlaying = false;
        Debug.Log("ムービー終了");
    }
    
    /// <summary>
    /// キャラクターの操作を無効化
    /// </summary>
    private void DisableControl(GameObject character)
    {
        // MonoBehaviourコンポーネントを無効化
        var controllers = character.GetComponents<MonoBehaviour>();
        foreach (var controller in controllers)
        {
            if (controller != null && controller.enabled)
            {
                controller.enabled = false;
            }
        }
        
        // CharacterController
        var characterController = character.GetComponent<CharacterController>();
        if (characterController != null)
        {
            characterController.enabled = false;
        }
        
        // Rigidbody
        var rb = character.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
    }
    
    /// <summary>
    /// キャラクターの操作を有効化
    /// </summary>
    private void EnableControl(GameObject character)
    {
        // MonoBehaviourコンポーネントを有効化
        var controllers = character.GetComponents<MonoBehaviour>();
        foreach (var controller in controllers)
        {
            if (controller != null && !controller.enabled)
            {
                controller.enabled = true;
            }
        }
        
        // CharacterController
        var characterController = character.GetComponent<CharacterController>();
        if (characterController != null)
        {
            characterController.enabled = true;
        }
        
        // Rigidbody
        var rb = character.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
        }
    }
    
    /// <summary>
    /// フェード用UIをセットアップ
    /// </summary>
    private void SetupFadeUI()
    {
        if (fadeImage == null)
        {
            // フェード用のCanvasとImageを自動生成
            GameObject canvasObj = new GameObject("FadeCanvas");
            fadeCanvas = canvasObj.AddComponent<Canvas>();
            fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            fadeCanvas.sortingOrder = 9999; // 最前面に表示
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // フェード用パネル
            fadePanel = new GameObject("FadePanel");
            fadePanel.transform.SetParent(canvasObj.transform, false);
            
            fadeImage = fadePanel.AddComponent<Image>();
            fadeImage.color = Color.black;
            
            // 画面全体を覆うように設定
            RectTransform rectTransform = fadePanel.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            
            // 最初は非表示
            fadePanel.SetActive(false);
            
            Debug.Log("フェード用UIを自動生成しました");
        }
        else
        {
            // 手動設定されたfadeImageの場合
            fadePanel = fadeImage.gameObject;
            fadePanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// フェードアウト（画面を暗くする）
    /// </summary>
    private IEnumerator FadeOut()
    {
        if (fadeImage == null)
        {
            Debug.LogWarning("FadeImageが設定されていません");
            yield break;
        }
        
        fadePanel.SetActive(true);
        
        // 透明から黒へ
        fadeImage.color = new Color(0, 0, 0, 0);
        yield return fadeImage.DOFade(1f, fadeDuration).WaitForCompletion();
    }
    
    /// <summary>
    /// フェードイン（画面を明るくする）
    /// </summary>
    private IEnumerator FadeIn()
    {
        if (fadeImage == null)
        {
            Debug.LogWarning("FadeImageが設定されていません");
            yield break;
        }
        
        // 黒から透明へ
        fadeImage.color = new Color(0, 0, 0, 1);
        yield return fadeImage.DOFade(0f, fadeDuration).WaitForCompletion();
        
        fadePanel.SetActive(false);
    }
    
    /// <summary>
    /// ムービーを強制停止
    /// </summary>
    public void StopMovie()
    {
        if (playableDirector != null && isPlaying)
        {
            playableDirector.Stop();
        }
    }
    
    /// <summary>
    /// ムービーをスキップ
    /// </summary>
    public void SkipMovie()
    {
        if (playableDirector != null && isPlaying)
        {
            playableDirector.time = playableDirector.duration;
            playableDirector.Stop();
        }
    }
    
    /// <summary>
    /// ムービー用アニメーションを設定
    /// </summary>
    private void SetMovieAnimation(CharacterMovieData data)
    {
        Animator animator = data.character.GetComponent<Animator>();
        if (animator == null) return;
        
        // Animatorパラメータをリセット
        if (data.resetAnimatorParameters)
        {
            // プロジェクトで使用しているパラメータをリセット（小文字のi）
            ResetAnimatorParameter(animator, "direction");
            ResetAnimatorParameter(animator, "isMoving");
            ResetAnimatorParameter(animator, "isDash");
            
            // 汎用的なパラメータもリセット（存在すれば）
            ResetAnimatorParameter(animator, "Speed");
            ResetAnimatorParameter(animator, "MoveSpeed");
            ResetAnimatorParameter(animator, "Velocity");
            
            Debug.Log($"[CineController] {data.character.name} のAnimatorパラメータをリセットしました");
        }
        
        // アニメーション制御タイプに応じて処理
        switch (data.animationControlType)
        {
            case AnimationControlType.None:
                // 何もしない
                break;
                
            case AnimationControlType.DirectState:
                // 状態名を直接指定
                if (!string.IsNullOrEmpty(data.movieAnimationState))
                {
                    animator.Play(data.movieAnimationState);
                    Debug.Log($"[CineController] {data.character.name} のアニメーションを {data.movieAnimationState} に設定しました");
                }
                break;
                
            case AnimationControlType.Parameters:
                // パラメータで制御（順序重要：isMovingを最後に設定）
                SetAnimatorParameter(animator, "direction", data.setDirection);
                SetAnimatorParameter(animator, "isDash", data.setIsDash);
                SetAnimatorParameter(animator, "isMoving", data.setIsMoving);
                
                Debug.Log($"[CineController] {data.character.name} のパラメータを設定: isMoving={data.setIsMoving}, direction={data.setDirection}, isDash={data.setIsDash}");
                
                // 念のため、設定後の実際の値も確認
                bool hasIsMoving = false;
                foreach (var param in animator.parameters)
                {
                    if (param.name == "isMoving") { hasIsMoving = true; break; }
                }
                
                if (hasIsMoving)
                {
                    Debug.Log($"[CineController] 実際の値: isMoving={animator.GetBool("isMoving")}, direction={animator.GetInteger("direction")}, isDash={animator.GetBool("isDash")}");
                }
                break;
        }
    }
    
    /// <summary>
    /// Animatorパラメータを設定（存在する場合のみ）
    /// </summary>
    private void SetAnimatorParameter(Animator animator, string parameterName, object value)
    {
        foreach (var param in animator.parameters)
        {
            if (param.name == parameterName)
            {
                switch (param.type)
                {
                    case AnimatorControllerParameterType.Float:
                        animator.SetFloat(parameterName, System.Convert.ToSingle(value));
                        break;
                    case AnimatorControllerParameterType.Int:
                        animator.SetInteger(parameterName, System.Convert.ToInt32(value));
                        break;
                    case AnimatorControllerParameterType.Bool:
                        animator.SetBool(parameterName, System.Convert.ToBoolean(value));
                        break;
                }
                return;
            }
        }
    }
    
    /// <summary>
    /// Animatorパラメータをリセット（存在する場合のみ）
    /// </summary>
    private void ResetAnimatorParameter(Animator animator, string parameterName)
    {
        foreach (var param in animator.parameters)
        {
            if (param.name == parameterName)
            {
                switch (param.type)
                {
                    case AnimatorControllerParameterType.Float:
                        animator.SetFloat(parameterName, 0f);
                        break;
                    case AnimatorControllerParameterType.Int:
                        animator.SetInteger(parameterName, 0);
                        break;
                    case AnimatorControllerParameterType.Bool:
                        animator.SetBool(parameterName, false);
                        break;
                    case AnimatorControllerParameterType.Trigger:
                        animator.ResetTrigger(parameterName);
                        break;
                }
                return;
            }
        }
    }
    
    void OnDestroy()
    {
        // イベントの登録解除
        if (playableDirector != null)
        {
            playableDirector.played -= OnMovieStarted;
            playableDirector.stopped -= OnMovieStopped;
        }
        
        // 自動生成したフェード用UIを削除
        if (fadeCanvas != null)
        {
            Destroy(fadeCanvas.gameObject);
        }
    }
}

