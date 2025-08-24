using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LogEntryUI : MonoBehaviour
{
    [Header("ログエントリのUI要素")]
    [SerializeField, Header("時刻表示用テキスト")] public TextMeshProUGUI timestampText;
    [SerializeField, Header("キャラクター名表示用テキスト")] public TextMeshProUGUI characterNameText;
    [SerializeField, Header("会話内容表示用テキスト")] public TextMeshProUGUI dialogueText;
    [SerializeField, Header("背景画像")] public Image backgroundImage;
    
    [Header("色設定")]
    [SerializeField, Header("偶数行の背景色")] public Color evenRowColor = new Color(1f, 1f, 1f, 0.1f);
    [SerializeField, Header("奇数行の背景色")] public Color oddRowColor = new Color(0.9f, 0.9f, 0.9f, 0.1f);
    
    public void SetupLogEntry(string timestamp, string characterName, string dialogue, bool isEvenRow)
    {
        Debug.Log($"SetupLogEntry開始: {timestamp}, {characterName}, {dialogue}");
        
        if (timestampText != null)
        {
            timestampText.text = timestamp;
            timestampText.fontSize = 14;
            timestampText.color = UnityEngine.Color.gray;
            Debug.Log($"timestampText設定: {timestampText.text}");
        }
        else
        {
            Debug.LogError("timestampTextがnullです！");
        }
            
        if (characterNameText != null)
        {
            characterNameText.text = characterName;
            characterNameText.fontSize = 16;
            characterNameText.color = UnityEngine.Color.yellow;
            Debug.Log($"characterNameText設定: {characterNameText.text}");
        }
        else
        {
            Debug.LogError("characterNameTextがnullです！");
        }
            
        if (dialogueText != null)
        {
            dialogueText.text = dialogue;
            dialogueText.fontSize = 18;
            dialogueText.color = UnityEngine.Color.white;
            Debug.Log($"dialogueText設定: {dialogueText.text}");
        }
        else
        {
            Debug.LogError("dialogueTextがnullです！");
        }
            
        // 背景色を設定（交互に色を変える）
        if (backgroundImage != null)
        {
            backgroundImage.color = isEvenRow ? evenRowColor : oddRowColor;
            Debug.Log($"背景色設定完了");
        }
        else
        {
            Debug.LogWarning("backgroundImageがnullです（オプション）");
        }
    }
} 