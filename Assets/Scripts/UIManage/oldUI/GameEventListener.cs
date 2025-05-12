using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameEventListener : MonoBehaviour
{
    public GameEvent gameEvent;     // Inspectorで設定するGameEvent
    public UnityEvent response;     // イベント発生時に実行される処理をInspectorで設定可能

    private void OnEnable()
    {
        gameEvent.RegisterListener(this);
    }

    private void OnDisable()
    {
        gameEvent.UnregisterListener(this);
    }

    // GameEventがRaiseされたときに呼ばれる
    public void OnEventRaised()
    {
        response.Invoke(); // UnityEventを実行
    }
}
