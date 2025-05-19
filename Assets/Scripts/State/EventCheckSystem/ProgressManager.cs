using System;
using System.Collections.Generic;
using UnityEngine;

public enum EventState
{
    Locked,
    Unlocked,
    Completed,
}

public class ProgressManager : Singleton<ProgressManager>
{
    private Dictionary<int, EventState> eventStates = new Dictionary<int, EventState>();

    // �C�x���g��unlock���ꂽ�ۂɒʒm����C�x���g
    public event Action<int> OnEventUnlocked;

    // �C�x���g��Ԃ�StatusData�ɕۑ�
    public void SaveEventStatesToStatus(StatusData statusData)
    {
        statusData.eventStates.Clear();

        foreach (var pair in eventStates)
        {
            statusData.eventStates.Add(new EventStateData(pair.Key, pair.Value));
        }

        Debug.Log($"ProgressManager: {eventStates.Count}���̃C�x���g��Ԃ�ۑ����܂���");
    }

    // StatusData����C�x���g��Ԃ𕜌�
    public void LoadEventStatesFromStatus(StatusData statusData)
    {
        eventStates.Clear();

        if (statusData.eventStates != null)
        {
            foreach (var eventStateData in statusData.eventStates)
            {
                eventStates[eventStateData.eventId] = eventStateData.state;
            }

            Debug.Log($"ProgressManager: {statusData.eventStates.Count}���̃C�x���g��Ԃ�ǂݍ��݂܂���");
        }
    }

    // �C�x���g�̏�Ԃ��擾���ĕԋp
    // �����C�x���g�����݂��Ȃ��ꍇ�́A�f�t�H���g��Locked��ݒ肵�ĕԋp
    public EventState GetEventState(int eventID)
    {
        if (!eventStates.ContainsKey(eventID))
        {
            eventStates[eventID] = EventState.Locked;
            Debug.Log($"ProgressManager: �C�x���gID {eventID} �͏��߂ẴA�N�Z�X�� Locked �ɐݒ�");
        }
        return eventStates[eventID];
    }

    // �C�x���g�̏�Ԃ�Unlocked�ɐݒ�
    public void UnlockEvent(int eventID)
    {
        eventStates[eventID] = EventState.Unlocked;
        Debug.Log($"ProgressManager: �C�x���gID {eventID} �̏�Ԃ� Unlocked �ɐݒ�");
        OnEventUnlocked?.Invoke(eventID);
    }

    // �C�x���g�̏�Ԃ�Completed�ɐݒ�
    public void CompleteEvent(int eventID)
    {
        eventStates[eventID] = EventState.Completed;
        Debug.Log($"ProgressManager: �C�x���gID {eventID} �̏�Ԃ� Completed �ɐݒ肵�܂���");
    }

    // �V�K�ǉ����\�b�h�F�C�x���g��Ԃ̒��ڐݒ�
    public void SetEventState(int eventID, EventState state)
    {
        eventStates[eventID] = state;
        Debug.Log($"ProgressManager: �C�x���gID {eventID} �̏�Ԃ� {state} �ɐݒ肵�܂���");
    }

    // �V�K�ǉ����\�b�h�F�S�C�x���g�̃��Z�b�g
    public void ResetAllEvents()
    {
        eventStates.Clear();
        Debug.Log("ProgressManager: �S�C�x���g�̏�Ԃ����Z�b�g���܂���");
    }
}
