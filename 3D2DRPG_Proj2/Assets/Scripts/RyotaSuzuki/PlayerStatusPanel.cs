using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

/// <summary>
/// プレイヤーのステータス情報を表示するUIパネル
/// </summary>
public class PlayerStatusPanel : MonoBehaviour
{
    [Header("基本情報UI")]
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI levelText;

    [Header("HP表示")]
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Image hpFillImage;

    [Header("MP表示")]
    [SerializeField] private TextMeshProUGUI mpText;
    [SerializeField] private Slider mpSlider;
    [SerializeField] private Image mpFillImage;

    [Header("ステータス表示")]
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI defenseText;
    [SerializeField] private TextMeshProUGUI speedText;

    [Header("経験値表示")]
    [SerializeField] private TextMeshProUGUI expText;
    [SerializeField] private Slider expSlider;

    [Header("カラー設定")]
    [SerializeField] private Color hpColor = Color.green;
    [SerializeField] private Color hpLowColor = Color.red;
    [SerializeField] private Color mpColor = Color.blue;
    [SerializeField] private Color expColor = Color.yellow;

    [Header("UnityEvents")]
    [SerializeField] private UnityEvent<PlayerData> OnPlayerDataReceived;

    private PlayerData currentPlayerData;

    void Start()
    {
        // 初期設定
        InitializeSliders();
        
        // ダミーデータで初期化（テスト用）
        UpdatePlayerStatus(PlayerData.CreateDummyData(0));
    }

    /// <summary>
    /// スライダーの初期設定
    /// </summary>
    private void InitializeSliders()
    {
        if (hpSlider != null)
        {
            hpSlider.minValue = 0f;
            hpSlider.maxValue = 1f;
            if (hpFillImage != null) hpFillImage.color = hpColor;
        }

        if (mpSlider != null)
        {
            mpSlider.minValue = 0f;
            mpSlider.maxValue = 1f;
            if (mpFillImage != null) mpFillImage.color = mpColor;
        }

        if (expSlider != null)
        {
            expSlider.minValue = 0f;
            expSlider.maxValue = 1f;
        }
    }

    /// <summary>
    /// プレイヤーステータスを更新（UnityEventから呼び出し可能）
    /// </summary>
    /// <param name="playerData">表示するプレイヤーデータ</param>
    public void UpdatePlayerStatus(PlayerData playerData)
    {
        if (playerData == null)
        {
            Debug.LogWarning("PlayerStatusPanel: PlayerDataがnullです");
            return;
        }

        currentPlayerData = playerData;

        // 基本情報更新
        UpdateBasicInfo();
        
        // HP/MP更新
        UpdateHealthAndMana();
        
        // ステータス更新
        UpdateStats();
        
        // 経験値更新
        UpdateExperience();

        // UnityEventを発火
        OnPlayerDataReceived?.Invoke(playerData);
    }

    /// <summary>
    /// 基本情報（名前、レベル）を更新
    /// </summary>
    private void UpdateBasicInfo()
    {
        if (playerNameText != null)
            playerNameText.text = currentPlayerData.playerName;

        if (levelText != null)
            levelText.text = $"Lv.{currentPlayerData.level}";
    }

    /// <summary>
    /// HP/MPを更新
    /// </summary>
    private void UpdateHealthAndMana()
    {
        // HP更新
        if (hpText != null)
            hpText.text = $"HP: {currentPlayerData.currentHP}/{currentPlayerData.maxHP}";

        if (hpSlider != null)
        {
            hpSlider.value = currentPlayerData.GetHPRatio();
            
            // HPが低い場合は色を変更
            if (hpFillImage != null)
            {
                float hpRatio = currentPlayerData.GetHPRatio();
                hpFillImage.color = hpRatio <= 0.3f ? hpLowColor : hpColor;
            }
        }

        // MP更新
        if (mpText != null)
            mpText.text = $"MP: {currentPlayerData.currentMP}/{currentPlayerData.maxMP}";

        if (mpSlider != null)
            mpSlider.value = currentPlayerData.GetMPRatio();
    }

    /// <summary>
    /// ステータス（攻撃力、防御力、素早さ）を更新
    /// </summary>
    private void UpdateStats()
    {
        if (attackText != null)
            attackText.text = $"攻撃: {currentPlayerData.attack}";

        if (defenseText != null)
            defenseText.text = $"防御: {currentPlayerData.defense}";

        if (speedText != null)
            speedText.text = $"素早さ: {currentPlayerData.speed}";
    }

    /// <summary>
    /// 経験値を更新
    /// </summary>
    private void UpdateExperience()
    {
        if (expText != null)
            expText.text = $"EXP: {currentPlayerData.currentEXP}/{currentPlayerData.nextLevelEXP}";

        if (expSlider != null)
            expSlider.value = currentPlayerData.GetEXPRatio();
    }

    /// <summary>
    /// 外部からキャラクター情報を設定するためのメソッド（UnityEventから呼び出し可能）
    /// </summary>
    /// <param name="characterIndex">キャラクターのインデックス（0-2）</param>
    public void SetCharacterByIndex(int characterIndex)
    {
        PlayerData dummyData = PlayerData.CreateDummyData(characterIndex);
        UpdatePlayerStatus(dummyData);
    }

    /// <summary>
    /// 現在のプレイヤーデータを取得
    /// </summary>
    public PlayerData GetCurrentPlayerData()
    {
        return currentPlayerData;
    }

    /// <summary>
    /// テスト用：ダミーデータでステータスを更新
    /// </summary>
    [ContextMenu("テスト用ダミーデータ更新")]
    public void TestUpdateWithDummyData()
    {
        int randomIndex = Random.Range(0, 3);
        SetCharacterByIndex(randomIndex);
        Debug.Log($"ダミーデータ更新: キャラクター{randomIndex}");
    }
} 