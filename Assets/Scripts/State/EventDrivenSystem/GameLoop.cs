using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // シーン管理用に追加

public class GameLoop : Singleton<GameLoop>
{
    [Header("UIコンテナ")]
    [SerializeField] private StatesContainer statesContainer;

    [Header("イベント進行システム")]
    [SerializeField] private PushdownStateMachine pushdownStack;
    [SerializeField] private MainStateMachine mainStateMachine;
    [SerializeField] private NovelEventScheduler novelEventScheduler;

    [Header("UI Settings")]
    [SerializeField] private Camera mainUICamera; // UI表示用メインカメラ

    // プロパティ定義
    public PushdownStateMachine PushdownStack => pushdownStack;
    public MainStateMachine MainStateMachine => mainStateMachine;
    public NovelEventScheduler NovelEventScheduler => novelEventScheduler;
    public StatesContainer StatesContainer => statesContainer;
    public Camera MainUICamera => mainUICamera;

    // シーン管理用の変数
    private string currentSceneName;
    private bool isInitialized = false;

    private void Awake()
    {
        base.Awake();

        // シーン遷移イベントリスナーを登録
        SceneManager.sceneLoaded += OnSceneLoaded;

        // 現在のシーン名を記録
        currentSceneName = SceneManager.GetActiveScene().name;

        // StatesContainerの取得（既存のコード）
        if (statesContainer == null)
        {
            statesContainer = FindObjectOfType<StatesContainer>();
            if (statesContainer == null)
            {
                Debug.LogError("GameLoop: StatesContainerが見つかりません");
            }
        }

        // 初期化の確認
        if (statesContainer == null || pushdownStack == null ||
            mainStateMachine == null || novelEventScheduler == null)
        {
            Debug.LogError("GameLoop: 必要なコンポーネントが設定されていません");
        }

        // メインシーンなら初期化を実行
        if (currentSceneName == "MainScene")
        {
            InitializeGameComponents();
        }
    }

    private void OnDestroy()
    {
        // イベントリスナーの解除（メモリリーク防止）
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // シーン読み込み完了時に呼ばれるイベントハンドラ
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"GameLoop: シーン「{scene.name}」が読み込まれました");

