using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.IO;

[System.Serializable]
public class DialogueData
{
    public string characterName;
    public string dialogueText;
    public Sprite characterImage;
    
    public DialogueData(string name, string text)
    {
        characterName = name;
        dialogueText = text;
        characterImage = null;
    }
}

[System.Serializable]
public class DialogueLogEntry
{
    public string characterName;
    public string dialogueText;
    public string timestamp;
    
    public DialogueLogEntry(string name, string text)
    {
        characterName = name;
        dialogueText = text;
        timestamp = System.DateTime.Now.ToString("HH:mm:ss");
    }
}

public class ConversationUI : MonoBehaviour
{
    [Header("UI要素")]
    [SerializeField,Header("会話UIのパネル")]public GameObject dialoguePanel;
    [SerializeField, Header("キャラクター名が入るテキストオブジェクト")] public TextMeshProUGUI characterNameText;
    [SerializeField, Header("会話内容が入るテキストオブジェクト")] public TextMeshProUGUI dialogueText;
    [SerializeField, Header("キャラクター画像")] public Image characterImage;
    
    [Header("アニメーション設定")]
    [Header("フェードイン設定設定")] public float fadeInDuration = 0.5f;
    [Header("テキストの流れる速さ設定設定")] public float typewriterSpeed = 0.05f;
    
    [Header("CSVファイル設定")]
    [Header("CSVの格納されているフォルダ名")] public string csvFolderName = "ScenarioCSV";
    [Header("CSVのファイル名")] public string csvFileName = "scenario01.csv";
    [Header("CSVファイルを使用する？")] public bool useCSVFile = true;
    
    [Header("ログUI設定")]
    [SerializeField, Header("ログビューアパネル")] public GameObject logPanel;
    [SerializeField, Header("ログコンテンツ（スクロールビュー内）")] public Transform logContent;
    [SerializeField, Header("ログエントリのプレハブ")] public GameObject logEntryPrefab;
    [SerializeField, Header("ログビューアのスクロールビュー")] public ScrollRect logScrollRect;
    
    [Header("テスト用会話データ")]
    [TextArea(3, 10)]
    public List<string> testDialogues = new List<string>();
    
    private List<DialogueData> dialogues = new List<DialogueData>();
    private List<DialogueLogEntry> dialogueLog = new List<DialogueLogEntry>();
    private int currentDialogueIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;
    private const int MAX_LOG_ENTRIES = 50;
    
    void Start()
    {
        // 初期化
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
            
        if (logPanel != null)
            logPanel.SetActive(false);
        
        // CSVファイルから読み込むかテストデータを使用するか
        if (useCSVFile)
        {
            LoadDialogueFromCSV();
        }
        else
        {
            LoadTestDialogues();
        }
    }
    
