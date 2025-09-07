using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

/// <summary>
/// コンソールゲーム風の技選択UI
/// ▶カーソルでの選択とUnityEventでのコールバックをサポート
/// </summary>
public class SkillSelectionUI : MonoBehaviour
{
    [Header("UI要素")]
    [SerializeField] private GameObject skillPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private List<TextMeshProUGUI> skillTexts = new List<TextMeshProUGUI>();
    [SerializeField] private TextMeshProUGUI instructionText;

    [Header("表示設定")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private string cursorSymbol = "▶";

    private List<SkillData> currentSkills;
    private UnityEvent<int> currentCallback;
    private int currentSelection = 0;
    private bool isActive = false;

    void Start()
    {
        // 初期状態では非表示
        if (skillPanel != null)
            skillPanel.SetActive(false);

        // 操作説明を設定
        if (instructionText != null)
            instructionText.text = "↑↓: 選択  Space: 決定  ESC: キャンセル";
    }

    /// <summary>
    /// 技選択UIを表示して選択を開始する
    /// 使用例:
    /// UnityEvent<int> callback = new UnityEvent<int>();
    /// callback.AddListener(OnSkillSelected);
    /// skillSelectionUI.ShowSkillSelection(characterSkills, callback);
    /// </summary>
    /// <param name="skills">選択可能な技のリスト</param>
    /// <param name="onSkillSelected">技が選択されたときのコールバック (インデックス、-1でキャンセル)</param>
    public void ShowSkillSelection(List<SkillData> skills, UnityEvent<int> onSkillSelected)
    {
        if (skills == null || skills.Count == 0)
        {
            Debug.LogWarning("SkillSelectionUI: スキルリストが空です");
            onSkillSelected?.Invoke(-1);
            return;
        }

        currentSkills = skills;
        currentCallback = onSkillSelected;
        currentSelection = 0;
        isActive = true;

        // パネルを表示
        if (skillPanel != null)
            skillPanel.SetActive(true);

        // タイトルを設定
        if (titleText != null)
            titleText.text = "技を選択してください";

        // スキルリストを更新
        UpdateSkillDisplay();

        // 入力処理を開始
        StartCoroutine(HandleInput());
    }

    /// <summary>
    /// 技選択UIを強制的に非表示にする
    /// </summary>
    public void HideSkillSelection()
    {
        isActive = false;
        if (skillPanel != null)
            skillPanel.SetActive(false);
    }

    /// <summary>
    /// スキル表示を更新する（▶カーソルと色変更）
    /// </summary>
    private void UpdateSkillDisplay()
    {
        for (int i = 0; i < skillTexts.Count; i++)
        {
            if (i < currentSkills.Count)
            {
                skillTexts[i].gameObject.SetActive(true);

                // カーソルと技名を表示
                string cursorDisplay = (i == currentSelection) ? cursorSymbol : "　"; // 全角スペース
                string skillName = currentSkills[i].name;
                string mpCost = $"(MP: {currentSkills[i].mpCost})";

                skillTexts[i].text = $"{cursorDisplay} {skillName} {mpCost}";

                // 選択中の色を変更
                skillTexts[i].color = (i == currentSelection) ? selectedColor : normalColor;
            }
            else
            {
                skillTexts[i].gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// キーボード入力を処理する（W/S or ↑↓で選択、Spaceで決定、ESCでキャンセル）
    /// </summary>
    private IEnumerator HandleInput()
    {
        while (isActive && skillPanel != null && skillPanel.activeInHierarchy)
        {
            // 上方向キー
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                MoveCursor(-1);
            }
            // 下方向キー
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                MoveCursor(1);
            }
            // 決定キー
            else if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                SelectSkill();
                yield break;
            }
            // キャンセルキー
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                CancelSelection();
                yield break;
            }

            yield return null;
        }
    }

    /// <summary>
    /// カーソルを移動する（循環選択）
    /// </summary>
    private void MoveCursor(int direction)
    {
        int newSelection = currentSelection + direction;

        // 循環選択
        if (newSelection < 0)
            newSelection = currentSkills.Count - 1;
        else if (newSelection >= currentSkills.Count)
            newSelection = 0;

        currentSelection = newSelection;
        UpdateSkillDisplay();
    }

    /// <summary>
    /// 技を選択する
    /// </summary>
    private void SelectSkill()
    {
        // 選択されたインデックスでコールバックを呼び出し
        currentCallback?.Invoke(currentSelection);

        // UI非表示
        HideSkillSelection();
    }

    /// <summary>
    /// 選択をキャンセルする
    /// </summary>
    private void CancelSelection()
    {
        // -1でキャンセルを通知
        currentCallback?.Invoke(-1);

        // UI非表示
        HideSkillSelection();
    }

    /// <summary>
    /// 現在選択中の技のインデックスを取得
    /// </summary>
    public int GetCurrentSelection()
    {
        return currentSelection;
    }

    /// <summary>
    /// アクティブ状態かどうかを取得
    /// </summary>
    public bool IsActive()
    {
        return isActive;
    }
}