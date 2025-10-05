# フローティングボタンアニメーション

DoTweenを使用して▲▼ボタンなどのUI要素に上下にゆっくり動くフローティングアニメーションを適用するシステムです。

## ファイル構成

- **FloatingButtonAnimation.cs**: メインのアニメーションコンポーネント
- **FloatingButtonTest.cs**: デモとテスト用スクリプト
- **README_FloatingButton.md**: このファイル（使用方法の説明）

## 基本的な使用方法

### 1. 基本セットアップ

任意のUIボタンに `FloatingButtonAnimation` コンポーネントを追加するだけで使用できます。

```csharp
// スクリプトから動的に追加する場合
FloatingButtonAnimation animation = myButton.gameObject.AddComponent<FloatingButtonAnimation>();
```

### 2. インスペクターでの設定

#### フローティングアニメーション設定
- **上下移動の距離**: アニメーションでの移動距離（ピクセル）
- **アニメーションの継続時間**: 1サイクルの時間（秒）
- **イージングタイプ**: アニメーションの動きの種類
- **自動開始**: ゲーム開始時に自動でアニメーションを開始するか
- **ボタンクリック時に一時停止**: ボタンがクリックされた時にアニメーションを一時的に停止するか

#### 方向制御設定（NEW!）
- **アニメーション方向**: Auto（自動）/ Up（上）/ Down（下）/ Manual（手動）
- **画面境界からの最小距離**: 画面端からの安全マージン（ピクセル）
- **手動方向設定時の方向**: Manual設定時に上方向に動かすかどうか

### 3. プログラムからの制御

```csharp
FloatingButtonAnimation animation = GetComponent<FloatingButtonAnimation>();

// アニメーション開始
animation.StartFloatingAnimation();

// アニメーション停止
animation.StopFloatingAnimation();

// 一時停止/再開
animation.TogglePause();

// 設定変更
animation.UpdateAnimationSettings(15f, 3f, Ease.InOutBounce);

// 方向設定変更（NEW!）
animation.UpdateDirectionSetting(AnimationDirection.Auto);
animation.UpdateDirectionSetting(AnimationDirection.Manual, true); // 手動で上方向

// 状態確認
bool isAnimating = animation.IsAnimating;
float progress = animation.AnimationProgress;
```

## デモの使用方法

### FloatingButtonTest.cs の使用

1. 空のGameObjectを作成し、`FloatingButtonTest` スクリプトを追加
2. インスペクターで以下を設定：
   - Up Button: ▲ボタンのButtonコンポーネント
   - Down Button: ▼ボタンのButtonコンポーネント
   - Counter Text: カウンター表示用のTextMeshProUGUI
   - 設定パネルとスライダー（任意）

### キーボード操作

- **Space**: 設定パネルの表示切り替え
- **R**: カウンターリセット
- **P**: アニメーションの一時停止/再開
- **1**: InOutSine イージング
- **2**: InOutBounce イージング
- **3**: InOutElastic イージング
- **A**: Auto方向（画面境界考慮）
- **U**: 強制的に上方向
- **D**: 強制的に下方向

## アニメーションの種類

### イージングタイプの例

- **Ease.InOutSine**: 滑らかな波型の動き
- **Ease.InOutBounce**: バウンドするような動き
- **Ease.InOutElastic**: 弾性のある動き
- **Ease.Linear**: 等速の動き

### おすすめ設定

#### ▲▼ボタン（標準）
- 距離: 10px
- 時間: 2秒
- イージング: InOutSine
- 方向: Auto（推奨）

#### ▲▼ボタン（目立つ）
- 距離: 15px
- 時間: 1.5秒
- イージング: InOutBounce
- 方向: Auto

#### 設定ボタン（控えめ）
- 距離: 5px
- 時間: 3秒
- イージング: InOutSine
- 方向: Auto

## 画面境界の考慮（重要！）

### Auto方向設定の動作

**Auto**設定では、ボタンの位置に応じて自動的にアニメーション方向を決定します：

1. **画面上部のボタン（▲ボタンなど）**: 下方向にアニメーション
2. **画面下部のボタン（▼ボタンなど）**: 上方向にアニメーション  
3. **中央部のボタン**: より余裕のある方向にアニメーション

### 安全マージン

`screenBoundaryMargin`（デフォルト50px）により、画面端から十分な距離を保ちます。これにより：

- ボタンが画面外に出ることを防止
- 見た目の美しさを保持
- ユーザビリティを向上

## 技術的な詳細

### DoTweenの依存関係

このシステムはDoTween（無料版または有料版）に依存しています。

### パフォーマンス

- アニメーションはDoTweenによって最適化されています
- 多数のボタンで同時に使用しても問題ありません
- オブジェクトが非アクティブになると自動的にアニメーションが停止されます

### 互換性

- Unity 2021.3 LTS以降
- DoTween 1.2.0以降
- UI Toolkit（uGUI）対応

## トラブルシューティング

### よくある問題

1. **アニメーションが動かない**
   - DoTweenが正しくインポートされているか確認
   - RectTransformコンポーネントがあるか確認

2. **ボタンクリックが反応しない**
   - ButtonコンポーネントのInteractableがtrueになっているか確認
   - UIのRaycast Targetが有効になっているか確認

3. **アニメーションがカクカクする**
   - Unityエディターでの再生では滑らかに見えない場合があります
   - ビルド後のゲームでは正常に動作します

### デバッグ

FloatingButtonAnimationコンポーネントの「デバッグログを表示する？」をtrueにすると、詳細なログが出力されます。

## 応用例

### 他のUI要素への適用

ボタン以外にも以下のUI要素に適用できます：
- Image
- Text
- Panel
- その他RectTransformを持つUIオブジェクト

### カスタマイズ

FloatingButtonAnimation.csを拡張して、以下のような機能を追加できます：
- 横方向の移動
- 回転アニメーション
- スケール変更
- 色の変化

## ライセンス

このスクリプトはプロジェクト内で自由に使用・改変可能です。 