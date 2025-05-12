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

    /// <summary>�����[ �Ȃǂ̑J�ڒ��O�ɌĂ΂��</summary>
    public bool TryLaunch(TriggerTiming timing)
    {
        if (!table.ContainsKey(timing)) return false;

        foreach (var e in table[timing])
        {
            // ���łɔ����ς݂̃C�x���g�̓X���[
            if (progress.GetEventState(e.eventID) == EventState.Unlocked) continue;

            // �����𖞂����Ă��Ȃ��C�x���g�̓X���[
            if (!ConditionEvaluator.Evaluate(e.conditions)) continue;

            // �����𖞂������C�x���g���������I
            progress.CompleteEvent(e.eventID);
            stateMachine.NovelState.SetEventData(e);          // NovelState �ɃC�x���g�f�[�^�𒍓�
            // stateMachine.ChangeState(stateMachine.NovelState);
            return true;                              // 1 ����������I��
        }

        return false;
    }
}