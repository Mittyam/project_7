using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EventType { Memory, Choice, Animation, None } // Memory / Choice / Animation...

public class SubStateMachine : MonoBehaviour
{
    private ISubState currentState;

    // 現在演出中のイベントID
    // 状況によっては複数管理も検討
    private int currentEventID;

    [Header("サブイベント終了時メインイベントスキップ用")]
    [SerializeField] private StateMachine mainStateMachine;

    [Header("各サブステート")]
    [SerializeField] private SubIdleState idleState;
    [SerializeField] private ChoiceTriggerState choiceTriggerState;
    [SerializeField] private MemoryTriggerState memoryTriggerState;
    [SerializeField] private AnimationTriggerState animationTriggerState;
    [SerializeField] private SubResultState resultState;

    private void Start()
    {
        // 初期状態をIdleに設定
        ChangeState(idleState);
    }

    public void ChangeState(ISubState newState)
    {
        if (currentState != null)
        {
            currentState.OnExit();
            currentState.enabled = false;
        }

        // 新しいステートのEnter処理を呼び出して有効化
        newState.enabled = true;
        newState.OnEnter();

        // 新しいステートをセット
        currentState = newState;
    }

    public void Update()
    {
        // ★ テスト用のキー入力判定 ★
        if (Input.GetKeyDown(KeyCode.A))
        {
            // Day → Evening → Night → Day …と順に遷移
            if (ReferenceEquals(currentState, idleState))
                ChangeState(resultState);
            else if (ReferenceEquals(currentState, resultState))
                ChangeState(idleState);
        }

        currentState?.OnUpdate();
    }

    // サブイベントを開始するメソッド
    public void StartSubEvent(int eventID, EventType eventType)
    {
        currentEventID = eventID;

        switch (eventType)
        {
            case EventType.Memory:
                memoryTriggerState.SetEventID(eventID);
                ChangeState(memoryTriggerState);
                break;

            case EventType.Choice:
                choiceTriggerState.SetEventID(eventID);
                ChangeState(choiceTriggerState);
                break;

            case EventType.Animation:
                animationTriggerState.SetEventID(eventID);
                ChangeState(animationTriggerState);
                break;
        }
    }

    // 演出終了後などに ResultState → Idle に戻る流れ
    public void EndSubEvent()
    {
        ChangeState(resultState);
    }

    public void ReturnToIdle()
    {
        ChangeState(idleState);
    }
    
    // メインイベントのスキップとサブステートの切り替え
    public void SkipToDayState()
    {
        mainStateMachine.ChangeState(mainStateMachine.DayState);
        ChangeState(idleState);
    }
    
    public void SkipToEveningState()
    {
        mainStateMachine.ChangeState(mainStateMachine.EveningState);
        ChangeState(idleState);
    }

    public void SkipToNightState()
    {
        mainStateMachine.ChangeState(mainStateMachine.NightState);
        ChangeState(idleState);
    }
}
