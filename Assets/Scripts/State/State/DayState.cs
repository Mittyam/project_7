using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static GameEvents;

public class DayState : StateBase, IPausableState
{
    [SerializeField] private MainStateData weekdayData;
    [SerializeField] private MainStateData holidayData;

    [Header("UI要素")]
    [SerializeField] private GameObject dayUIContainer;
    [SerializeField] private GameObject commonUIContainer;

    [Header("Events")]
    public GameEvent weekDayStateEventSO;
    public GameEvent holiDayStateEventSO;

    private bool isWeekday; // 平日判定用

    private MainStateData activeStateData;

    private GameObject activeLive2DModel; // アクティブなLive2Dモデルへの参照
    private string currentModelID; // 現在表示中のモデルID

    // UI生成状態管理用フラグ
    private bool isWeekdayUICreated = false;
    private bool isHolidayUICreated = false;
    private bool isCurrentUIWeekday = false; // 現在表示中のUIが平日用かどうか

    private void Awake()
    {
        if (weekdayData == null || holidayData == null)
        {
            Debug.LogError("DayState: weekdayData か holidayData が存在しません");
        }
    }

    public override void OnEnter()
    {
        // ステートに入ったら現在の日付に1日加算
        StatusManager.Instance.UpdateStatus(1, 0, 0, 0);

        // 曜日判定など
        var day = StatusManager.Instance.GetStatus().day;
        int dayOfWeek = (day - 1) % 7; // 0-6の範囲（0が1日目、1が2日目...）
        isWeekday = dayOfWeek >= 2;    // 2以上（3日目以降）が平日

        // 平日/休日に応じたステートデータを選択
        activeStateData = isWeekday ? weekdayData : holidayData;

        // UI要素のセットアップ
        SetupUI();
        ShowDayUI();

        // イベント購読のセットアップ - 進行度更新とボタン状態更新のみに絞る
        SubscribeToEvents();

        // MiniEventSelectionHandler コンポーネントの取得とイベントリスナーの設定
        MiniEventSelectionHandler miniEventHandler = GetComponent<MiniEventSelectionHandler>();
        if (miniEventHandler == null)
        {
            // コンポーネントがなければ追加
            miniEventHandler = gameObject.AddComponent<MiniEventSelectionHandler>();
            Debug.Log("DayState: MiniEventSelectionHandler を追加しました");
        }

        // 平日/休日に応じたイベント発火
        if (isWeekday)
        {
            // Index 2~4 のBGMをランダムに再生
            int randomIndex = Random.Range(2, 5);
            SoundManager.Instance.PlayBGMWithFadeIn(randomIndex, 1f);

            weekDayStateEventSO?.Raise();
            Debug.Log("DayState: 今日は平日です。");
        }
        else
        {
            // Index 2~4 のBGMをランダムに再生
            int randomIndex = Random.Range(5, 8);
            SoundManager.Instance.PlayBGMWithFadeIn(randomIndex, 1f);

            holiDayStateEventSO?.Raise();
            Debug.Log("DayState: 今日は休日です。");
        }

        // Live2Dモデルを初期化して表示
        SetupLive2DModel();
    }

    public override void OnUpdate()
    {
        
    }

    public override void OnExit()
    {
        Debug.Log("DayState: OnExit");

        // BGMを停止
        SoundManager.Instance.StopBGM();

        // イベント購読の解除
        UnsubscribeFromEvents();

        // タッチハンドラーを無効化
        DisableLive2DTouchHandler();

        // UIの非表示化
        HideAllUI();

        // ここでエンディング判定などの処理があれば追加 

        // 次のステートに移行する前に新たなノベルイベントの発火チェックをするならここで
        EventTriggerChecker.Check(TriggerTiming.DayToEvening);
    }

    public void OnPause()
    {
        Debug.Log("DayState: 一時停止します");

        // UI要素を非表示にする
        HideAllUI();

        // BGMを停止
        SoundManager.Instance.StopBGM();
    }

