using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoiceTriggerState : MonoBehaviour, ISubState
{
    [SerializeField] private SubStateMachine subStateMachine;
    [SerializeField] private ProgressManager progressManager;

    private int eventID;
    private bool isFinished;

    bool ISubState.enabled
    {
        get => base.enabled;    // MonoBehaviour‚Ìenabled
        set => base.enabled = value;
    }

    public void SetEventID(int id)
    {
        eventID = id;
    }

    public void OnEnter()
    {

    }

    public void OnUpdate()
    {

    }

    public void OnExit()
    {

    }
}
