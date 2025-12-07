# RPG戦闘システム - 追加推奨システム一覧

## 既に実装されているシステム

1. **ターン制バトルシステム** - SPD順のターン管理
2. **スキルシステム** - 攻撃、回復、バフ
3. **バフ/デバフシステム** - ターン制のバフ管理
4. **コンボ攻撃システム** - タイミング連打
5. **ダメージエフェクト表示** - UI表示
6. **レベルアップシステム** - 経験値とレベルアップ処理

---

## 追加を推奨するシステム

### 1. クリティカルヒットシステム ⭐⭐⭐
**優先度: 高**

`SkillData.cs`に`criticalRate`は定義されていますが、ダメージ計算に反映されていません。

**実装内容:**
- クリティカル判定の実装
- クリティカル時のダメージ倍率（通常1.5倍）
- クリティカルエフェクトの表示

**実装例:**p
// PlayerManager.cs の ApplyAttack メソッドに追加
bool isCritical = Random.Range(0f, 1f) < selectedSkill.criticalRate;
float criticalMultiplier = isCritical ? 1.5f : 1f;
var finalDamage = (damage * skill.power * criticalMultiplier) - enemy.def;---

### 2. 命中率システム ⭐⭐⭐
**優先度: 高**

`SkillData.cs`に`accuracy`は定義されていますが、実装されていません。

**実装内容:**
- 命中判定の実装
- ミス時の処理
- ミスエフェクトの表示

**実装例:**
// 命中判定を追加
bool hit = Random.Range(0f, 1f) < selectedSkill.accuracy;
if (!hit) {
    // ミス処理
    Debug.Log("攻撃が外れた！");
    return;
}---

### 3. 状態異常システム ⭐⭐⭐
**優先度: 高**

`StatusEffect` enumは定義されていますが、実装が未完成です。

**実装内容:**
- **毒（Poison）**: ターンごとにHP減少
- **スタン（Stun）**: 行動不能
- **燃焼（Burn）**: ターンごとにダメージ
- **凍結（Freeze）**: 行動不能、ダメージで解除
- **睡眠（Sleep）**: 行動不能、ダメージで解除

**必要な実装:**
- `Character.cs`に状態異常リストの追加
- ターン開始時の状態異常処理
- 状態異常の付与・解除処理
- 状態異常のUI表示

---

### 4. 属性システム ⭐⭐
**優先度: 中**

`SkillData.cs`でコメントアウトされています。

**実装内容:**
- 属性の定義（火、氷、雷、風、光、闇）
- 弱点・耐性によるダメージ倍率
- 属性相性表の実装

**実装例:**
public enum ElementType { None, Fire, Ice, Thunder, Wind, Light, Dark }

// 弱点判定
float GetElementMultiplier(ElementType attackElement, ElementType defenseElement)
{
    // 相性表に基づいて倍率を返す
    // 例: 火→氷 = 2.0倍、火→火 = 0.5倍
}---

### 5. アイテムシステム ⭐⭐⭐
**優先度: 高**

戦闘中のアイテム使用ができません。

**実装内容:**
- アイテムデータ構造の定義
- アイテムインベントリ管理
- 戦闘中のアイテム使用UI
- アイテム効果の実装
  - 回復アイテム（HP/MP回復）
  - 状態異常回復アイテム
  - バフアイテム

**必要なクラス:**
- `ItemData.cs` - アイテムのScriptableObject
- `Inventory.cs` - インベントリ管理
- `ItemManager.cs` - アイテム使用処理

---

### 6. 逃走システム ⭐⭐
**優先度: 中**

バトルからの逃走機能がありません。

**実装内容:**
- 逃走成功率の計算（SPD差などに基づく）
- 逃走失敗時のペナルティ
- 逃走UIボタン
- 逃走アニメーション

**実装例:**p
public bool TryEscape()
{
    float escapeChance = CalculateEscapeChance();
    bool success = Random.Range(0f, 1f) < escapeChance;
    
    if (success) {
        GameManager.Instance.EndBattle();
    } else {
        // 逃走失敗、敵のターンに移行
    }
    return success;
}---

### 7. 経験値・報酬配布システム ⭐⭐⭐
**優先度: 高**

バトル終了後の処理が未実装です。

**実装内容:**
- 経験値の配布（倒した敵に基づく）
- アイテムドロップ
- ゴールド獲得
- リワード画面の表示
- レベルアップ時の演出

**実装場所:**
- `TurnManager.cs`の`VictoryProcess()`メソッドに追加

**必要な実装:**harp
private void VictoryProcess()
{
    // 経験値配布
    DistributeExperience();
    // アイテムドロップ
    DropItems();
    // ゴールド獲得
    GainGold();
    // リワード画面表示
    ShowRewardScreen();
    
    GameManager.Instance.EndBattle();
}---

### 8. 防御・回避システム ⭐⭐
**優先度: 中**

防御コマンドと回避率が未実装です。

**実装内容:**
- 防御コマンド（ダメージ軽減、通常の50%など）
- 回避率の実装（SPDや運に基づく）
- 回避時のアニメーション
- 完全回避の演出

**実装例:**
public enum ActionType { Attack, Defend, Skill, Item, Escape }

// 防御時のダメージ計算
if (target.ActionType == ActionType.Defend) {
    finalDamage *= 0.5f; // ダメージ半減
}

// 回避判定
float dodgeChance = target.spd * 0.01f; // 例: SPD * 1%
bool dodged = Random.Range(0f, 1f) < dodgeChance;---

### 9. BP（ブーストポイント）システム ⭐⭐⭐
**優先度: 高（オクトパストラベラー風）**

