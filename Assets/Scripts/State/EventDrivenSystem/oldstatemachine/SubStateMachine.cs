using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EventType { Memory, Choice, Animation, None } // Memory / Choice / Animation...

public class SubStateMachine : MonoBehaviour
{
    private ISubState currentState;

    // ���݉��o���̃C�x���gID
    // �󋵂ɂ���Ă͕����Ǘ�������
    private int currentEventID;

    [Header("�T�u�C�x���g�I�������C���C�x���g�X�L�b�v�p")]
    [SerializeField] private StateMachine mainStateMachine;

    [Header("�e�T�u�X�e�[�g")]
    [SerializeField] private SubIdleState idleState;
    [SerializeField] private ChoiceTriggerState choiceTriggerState;
    [SerializeField] private MemoryTriggerState memoryTriggerState;
    [SerializeField] private AnimationTriggerState animationTriggerState;
    [SerializeField] private SubResultState resultState;

    private void Start()
    {
        // ������Ԃ�Idle�ɐݒ�
        ChangeState(idleState);
    }

    public void ChangeState(ISubState newState)
    {
        if (currentState != null)
        {
            currentState.OnExit();
            currentState.enabled = false;
        }

        // �V�����X�e�[�g��Enter�������Ăяo���ėL����
        newState.enabled = true;
        newState.OnEnter();

        // �V�����X�e�[�g���Z�b�g
        currentState = newState;
    }

    public void Update()
    {
        // �� �e�X�g�p�̃L�[���͔��� ��
        if (Input.GetKeyDown(KeyCode.A))
        {
            // Day �� Evening �� Night �� Day �c�Ə��ɑJ��
            if (ReferenceEquals(currentState, idleState))
                ChangeState(resultState);
            else if (ReferenceEquals(currentState, resultState))
                ChangeState(idleState);
        }

        currentState?.OnUpdate();
    }

    // �T�u�C�x���g���J�n���郁�\�b�h
    public void StartSubEvent(int eventID, EventType eventType)
    {
        currentEventID = eventID;

        switch (eventType)
        {
            case EventType.Memory:
                memoryTriggerState.SetEventID(eventID);
                ChangeState(memoryTriggerState);
                break;

            case EventType.Choice:
                choiceTriggerState.SetEventID(eventID);
                ChangeState(choiceTriggerState);
                break;

            case EventType.Animation:
                animationTriggerState.SetEventID(eventID);
                ChangeState(animationTriggerState);
                break;
        }
    }

    // ���o�I����Ȃǂ� ResultState �� Idle �ɖ߂闬��
    public void EndSubEvent()
    {
        ChangeState(resultState);
    }

    public void ReturnToIdle()
    {
        ChangeState(idleState);
    }
    
    // ���C���C�x���g�̃X�L�b�v�ƃT�u�X�e�[�g�̐؂�ւ�
    public void SkipToDayState()
    {
        mainStateMachine.ChangeState(mainStateMachine.DayState);
        ChangeState(idleState);
    }
    
    public void SkipToEveningState()
    {
        mainStateMachine.ChangeState(mainStateMachine.EveningState);
        ChangeState(idleState);
    }

    public void SkipToNightState()
    {
        mainStateMachine.ChangeState(mainStateMachine.NightState);
        ChangeState(idleState);
    }
}
