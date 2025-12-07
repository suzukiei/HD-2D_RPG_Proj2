using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//UI要素の参照
public class ViewHolder
{
    public ViewHolder(Character character)
    {
        characterIcon = character.characterIcon;
        characterName = character.charactername;

    }
    public Sprite characterIcon;
    public string characterName;
    public Object viewObject;
}

public class TurnUI : MonoBehaviour
{
    private List<ViewHolder> viewHolders;

    [SerializeField, Header("ターンUIプレハブ")]
    private GameObject turnUIPrefab;
    [SerializeField, Header("ターンUI配置場所")]
    private Transform turnUIParent;
    [SerializeField, Header("ターン順番表示リスト")]
    private List<GameObject> activeTurnUIs = new List<GameObject>();
    // 追加: ターン順番表示用リスト
    public void UpdateTurnUI(List<GameObject> sortedTurnList, int turnNumber)
    {
        if (turnUIParent == null || turnUIPrefab == null)
        {
            Debug.LogWarning("TurnUIParentまたはTurnUIPrefabが設定されていません");
            return;
        }
        // 既存のUI要素を削除
        foreach (var ui in activeTurnUIs)
        {
            Destroy(ui);
        }
        activeTurnUIs.Clear();
        // 新しいUI要素を作成
        for (int i = 0; i < sortedTurnList.Count; i++)
        {
            if(sortedTurnList[i]== null)
                continue;
            GameObject turnUI = Instantiate(turnUIPrefab, turnUIParent);
            // ターンUIの位置を設定（横並び）
            turnUI.GetComponent<RectTransform>().anchoredPosition = new Vector2(i * 150, 0); // 150はアイコンの間隔
            // キャラクターの情報を取得してUIに反映
            Character character = sortedTurnList[i].GetComponent<Character>();
            if (character != null)
            {
                // アイコンの設定
                Image iconImage = turnUI.transform.Find("CharacterIcon").GetComponent<Image>();
                iconImage.sprite = character.characterIcon;
                // 名前の設定
                TextMeshProUGUI nameText = turnUI.transform.Find("CharacterName").GetComponent<TextMeshProUGUI>();
                nameText.text = character.charactername;
            }
            activeTurnUIs.Add(turnUI);
        }
        // ターン番号の表示更新
        //TextMeshProUGUI turnNumberText = turnUIParent.parent.Find("TurnNumberText").GetComponent<TextMeshProUGUI>();
        //if (turnNumberText != null)
        //{
        //     turnNumberText.text = "Turn: " + turnNumber.ToString();
        //}
    }
    //ターンを進める
    public void AdvanceTurn()
    {
        // ターンUIを更新する処理をここに追加
        //UIの先頭を削除して、次のキャラクターを追加
        

        var removeobj = activeTurnUIs[0];
        //ゲームオブジェクトを削除
        Destroy(removeobj);
        activeTurnUIs.RemoveAt(0);
        //残りのゲームオブジェクトを左に詰める
        for (int i = 0; i < activeTurnUIs.Count; i++)
        {
            activeTurnUIs[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(i * 150, 0);
        }
    }
}
