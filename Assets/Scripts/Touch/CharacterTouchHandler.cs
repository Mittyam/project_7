using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Live2D.Cubism.Framework.Raycasting;
using UnityEngine.EventSystems;

/// <summary>
/// Live2Dキャラクターのタッチ機能を管理するコンポーネント
/// メインステートでのみ動的にアタッチされ、ノベルステートでは使用されない
/// </summary>
public class CharacterTouchHandler : MonoBehaviour
{
    private CubismRaycaster raycaster;
    private List<CubismRaycastable> raycastables = new List<CubismRaycastable>();
    private Live2DController live2DController;
    private string currentModelID;

    // モデルとアタッチ状態のデバッグ用
    private bool isInitialized = false;
    private bool hasRaycastablesSetup = false;

    // 当たり判定のマッピング定義
    [System.Serializable]
    public class DrawableMapping
    {
        public string drawableId;  // モデルのDrawable ID（例：「Head」「Body」など）
        public TouchArea touchArea;  // 対応するタッチ領域
    }

    [SerializeField] private List<DrawableMapping> drawableMappings = new List<DrawableMapping>();

    [System.Serializable]
    public class TouchReaction
    {
        public TouchArea area;
        public string[] animationTriggers;
    }

    [SerializeField] private List<TouchReaction> touchReactions = new List<TouchReaction>();

    // デフォルトのリアクション設定
    private void SetDefaultReactions()
    {
        if (touchReactions.Count == 0)
        {
            // 頭部タッチ用リアクション
            touchReactions.Add(new TouchReaction
            {
                area = TouchArea.Head,
                animationTriggers = new string[] { "Anim_1", "Anim_3" }
            });

            // 胴体タッチ用リアクション
            touchReactions.Add(new TouchReaction
            {
                area = TouchArea.Chest,
                animationTriggers = new string[] { "body_touch_1", "body_touch_2" }
            });

            // 特殊エリアタッチ用リアクション
            touchReactions.Add(new TouchReaction
            {
                area = TouchArea.Leg,
                animationTriggers = new string[] { "special_touch_1", "special_touch_2" }
            });
        }

        // デフォルトのDrawableマッピング
        if (drawableMappings.Count == 0)
        {
            // 一般的なLive2Dモデルの命名規則に基づくマッピング例
            // 実際のモデルに合わせて変更が必要
            drawableMappings.Add(new DrawableMapping { drawableId = "Head", touchArea = TouchArea.Head });
            drawableMappings.Add(new DrawableMapping { drawableId = "Chest", touchArea = TouchArea.Chest });
            drawableMappings.Add(new DrawableMapping { drawableId = "Leg", touchArea = TouchArea.Leg });
        }
    }

    /// <summary>
    /// 初期化処理 - 強化版
    /// </summary>
    public void Initialize(string modelID = "")
    {
        // 重複初期化を防止
        if (isInitialized)
        {
            Debug.Log($"CharacterTouchHandler already initialized for model: {currentModelID}");
            return;
        }

        currentModelID = modelID;

        // Live2Dコントローラーの参照取得
        live2DController = GetComponent<Live2DController>();
        if (live2DController == null)
        {
            live2DController = GetComponentInParent<Live2DController>();
        }

        // デフォルトのリアクション設定
        SetDefaultReactions();

        // CubismRaycasterの取得またはアタッチ
        raycaster = GetComponent<CubismRaycaster>();
        if (raycaster == null)
        {
            raycaster = gameObject.AddComponent<CubismRaycaster>();
            Debug.Log("Added CubismRaycaster to model");
        }

        // Drawablesを検索して対応するCubismRaycastableをアタッチ
        SetupRaycastables();

        // Update関数でタッチ入力を検出するようにする
        enabled = true;
        isInitialized = true;
    }

