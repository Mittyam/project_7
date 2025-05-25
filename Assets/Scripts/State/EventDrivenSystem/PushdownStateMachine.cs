using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PushdownStateMachine : MonoBehaviour
{
    private Stack<IState> stateStack = new Stack<IState>();

    [SerializeField] private MainStateMachine mainStateMachine;

    // スタックが空かどうか
    public bool IsEmpty => stateStack.Count == 0;

    // 現在のトップステート
    public IState CurrentState => IsEmpty ? null : stateStack.Peek();

    // 新しいステートをスタックに積む
    public void Push(IState newState)
    {
        if (newState == null)
        {
            Debug.LogError("PushdownStateMachine: 追加しようとしたステートがnullです");
            return;
        }

        // デバッグログを追加
        Debug.Log($"PushdownStateMachine: {newState.GetType().Name} をスタックに Push します (GameObject: {newState.gameObject.name}, アクティブ状態: {newState.gameObject.activeSelf})");

        if (!IsEmpty)
        {
            // 現在のステートを非アクティブ化
            IState currentState = stateStack.Peek();

            Debug.Log($"PushdownStateMachine: 現在のステート {currentState.GetType().Name} を一時停止します (GameObject: {currentState.gameObject.name})");

            currentState.OnExit();
            currentState.gameObject.SetActive(false);

            // IPausableStateの場合はPauseを呼び出す
            if (currentState is IPausableState pausable)
            {
                pausable.OnPause();
            }
        }
        else if (mainStateMachine != null && mainStateMachine.CurrentState != null)
        {
            // スタックが空の場合、MainStateMachineのステートを非アクティブ化
            mainStateMachine.CurrentState.gameObject.SetActive(false);

            // IPausableの場合はOnPauseを呼び出す
            if (mainStateMachine.CurrentState is IPausableState pausable)
            {
                pausable.OnPause();
            }
        }

        // 新しいステートをスタックに積み、開始する
        stateStack.Push(newState);

        // ステートが非アクティブならアクティブにする
        if (!newState.gameObject.activeSelf)
        {
            Debug.Log($"PushdownStateMachine: {newState.GetType().Name} を明示的にアクティブ化します");
            newState.gameObject.SetActive(true);
        }

        newState.OnEnter();

        Debug.Log($"PushdownStateMachine: {newState.GetType().Name} をスタックに Push しました (スタック数: {stateStack.Count})");
    }


    // 現在のステートをスタックから取り除く
    public void Pop()
    {
        if (IsEmpty)
        {
            Debug.LogWarning("PushdownStateMachine: スタックが空のためpopできません");
            return;
        }

        // 現在のステートを終了し、スタックから取り除く
        IState poppedState = stateStack.Pop();

        Debug.Log($"PushdownStateMachine: {poppedState.GetType().Name} をスタックから Pop します");

        // ミニイベントステートの場合、メインステート遷移フラグをチェック
        bool shouldAdvanceMainState = false;
        if (poppedState is MiniEventState miniEventState)
        {
            shouldAdvanceMainState = miniEventState.ShouldAdvanceMainStateOnCompletion;
        }

        poppedState.OnExit();
        poppedState.gameObject.SetActive(false);

        Debug.Log($"PushdownsStateMachine: {poppedState.GetType().Name} をスタックから Pop しました (残りスタック数: {stateStack.Count})");

        // スタックが空でなければ、次のステートをResumeする
        if (!IsEmpty)
        {
            IState nextState = stateStack.Peek();

            Debug.Log($"PushdownStateMachine: 次のステート {nextState.GetType().Name} を再開します");

            // まずGameObjectをアクティブ化
            nextState.gameObject.SetActive(true);

            // Resumeメソッドがあれば、Raumeを呼び出す
            if (nextState is IPausableState pausable)
            {
                pausable.OnResume();
            }
            else
            {
                // 通常のIStateの場合はOnEnterを再度呼ぶ
                nextState.OnEnter();
            }
        }
        else if (mainStateMachine != null && mainStateMachine.CurrentState != null)
        {
            // スタックが空になった場合、MainStateMachineのステートをアクティブ化
            mainStateMachine.CurrentState.gameObject.SetActive(true);

            // IPausableの場合はOnResumeを呼び出す
            if (mainStateMachine.CurrentState is IPausableState pausable)
            {
                pausable.OnResume();
            }

            // ミニイベントが完了し、フラグが設定されていればメインステートを次に進める
            if (shouldAdvanceMainState)
            {
                Debug.Log("PushdownStateMachine: ミニイベント完了によりメインステートを遷移します");
                mainStateMachine.AdvanceToNextState();
            }
        }

        TransitionManager.Instance.FadeIn();
    }

    // 現在のトップステートを更新する
    public void Update()
    {
        // メインシーン以外では処理をスキップ
        if (SceneManager.GetActiveScene().name != "MainScene")
        {
            return;
        }

        if (!IsEmpty)
        {
            IState currentState = stateStack.Peek();
            currentState.OnUpdate();
        }
    }
}