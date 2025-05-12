using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NovelEventManager : MonoBehaviour
{
    [SerializeField] private ProgressManager progress;
    [SerializeField] private StateMachine stateMachine;

    private Dictionary<TriggerTiming, List<NovelEventData>> table;

    void Awake()
    {
        // EventTriggerChecker.Init(this);

        table = new();
        foreach (var e in Resources.LoadAll<NovelEventData>("Events"))
        {
            if (!table.ContainsKey(e.triggerTiming)) table[e.triggerTiming] = new();
            table[e.triggerTiming].Add(e);
        }
    }

    /// <summary>昼→夕 などの遷移直前に呼ばれる</summary>
    public bool TryLaunch(TriggerTiming timing)
    {
        if (!table.ContainsKey(timing)) return false;

        foreach (var e in table[timing])
        {
            // すでに発生済みのイベントはスルー
            if (progress.GetEventState(e.eventID) == EventState.Unlocked) continue;

            // 条件を満たしていないイベントはスルー
            if (!ConditionEvaluator.Evaluate(e.conditions)) continue;

            // 条件を満たしたイベントがあった！
            progress.CompleteEvent(e.eventID);
            stateMachine.NovelState.SetEventData(e);          // NovelState にイベントデータを注入
            // stateMachine.ChangeState(stateMachine.NovelState);
            return true;                              // 1 つ発動したら終了
        }

        return false;
    }
}