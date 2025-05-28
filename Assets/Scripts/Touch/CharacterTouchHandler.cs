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

        // 音声インデックスの初期値
        // 頭、胸、足のタッチに対応するベースインデックス
        public int weekdayBaseVoiceIndex; // 平日のベースインデックス
        public int holidayBaseVoiceIndex; // 休日のベースインデックス
        public int nightBaseVoiceIndex;   // 夜のベースインデックス
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
                animationTriggers = new string[] { "Week_Head_1", "Week_Head_2", "Week_Head_3", "Week_Head_4", "Week_Head_5",
                                              "Holi_Head_1", "Holi_Head_2", "Holi_Head_3", "Holi_Head_4", "Holi_Head_5",
                                              "Night_Head_1", "Night_Head_2", "Night_Head_3", "Night_Head_4", "Night_Head_5"},
                weekdayBaseVoiceIndex = 0,    // 平日頭部 0〜4
                holidayBaseVoiceIndex = 15,   // 休日頭部 15〜19
                nightBaseVoiceIndex = 30      // 夜頭部 30~34
            });

            // 胸部タッチ用リアクション
            touchReactions.Add(new TouchReaction
            {
                area = TouchArea.Chest,
                animationTriggers = new string[] { "Week_Chest_1", "Week_Chest_2", "Week_Chest_3", "Week_Chest_4", "Week_Chest_5",
                                              "Holi_Chest_1", "Holi_Chest_2", "Holi_Chest_3", "Holi_Chest_4", "Holi_Chest_5",
                                              "Night_Chest_1", "Night_Chest_2", "Night_Chest_3", "Night_Chest_4", "Night_Chest_5"},
                weekdayBaseVoiceIndex = 5,    // 平日胸部 5〜9
                holidayBaseVoiceIndex = 20,   // 休日胸部 20〜24
                nightBaseVoiceIndex = 35      // 夜胸部 35〜39
            });

            // 脚部エリアタッチ用リアクション
            touchReactions.Add(new TouchReaction
            {
                area = TouchArea.Leg,
                animationTriggers = new string[] { "Week_Leg_1", "Week_Leg_2", "Week_Leg_3", "Week_Leg_4", "Week_Leg_5",
                                              "Holi_Leg_1", "Holi_Leg_2", "Holi_Leg_3", "Holi_Leg_4", "Holi_Leg_5",
                                              "Night_Leg_1", "Night_Leg_2", "Night_Leg_3", "Night_Leg_4", "Night_Leg_5"},
                weekdayBaseVoiceIndex = 10,   // 平日脚部 10〜14
                holidayBaseVoiceIndex = 25,   // 休日脚部 25〜29
                nightBaseVoiceIndex = 40      // 夜脚部 40〜44
            });
        }

        // デフォルトのDrawableマッピング（変更なし）
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
                        Debug.Log("!!!!!!!!!!!!!!!");
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
                // 現在のステートと好感度に応じたアニメーショントリガーを選択
                // 現在のステートを取得
                var currentStateID = GameLoop.Instance.MainStateMachine.CurrentStateID;

                // ステート別のインデックスベース
                int stateBaseIndex = 0;
                int voiceBaseIndex = 0;

                // ステートがDayならWeekday,Holidayのどちらであるかを判定
                if (currentStateID == StateID.Day)
                {
                    // 曜日判定（平日か休日か）
                    bool isWeekday = true;

                    // StatusManagerから日付を取得して曜日を判定
                    if (StatusManager.Instance != null)
                    {
                        int day = StatusManager.Instance.GetStatus().day;
                        int dayOfWeek = (day - 1) % 7; // 0-6の範囲（0が1日目、1が2日目...）
                        isWeekday = dayOfWeek >= 2;    // 2以上（3日目以降）が平日
                    }

                    // 平日なら0-4、休日なら5-9のインデックスベース
                    stateBaseIndex = isWeekday ? 0 : 5;
                    // 音声インデックスも同様に設定
                    voiceBaseIndex = isWeekday ? reaction.weekdayBaseVoiceIndex : reaction.holidayBaseVoiceIndex;
                }
                else if (currentStateID == StateID.Night)
                {
                    // 夜のステートなら10-14のインデックスベース
                    stateBaseIndex = 10;
                    // 夜の音声インデックス
                    voiceBaseIndex = reaction.nightBaseVoiceIndex;
                }
                else if (currentStateID == StateID.Evening)
                {
                    // 夕方の場合も考慮する（仮に平日と同じ扱い）
                    stateBaseIndex = 0;
                    voiceBaseIndex = reaction.weekdayBaseVoiceIndex;
                }

                // 好感度に基づいてインデックスオフセットを計算
                int affectionOffset = 0;
                if (StatusManager.Instance != null)
                {
                    // 好感度を取得
                    int affection = StatusManager.Instance.GetStatus().affection;

                    // 好感度に応じてオフセットを決定
                    if (affection < 40) affectionOffset = 0;
                    else if (affection < 80) affectionOffset = 1;
                    else if (affection < 120) affectionOffset = 2;
                    else if (affection < 160) affectionOffset = 3;
                    else affectionOffset = 4;
                }

                // 最終的なインデックスを計算（ステートベース + 好感度オフセット）
                int finalIndex = stateBaseIndex + affectionOffset;
                // 最終的な音声インデックスを計算
                int finalVoiceIndex = voiceBaseIndex + affectionOffset;

                // インデックスが配列の範囲内かチェック
                if (finalIndex < reaction.animationTriggers.Length)
                {
                    string trigger = reaction.animationTriggers[finalIndex];

                    // モデルIDの判定
                    string modelID = !string.IsNullOrEmpty(currentModelID) ?
                                    currentModelID : live2DController.GetActiveModelID();

                    if (!string.IsNullOrEmpty(modelID) && !string.IsNullOrEmpty(trigger))
                    {
                        // Live2Dコントローラーでアニメーション再生
                        live2DController.PlayAnimation(modelID, trigger);
                        Debug.Log($"モデル '{modelID}' のアニメーション '{trigger}' を再生しています");

                        // 音声の再生
                        if (SoundManager.Instance != null)
                        {
                            SoundManager.Instance.PlayMainVoice(finalVoiceIndex);
                            Debug.Log($"音声インデックス {finalVoiceIndex} を再生しています");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"モデルID '{modelID}' またはトリガー '{trigger}' が無効です");
                    }
                }
                else
                {
                    Debug.LogWarning($"計算されたインデックス {finalIndex} はアニメーショントリガー配列の範囲外です");

                    // 範囲外の場合はランダムで選ぶ（フォールバック）
                    string trigger = reaction.animationTriggers[Random.Range(0, reaction.animationTriggers.Length)];
                    string modelID = !string.IsNullOrEmpty(currentModelID) ?
                                    currentModelID : live2DController.GetActiveModelID();

                    if (!string.IsNullOrEmpty(modelID))
                    {
                        live2DController.PlayAnimation(modelID, trigger);
                        Debug.Log($"フォールバック: モデル '{modelID}' のアニメーション '{trigger}' を再生しています");

                        // フォールバック時も音声を再生（基本インデックスを使用）
                        if (SoundManager.Instance != null)
                        {
                            SoundManager.Instance.PlayMainVoice(voiceBaseIndex);
                            Debug.Log($"フォールバック: 音声インデックス {voiceBaseIndex} を再生しています");
                        }
                    }
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
