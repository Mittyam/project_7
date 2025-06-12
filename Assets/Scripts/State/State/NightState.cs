using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static GameEvents;

// 服装タイプのEnum（Live2DControllerと共有）
public enum ClothingType
{
    Casual,     // 私服
    Pajamas     // パジャマ
}

/// <summary>
/// 夜ステート
/// ・ステータス更新
/// ・夜ならではのサブイベントの判定など
/// </summary>
public class NightState : StateBase, IPausableState
{
    [Header("State Data")]
    [SerializeField] private MainStateData stateData;

    [Header("UI要素")]
    [SerializeField] private GameObject nightUIContainer;
    [SerializeField] private GameObject commonUIContainer;

    [Header("Events")]
    [SerializeField] private GameEvent nightStateEventSO;

    [Header("Bath Button Settings")]
    [SerializeField] private string bathButtonTag = "BathButton"; // お風呂ボタンのタグ
    [SerializeField] private int bathEventID = 7; // お風呂イベントのID
    private GameObject bathButton; // お風呂ボタンのGameObject（動的に取得）

    [Header("Live2D Controller")]
    [SerializeField] private Live2DController live2DController; // Live2Dモデルを制御するコンポーネント

    // Live2D関連フィールド追加
    private GameObject activeLive2DModel; // アクティブなLive2Dモデルへの参照
    private string currentModelID; // 現在表示中のモデルID

    // UI生成状態管理用フラグ
    private bool isUICreated = false;

    // お風呂ボタン関連のコンポーネント参照
    private UIEventPublisher bathEventPublisher;
    private UISoundHandler bathSoundHandler;
    private Image bathButtonImage;


    public override void OnEnter()
    {
        Debug.Log("NightStateに入ります。");
        StatusManager.Instance.OnStateChanged();

        // StatusManagerに服装状態管理を有効化
        StatusManager.Instance.EnableClothingState();

        // UI要素のセットアップと表示
        SetupUI();
        ShowNightUI();

        // お風呂ボタンの状態を更新
        UpdateBathButtonState();

        // Index 2~4 のBGMをランダムに再生
        int randomIndex = Random.Range(8, 11);
        SoundManager.Instance.PlayBGMWithFadeIn(randomIndex, 1f);

        // イベント発火
        nightStateEventSO.Raise();

        // Live2Dモデルを初期化して表示
        SetupLive2DModel();

        // StatusManagerから服装状態を取得して適用
        ApplyClothingFromStatusManager();

        // ProgressManagerのイベント更新を監視
        ProgressManager.Instance.OnProgressUpdated += OnProgressUpdated;

        // StatusManagerの服装変更イベントを購読
        if (StatusManager.Instance != null)
        {
            StatusManager.Instance.OnClothingStateChanged += OnClothingStateChanged;
        }
    }

    public override void OnUpdate()
    {

    }

    public override void OnExit()
    {
        Debug.Log("NightState: 出ます。");

        // StatusManagerに服装状態管理を無効化（私服にリセット）
        StatusManager.Instance.DisableClothingState();

        // BGMを停止
        SoundManager.Instance.StopBGM();

        // タッチハンドラーを無効化
        DisableLive2DTouchHandler();

        // Live2Dモデルのクリーンアップ
        CleanupLive2DModel();

        // UIの非表示化
        HideAllUI();

        // イベント監視を解除
        ProgressManager.Instance.OnProgressUpdated -= OnProgressUpdated;

        // StatusManagerの服装変更イベントの購読解除
        if (StatusManager.Instance != null)
        {
            StatusManager.Instance.OnClothingStateChanged -= OnClothingStateChanged;
        }

        // 次の日への遷移なので、ここでのノベルイベントチェック
        EventTriggerChecker.Check(TriggerTiming.NightToDay);
    }

    public void OnPause()
    {
        Debug.Log("NightState: 一時停止します");

        // UI要素を非表示にする
        HideAllUI();

        SoundManager.Instance.StopBGM();
    }