    /// <summary>
    /// Drawablesを検索し、CubismRaycastableをセットアップ
    /// </summary>
    private void SetupRaycastables()
    {
        // 重複セットアップを防止
        if (hasRaycastablesSetup)
        {
            Debug.Log("Raycastables already set up - skipping");
            return;
        }

        // モデルのDrawablesを取得
        var drawables = GetComponentsInChildren<Live2D.Cubism.Core.CubismDrawable>(true);
        if (drawables == null || drawables.Length == 0)
        {
            Debug.LogWarning($"No CubismDrawables found in model: {gameObject.name}");
            return;
        }

        // Debug.Log($"Found {drawables.Length} drawables in model {gameObject.name}");
        int setupCount = 0;

        foreach (var drawable in drawables)
        {
            // DrawableのIDを確認
            string drawableId = drawable.name;

            // マッピング対象かチェック
            var mapping = drawableMappings.FirstOrDefault(m => drawableId.Contains(m.drawableId));
            if (mapping != null)
            {
                // CubismRaycastableがなければ追加
                var raycastable = drawable.gameObject.GetComponent<CubismRaycastable>();
                if (raycastable == null)
                {
                    raycastable = drawable.gameObject.AddComponent<CubismRaycastable>();
                    // 精度設定（Trianglesの方が精確だが負荷が高い）
                    raycastable.Precision = CubismRaycastablePrecision.Triangles;
                    setupCount++;
                }

                raycastables.Add(raycastable);
            }
        }

        // Debug.Log($"SetupRaycastables completed: {setupCount} new raycastables added");
        hasRaycastablesSetup = true;
    }

    /// <summary>
    /// 毎フレームの更新処理
    /// </summary>
    private void Update()
    {
        // マウスクリックがない場合は早期リターン
        if (!Input.GetMouseButtonDown(0))
        {
            return;
        }

        // UIの上でクリックされた場合は無視する
        if (IsPointerOverUI())
        {
            return;
        }

        // レイをカメラからマウス位置に向けて飛ばす
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // 当たり判定結果を格納する配列
        var results = new CubismRaycastHit[4];

        // レイキャスト実行
        var hitCount = raycaster.Raycast(ray, results);

        if (hitCount > 0)
        {
            // 当たったDrawableを処理
            for (var i = 0; i < hitCount; i++)
            {
                string drawableName = results[i].Drawable.name;
                Debug.Log($"Hit drawable: {drawableName}");

                // マッピング情報を検索
                foreach (var mapping in drawableMappings)
                {
                    if (drawableName.Contains(mapping.drawableId))
                    {
                        // 対応するタッチ領域が見つかった
                        OnAreaTouched(mapping.touchArea);
                        return; // 最初に一致したもので反応
                    }
                }
            }
        }
    }

    /// <summary>
    /// タッチ機能の無効化
    /// </summary>
    public void DisableTouchDetection()
    {
        // Update関数を無効化してタッチ検出を停止
        enabled = false;
    }

    /// <summary>
    /// ポインターがUI要素の上にあるかどうかを判定
    /// </summary>
    private bool IsPointerOverUI()
    {
        // EventSystemが存在するか確認
        if (EventSystem.current == null)
            return false;

        // ポインターがUI要素の上にあるかチェック (タッチ対応版)
        if (Input.touchCount > 0)
        {
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        }

        // マウス用のチェック
        return EventSystem.current.IsPointerOverGameObject();
    }

    /// <summary>
    /// 領域タッチ時の処理
    /// </summary>
    public void OnAreaTouched(TouchArea area)
    {
        Debug.Log($"Live2Dキャラクターが{area}エリアに触れました");

        // 有効なタッチエリアに対する反応を検索
        var validReactions = touchReactions.Where(r => r.area == area).ToArray();

        if (validReactions.Length > 0 && live2DController != null)
        {
            // ランダムに反応を選択
            var reaction = validReactions[Random.Range(0, validReactions.Length)];

            if (reaction.animationTriggers != null && reaction.animationTriggers.Length > 0)
            {
                // ランダムなアニメーショントリガーを選択
                string trigger = reaction.animationTriggers[
                    Random.Range(0, reaction.animationTriggers.Length)];

                // モデルIDの判定
                string modelID = !string.IsNullOrEmpty(currentModelID) ?
                                 currentModelID : live2DController.GetActiveModelID();

                if (!string.IsNullOrEmpty(modelID))
                {
                    // Live2Dコントローラーでアニメーション再生
                    live2DController.PlayAnimation(modelID, trigger);
                    Debug.Log($"モデル '{modelID}' のアニメーション '{trigger}' を再生しています");
                }
                else
                {
                    Debug.LogWarning("アクティブなLive2Dモデルが見つかりません");
                }
            }
        }
    }

    /// <summary>
    /// コンポーネント破棄時の処理
    /// </summary>
    private void OnDestroy()
    {
        // タッチ機能を無効化
        DisableTouchDetection();

        // 状態をリセット
        isInitialized = false;
        hasRaycastablesSetup = false;
        raycastables.Clear();
    }
}
