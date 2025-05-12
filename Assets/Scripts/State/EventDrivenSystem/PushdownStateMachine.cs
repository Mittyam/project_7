using System.Collections.Generic;
using UnityEngine;

public class PushdownStateMachine : MonoBehaviour
{
    private Stack<IState> stateStack = new Stack<IState>();

    [SerializeField] private MainStateMachine mainStateMachine;

    // �X�^�b�N���󂩂ǂ���
    public bool IsEmpty => stateStack.Count == 0;

    // ���݂̃g�b�v�X�e�[�g
    public IState CurrentState => IsEmpty ? null : stateStack.Peek();

    // �V�����X�e�[�g���X�^�b�N�ɐς�
    public void Push(IState newState)
    {
        if (newState == null)
        {
            Debug.LogError("PushdownStateMachine: �ǉ����悤�Ƃ����X�e�[�g��null�ł�");
            return;
        }

        // �f�o�b�O���O��ǉ�
        Debug.Log($"PushdownStateMachine: {newState.GetType().Name} ���X�^�b�N�� Push ���܂� (GameObject: {newState.gameObject.name}, �A�N�e�B�u���: {newState.gameObject.activeSelf})");

        if (!IsEmpty)
        {
            // ���݂̃X�e�[�g���A�N�e�B�u��
            IState currentState = stateStack.Peek();

            Debug.Log($"PushdownStateMachine: ���݂̃X�e�[�g {currentState.GetType().Name} ���ꎞ��~���܂� (GameObject: {currentState.gameObject.name})");

            currentState.OnExit();
            currentState.gameObject.SetActive(false);

            // IPausableState�̏ꍇ��Pause���Ăяo��
            if (currentState is IPausableState pausable)
            {
                pausable.OnPause();
            }
        }
        else if (mainStateMachine != null && mainStateMachine.CurrentState != null)
        {
            // �X�^�b�N����̏ꍇ�AMainStateMachine�̃X�e�[�g���A�N�e�B�u��
            mainStateMachine.CurrentState.gameObject.SetActive(false);

            // IPausable�̏ꍇ��OnPause���Ăяo��
            if (mainStateMachine.CurrentState is IPausableState pausable)
            {
                pausable.OnPause();
            }
        }

        // �V�����X�e�[�g���X�^�b�N�ɐς݁A�J�n����
        stateStack.Push(newState);

        // �X�e�[�g����A�N�e�B�u�Ȃ�A�N�e�B�u�ɂ���
        if (!newState.gameObject.activeSelf)
        {
            Debug.Log($"PushdownStateMachine: {newState.GetType().Name} �𖾎��I�ɃA�N�e�B�u�����܂�");
            newState.gameObject.SetActive(true);
        }

        newState.OnEnter();

        Debug.Log($"PushdownStateMachine: {newState.GetType().Name} ���X�^�b�N�� Push ���܂��� (�X�^�b�N��: {stateStack.Count})");
    }


    // ���݂̃X�e�[�g���X�^�b�N�����菜��
    public void Pop()
    {
        if (IsEmpty)
        {
            Debug.LogWarning("PushdownStateMachine: �X�^�b�N����̂���pop�ł��܂���");
            return;
        }

        // ���݂̃X�e�[�g���I�����A�X�^�b�N�����菜��
        IState poppedState = stateStack.Pop();

        Debug.Log($"PushdownStateMachine: {poppedState.GetType().Name} ���X�^�b�N���� Pop ���܂�");

        poppedState.OnExit();
        poppedState.gameObject.SetActive(false);

        Debug.Log($"PushdownsStateMachine: {poppedState.GetType().Name} ���X�^�b�N���� Pop ���܂��� (�c��X�^�b�N��: {stateStack.Count})");

        // �X�^�b�N����łȂ���΁A���̃X�e�[�g��Resume����
        if (!IsEmpty)
        {
            IState nextState = stateStack.Peek();

            Debug.Log($"PushdownStateMachine: ���̃X�e�[�g {nextState.GetType().Name} ���ĊJ���܂�");

            // �܂�GameObject���A�N�e�B�u��
            nextState.gameObject.SetActive(true);

            // Resume���\�b�h�������Raumem���Ăяo��
            if (nextState is IPausableState pausable)
            {
                pausable.OnResume();
            }
            else
            {
                // �ʏ��IState�̏ꍇ��OnEnter���ēx�Ă�
                nextState.OnEnter();
            }
        }
        else if (mainStateMachine != null && mainStateMachine.CurrentState != null)
        {
            // �X�^�b�N����ɂȂ����ꍇ�AMainStateMachine�̃X�e�[�g���A�N�e�B�u��
            mainStateMachine.CurrentState.gameObject.SetActive(true);

            // IPausable�̏ꍇ��OnResume���Ăяo��
            if (mainStateMachine.CurrentState is IPausableState pausable)
            {
                pausable.OnResume();
            }
        }

        TransitionManager.Instance.FadeIn();
    }

    // ���݂̃g�b�v�X�e�[�g���X�V����
    public void Update()
    {
        if (!IsEmpty)
        {
            IState currentState = stateStack.Peek();
            currentState.OnUpdate();
        }
    }
}