    public void OnResume()
    {
        Debug.Log("NightState: 再開します");

        // UI要素を再表示する
        SetupUI();
        ShowNightUI();

        // お風呂ボタンの状態を更新
        UpdateBathButtonState();

        // Index 2~4 のBGMをランダムに再生
        int randomIndex = Random.Range(8, 11);
        SoundManager.Instance.PlayBGMWithFadeIn(randomIndex, 1f);

        // StatusManagerから服装状態を取得して適用（BathStateから戻った場合など）
        ApplyClothingFromStatusManager();
    }

    // 次のステートの取得メソッド
    public StateID GetNextStateID()
    {
        return stateData != null ? stateData.nextStateID : StateID.Day;
    }

    #region --- お風呂ボタン関連メソッド ---

    // タグを使用してお風呂ボタンを検索
    private void FindBathButton()
    {
        // タグでお風呂ボタンを検索
        bathButton = GameObject.FindWithTag(bathButtonTag);

        if (bathButton == null)
        {
            // タグで見つからない場合は、少し待ってから再試行するコルーチンを開始
            StartCoroutine(DelayedFindBathButton());
        }
        else
        {
            Debug.Log($"NightState: お風呂ボタンを発見しました - {bathButton.name}");
            // コンポーネント参照を初期化
            InitializeBathButtonReferences();
        }
    }

    // 遅延してお風呂ボタンを検索するコルーチン
    private IEnumerator DelayedFindBathButton()
    {
        float maxWaitTime = 2.0f; // 最大待機時間
        float elapsedTime = 0f;
        float checkInterval = 0.1f; // チェック間隔

        while (elapsedTime < maxWaitTime)
        {
            yield return new WaitForSeconds(checkInterval);
            elapsedTime += checkInterval;

            bathButton = GameObject.FindWithTag(bathButtonTag);
            if (bathButton != null)
            {
                Debug.Log($"NightState: お風呂ボタンを遅延検索で発見しました - {bathButton.name}");
                InitializeBathButtonReferences();
                UpdateBathButtonState();
                yield break;
            }
        }

        Debug.LogWarning($"NightState: タグ '{bathButtonTag}' のお風呂ボタンが見つかりませんでした");
    }

    // お風呂ボタンのコンポーネント参照を取得
    private void InitializeBathButtonReferences()
    {
        if (bathButton == null)
        {
            Debug.LogWarning("NightState: bathButtonが設定されていません");
            return;
        }

        // 各コンポーネントの参照を取得
        bathEventPublisher = bathButton.GetComponent<UIEventPublisher>();
        bathSoundHandler = bathButton.GetComponent<UISoundHandler>();

        // BathImageという名前の子オブジェクトからImageコンポーネントを取得
        Transform bathImageTransform = bathButton.transform.Find("BathImage");
        if (bathImageTransform != null)
        {
            bathButtonImage = bathImageTransform.GetComponent<Image>();
        }
        else
        {
            // GetComponentInChildrenで深い階層も含めて検索
            bathButtonImage = bathButton.GetComponentInChildren<Image>();
        }

        if (bathEventPublisher == null)
            Debug.LogWarning("NightState: UIEventPublisherが見つかりません");
        if (bathSoundHandler == null)
            Debug.LogWarning("NightState: UISoundHandlerが見つかりません");
        if (bathButtonImage == null)
            Debug.LogWarning("NightState: Imageコンポーネントが見つかりません");
    }

    // お風呂ボタンの状態を更新
    private void UpdateBathButtonState()
    {
        // お風呂ボタンがまだ見つかっていない場合は検索を試みる
        if (bathButton == null)
        {
            FindBathButton();
            return;
        }

        // コンポーネント参照を初期化（まだ取得していない場合）
        if (bathEventPublisher == null || bathSoundHandler == null || bathButtonImage == null)
        {
            InitializeBathButtonReferences();
        }

        // ProgressManagerからイベント7の状態を取得
        EventState eventState = ProgressManager.Instance.GetEventState(bathEventID);

        if (eventState == EventState.Completed)
        {
            // Completedの場合：ボタンを有効化
            SetBathButtonCompleted();
        }
        else
        {
            // Complete前の場合：ボタンを無効化
            SetBathButtonLocked();
        }

        Debug.Log($"NightState: お風呂ボタンの状態を更新 - Event {bathEventID} : {eventState}");
    }

