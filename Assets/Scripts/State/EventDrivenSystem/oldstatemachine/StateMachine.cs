using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : Singleton<StateMachine>
{
    // すべてのステートを取得
    [SerializeField] private DayState dayState;
    [SerializeField] private EveningState eveningState;
    [SerializeField] private NightState nightState;
    [SerializeField] private NovelState novelState;

    public DayState DayState => dayState;
    public EveningState EveningState => eveningState;
    public NightState NightState => nightState;
    public NovelState NovelState => novelState;

    private IState currentState;

    private void Start()
    {
        ChangeState(dayState);
    }

    // 現在のステート名を外部へ公開
    public string CurrentState => currentState?.GetType().Name;

    /// <summary>
    /// ステートを切り替えるメソッド
    /// ステート遷移例
    /// stateMachine.ChangeState(new DayState(isWeekday));
    /// </summary>
    public void ChangeState(IState newState)
    {
        if (currentState != null)
        {
            // 古いステートのOnExit()を呼んで有効フラグをfalseに
            currentState.OnExit();
            currentState.enabled = false;

            // もし古いステート側でNextStateが設定されていれば、それを優先
            // つまり引数で渡されたnewStateは無視される
            if (currentState.NextState != null)
            {
                newState = currentState.NextState;
            }
        }

        // 新しいステートを起動
        newState.enabled = true;
        newState.OnEnter();

        // 現在のステートを更新
        currentState = newState;
    }

    /// <summary>
    /// 現在のステートのUpdateを呼び出すメソッド
    /// </summary>
    public void Update()
    {
        // ★ テスト用のキー入力判定 ★
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Day → Evening → Night → Day …と順に遷移
            if (ReferenceEquals(currentState, dayState))
                ChangeState(eveningState);
            else if (ReferenceEquals(currentState, eveningState))
                ChangeState(nightState);
            else if (ReferenceEquals(currentState, nightState))
                ChangeState(dayState);
        }

        currentState?.OnUpdate();
    }
}