    void LoadDialogueFromCSV()
    {
        string csvFolderPath = Path.Combine(Application.streamingAssetsPath, csvFolderName);
        string filePath = Path.Combine(csvFolderPath, csvFileName);
        
        // デバッグ用：パス情報を表示
        Debug.Log($"StreamingAssetsパス: {Application.streamingAssetsPath}");
        Debug.Log($"ScenarioCSVフォルダパス: {csvFolderPath}");
        Debug.Log($"CSVファイルパス: {filePath}");
        
        // StreamingAssetsフォルダが存在しない場合は作成
        if (!Directory.Exists(Application.streamingAssetsPath))
        {
            Directory.CreateDirectory(Application.streamingAssetsPath);
        }
        
        // ScenarioCSVフォルダが存在しない場合は作成
        if (!Directory.Exists(csvFolderPath))
        {
            Directory.CreateDirectory(csvFolderPath);
            Debug.Log($"ScenarioCSVフォルダを作成しました: {csvFolderPath}");
        }
        
        if (File.Exists(filePath))
        {
            try
            {
                string[] lines = File.ReadAllLines(filePath);
                dialogues.Clear();
                
                // ヘッダー行をスキップ（1行目）
                for (int i = 1; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    if (string.IsNullOrEmpty(line)) continue;
                    
                    DialogueData data = ParseCSVLine(line);
                    if (data != null)
                    {
                        dialogues.Add(data);
                    }
                }
                
                Debug.Log($"CSVファイルから{dialogues.Count}行の会話データを読み込みました。");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"CSVファイルの読み込みエラー: {e.Message}");
                LoadTestDialogues(); // エラー時はテストデータを使用
            }
        }
        else
        {
            Debug.LogWarning($"CSVファイルが見つかりません: {filePath}");
            CreateSampleCSV(filePath); // サンプルCSVを作成
            LoadTestDialogues(); // テストデータを使用
        }
    }
    
    DialogueData ParseCSVLine(string csvLine)
    {
        // シンプルなCSVパーサー（カンマ区切り、クォート対応）
        List<string> fields = new List<string>();
        bool inQuotes = false;
        string currentField = "";
        
        for (int i = 0; i < csvLine.Length; i++)
        {
            char c = csvLine[i];
            
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(currentField.Trim());
                currentField = "";
            }
            else
            {
                currentField += c;
            }
        }
        fields.Add(currentField.Trim()); // 最後のフィールド
        
        if (fields.Count >= 2)
        {
            string name = fields[0].Replace("\"", ""); // クォートを削除
            string dialogue = fields[1].Replace("\"", ""); // クォートを削除
            return new DialogueData(name, dialogue);
        }
        
        return null;
    }
    
    void CreateSampleCSV(string filePath)
    {
        string sampleContent = @"Name,Dialogue
                                主人公,こんにちは！はじめまして。
                                友達,よろしくお願いします！
                                主人公,今日はいい天気ですね。
                                友達,そうですね、散歩でもしませんか？
                                主人公,いいアイデアですね！";
        
        try
        {
            File.WriteAllText(filePath, sampleContent, System.Text.Encoding.UTF8);
            Debug.Log($"サンプルCSVファイルを作成しました: {filePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"サンプルCSVファイルの作成エラー: {e.Message}");
        }
    }
    
    void LoadTestDialogues()
    {
        // テスト用データをDialogueDataに変換
        dialogues.Clear();
        
        if (testDialogues.Count == 0)
        {
            testDialogues.Add("あ");
            testDialogues.Add("い");
            testDialogues.Add("う");
            testDialogues.Add("Thank you for playing!");
        }
        
        foreach (string text in testDialogues)
        {
            dialogues.Add(new DialogueData("システム", text));
        }
    }
    
    public void StartDialogue()
    {
        if (dialogues.Count == 0) return;
        
        currentDialogueIndex = 0;
        dialoguePanel.SetActive(true);
        
        // パネルをフェードイン
        CanvasGroup canvasGroup = dialoguePanel.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.DOFade(1, fadeInDuration);
        }
        
        ShowDialogue(currentDialogueIndex);
    }
    
    void ShowDialogue(int index)
    {
        if (index >= dialogues.Count) return;
        
        DialogueData data = dialogues[index];
        
        // キャラクター名設定
        if (characterNameText != null)
        {
            characterNameText.text = data.characterName;
            Debug.Log($"キャラクター名設定: {data.characterName}");
            Debug.Log($"CharacterNameText位置: {characterNameText.transform.position}");
            Debug.Log($"CharacterNameTextサイズ: {characterNameText.fontSize}");
            Debug.Log($"CharacterNameText色: {characterNameText.color}");
        }
        else
        {
            Debug.LogWarning("CharacterNameTextが設定されていません！");
        }
        
        // キャラクター画像設定
        if (characterImage != null && data.characterImage != null)
            characterImage.sprite = data.characterImage;
        
        // テキストをタイプライター効果で表示
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
            
        typingCoroutine = StartCoroutine(TypewriterEffect(data.dialogueText));
        
        // ログに追加
        AddToLog(data.characterName, data.dialogueText);
    }
    
    IEnumerator TypewriterEffect(string text)
    {
        isTyping = true;
        dialogueText.text = "";
        
        for (int i = 0; i < text.Length; i++)
        {
            dialogueText.text += text[i];
            yield return new WaitForSeconds(typewriterSpeed);
        }
        
        isTyping = false;
    }
    
    public void NextDialogue()
    {
        if (isTyping)
        {
            // タイピング中の場合は即座に全文表示
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);
                
            DialogueData data = dialogues[currentDialogueIndex];
            dialogueText.text = data.dialogueText;
            isTyping = false;
            return;
        }
        
        currentDialogueIndex++;
        
        if (currentDialogueIndex >= dialogues.Count)
        {
            EndDialogue();
        }
        else
        {
            ShowDialogue(currentDialogueIndex);
        }
    }
    
    void EndDialogue()
    {
        // パネルをフェードアウト
        CanvasGroup canvasGroup = dialoguePanel.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.DOFade(0, fadeInDuration)
                .OnComplete(() => {
                    dialoguePanel.SetActive(false);
                });
        }
        else
        {
            dialoguePanel.SetActive(false);
        }
    }
    
    void Update()
    {
        // テスト用：Tキーで会話開始
        if (Input.GetKeyDown(KeyCode.T) && !dialoguePanel.activeInHierarchy)
        {
            StartDialogue();
        }
        
        // ログビューアの表示切り替え：Lキー
        if (Input.GetKeyDown(KeyCode.L))
        {
            ToggleLogViewer();
        }
        
        // 会話中の操作：スペースキーまたはマウスクリックで次の会話に進める
        if (dialoguePanel.activeInHierarchy && 
            (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)))
        {
            NextDialogue();
        }
        
        // ログビューア中の操作：ESCキーで閉じる
        if (logPanel != null && logPanel.activeInHierarchy && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseLogViewer();
        }
    }
    
    // ログ機能
    void AddToLog(string characterName, string dialogueText)
    {
        DialogueLogEntry newEntry = new DialogueLogEntry(characterName, dialogueText);
        dialogueLog.Add(newEntry);
        
        // 最大件数を超えた場合は古いログを削除
        if (dialogueLog.Count > MAX_LOG_ENTRIES)
        {
            dialogueLog.RemoveAt(0);
        }
        
        Debug.Log($"ログに追加: {characterName} - {dialogueText}");
    }
    
    public void ToggleLogViewer()
    {
        if (logPanel == null) return;
        
        if (logPanel.activeInHierarchy)
        {
            CloseLogViewer();
        }
        else
        {
            OpenLogViewer();
        }
    }
    
    public void OpenLogViewer()
    {
        if (logPanel == null) return;
        
        logPanel.SetActive(true);
        RefreshLogDisplay();
        
        // ログパネルをフェードイン
        CanvasGroup logCanvasGroup = logPanel.GetComponent<CanvasGroup>();
        if (logCanvasGroup != null)
        {
            logCanvasGroup.alpha = 0;
            logCanvasGroup.DOFade(1, fadeInDuration);
        }
    }
    
    public void CloseLogViewer()
    {
        if (logPanel == null) return;
        
        // ログパネルをフェードアウト
        CanvasGroup logCanvasGroup = logPanel.GetComponent<CanvasGroup>();
        if (logCanvasGroup != null)
        {
            logCanvasGroup.DOFade(0, fadeInDuration)
                .OnComplete(() => {
                    logPanel.SetActive(false);
                });
        }
        else
        {
            logPanel.SetActive(false);
        }
    }
    
    void RefreshLogDisplay()
    {
        Debug.Log($"RefreshLogDisplay開始 - ログ件数: {dialogueLog.Count}");
        Debug.Log($"logContent: {logContent}");
        Debug.Log($"logEntryPrefab: {logEntryPrefab}");
        
        if (logContent == null || logEntryPrefab == null) 
        {
            Debug.LogError("logContentまたはlogEntryPrefabがnullです！");
            return;
        }
        
        // 既存のログエントリを削除
        foreach (Transform child in logContent)
        {
            Destroy(child.gameObject);
        }
        
        // ログエントリを作成（新しい順に表示）
        for (int i = dialogueLog.Count - 1; i >= 0; i--)
        {
            DialogueLogEntry entry = dialogueLog[i];
            Debug.Log($"ログエントリ作成中: {entry.characterName} - {entry.dialogueText}");
            
            GameObject logEntry = Instantiate(logEntryPrefab, logContent);
            Debug.Log($"プレハブ作成完了: {logEntry.name}");
            
            // 強制的に位置とサイズを設定
            RectTransform rect = logEntry.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = new Vector2(400, 100);
                Debug.Log($"RectTransform設定: {rect.anchoredPosition}, {rect.sizeDelta}");
            }
            else
            {
                // LogPrefab自体にRectTransformがない場合、子のCanvasを探す
                RectTransform canvasRect = logEntry.GetComponentInChildren<RectTransform>();
                if (canvasRect != null)
                {
                    canvasRect.sizeDelta = new Vector2(400, 100);
                    Debug.Log($"子RectTransform設定: {canvasRect.sizeDelta}");
                }
            }
            
            // LogEntryUIコンポーネントがある場合はそれを使用
            LogEntryUI logEntryUI = logEntry.GetComponent<LogEntryUI>();
            if (logEntryUI != null)
            {
                Debug.Log("LogEntryUIコンポーネント使用");
                int displayIndex = dialogueLog.Count - 1 - i;
                bool isEvenRow = displayIndex % 2 == 0;
                logEntryUI.SetupLogEntry(entry.timestamp, entry.characterName, entry.dialogueText, isEvenRow);
            }
            else
            {
                Debug.Log("フォールバック：直接テキスト設定");
                // フォールバック：TextMeshProUGUIコンポーネントを直接設定
                TextMeshProUGUI[] texts = logEntry.GetComponentsInChildren<TextMeshProUGUI>();
                Debug.Log($"見つかったTextコンポーネント数: {texts.Length}");
                if (texts.Length >= 3)
                {
                    texts[0].text = entry.timestamp;          // 時刻
                    texts[1].text = entry.characterName;      // キャラクター名
                    texts[2].text = entry.dialogueText;       // 会話内容
                    Debug.Log($"テキスト設定完了: {texts[0].text}, {texts[1].text}, {texts[2].text}");
                }
            }
        }
        
        // スクロールを最上部に移動
        if (logScrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            logScrollRect.verticalNormalizedPosition = 1f;
        }
    }
    
    // エディタ用：CSVファイルの再読み込み
    [ContextMenu("CSVファイルを再読み込み")]
    public void ReloadCSV()
    {
        if (useCSVFile)
        {
            LoadDialogueFromCSV();
            Debug.Log("CSVファイルを再読み込みしました。");
        }
    }
    
    // エディタ用：ログをクリア
    [ContextMenu("ログをクリア")]
    public void ClearLog()
    {
        dialogueLog.Clear();
        Debug.Log("ログをクリアしました。");
    }
}
