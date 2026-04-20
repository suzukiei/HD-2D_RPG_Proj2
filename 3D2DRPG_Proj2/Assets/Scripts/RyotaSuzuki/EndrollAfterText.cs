using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;
using System.IO;

/// <summary>
/// プロローグ用のテキスト表示システム
/// ConversationUIをベースにシンプル化
/// </summary>
public class EndRollAfterText : MonoBehaviour
{
    [Header("UI要素")]
    [SerializeField, Tooltip("黒背景パネル")]
    private GameObject backgroundPanel;

    [SerializeField, Tooltip("テキスト表示用")]
    private TextMeshProUGUI prologueText;

    [Header("アニメーション設定")]
    [SerializeField, Tooltip("フェードイン時間")]
    private float fadeInDuration = 1.0f;

    [SerializeField, Tooltip("フェードアウト時間")]
    private float fadeOutDuration = 1.0f;

    [SerializeField, Tooltip("テキストの流れる速さ")]
    private float typewriterSpeed = 0.05f;

    [SerializeField, Tooltip("次のテキストまでの待機時間")]
    private float waitBetweenTexts = 2.0f;

    [Header("CSV設定")]
    [SerializeField, Tooltip("CSVフォルダ名")]
    private string csvFolderName = "ScenarioCSV";

    [SerializeField, Tooltip("プロローグCSVファイル名")]
    private string prologueCsvFileName = "prologue.csv";

    [SerializeField, Tooltip("CSVファイルを使用するか")]
    private bool useCSVFile = true;

    [Header("テスト用テキスト")]
    [TextArea(3, 10)]
    [SerializeField]
    private List<string> testTexts = new List<string>
    {
        "プレイいただきありがとうございました。",
        "ゲーム画面へ戻ります。"
    };

    [Header("開始設定")]
    [SerializeField, Tooltip("シーン開始時に自動的にプロローグを開始するか")]
    private bool autoStartOnSceneLoad = true;

    [Header("完了後の動作")]
    [SerializeField, Tooltip("プロローグ終了後に自動で非表示にするか")]
    private bool autoHideOnComplete = true;

    [SerializeField, Tooltip("プロローグ終了後に次のシーンへ遷移するか")]
    private bool autoLoadNextScene = false;

    [SerializeField, Tooltip("次のシーン名")]
    private string nextSceneName = "Map";

    [SerializeField, Tooltip("完了時に呼び出すイベント")]
    private UnityEngine.Events.UnityEvent onPrologueComplete;

    private List<string> prologueTexts = new List<string>();
    private int currentTextIndex = 0;
    private bool isTyping = false;
    private bool isSkipping = false;
    private Coroutine currentCoroutine;
    private CanvasGroup canvasGroup;

    [SerializeField, Tooltip("エンドロール終わってるか")]
    public EndRollScript endrollScript;

    void Awake()
    {

    }

    void Start()
    {

         prologueTexts = new List<string>(testTexts);
        
    }

    void Update()
    {
        if(endrollScript.isStopEndRoll == true)
        {
            // スペースキーまたはマウスクリックでスキップ
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                if (isTyping)
                {
                    // タイピング中ならスキップ
                    isSkipping = true;
                }
                else
                {
                    // 次のテキストへ
                    ShowNextText();
                }
            }
        }
    }


    /// <summary>
    /// プロローグを開始
    /// </summary>
    public void StartPrologue()
    {
        if (prologueTexts.Count == 0)
        {
            Debug.LogWarning("プロローグテキストがありません。");
            return;
        }

        currentTextIndex = 0;

        // フェードイン
        //if (canvasGroup != null)
        //{
        //    canvasGroup.alpha = 0;
        //    canvasGroup.DOFade(1, fadeInDuration).OnComplete(() =>
        //    {
        //        ShowText(currentTextIndex);
        //    });
        //}
        //else
        //{
        ShowText(currentTextIndex);
        //}
    }

    /// <summary>
    /// 指定インデックスのテキストを表示
    /// </summary>
    void ShowText(int index)
    {
        if (index >= prologueTexts.Count)
        {
            EndPrologue();
            return;
        }

        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);

        currentCoroutine = StartCoroutine(TypewriterEffect(prologueTexts[index]));
    }

    /// <summary>
    /// タイプライター効果
    /// </summary>
    IEnumerator TypewriterEffect(string text)
    {
        isTyping = true;
        isSkipping = false;
        prologueText.text = "";

        for (int i = 0; i < text.Length; i++)
        {
            prologueText.text += text[i];

            if (!isSkipping)
            {
                yield return new WaitForSeconds(typewriterSpeed);
            }
        }

        isTyping = false;

        // 次のテキストまで待機
        yield return new WaitForSeconds(waitBetweenTexts);

        // 自動的に次へ
        ShowNextText();
    }

    /// <summary>
    /// 次のテキストを表示
    /// </summary>
    void ShowNextText()
    {
        if (isTyping) return;

        currentTextIndex++;
        ShowText(currentTextIndex);
    }

    /// <summary>
    /// プロローグを終了
    /// </summary>
    void EndPrologue()
    {
        //if (canvasGroup != null)
        //{
        //    canvasGroup.DOFade(0, fadeOutDuration).OnComplete(() =>
        //    {
        //        CompletePrologue();
        //    });
        //}
        //else
        //{
        CompletePrologue();
        // }
    }

    /// <summary>
    /// プロローグ完了処理
    /// </summary>
    void CompletePrologue()
    {
        if (autoHideOnComplete && backgroundPanel != null)
        {
            backgroundPanel.SetActive(false);
        }

        // 完了イベントを呼び出し
        onPrologueComplete?.Invoke();

        // SceneTransitionManagerがあれば使用（画面全体のフェード）
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadSceneWithFade(nextSceneName);
        }
        else
        {
            // なければ直接遷移
            SceneManager.LoadScene(nextSceneName);
        }
    }
}

