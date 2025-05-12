using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : Singleton<StateMachine>
{
    // ���ׂẴX�e�[�g���擾
    [SerializeField] private DayState dayState;
    [SerializeField] private EveningState eveningState;
    [SerializeField] private NightState nightState;
    [SerializeField] private NovelState novelState;

    public DayState DayState => dayState;
    public EveningState EveningState => eveningState;
    public NightState NightState => nightState;
    public NovelState NovelState => novelState;

    private IState currentState;

    private void Start()
    {
        ChangeState(dayState);
    }

    // ���݂̃X�e�[�g�����O���֌��J
    public string CurrentState => currentState?.GetType().Name;

    /// <summary>
    /// �X�e�[�g��؂�ւ��郁�\�b�h
    /// �X�e�[�g�J�ڗ�
    /// stateMachine.ChangeState(new DayState(isWeekday));
    /// </summary>
    public void ChangeState(IState newState)
    {
        if (currentState != null)
        {
            // �Â��X�e�[�g��OnExit()���Ă�ŗL���t���O��false��
            currentState.OnExit();
            currentState.enabled = false;

            // �����Â��X�e�[�g����NextState���ݒ肳��Ă���΁A�����D��
            // �܂�����œn���ꂽnewState�͖��������
            if (currentState.NextState != null)
            {
                newState = currentState.NextState;
            }
        }

        // �V�����X�e�[�g���N��
        newState.enabled = true;
        newState.OnEnter();

        // ���݂̃X�e�[�g���X�V
        currentState = newState;
    }

    /// <summary>
    /// ���݂̃X�e�[�g��Update���Ăяo�����\�b�h
    /// </summary>
    public void Update()
    {
        // �� �e�X�g�p�̃L�[���͔��� ��
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Day �� Evening �� Night �� Day �c�Ə��ɑJ��
            if (ReferenceEquals(currentState, dayState))
                ChangeState(eveningState);
            else if (ReferenceEquals(currentState, eveningState))
                ChangeState(nightState);
            else if (ReferenceEquals(currentState, nightState))
                ChangeState(dayState);
        }

        currentState?.OnUpdate();
    }
}
