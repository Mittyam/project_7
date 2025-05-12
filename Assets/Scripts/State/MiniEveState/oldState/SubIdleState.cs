using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubIdleState : MonoBehaviour, ISubState
{
    bool ISubState.enabled
    {
        get => base.enabled;
        set => base.enabled = value;
    }

    public void OnEnter()
    {
        Debug.Log("SubIdleState‚É“ü‚è‚Ü‚·B");
    }

    public void OnUpdate()
    {

    }

    public void OnExit()
    {

    }
}
