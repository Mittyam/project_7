using UnityEngine;
using UnityEngine.SceneManagement;

public class MainStateMachine : MonoBehaviour
{
    // 現在のステート
    private IState currentState;
    private StateID currentStateID = StateID.None;

    // currentStateを外部へ公開するためのプロパティ
    public IState CurrentState => currentState;
    public StateID CurrentStateID => currentStateID;

    /// <summary>
    /// ステートマシンの初期化（GameLoopから呼び出される）
    /// </summary>
    public void Initialize(StateID initialStateID)
    {
        // 初期ステートを開始
        ChangeState(initialStateID);
    }

    /// <summary>
    /// 毎フレームの更新
    /// </summary>
    public void Update()
    {
        // メインシーン以外では処理をスキップ
        if (SceneManager.GetActiveScene().name != "MainScene")
        {
            return;
        }

        if (currentState != null && currentState.gameObject.activeSelf)
        {
            currentState.OnUpdate();
        }
    }

    /// <summary>
    /// 指定されたIDのステートに遷移する
    /// </summary>
    public void ChangeState(StateID newStateID)
    {
        StateID oldStateID = currentStateID;

        // 現在のステートがあれば終了処理
        if (currentState != null)
        {
            currentState.OnExit();
            currentState.gameObject.SetActive(false);
        }

        // 次のステートに遷移
        IState nextState = GameLoop.Instance.StatesContainer.GetMainState(newStateID);
        if (nextState != null)
        {
            // 新しいステートを開始
            currentState = nextState;
            currentStateID = newStateID;
            currentState.gameObject.SetActive(true);
            currentState.OnEnter();

            Debug.Log($"MainStateMachine: {oldStateID} から {newStateID} に遷移しました");

            // イベントチェックのリクエスト
            GameLoop.Instance.NovelEventScheduler.RequestCheck(oldStateID, newStateID);
        }
        else
        {
            Debug.LogError($"MainStateMachine: {newStateID} に対応するステートが見つかりません");
        }
    }

    /// <summary>
    /// 次のステートに進む（現在のStateDataに設定されたnextStateIDに基づく）
    /// </summary>
    public void AdvanceToNextState()
    {
        if (currentState == null)
        {
            Debug.LogError("MainStateMachine: 現在のステートがnullです");
            return;
        }

        // 現在のステートから次のStateIDを取得する方法
        StateID nextStateID = StateID.None;

        // 各ステートクラスに応じて処理を分岐
        if (currentState is DayState dayState)
        {
            nextStateID = dayState.GetNextStateID();
        }
        else if (currentState is EveningState eveningState)
        {
            nextStateID = eveningState.GetNextStateID();
        }
        else if (currentState is NightState nightState)
        {
            nextStateID = nightState.GetNextStateID();
        }

        if (nextStateID != StateID.None)
        {
            ChangeState(nextStateID);
        }
        else
        {
            Debug.LogError($"MainStateMachine: 現在のステート {currentStateID} に次のステートが設定されていません");
        }
    }
}