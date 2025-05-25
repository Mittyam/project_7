using UnityEngine;
using UnityEngine.SceneManagement;

public class MainStateMachine : MonoBehaviour
{
    // ���݂̃X�e�[�g
    private IState currentState;
    private StateID currentStateID = StateID.None;

    // currentState���O���֌��J���邽�߂̃v���p�e�B
    public IState CurrentState => currentState;
    public StateID CurrentStateID => currentStateID;

    /// <summary>
    /// �X�e�[�g�}�V���̏������iGameLoop����Ăяo�����j
    /// </summary>
    public void Initialize(StateID initialStateID)
    {
        // �����X�e�[�g���J�n
        ChangeState(initialStateID);
    }

    /// <summary>
    /// ���t���[���̍X�V
    /// </summary>
    public void Update()
    {
        // ���C���V�[���ȊO�ł͏������X�L�b�v
        if (SceneManager.GetActiveScene().name != "MainScene")
        {
            return;
        }

        if (currentState != null && currentState.gameObject.activeSelf)
        {
            currentState.OnUpdate();
        }
    }

    /// <summary>
    /// �w�肳�ꂽID�̃X�e�[�g�ɑJ�ڂ���
    /// </summary>
    public void ChangeState(StateID newStateID)
    {
        StateID oldStateID = currentStateID;

        // ���݂̃X�e�[�g������ΏI������
        if (currentState != null)
        {
            currentState.OnExit();
            currentState.gameObject.SetActive(false);
        }

        // ���̃X�e�[�g�ɑJ��
        IState nextState = GameLoop.Instance.StatesContainer.GetMainState(newStateID);
        if (nextState != null)
        {
            // �V�����X�e�[�g���J�n
            currentState = nextState;
            currentStateID = newStateID;
            currentState.gameObject.SetActive(true);
            currentState.OnEnter();

            Debug.Log($"MainStateMachine: {oldStateID} ���� {newStateID} �ɑJ�ڂ��܂���");

            // �C�x���g�`�F�b�N�̃��N�G�X�g
            GameLoop.Instance.NovelEventScheduler.RequestCheck(oldStateID, newStateID);
        }
        else
        {
            Debug.LogError($"MainStateMachine: {newStateID} �ɑΉ�����X�e�[�g��������܂���");
        }
    }

    /// <summary>
    /// ���̃X�e�[�g�ɐi�ށi���݂�StateData�ɐݒ肳�ꂽnextStateID�Ɋ�Â��j
    /// </summary>
    public void AdvanceToNextState()
    {
        if (currentState == null)
        {
            Debug.LogError("MainStateMachine: ���݂̃X�e�[�g��null�ł�");
            return;
        }

        // ���݂̃X�e�[�g���玟��StateID���擾������@
        StateID nextStateID = StateID.None;

        // �e�X�e�[�g�N���X�ɉ����ď����𕪊�
        if (currentState is DayState dayState)
        {
            nextStateID = dayState.GetNextStateID();
        }
        else if (currentState is EveningState eveningState)
        {
            nextStateID = eveningState.GetNextStateID();
        }
        else if (currentState is NightState nightState)
        {
            nextStateID = nightState.GetNextStateID();
        }

        if (nextStateID != StateID.None)
        {
            ChangeState(nextStateID);
        }
        else
        {
            Debug.LogError($"MainStateMachine: ���݂̃X�e�[�g {currentStateID} �Ɏ��̃X�e�[�g���ݒ肳��Ă��܂���");
        }
    }
}