using UnityEngine;

/// <summary>
/// 状態遷移などのタイミングでイベント発生チェックを呼び出すためのユーティリティ
/// </summary>
public static class EventTriggerChecker
{
    /// <summary>
    /// 指定されたタイミングでのイベント発生チェックを要求
    /// </summary>
    /// <param name="timing"></param>
    public static void Check(TriggerTiming timing)
    {
        if (GameLoop.Instance == null)
        {
            Debug.LogError("EventTriggerChecker: GameLoopのインスタンスが見つかりません");
            return;
        }

        // 現在の状態を取得
        StateID currentState = GameLoop.Instance.MainStateMachine.CurrentStateID;
        StateID nextState;

        // タイミングに応じた次の状態を推定
        switch (timing)
        {
            case TriggerTiming.DayToEvening:
                nextState = StateID.Evening;
                break;

            case TriggerTiming.EveningToNight:
                nextState = StateID.Night;
                break;

            case TriggerTiming.NightToDay:
                nextState = StateID.Day;
                break;

            default:
                Debug.LogWarning($"EventTriggerChecker: サポートされていないトリガータイミング {timing}");
                return;
        }

        // NovelEventSchedulerのチェックを要求
        GameLoop.Instance.NovelEventScheduler.RequestCheck(currentState, nextState);

        // 次フレームでCheckAndPushIfNeededがGameLoopから呼ばれるのを待つ
    }
}