        // シーン名が変わった場合のみ処理
        if (currentSceneName != scene.name)
        {
            currentSceneName = scene.name;

            // MainSceneに遷移した場合
            if (scene.name == "MainScene")
            {
                Debug.Log("GameLoop: MainSceneに遷移しました。必要なコンポーネントを再取得します。");
                InitializeGameComponents();
            }
        }
    }

    // ゲームコンポーネントの初期化（メインシーン専用）
    private void InitializeGameComponents()
    {
        Debug.Log("GameLoop: ゲームコンポーネントの初期化を開始します");

        // StatesContainerの再取得
        if (statesContainer == null)
        {
            // まずはオブジェクト名で検索
            GameObject statesContainerObj = GameObject.Find("StatesContainer");
            if (statesContainerObj != null)
            {
                statesContainer = statesContainerObj.GetComponent<StatesContainer>();
                Debug.Log("GameLoop: StatesContainerをオブジェクト名で再取得しました");
            }
            else
            {
                // オブジェクト名で見つからない場合は型で検索
                statesContainer = FindObjectOfType<StatesContainer>();
                if (statesContainer != null)
                {
                    Debug.Log("GameLoop: StatesContainerを型で再取得しました");
                }
                else
                {
                    Debug.LogError("GameLoop: StatesContainerが見つかりません");
                }
            }
        }

        // MainCameraの再取得
        if (mainUICamera == null)
        {
            // カメラの検索（名前で検索）
            GameObject mainCameraObj = GameObject.Find("MainCamera");
            if (mainCameraObj != null)
            {
                mainUICamera = mainCameraObj.GetComponent<Camera>();
                Debug.Log("GameLoop: MainCameraを再取得しました");
            }
            else
            {
                // Tagで検索
                mainCameraObj = GameObject.FindWithTag("MainCamera");
                if (mainCameraObj != null)
                {
                    mainUICamera = mainCameraObj.GetComponent<Camera>();
                    Debug.Log("GameLoop: MainCameraをタグで再取得しました");
                }
                else
                {
                    Debug.LogWarning("GameLoop: MainCameraが見つかりません");
                }
            }
        }

        // MainStateを初期化（既に初期化済みでない場合のみ）
        if (mainStateMachine != null && !isInitialized && statesContainer != null)
        {
            isInitialized = true;

            // PlayerPrefsをチェックして、TitleSceneからのロード要求があるか確認
            if (PlayerPrefs.GetInt("ShouldLoadOnStart", 0) == 1)
            {
                string slotName = PlayerPrefs.GetString("SelectedSlotName", "");
                if (!string.IsNullOrEmpty(slotName))
                {
                    // 一旦Dayステートで初期化（ロード前の初期状態として）
                    mainStateMachine.Initialize(StateID.Day);

                    // フラグをリセット
                    PlayerPrefs.SetInt("ShouldLoadOnStart", 0);
                    PlayerPrefs.Save();

                    // 少し遅延してからロードする（シーン遷移が完全に終わった後）
                    StartCoroutine(LoadGameWithDelay(slotName, 0.5f));
                }
                else
                {
                    // スロット名が空の場合は通常通り初期化
                    mainStateMachine.Initialize(StateID.Day);
                }
            }
            else
            {
                // 通常通り初期化
                mainStateMachine.Initialize(StateID.Day);
            }

            // UIカメラが取得できていれば各ステートに設定
            if (mainUICamera != null)
            {
                SetUICameraToAllStates();
            }
        }
    }

    private void Start()
    {
        // タイトルシーンの場合はここで初期化しない（メインシーンへの遷移時に行う）
        if (currentSceneName != "MainScene")
        {
            Debug.Log($"GameLoop: 現在{currentSceneName}シーンです。メインシーン遷移時に初期化を行います。");
            return;
        }

        // 既に初期化済みならスキップ
        if (isInitialized)
        {
            return;
        }

        // メインシーンでの初期化処理
        // メインステートの初期化と開始
        mainStateMachine.Initialize(StateID.Day);
        isInitialized = true;

        // 各ステートにUIカメラを設定
        SetUICameraToAllStates();
    }

    private void Update()
    {
        // メインシーン以外では処理をスキップ
        if (currentSceneName != "MainScene" || !isInitialized)
        {
            return;
        }

        // 1. スタックがあればまずはそれを実行（優先）
        if (!pushdownStack.IsEmpty)
        {
            // MainStateMachineのステートを非アクティブ化
            if (mainStateMachine.CurrentState != null &&
                mainStateMachine.CurrentState.gameObject != null &&  // nullチェックを追加
                mainStateMachine.CurrentState.gameObject.activeSelf)
            {
                mainStateMachine.CurrentState.gameObject.SetActive(false);
            }

            pushdownStack.Update();
        }
        // 2. スタックが空ならノベルイベントをチェック
        else if (novelEventScheduler.CheckAndPushIfNeeded())
        {
            // CheckAndPushIfNeeded内でPushを実行済み
            Debug.Log("GameLoop: ノベルイベントを開始しました");
        }
        // 3. 何もなければメインステートを更新
        else
        {
            // MainStateMachineのステートがアクティブでなければアクティブ化
            if (mainStateMachine.CurrentState != null &&
                mainStateMachine.CurrentState.gameObject != null &&  // nullチェックを追加
                !mainStateMachine.CurrentState.gameObject.activeSelf)
            {
                mainStateMachine.CurrentState.gameObject.SetActive(true);
            }

            mainStateMachine.Update();
        }
    }

    /// <summary>
    /// ミニイベントを起動するメソッド
    /// </summary>
    /// <param name="stateID"></param>
    /// <param name="parameters"></param>
    public void PushMiniEvent(StateID stateID, Dictionary<string, object> parameters = null)
    {
        // 既存のコード（変更なし）
        IState miniEventState = statesContainer.GetMiniEventState(stateID);

        if (miniEventState != null)
        {
            // ActionTypeが必要なStateIDの場合、StateIDに基づいてActionTypeを自動設定
            if (parameters == null)
            {
                parameters = new Dictionary<string, object>();
            }

            if (stateID == StateID.Library && !parameters.ContainsKey("ActionType"))
            {
                parameters["ActionType"] = "Library";
            }
            else if (stateID == StateID.Cafe && !parameters.ContainsKey("ActionType"))
            {
                parameters["ActionType"] = "Cafe";
            }
            else if (stateID == StateID.PartJob && !parameters.ContainsKey("ActionType"))
            {
                parameters["ActionType"] = "Work";
            }
            else if (stateID == StateID.Walk && !parameters.ContainsKey("ActionType"))
            {
                parameters["ActionType"] = "Walk";
            }
            else if (stateID == StateID.Game && !parameters.ContainsKey("ActionType"))
            {
                parameters["ActionType"] = "Game";
            }
            else if (stateID == StateID.Outing && !parameters.ContainsKey("ActionType"))
            {
                parameters["ActionType"] = "Outing";
            }
            else if (stateID == StateID.Talk && !parameters.ContainsKey("ActionType"))
            {
                parameters["ActionType"] = "Talk";
            }
            else if (stateID == StateID.Sleep && !parameters.ContainsKey("ActionType"))
            {
                parameters["ActionType"] = "Sleep";
            }

            // パラメータがある場合は設定
            if (parameters != null && miniEventState is MiniEventState miniEvent)
            {
                miniEvent.SetParameters(parameters);
            }

            // PushdownStackにプッシュ
            pushdownStack.Push(miniEventState);

            Debug.Log($"GameLoop: ミニイベント {stateID} をプッシュしました");
        }
        else
        {
            Debug.LogError($"GameLoop: ミニイベント {stateID} が見つかりません");
        }
    }

    // 全ステートにUIカメラを設定
    private void SetUICameraToAllStates()
    {
        if (mainUICamera == null)
        {
            Debug.LogWarning("MainUICamera is not assigned in GameLoop");
            return;
        }

        if (statesContainer == null)
        {
            Debug.LogWarning("StatesContainer is not assigned in GameLoop");
            return;
        }

        // MainStateにカメラを設定
        SetUICameraToState(statesContainer.GetMainState(StateID.Day) as StateBase);
        SetUICameraToState(statesContainer.GetMainState(StateID.Evening) as StateBase);
        SetUICameraToState(statesContainer.GetMainState(StateID.Night) as StateBase);

        // 必要に応じてミニイベントステートなど他のステートにも設定
        Debug.Log("GameLoop: 全ステートにUIカメラを設定しました");
    }

    // 個別ステートへのカメラ設定ヘルパーメソッド
    private void SetUICameraToState(StateBase state)
    {
        if (state != null)
        {
            // リフレクションを使用してuiRenderCameraフィールドを設定
            var field = typeof(StateBase).GetField("uiRenderCamera",
                             System.Reflection.BindingFlags.Instance |
                             System.Reflection.BindingFlags.NonPublic);

            if (field != null)
            {
                field.SetValue(state, mainUICamera);
                // Debug.Log($"Set UI camera to {state.GetType().Name}");
            }
        }
    }

    // 遅延ロード用のコルーチン
    private IEnumerator LoadGameWithDelay(string slotName, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (StatusManager.Instance != null)
        {
            try
            {
                // ロードパネルを非表示にする（万が一表示されていた場合）
                if (SaveLoadUI.Instance != null)
                {
                    SaveLoadUI.Instance.CloseAllPanels();
                }

                // ロード実行
                StatusManager.Instance.LoadStatus(slotName);

                // この時点でStatusManager内でステート変更が行われているはず
                // 万が一失敗している場合に備えて確認
                if (mainStateMachine.CurrentState == null &&
                    StatusManager.Instance.GetStatus().savedStateID != StateID.None)
                {
                    // セーブされたステートIDで初期化
                    mainStateMachine.Initialize(StatusManager.Instance.GetStatus().savedStateID);
                }

                Debug.Log($"GameLoop: タイトルシーンからの要求により、スロット「{slotName}」のデータをロードしました");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"GameLoop: ロード中にエラーが発生しました: {e.Message}");
                // エラーが発生した場合はDay状態で初期化
                mainStateMachine.Initialize(StateID.Day);
            }
        }
        else
        {
            Debug.LogError("GameLoop: StatusManagerが見つかりません");
            // StatusManagerが見つからない場合は通常初期化
            mainStateMachine.Initialize(StateID.Day);
        }
    }
}