オクトパストラベラーの特徴的なシステムです。

**実装内容:**
- BPゲージの管理（0～最大値）
- BPを消費して行動回数増加（最大4回まで）
- BPを貯めて強力な技を発動
- BPゲージのUI表示
- ターン開始時にBP+1

**必要な実装:**
- `Character.cs`に`bp`フィールド追加
- BP消費スキルの実装
- BPゲージUIの作成

---

### 10. 必殺技/バーストシステム ⭐⭐
**優先度: 中**

ゲージを貯めて発動する強力な技です。

**実装内容:**
- 必殺技ゲージの管理
- ゲージを貯める条件（ダメージを受ける、与えるなど）
- 必殺技の発動条件
- 強力な全体攻撃や特殊効果
- 必殺技演出

**実装例:**
public class BurstSkill : SkillData
{
    public int requiredGauge = 100; // 発動に必要なゲージ量
    public float gaugeMultiplier = 1.0f; // ゲージ増加倍率
}---

### 11. 戦闘アニメーションシステム ⭐⭐
**優先度: 中**

スキルごとのアニメーションが未実装です。

**実装内容:**
- スキルごとのアニメーション再生
- ヒットエフェクト
- キャラクターのモーション
- カメラワーク（ズームインなど）

**実装例:**arp
// SkillDataにanimationNameがあるので、それを活用
if (!string.IsNullOrEmpty(skill.animationName)) {
    characterAnimator.Play(skill.animationName);
}---

### 12. セーブ/ロードシステム ⭐⭐⭐
**優先度: 高**

プレイヤーの進行状況を保存できません。

**実装内容:**
- プレイヤーのステータス保存
- 進行状況の保存
- 装備・アイテムの保存
- セーブデータの管理

**実装方法:**
- JSON形式での保存
- PlayerPrefsを使用
- ScriptableObjectでの保存

---

### 13. 装備システム ⭐⭐
**優先度: 中**

武器・防具の装備機能がありません。

**実装内容:**
- 武器・防具データ構造の定義
- 装備スロット管理
- ステータスへの影響
- 装備UI

**必要なクラス:**
- `EquipmentData.cs` - 装備のScriptableObject
- `EquipmentSlot.cs` - 装備スロット（武器、防具、アクセサリなど）
- `EquipmentManager.cs` - 装備管理

---

### 14. 戦闘ログシステム ⭐
**優先度: 低**

行動履歴の表示機能がありません。

**実装内容:**
- 行動履歴の記録
- ダメージログ
- 状態変化の記録
- ログUIの表示

**実装例:**
public class BattleLog
{
    public List<string> logEntries = new List<string>();
    
    public void AddLog(string message) {
        logEntries.Add($"[{Time.time:F2}] {message}");
    }
}---

### 15. AI改善システム ⭐⭐
**優先度: 中**

敵のAIがシンプルです。

**実装内容:**
- より高度な行動選択
- 状態異常の考慮
- プレイヤーの弱点を狙う
- 回復タイミングの判断
- バフ/デバフの使用判断

**実装例:**harp
public class EnemyAI
{
    public SkillData SelectBestSkill(Character enemy, List<Character> players) {
        // HPが低い場合は回復を優先
        // プレイヤーが弱っている場合は強攻撃
        // バフが切れそうならバフをかける
    }
}---

## 優先度別まとめ

### 最優先（すぐに実装すべき）
1. ✅ **経験値・報酬配布システム** - バトル終了後の処理
2. ✅ **クリティカルヒット** - 既存データの活用
3. ✅ **命中率システム** - 既存データの活用
4. ✅ **状態異常システム** - enumはあるが実装が必要
5. ✅ **アイテムシステム** - 基本機能

### 高優先度（次に実装すべき）
6. ✅ **逃走システム** - 基本機能
7. ✅ **BP（ブーストポイント）システム** - オクトパストラベラー風
8. ✅ **セーブ/ロードシステム** - 進行状況の保存

### 中優先度（余裕があれば）
9. ⚠️ **属性システム** - 戦略性の向上
10. ⚠️ **防御・回避システム** - 戦術の幅を広げる
11. ⚠️ **必殺技/バーストシステム** - 演出の強化
12. ⚠️ **装備システム** - キャラクター育成要素
13. ⚠️ **AI改善システム** - 敵の行動を豊かに

### 低優先度（後回しでも可）
14. ⚠️ **戦闘アニメーションシステム** - 演出の強化
15. ⚠️ **戦闘ログシステム** - デバッグ・確認用

---

## 実装時の注意点

1. **既存コードとの整合性**
   - `SkillData.cs`に既に定義されている項目を活用する
   - `Character.cs`の構造を拡張する際は既存機能を壊さない

2. **パフォーマンス**
   - 状態異常の処理は効率的に実装する
   - UI更新は必要最小限に

3. **拡張性**
   - 新しい状態異常や属性を追加しやすい設計にする
   - ScriptableObjectを活用してデータ駆動型にする

4. **UI/UX**
   - 各システムに対応するUIを用意する
   - プレイヤーが状況を把握しやすい表示にする

---

## 参考ゲーム: オクトパストラベラー

オクトパストラベラーの特徴的なシステム：
- **BPシステム**: ターンごとにBP+1、最大5まで。BPを消費して行動回数を増やす
- **ブレイクシステム**: 弱点を攻撃してブレイク状態にする
- **ブーストアタック**: BPを消費した強力な攻撃
- **防御**: BPを消費して防御力を上げる

これらの要素を参考にすると、より戦略的なバトルシステムになります。