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

    // イベントがunlockされた際に通知するイベント
    public event Action<int> OnEventUnlocked;

    // イベント状態をStatusDataに保存
    public void SaveEventStatesToStatus(StatusData statusData)
    {
        statusData.eventStates.Clear();

        foreach (var pair in eventStates)
        {
            statusData.eventStates.Add(new EventStateData(pair.Key, pair.Value));
        }

        Debug.Log($"ProgressManager: {eventStates.Count}件のイベント状態を保存しました");
    }

    // StatusDataからイベント状態を復元
    public void LoadEventStatesFromStatus(StatusData statusData)
    {
        eventStates.Clear();

        if (statusData.eventStates != null)
        {
            foreach (var eventStateData in statusData.eventStates)
            {
                eventStates[eventStateData.eventId] = eventStateData.state;
            }

            Debug.Log($"ProgressManager: {statusData.eventStates.Count}件のイベント状態を読み込みました");
        }
    }

    // イベントの状態を取得して返却
    // もしイベントが存在しない場合は、デフォルトでLockedを設定して返却
    public EventState GetEventState(int eventID)
    {
        if (!eventStates.ContainsKey(eventID))
        {
            eventStates[eventID] = EventState.Locked;
            Debug.Log($"ProgressManager: イベントID {eventID} は初めてのアクセスで Locked に設定");
        }
        return eventStates[eventID];
    }

    // イベントの状態をUnlockedに設定
    public void UnlockEvent(int eventID)
    {
        eventStates[eventID] = EventState.Unlocked;
        Debug.Log($"ProgressManager: イベントID {eventID} の状態を Unlocked に設定");
        OnEventUnlocked?.Invoke(eventID);
    }

    // イベントの状態をCompletedに設定
    public void CompleteEvent(int eventID)
    {
        eventStates[eventID] = EventState.Completed;
        Debug.Log($"ProgressManager: イベントID {eventID} の状態を Completed に設定しました");
    }

    // 新規追加メソッド：イベント状態の直接設定
    public void SetEventState(int eventID, EventState state)
    {
        eventStates[eventID] = state;
        Debug.Log($"ProgressManager: イベントID {eventID} の状態を {state} に設定しました");
    }

    // 新規追加メソッド：全イベントのリセット
    public void ResetAllEvents()
    {
        eventStates.Clear();
        Debug.Log("ProgressManager: 全イベントの状態をリセットしました");
    }
}