    public void OnResume()
    {
        Debug.Log("DayState: 再開します");

        // UI要素を再表示する
        SetupUI();
        ShowDayUI();

        // ボタン状態の更新
        UpdateButtonStates();

        // Index 2~4 のBGMをランダムに再生
        int randomIndex = Random.Range(5, 8);
        SoundManager.Instance.PlayBGMWithFadeIn(randomIndex, 1f);
    }

    // 次のステートの取得メソッド
    public StateID GetNextStateID()
    {
        return activeStateData != null ? activeStateData.nextStateID : StateID.Evening;
    }

    #region --- ここから下はUI関連の新メソッド ---

    // UIのセットアップ
    private void SetupUI()
    {
        // 親クラスのSetupUIを呼び出さない（完全に上書き）

        // UIコンテナの初期化
        if (dayUIContainer == null)
        {
            dayUIContainer = new GameObject("DayUIContainer");
            dayUIContainer.transform.SetParent(transform);
            dayUIContainer.AddComponent<RectTransform>();
        }

        if (commonUIContainer == null)
        {
            commonUIContainer = new GameObject("CommonUIContainer");
            commonUIContainer.transform.SetParent(transform);
            commonUIContainer.AddComponent<RectTransform>();
        }

        // 平日/休日の状態に応じたUI生成判定
        bool needToCreateUI = isWeekday ? !isWeekdayUICreated : !isHolidayUICreated;

        // 平日/休日が切り替わった場合は、既存のUIを削除
        if (isCurrentUIWeekday != isWeekday && dayUIContainer.transform.childCount > 0)
        {
            foreach (Transform child in dayUIContainer.transform)
            {
                Destroy(child.gameObject);
            }
            needToCreateUI = true;
        }

        // UIプレハブの生成（必要な場合のみ）
        if (needToCreateUI && activeStateData != null && activeStateData.uiPrefab != null)
        {
            foreach (var prefab in activeStateData.uiPrefab)
            {
                if (prefab != null)
                {
                    // カメラ設定付きのインスタンス化を使用
                    InstantiateUIWithCamera(prefab, dayUIContainer.transform);
                }
            }

            // UIの生成状態を更新
            if (isWeekday)
            {
                isWeekdayUICreated = true;
                isCurrentUIWeekday = true;
            }
            else
            {
                isHolidayUICreated = true;
                isCurrentUIWeekday = false;
            }
        }
    }

    // 個別ボタンの状態更新
    private void UpdateButtonState(string buttonName, bool visible, bool interactable)
    {
        // dayUIContainer内からボタンを探す
        Transform buttonTransform = FindButtonInContainer(buttonName);
        if (buttonTransform != null)
        {
            // ボタンの表示/非表示
            buttonTransform.gameObject.SetActive(visible);

            // ボタンの有効/無効
            Button button = buttonTransform.GetComponent<Button>();
            if (button != null)
            {
                button.interactable = interactable;
            }
        }
    }

    // コンテナ内からボタンを探すヘルパーメソッド
    private Transform FindButtonInContainer(string buttonName)
    {
        if (dayUIContainer == null) return null;

        // 直接の子から検索
        Transform result = dayUIContainer.transform.Find(buttonName);
        if (result != null) return result;

        // 階層を下って検索
        foreach (Transform child in dayUIContainer.transform)
        {
            result = child.transform.Find(buttonName);
            if (result != null) return result;

            // さらに深い階層を検索
            foreach (Transform grandchild in child)
            {
                result = grandchild.transform.Find(buttonName);
                if (result != null) return result;
            }
        }

        return null;
    }

    // 昼UIの表示
    private void ShowDayUI()
    {
        if (dayUIContainer != null)
        {
            dayUIContainer.SetActive(true);
        }

        if (commonUIContainer != null)
        {
            commonUIContainer.SetActive(true);
        }
    }