    // お風呂ボタンをCompleted状態に設定
    private void SetBathButtonCompleted()
    {
        // UIEventPublisher設定
        if (bathEventPublisher != null)
        {
            bathEventPublisher.SetProperties(
                UIEventPublisher.EventType.BathSelect,
                0, // itemID
                "", // itemName
                0, // actionPointCost
                "" // panelName
            );
        }

        // UISoundHandler設定
        if (bathSoundHandler != null)
        {
            bathSoundHandler.SetClickSoundType(UISoundHandler.SoundType.Minimal5);
            bathSoundHandler.SetHoverSoundType(UISoundHandler.SoundType.start); // ホバー音も設定
        }

        // Image Color設定（白色）
        if (bathButtonImage != null)
        {
            bathButtonImage.color = new Color(255f / 255f, 255f / 255f, 255f / 255f, 1f);
        }

        // ボタンは常に有効（interactableは変更しない）
        Button buttonComponent = bathButton.GetComponent<Button>();
        if (buttonComponent != null)
        {
            buttonComponent.interactable = true;
        }
    }

    // お風呂ボタンをLocked状態に設定
    private void SetBathButtonLocked()
    {
        // UIEventPublisher設定
        if (bathEventPublisher != null)
        {
            bathEventPublisher.SetProperties(
                UIEventPublisher.EventType.None,
                0, // itemID
                "", // itemName
                0, // actionPointCost
                "" // panelName
            );
        }

        // UISoundHandler設定
        if (bathSoundHandler != null)
        {
            bathSoundHandler.SetClickSoundType(UISoundHandler.SoundType.Cancel);
            bathSoundHandler.SetHoverSoundType(UISoundHandler.SoundType.start); // ホバー音は通常の音
        }

        // Image Color設定（グレー）
        if (bathButtonImage != null)
        {
            bathButtonImage.color = new Color(180f / 255f, 180f / 255f, 180f / 255f, 1f);
        }

        // ボタンは常に有効にして、見た目だけグレーにする
        Button buttonComponent = bathButton.GetComponent<Button>();
        if (buttonComponent != null)
        {
            buttonComponent.interactable = true; // 常にtrueにする
        }
    }

    // ProgressManagerの更新イベントハンドラ
    private void OnProgressUpdated()
    {
        // お風呂ボタンの状態を更新
        UpdateBathButtonState();
    }

    // StatusManagerから服装状態を取得して適用
    private void ApplyClothingFromStatusManager()
    {
        if (StatusManager.Instance != null)
        {
            ClothingType currentClothing = StatusManager.Instance.GetClothingState();
            ApplyClothingToModel(currentClothing);
        }
    }

    // StatusManagerの服装変更イベントハンドラ
    private void OnClothingStateChanged(ClothingType newClothingType)
    {
        Debug.Log($"NightState: 服装変更イベントを受信 - {newClothingType}");
        ApplyClothingToModel(newClothingType);
    }

    // モデルに服装を適用
    private void ApplyClothingToModel(ClothingType clothingType)
    {
        if (string.IsNullOrEmpty(currentModelID))
            return;

        if (live2DController != null)
        {
            // Live2DControllerに服装変更を通知
            live2DController.ApplyClothingToModel(currentModelID, clothingType);
            Debug.Log($"NightState: モデルに服装 {clothingType} を適用しました");
        }
    }

    #endregion

    #region --- UI関連メソッド ---

