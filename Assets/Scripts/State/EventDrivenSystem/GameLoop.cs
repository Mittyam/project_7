using System.Collections.Generic;
using UnityEngine;

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

    private void Awake()
    {
        base.Awake();

        // 初期化の確認（UIManagerProviderの確認を削除）
        if (statesContainer == null || pushdownStack == null ||
            mainStateMachine == null || novelEventScheduler == null)
        {
            Debug.LogError("GameLoop: 必要なコンポーネントが設定されていません");
        }
    }

    private void Start()
    {
        // メインステートの初期化と開始
        mainStateMachine.Initialize(StateID.Day);

        // 各ステートにUIカメラを設定
        SetUICameraToAllStates();

        // ProgressManagerとStatusManagerの連携を確認
        if (StatusManager.Instance != null)
        {
            ProgressManager progressManager = FindObjectOfType<ProgressManager>();
            if (progressManager != null && !StatusManager.Instance.GetProgressManager())
            {
                StatusManager.Instance.SetProgressManager(progressManager);
                Debug.Log("GameLoop: ProgressManagerとStatusManagerを連携しました");
            }
        }
    }

    private void Update()
    {
        // 1. スタックがあればまずはそれを実行（優先）
        if (!pushdownStack.IsEmpty)
        {
            // MainStateMachineのステートを非アクティブ化
            if (mainStateMachine.CurrentState != null &&
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

        // MainStateにカメラを設定
        SetUICameraToState(statesContainer.GetMainState(StateID.Day) as StateBase);
        SetUICameraToState(statesContainer.GetMainState(StateID.Evening) as StateBase);
        SetUICameraToState(statesContainer.GetMainState(StateID.Night) as StateBase);

        // 必要に応じてミニイベントステートなど他のステートにも設定
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
}