    // 全UIの非表示
    private void HideAllUI()
    {
        if (dayUIContainer != null)
        {
            dayUIContainer.SetActive(false);
        }

        if (commonUIContainer != null)
        {
            commonUIContainer.SetActive(false);
        }
    }

    #endregion

    #region --- ここから下はLive2Dタッチメソッド ---

    // Live2Dモデルをセットアップするメソッド
    private void SetupLive2DModel()
    {
        // メインステートのデータを取得
        var stateData = isWeekday ? weekdayData : holidayData;

        // Live2Dの表示が無効の場合は処理しない
        if (stateData == null || stateData.live2DData == null || !stateData.showLive2DModel)
        {
            return;
        }

        // Live2Dモデルのデータを取得
        var live2DData = stateData.live2DData;

        // すでにモデルが存在している場合は削除
        CleanupLive2DModel();

        // モデルプレハブがあれば生成
        if (live2DData.modelPrefab != null)
        {
            // Live2DControllerコンポーネントを取得
            Live2DController live2DController = FindLive2DController();

            if (live2DController != null)
            {
                // モデルIDを保存
                currentModelID = live2DData.modelID;

                // Live2Dモデルを表示（Live2DControllerのShowModelメソッドを使用）
                live2DController.ShowModel(
                    currentModelID,
                    live2DData.scale,
                    live2DData.position.ToString()
                );

                // モデル参照取得を確実に遅延させるためのコルーチンを開始
                StartCoroutine(GetModelAndAttachHandler());

                // アニメーション再生
                if (!string.IsNullOrEmpty(live2DData.defaultAnimTrigger))
                {
                    live2DController.PlayAnimation(currentModelID, live2DData.defaultAnimTrigger);
                }

                // 表示中のLive2Dモデルのゲームオブジェクトを取得
                if (live2DController.transform.childCount > 0)
                {
                    foreach (Transform child in live2DController.transform)
                    {
                        // モデルの取得ロジック（適宜調整が必要）
                        if (child.gameObject.name.Contains(currentModelID) || child.gameObject.tag == "Live2DModel")
                        {
                            activeLive2DModel = child.gameObject;
                            break;
                        }
                    }
                }

                // タッチ機能が有効なら、タッチハンドラーをアタッチ
                if (activeLive2DModel != null && live2DData.enableTouch)
                {
                    SetupLive2DTouchHandler(activeLive2DModel);
                }

                Debug.Log($"DayState に表示された Live2D モデル '{currentModelID}'");
            }
            else
            {
                Debug.LogError("Live2DControllerが見つかりません");
            }
        }
    }

    // モデル参照を取得してハンドラーをアタッチするコルーチン
    private IEnumerator GetModelAndAttachHandler()
    {
        // Live2Dモデルが生成されるまで少し待機
        yield return new WaitForSeconds(0.1f);

        // Live2Dコントローラーから現在のモデルを直接取得
        GameObject modelObject = null;
        Live2DController controller = FindLive2DController();

        if (controller != null)
        {
            // コントローラーから現在のモデルオブジェクトを取得する新しいメソッドを使用
            modelObject = controller.GetModelObject(currentModelID);

            if (modelObject != null)
            {
                // 取得したモデルを保存
                activeLive2DModel = modelObject;

                // 現在のStateDataを取得
                var stateData = isWeekday ? weekdayData : holidayData;

                // タッチハンドラをアタッチ（StateDataがnullでなく、Live2D表示が有効で、タッチ機能が有効な場合）
                if (stateData != null && stateData.showLive2DModel &&
                    stateData.live2DData != null && stateData.live2DData.enableTouch)
                {
                    SetupLive2DTouchHandler(activeLive2DModel);
                    Debug.Log($"モデル オブジェクトが見つかり、ハンドラーがアタッチされました: {modelObject.name}");
                }
            }
            else
            {
                Debug.LogWarning("Could not find Live2D model object after delay");
            }
        }
    }