    // UIのセットアップ
    private void SetupUI()
    {
        // UIコンテナの初期化
        if (nightUIContainer == null)
        {
            nightUIContainer = new GameObject("NightUIContainer");
            nightUIContainer.transform.SetParent(transform);
            nightUIContainer.AddComponent<RectTransform>();
        }

        if (commonUIContainer == null)
        {
            commonUIContainer = new GameObject("CommonUIContainer");
            commonUIContainer.transform.SetParent(transform);
            commonUIContainer.AddComponent<RectTransform>();
        }

        // UIプレハブの生成（まだ生成されていない場合のみ）
        if (!isUICreated && stateData != null && stateData.uiPrefab != null)
        {
            foreach (var prefab in stateData.uiPrefab)
            {
                if (prefab != null)
                {
                    // カメラ設定付きのインスタンス化を使用
                    InstantiateUIWithCamera(prefab, nightUIContainer.transform);
                }
            }
            isUICreated = true;

            Debug.Log("NightState: UIプレハブを生成し、カメラ設定を適用しました");
        }

        // UIが生成された後、お風呂ボタンを検索
        FindBathButton();

        // 夜特有のUI設定や状態更新があればここで実行
        // 例: 夜の雰囲気エフェクト適用など
        // UpdateNightUIEffects();
    }

    // 夜UIの表示
    private void ShowNightUI()
    {
        if (nightUIContainer != null)
        {
            nightUIContainer.SetActive(true);
        }

        if (commonUIContainer != null)
        {
            commonUIContainer.SetActive(true);
        }
    }

    // 夜特有のUIエフェクト更新（オプション - 必要に応じて実装）
    private void UpdateNightUIEffects()
    {
        // 例: ライティングエフェクトやカラーフィルターの適用
        // 夜の雰囲気を演出するUI調整
        if (nightUIContainer != null)
        {
            // 例: 明かりエフェクトの点滅設定など
            // ここに夜特有のエフェクト処理を追加
        }
    }

    // 全UIの非表示
    private void HideAllUI()
    {
        if (nightUIContainer != null)
        {
            nightUIContainer.SetActive(false);
        }

        if (commonUIContainer != null)
        {
            commonUIContainer.SetActive(false);
        }
    }

    #endregion

    #region --- Live2D関連メソッド ---

    // Live2Dモデルをセットアップするメソッド
    private void SetupLive2DModel()
    {
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
            if (live2DController != null)
            {
                // モデルIDを保存
                currentModelID = live2DData.modelID;

                // Live2Dモデルを表示
                live2DController.ShowModel(
                    currentModelID,
                    live2DData.scale,
                    $"{live2DData.position.x},{live2DData.position.y}"
                );

                // アニメーション再生
                if (!string.IsNullOrEmpty(live2DData.defaultAnimTrigger))
                {
                    live2DController.PlayAnimation(currentModelID, live2DData.defaultAnimTrigger);
                }

                // モデル参照取得を確実に遅延させるためのコルーチンを開始
                StartCoroutine(GetModelAndAttachHandler());

                Debug.Log($"Live2D model '{currentModelID}' displayed in NightState");
            }
            else
            {
                Debug.LogError("Live2DController not found");
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

        if (live2DController != null)
        {
            // コントローラーから現在のモデルオブジェクトを取得する新しいメソッドを使用
            modelObject = live2DController.GetModelObject(currentModelID);

            if (modelObject != null)
            {
                // 取得したモデルを保存
                activeLive2DModel = modelObject;

                // タッチハンドラをアタッチ（StateDataがnullでなく、Live2D表示が有効で、タッチ機能が有効な場合）
                if (stateData != null && stateData.showLive2DModel &&
                    stateData.live2DData != null && stateData.live2DData.enableTouch)
                {
                    SetupLive2DTouchHandler(activeLive2DModel);
                    Debug.Log($"Model object found and handler attached: {modelObject.name}");
                }
            }
            else
            {
                Debug.LogWarning("Could not find Live2D model object after delay");
            }
        }
    }

    // Live2Dモデルのクリーンアップ
    private void CleanupLive2DModel()
    {
        if (!string.IsNullOrEmpty(currentModelID))
        {
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

    // タッチハンドラーを無効化するメソッド
    private void DisableLive2DTouchHandler()
    {
        if (activeLive2DModel != null)
        {
            CharacterTouchHandler handler = activeLive2DModel.GetComponent<CharacterTouchHandler>();
            if (handler != null)
            {
                handler.DisableTouchDetection();
                Debug.Log("Live2D touch handler disabled in NightState");
            }
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

    #endregion
}