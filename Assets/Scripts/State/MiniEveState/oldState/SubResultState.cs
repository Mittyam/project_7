using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubResultState : MonoBehaviour, ISubState
{
    bool ISubState.enabled
    {
        get => base.enabled;
        set => base.enabled = value;
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