    // Live2DControllerを検索して取得
    private Live2DController FindLive2DController()
    {
        // まずはGameLoopから取得を試みる
        if (GameLoop.Instance != null)
        {
            // 1. 専用のLive2DContainerがある場合
            GameObject container = GameObject.Find("Live2DContainer");
            if (container != null)
            {
                Live2DController controller = container.GetComponent<Live2DController>();
                if (controller != null) return controller;
            }

            // 2. シーン内を検索
            return FindObjectOfType<Live2DController>();
        }

        return null;
    }

    // Live2Dモデルのクリーンアップ
    private void CleanupLive2DModel()
    {
        if (!string.IsNullOrEmpty(currentModelID))
        {
            // Live2DControllerを取得
            Live2DController live2DController = FindLive2DController();
            if (live2DController != null)
            {
                // モデルの非表示
                live2DController.HideModel(currentModelID);
            }

            // 参照をクリア
            activeLive2DModel = null;
            currentModelID = string.Empty;
        }
    }

    // タッチハンドラーをアタッチするメソッド - 強化版
    private void SetupLive2DTouchHandler(GameObject live2DModel)
    {
        if (live2DModel == null)
        {
            Debug.LogError("Cannot attach touch handler to null model");
            return;
        }

        // モデルオブジェクトの情報を詳細にログ出力
        Debug.Log($"モデルのタッチハンドラーを設定: {live2DModel.name}, " +
                  $"Active: {live2DModel.activeSelf}, " +
                  $"Path: {GetGameObjectPath(live2DModel)}");

        // 既にアタッチされたハンドラーを確実に削除（重複防止）
        CharacterTouchHandler existingHandler = live2DModel.GetComponent<CharacterTouchHandler>();
        if (existingHandler != null)
        {
            DestroyImmediate(existingHandler);
        }

        // 新規にハンドラーをアタッチ
        CharacterTouchHandler touchHandler = live2DModel.AddComponent<CharacterTouchHandler>();

        // 即座に初期化を実行（コルーチンのタイミングに依存しない）
        if (touchHandler != null)
        {
            touchHandler.Initialize(currentModelID);
            // Debug.Log("Touch handler successfully attached and initialized");
        }
        else
        {
            Debug.LogError("Failed to attach CharacterTouchHandler component");
        }
    }

    // GameObject階層パスを取得するヘルパーメソッド（デバッグ用）
    private string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        while (obj.transform.parent != null)
        {
            obj = obj.transform.parent.gameObject;
            path = obj.name + "/" + path;
        }
        return path;
    }

    // タッチハンドラーを無効化するメソッド
    private void DisableLive2DTouchHandler()
    {
        if (activeLive2DModel != null)
        {
            CharacterTouchHandler handler = activeLive2DModel.GetComponent<CharacterTouchHandler>();
            if (handler != null)
            {
                handler.DisableTouchDetection();
                Debug.Log("Live2D touch handler disabled in DayState");
            }
        }
    }

    #endregion

    #region --- イベント関連 ---

    // イベント購読の設定 - 変更
    private void SubscribeToEvents()
    {
        // 進行度更新とボタン状態更新のみを購読
        TypedEventManager.Instance.Subscribe<GameEvents.ButtonStateUpdateRequested>(OnButtonStateUpdateRequested);
    }

    // イベント購読の解除 - 変更
    private void UnsubscribeFromEvents()
    {
        if (TypedEventManager.Instance == null) return;

        TypedEventManager.Instance.Unsubscribe<GameEvents.ButtonStateUpdateRequested>(OnButtonStateUpdateRequested);
    }

    private void OnButtonStateUpdateRequested(GameEvents.ButtonStateUpdateRequested eventData)
    {
        // このステートのIDと一致する場合のみ処理
        if (eventData.CurrentStateID == StateID.Day)
        {
            UpdateButtonStates();
        }
    }

    // ボタン状態の更新（既存のメソッドを維持）
    private void UpdateButtonStates()
    {
        // 実装省略（既存のボタン状態更新処理を使用）
    }

    #endregion
}