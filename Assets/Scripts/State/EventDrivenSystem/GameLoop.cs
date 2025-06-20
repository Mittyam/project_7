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

    // クラスのフィールドに追加
    private static bool isLoadedFromTitle = false;
    private static StateID loadedStateID = StateID.None;

    private static bool isReturningFromMemory = false;

    private void Awake()
    {
        base.Awake();

        // シーン遷移イベントリスナーを登録
        SceneManager.sceneLoaded += OnSceneLoaded;

        // 現在のシーン名を記録
        currentSceneName = SceneManager.GetActiveScene().name;

        // メインシーン以外では処理をスキップ
        if (currentSceneName != "MainScene" || !isInitialized)
        {
            return;
        }

        // StatesContainerの取得（既存のコード）
        if (statesContainer == null)
        {
            statesContainer = FindObjectOfType<StatesContainer>();
            if (statesContainer == null)
            {
                Debug.LogError("GameLoop: StatesContainerが見つかりません");
            }
        }

        if (pushdownStack == null)
        {
            pushdownStack = FindObjectOfType<PushdownStateMachine>();
            if (pushdownStack == null)
            {
                Debug.LogError("GameLoop: StatesContainerが見つかりません");
            }
        }

        if (mainStateMachine == null)
        {
            mainStateMachine = FindObjectOfType<MainStateMachine>();
            if (mainStateMachine == null)
            {
                Debug.LogError("GameLoop: StatesContainerが見つかりません");
            }
        }

        if (novelEventScheduler == null)
        {
            novelEventScheduler = FindObjectOfType<NovelEventScheduler>();
            if (novelEventScheduler == null)
            {
                Debug.LogError("GameLoop: NovelEventSchedulerが見つかりません");
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

                // 新規ゲーム開始チェックをここで実行
                CheckAndStartNewGame();
            }
        }

        currentSceneName = SceneManager.GetActiveScene().name;
    }

    // タイトルからロードされたことを記録するメソッドを追加
    public static void SetLoadedStateFromTitle(StateID stateID)
    {
        isLoadedFromTitle = true;
        loadedStateID = stateID;
        Debug.Log($"GameLoop: タイトルからのロード状態を記録 - StateID: {stateID}");
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

        // PushdownStackの再取得
        if (pushdownStack == null)
        {
            // まずはオブジェクト名で検索
            GameObject pushdownStackObj = GameObject.Find("PushdownStateMachine");
            if (pushdownStackObj != null)
            {
                pushdownStack = pushdownStackObj.GetComponent<PushdownStateMachine>();
                Debug.Log("GameLoop: PushdownStateMachineをオブジェクト名で再取得しました");
            }
            else
            {
                // オブジェクト名で見つからない場合は型で検索
                pushdownStack = FindObjectOfType<PushdownStateMachine>();
                if (pushdownStack != null)
                {
                    Debug.Log("GameLoop: PushdownStateMachineを型で再取得しました");
                }
                else
                {
                    Debug.LogError("GameLoop: PushdownStateMachineが見つかりません");
                }
            }
        }

        // MainStateMachineの再取得
        if (mainStateMachine == null)
        {
            // まずはオブジェクト名で検索
            GameObject mainStateMachineObj = GameObject.Find("MainStateMachine");
            if (mainStateMachineObj != null)
            {
                mainStateMachine = mainStateMachineObj.GetComponent<MainStateMachine>();
                Debug.Log("GameLoop: MainStateMachineをオブジェクト名で再取得しました");
            }
            else
            {
                // オブジェクト名で見つからない場合は型で検索
                mainStateMachine = FindObjectOfType<MainStateMachine>();
                if (mainStateMachine != null)
                {
                    Debug.Log("GameLoop: MainStateMachineを型で再取得しました");
                }
                else
                {
                    Debug.LogError("GameLoop: MainStateMachineが見つかりません");
                }
            }
        }

        // NovelEventSchedulerの再取得
        if (novelEventScheduler == null)
        {
            // まずはオブジェクト名で検索
            GameObject novelEventSchedulerObj = GameObject.Find("NovelEventScheduler");
            if (novelEventSchedulerObj != null)
            {
                novelEventScheduler = novelEventSchedulerObj.GetComponent<NovelEventScheduler>();
                Debug.Log("GameLoop: NovelEventSchedulerをオブジェクト名で再取得しました");
            }
            else
            {
                // オブジェクト名で見つからない場合は型で検索
                novelEventScheduler = FindObjectOfType<NovelEventScheduler>();
                if (novelEventScheduler != null)
                {
                    Debug.Log("GameLoop: NovelEventSchedulerを型で再取得しました");
                }
                else
                {
                    Debug.LogError("GameLoop: NovelEventSchedulerが見つかりません");
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
            }
            else
            {
                // Tagで検索
                mainCameraObj = GameObject.FindWithTag("MainCamera");
                if (mainCameraObj != null)
                {
                    mainUICamera = mainCameraObj.GetComponent<Camera>();
                }
                else
                {
                    Debug.LogWarning("GameLoop: MainCameraが見つかりません");
                }
            }
        }

        if (mainStateMachine != null && !isInitialized && statesContainer != null)
        {
            isInitialized = true;

            // ロードされたデータがある場合はそのステートで初期化
            StateID initialStateID = StateID.Day; // デフォルトはDay

            if (isLoadedFromTitle && loadedStateID != StateID.None)
            {
                initialStateID = loadedStateID;
                Debug.Log($"GameLoop: タイトルからロードされたステート {loadedStateID} で初期化します");

                // フラグをリセット
                isLoadedFromTitle = false;
                loadedStateID = StateID.None;
            }
            else if (StatusManager.Instance != null && StatusManager.Instance.GetStatus() != null)
            {
                // StatusManagerに保存されたステートIDがあればそれを使用
                var savedState = StatusManager.Instance.GetStatus().savedStateID;
                if (savedState != StateID.None)
                {
                    initialStateID = savedState;
                    Debug.Log($"GameLoop: 保存されたステート {savedState} で初期化します");
                }
            }

            mainStateMachine.Initialize(initialStateID);

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

        // 新規ゲーム開始チェック
        CheckAndStartNewGame();
    }

    // 新しいメソッドを追加
    private void CheckAndStartNewGame()
    {
        // PlayerPrefsから新規ゲーム開始フラグを取得
        int isNewGameStart = PlayerPrefs.GetInt("IsNewGameStart", 0);

        if (isNewGameStart == 1 && currentSceneName == "MainScene")
        {
            // フラグをリセット
            PlayerPrefs.SetInt("IsNewGameStart", 0);
            PlayerPrefs.Save();

            // 少し遅延してから最初のイベントを開始（初期化が完了するのを待つ）
            StartCoroutine(StartFirstNovelEvent());
        }
    }

    // 最初のNovelEventを開始するコルーチンを追加
    private IEnumerator StartFirstNovelEvent()
    {
        // 初期化が完了するまで待機
        yield return new WaitForEndOfFrame();

        // EventID = 1のイベントデータを取得
        NovelEventData firstEvent = Resources.Load<NovelEventData>("Events/1.出会い");

        if (firstEvent == null)
        {
            Debug.LogError("GameLoop: 最初のイベント(EventID=1)が見つかりません");
            yield break;
        }

        // NovelEventSchedulerを使って手動でイベントをプッシュ
        if (novelEventScheduler != null)
        {
            novelEventScheduler.PushEvent(firstEvent);
            Debug.Log("GameLoop: 最初のNovelEventを開始しました");
        }
        else
        {
            Debug.LogError("GameLoop: NovelEventSchedulerが見つかりません");
        }
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
            // MainStateMachineのステートを非アクティブ化（nullチェック追加）
            if (mainStateMachine != null &&
                mainStateMachine.CurrentState != null &&
                mainStateMachine.CurrentState.gameObject != null &&
                mainStateMachine.CurrentState.gameObject.activeSelf)
            {
                mainStateMachine.CurrentState.gameObject.SetActive(false);
            }

            pushdownStack.Update();
        }
        // 2. スタックが空ならノベルイベントをチェック
        else if (novelEventScheduler != null && novelEventScheduler.CheckAndPushIfNeeded())
        {
            // CheckAndPushIfNeeded内でPushを実行済み
            Debug.Log("GameLoop: ノベルイベントを開始しました");
        }
        // 3. 何もなければメインステートを更新
        else
        {
            // MainStateMachineとCurrentStateの存在を確認
            if (mainStateMachine != null && mainStateMachine.CurrentState != null)
            {
                // MainStateMachineのステートがアクティブでなければアクティブ化
                if (mainStateMachine.CurrentState.gameObject != null &&
                    !mainStateMachine.CurrentState.gameObject.activeSelf)
                {
                    mainStateMachine.CurrentState.gameObject.SetActive(true);
                }

                mainStateMachine.Update();
            }
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

    /// <summary>
    /// メモリーからの復帰であることを設定
    /// </summary>
    public static void SetReturningFromMemory(bool returning)
    {
        isReturningFromMemory = returning;
        Debug.Log($"GameLoop: メモリー復帰フラグを設定 - {returning}");
    }

    /// <summary>
    /// メモリーからの復帰かどうかを取得
    /// </summary>
    public static bool IsReturningFromMemory()
    {
        return isReturningFromMemory;
    }

    /// <summary>
    /// メモリー復帰フラグをリセット
    /// </summary>
    public static void ResetMemoryReturnFlag()
    {
        isReturningFromMemory = false;
    }
}