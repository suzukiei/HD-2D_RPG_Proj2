using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ResultWin : MonoBehaviour
{
    [SerializeField, Header("オブジェクト受け取り用のプレイヤーマネージャースクリプト")]
    private PlayerManager playerManager;

    //プレイヤーのデータ
    private List<GameObject> players;

    [SerializeField, Header("UIプレハブ")]
    private GameObject UIPrefab;
    [SerializeField, Header("UI配置場所")]
    private Transform UIParent;

    [Header("スグル表示")]
    [SerializeField] private Image SuguruExpBarBackground;
    [SerializeField] private Image SuguruExpFill;
    [SerializeField] private TextMeshProUGUI SuguruExpText;
    [SerializeField] private TextMeshProUGUI SuguruLevelText;

    [Header("照表示")]
    [SerializeField] private Image TeruExpBarBackground;
    [SerializeField] private Image TeruExpFill;
    [SerializeField] private TextMeshProUGUI TeruExpText;
    [SerializeField] private TextMeshProUGUI TeruLevelText;
    
    [Header("経験値アニメーション設定")]
    [SerializeField] private float expAnimationDuration = 2f;
    [SerializeField] private Ease expAnimationEase = Ease.OutQuad;

    // Start is called before the first frame update
    void Start()
    {
        // プレイヤーを取得
        players = playerManager.GetPlayerCharacters();

        ViewSet(players);
        
        // 経験値アニメーション開始
        StartCoroutine(AnimateExpGain());
    }
    
    public void ViewSet(List<GameObject> sortedTurnList)
    {
        if (UIParent == null || UIPrefab == null)
        {
            Debug.LogWarning("TurnUIParentまたはTurnUIPrefabが設定されていません");
            return;
        }
        // 新しいUI要素を作成
        for (int i = 0; i < sortedTurnList.Count; i++)
        {
            if (sortedTurnList[i] == null)
                continue;
            GameObject turnUI = Instantiate(UIPrefab, UIParent);
            // ターンUIの位置を設定（横並び）
            turnUI.GetComponent<RectTransform>().anchoredPosition = new Vector2(i * 250, 0); // 250はアイコンの間隔
            // キャラクターの情報を取得してUIに反映
            Character character = sortedTurnList[i].GetComponent<Character>();
            if (character != null)
            {
                // アイコンの設定
                Image iconImage = turnUI.transform.Find("Icon").GetComponent<Image>();
                iconImage.sprite = character.characterIcon;
                // 名前の設定
                TextMeshProUGUI nameText = turnUI.transform.Find("TextExp").GetComponent<TextMeshProUGUI>();
                nameText.text = character.charactername; //ここにはEXPポイントが入る。
            }
        }
    }
    
    /// <summary>
    /// 経験値獲得アニメーション
    /// </summary>
    private IEnumerator AnimateExpGain()
    {
        // 少し待ってからアニメーション開始
        yield return new WaitForSeconds(0.5f);
        
        // GameManagerから戦闘前のスナップショットと現在のデータを取得
        foreach (var playerChar in GameManager.Instance.PlayerData)
        {
            if (playerChar == null) continue;
            
            // 戦闘前のスナップショットを取得
            var snapshot = GameManager.Instance.GetPreBattleSnapshot(playerChar.charactername);
            if (snapshot == null) continue;
            
            // キャラクターに応じて経験値バーを選択
            Image expFill = null;
            TextMeshProUGUI expText = null;
            TextMeshProUGUI levelText = null;
            
            if (playerChar.charactername == "スグル")
            {
                expFill = SuguruExpFill;
                expText = SuguruExpText;
                levelText = SuguruLevelText;
            }
            else if (playerChar.charactername == "照")
            {
                expFill = TeruExpFill;
                expText = TeruExpText;
                levelText = TeruLevelText;
            }
            
            if (expFill != null)
            {
                // 経験値アニメーション開始
                StartCoroutine(AnimateExpForCharacter(playerChar, snapshot, expFill, expText, levelText));
            }
        }
    }
    
    /// <summary>
    /// 個別キャラクターの経験値アニメーション
    /// </summary>
    private IEnumerator AnimateExpForCharacter(
        CharacterData character,
        GameManager.PreBattleSnapshot snapshot,
        Image expFill,
        TextMeshProUGUI expText,
        TextMeshProUGUI levelText)
    {
        // 戦闘前の状態を設定
        int currentLevel = snapshot.level;
        int currentExp = snapshot.exp;
        int requiredExp = snapshot.requiredExp;
        
        // 獲得経験値を計算（戦闘後の累積経験値 - 戦闘前の累積経験値）
        int currentTotalExp = GameManager.Instance.CalculateTotalExp(character.level, character.exp);
        int gainedExp = currentTotalExp - snapshot.totalExp;
        
        // 初期表示
        if (levelText != null)
        {
            levelText.text = $"Lv.{currentLevel}";
        }
        
        if (expText != null)
        {
            expText.text = $"{currentExp}/{requiredExp}";
        }
        
        expFill.fillAmount = (float)currentExp / requiredExp;
        
        // 獲得経験値を加算
        int targetExp = currentExp + gainedExp;
        
        Debug.Log($"[ResultWin] {character.charactername} 戦闘前: Lv.{currentLevel} {currentExp}/{requiredExp}EXP → 獲得: +{gainedExp}EXP (累積: {snapshot.totalExp}→{currentTotalExp})");
        
        // レベルアップを考慮しながらアニメーション
        while (targetExp > 0)
        {
            int expToGain = Mathf.Min(targetExp, requiredExp - currentExp);
            int finalExp = currentExp + expToGain;
            float targetRatio = (float)finalExp / requiredExp;
            
            // アニメーション実行
            float animDuration = expAnimationDuration * ((float)expToGain / Mathf.Max(gainedExp, 1));
            
            // 経験値バーのアニメーション
            yield return expFill.DOFillAmount(targetRatio, animDuration)
                .SetEase(expAnimationEase)
                .OnUpdate(() =>
                {
                    // アニメーション中も経験値テキストを更新
                    if (expText != null)
                    {
                        int displayExp = Mathf.RoundToInt(expFill.fillAmount * requiredExp);
                        expText.text = $"{displayExp}/{requiredExp}";
                    }
                })
                .WaitForCompletion();
            
            currentExp = finalExp;
            targetExp -= expToGain;
            
            // レベルアップチェック
            if (currentExp >= requiredExp)
            {
                // レベルアップ！
                currentLevel++;
                currentExp -= requiredExp;
                requiredExp = GameManager.Instance.GetRequiredExp(currentLevel);
                
                Debug.Log($"★ {character.charactername} がレベル {currentLevel} にアップ！");
                
                // レベルアップ演出
                if (levelText != null)
                {
                    levelText.text = $"Lv.{currentLevel}";
                    levelText.transform.DOPunchScale(Vector3.one * 0.3f, 0.5f);
                }
                
                // バーをリセット
                expFill.fillAmount = 0f;
                yield return new WaitForSeconds(0.3f);
            }
            
            // テキスト更新
            if (expText != null)
            {
                expText.text = $"{currentExp}/{requiredExp}";
            }
        }
        
        Debug.Log($"[ResultWin] {character.charactername} アニメーション完了: Lv.{currentLevel} {currentExp}/{requiredExp}EXP");
    }
}
