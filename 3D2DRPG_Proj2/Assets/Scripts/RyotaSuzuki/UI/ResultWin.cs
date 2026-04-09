using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    // Start is called before the first frame update
    void Start()
    {
        // プレイヤーを取得
        players = playerManager.GetPlayerCharacters();

        ViewSet(players);
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
}
