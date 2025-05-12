using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameEventListener : MonoBehaviour
{
    public GameEvent gameEvent;     // Inspector�Őݒ肷��GameEvent
    public UnityEvent response;     // �C�x���g�������Ɏ��s����鏈����Inspector�Őݒ�\

    private void OnEnable()
    {
        gameEvent.RegisterListener(this);
    }

    private void OnDisable()
    {
        gameEvent.UnregisterListener(this);
    }

    // GameEvent��Raise���ꂽ�Ƃ��ɌĂ΂��
    public void OnEventRaised()
    {
        response.Invoke(); // UnityEvent�����s
    }
}
