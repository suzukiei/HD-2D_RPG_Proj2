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

public class ConversationUI : MonoBehaviour
{
    [Header("UI要素")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI characterNameText;
    public TextMeshProUGUI dialogueText;
    public Image characterImage;
    
    [Header("アニメーション設定")]
    public float fadeInDuration = 0.5f;
    public float typewriterSpeed = 0.05f;
    
    [Header("CSVファイル設定")]
    public string csvFolderName = "ScenarioCSV";
    public string csvFileName = "scenario01.csv";
    public bool useCSVFile = true;
    
    [Header("テスト用会話データ")]
    [TextArea(3, 10)]
    public List<string> testDialogues = new List<string>();
    
    private List<DialogueData> dialogues = new List<DialogueData>();
    private int currentDialogueIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;
    
    void Start()
    {
        // 初期化
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        
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
        
        // 会話中の操作：スペースキーまたはマウスクリックで次の会話に進める
        if (dialoguePanel.activeInHierarchy && 
            (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)))
        {
            NextDialogue();
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
}
