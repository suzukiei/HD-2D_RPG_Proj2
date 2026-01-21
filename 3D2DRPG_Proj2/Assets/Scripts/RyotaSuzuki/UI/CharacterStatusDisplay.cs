using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace RyotaSuzuki.UI
{
    /// <summary>
    /// メニューUIでキャラクターステータスを表示するクラス
    /// </summary>
    public class CharacterStatusDisplay : MonoBehaviour
    {
        [Header("キャラクターデータ")]
        [SerializeField, Tooltip("プレイアブルキャラクターのデータ（スグル, 照, 月）")]
        private List<CharacterData> characterDataList = new List<CharacterData>();
        
        [Header("ステータス表示UI")]
        [SerializeField] private TextMeshProUGUI characterNameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private TextMeshProUGUI mpText;
        [SerializeField] private TextMeshProUGUI atkText;
        [SerializeField] private TextMeshProUGUI defText;
        [SerializeField] private TextMeshProUGUI spdText;
        
        [Header("キャラクター画像")]
        [SerializeField] private Image characterImage;
        
        [SerializeField, Tooltip("Resourcesから画像を読み込むパス（{name}はキャラ名に置換）")]
        private string characterImagePath = "Image/Sanmenzu/{name}立ち絵";
        
        [SerializeField, Tooltip("スプライト名のサフィックス（例: _0）")]
        private string spriteSuffix = "_0";
        
        [Header("キャラクター選択ボタン")]
        [SerializeField] private Button character1Button;
        [SerializeField] private Button character2Button;
        [SerializeField] private Button character3Button;
        
        [Header("選択中表示")]
        [SerializeField] private Color selectedColor = new Color(1f, 0.8f, 0.2f, 1f);
        [SerializeField] private Color normalColor = Color.white;
        
        [Header("デバッグ")]
        [SerializeField] private bool showDebugLog = true;
        
        private int currentCharacterIndex = 0;
        private List<Button> characterButtons = new List<Button>();
        
        void Start()
        {
            InitializeButtons();
            LoadCharacterDataFromResources();
            
            // 最初のキャラクターを表示
            if (characterDataList.Count > 0)
            {
                SelectCharacter(0);
            }
        }
        
        /// <summary>
        /// ボタンの初期化
        /// </summary>
        private void InitializeButtons()
        {
            characterButtons.Clear();
            
            if (character1Button != null)
            {
                characterButtons.Add(character1Button);
                character1Button.onClick.RemoveAllListeners(); // 重複防止
                character1Button.onClick.AddListener(() => {
                    Debug.Log("[CharacterStatusDisplay] Character1ボタンがクリックされました");
                    SelectCharacter(0);
                });
            }
            else
            {
                Debug.LogWarning("[CharacterStatusDisplay] Character1Buttonが設定されていません");
            }
            
            if (character2Button != null)
            {
                characterButtons.Add(character2Button);
                character2Button.onClick.RemoveAllListeners();
                character2Button.onClick.AddListener(() => {
                    Debug.Log("[CharacterStatusDisplay] Character2ボタンがクリックされました");
                    SelectCharacter(1);
                });
            }
            else
            {
                Debug.LogWarning("[CharacterStatusDisplay] Character2Buttonが設定されていません");
            }
            
            if (character3Button != null)
            {
                characterButtons.Add(character3Button);
                character3Button.onClick.RemoveAllListeners();
                character3Button.onClick.AddListener(() => {
                    Debug.Log("[CharacterStatusDisplay] Character3ボタンがクリックされました");
                    SelectCharacter(2);
                });
            }
            else
            {
                Debug.LogWarning("[CharacterStatusDisplay] Character3Buttonが設定されていません");
            }
            
            Debug.Log($"[CharacterStatusDisplay] ボタン初期化完了: {characterButtons.Count}個");
        }
        
        void OnEnable()
        {
            // メニューが表示されたときに再初期化（必要に応じて）
            if (characterButtons.Count == 0)
            {
                InitializeButtons();
            }
            
            // 現在のキャラクターを再表示
            if (characterDataList.Count > 0 && characterDataList[currentCharacterIndex] != null)
            {
                UpdateStatusDisplay(characterDataList[currentCharacterIndex]);
                UpdateButtonSelection(currentCharacterIndex);
            }
        }
        
        /// <summary>
        /// ResourcesフォルダからCharacterDataを読み込む（Inspectorで設定されていない場合）
        /// </summary>
        private void LoadCharacterDataFromResources()
        {
            // Inspectorで既に設定されている場合はスキップ
            if (characterDataList.Count > 0 && characterDataList[0] != null)
            {
                if (showDebugLog)
                {
                    Debug.Log($"[CharacterStatusDisplay] Inspectorから{characterDataList.Count}件のキャラクターデータを使用");
                }
                return;
            }
            
            // Resourcesから読み込みを試みる（必要に応じて）
            if (showDebugLog)
            {
                Debug.Log("[CharacterStatusDisplay] キャラクターデータをInspectorで設定してください");
            }
        }
        
        /// <summary>
        /// キャラクターを選択
        /// </summary>
        public void SelectCharacter(int index)
        {
            if (index < 0 || index >= characterDataList.Count)
            {
                Debug.LogWarning($"[CharacterStatusDisplay] 無効なキャラクターインデックス: {index}");
                return;
            }
            
            if (characterDataList[index] == null)
            {
                Debug.LogWarning($"[CharacterStatusDisplay] キャラクターデータがnullです: インデックス {index}");
                return;
            }
            
            currentCharacterIndex = index;
            
            // ステータス表示を更新
            UpdateStatusDisplay(characterDataList[index]);
            
            // ボタンの選択状態を更新
            UpdateButtonSelection(index);
            
            if (showDebugLog)
            {
                Debug.Log($"[CharacterStatusDisplay] キャラクター選択: {characterDataList[index].charactername}");
            }
        }
        
        /// <summary>
        /// ステータス表示を更新
        /// </summary>
        private void UpdateStatusDisplay(CharacterData data)
        {
            if (data == null) return;
            
            // キャラクター名
            if (characterNameText != null)
            {
                characterNameText.text = data.charactername;
            }
            
            // レベル
            if (levelText != null)
            {
                levelText.text = $"Lv.{data.level}";
            }
            
            // HP（ラベル付き）
            if (hpText != null)
            {
                hpText.text = $"HP: {data.hp}/{data.maxHp}";
            }
            
            // MP（ラベル付き）
            if (mpText != null)
            {
                mpText.text = $"MP: {data.mp}/{data.maxMp}";
            }
            
            // ATK（ラベル付き）
            if (atkText != null)
            {
                atkText.text = $"ATK: {data.atk}";
            }
            
            // DEF（ラベル付き）
            if (defText != null)
            {
                defText.text = $"DEF: {data.def}";
            }
            
            // SPD（ラベル付き）
            if (spdText != null)
            {
                spdText.text = $"SPD: {data.spd}";
            }
            
            // キャラクター画像（ハイブリッド方式）
            if (characterImage != null)
            {
                Sprite sprite = GetCharacterSprite(data);
                if (sprite != null)
                {
                    characterImage.sprite = sprite;
                    characterImage.enabled = true;
                }
                else
                {
                    characterImage.enabled = false;
                }
            }
        }
        
        /// <summary>
        /// キャラクター画像を取得（ハイブリッド方式）
        /// CharacterData.characterIconがあればそれを使用、なければResourcesから読み込み
        /// </summary>
        private Sprite GetCharacterSprite(CharacterData data)
        {
            if (data == null) return null;
            
            // 1. CharacterDataにアイコンが設定されていればそれを使用
            if (data.characterIcon != null)
            {
                if (showDebugLog)
                {
                    Debug.Log($"[CharacterStatusDisplay] CharacterData.characterIconを使用: {data.charactername}");
                }
                return data.characterIcon;
            }
            
            // 2. Resourcesから読み込み
            return LoadCharacterSpriteFromResources(data.charactername);
        }
        
        /// <summary>
        /// Resourcesフォルダからキャラクター画像を読み込む
        /// ConversationUIと同じロジック
        /// </summary>
        private Sprite LoadCharacterSpriteFromResources(string characterName)
        {
            if (string.IsNullOrEmpty(characterName)) return null;
            
            // パスを生成（{name}をキャラ名に置換）
            string path = characterImagePath.Replace("{name}", characterName);
            
            // スライスされたスプライトを読み込む
            Sprite[] sprites = Resources.LoadAll<Sprite>(path);
            
            if (sprites != null && sprites.Length > 0)
            {
                // サフィックス付きのスプライトを探す（例: スグル立ち絵_0）
                string targetName = $"{characterName}立ち絵{spriteSuffix}";
                foreach (Sprite sprite in sprites)
                {
                    if (sprite.name == targetName)
                    {
                        if (showDebugLog)
                        {
                            Debug.Log($"[CharacterStatusDisplay] Resourcesから画像読み込み成功: {sprite.name}");
                        }
                        return sprite;
                    }
                }
                
                // 見つからなければ最初のスプライトを返す
                if (showDebugLog)
                {
                    Debug.Log($"[CharacterStatusDisplay] Resourcesから画像読み込み（最初のスプライト）: {sprites[0].name}");
                }
                return sprites[0];
            }
            
            if (showDebugLog)
            {
                Debug.LogWarning($"[CharacterStatusDisplay] キャラ画像が見つかりません: {path}");
            }
            return null;
        }
        
        /// <summary>
        /// ボタンの選択状態を更新
        /// </summary>
        private void UpdateButtonSelection(int selectedIndex)
        {
            for (int i = 0; i < characterButtons.Count; i++)
            {
                if (characterButtons[i] == null) continue;
                
                // ボタンの色を変更
                var colors = characterButtons[i].colors;
                if (i == selectedIndex)
                {
                    colors.normalColor = selectedColor;
                    colors.highlightedColor = selectedColor;
                }
                else
                {
                    colors.normalColor = normalColor;
                    colors.highlightedColor = normalColor;
                }
                characterButtons[i].colors = colors;
                
                // または、子のテキストの色を変更
                var buttonText = characterButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.color = (i == selectedIndex) ? selectedColor : normalColor;
                }
            }
        }
        
        /// <summary>
        /// 次のキャラクターを選択
        /// </summary>
        public void SelectNextCharacter()
        {
            int nextIndex = (currentCharacterIndex + 1) % characterDataList.Count;
            SelectCharacter(nextIndex);
        }
        
        /// <summary>
        /// 前のキャラクターを選択
        /// </summary>
        public void SelectPreviousCharacter()
        {
            int prevIndex = (currentCharacterIndex - 1 + characterDataList.Count) % characterDataList.Count;
            SelectCharacter(prevIndex);
        }
        
        /// <summary>
        /// 現在選択中のキャラクターデータを取得
        /// </summary>
        public CharacterData GetCurrentCharacterData()
        {
            if (currentCharacterIndex >= 0 && currentCharacterIndex < characterDataList.Count)
            {
                return characterDataList[currentCharacterIndex];
            }
            return null;
        }
        
        /// <summary>
        /// キャラクターデータを外部から設定
        /// </summary>
        public void SetCharacterDataList(List<CharacterData> dataList)
        {
            characterDataList = dataList;
            if (characterDataList.Count > 0)
            {
                SelectCharacter(0);
            }
        }
        
        /// <summary>
        /// ステータスを強制更新（データが変更された時用）
        /// </summary>
        public void RefreshCurrentStatus()
        {
            if (currentCharacterIndex >= 0 && currentCharacterIndex < characterDataList.Count)
            {
                UpdateStatusDisplay(characterDataList[currentCharacterIndex]);
            }
        }
        
        void OnDestroy()
        {
            // ボタンのリスナーを解除
            if (character1Button != null) character1Button.onClick.RemoveAllListeners();
            if (character2Button != null) character2Button.onClick.RemoveAllListeners();
            if (character3Button != null) character3Button.onClick.RemoveAllListeners();
        }
    }
}
