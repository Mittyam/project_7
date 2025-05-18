using System.Collections.Generic;
using UnityEngine;

public class GameLoop : Singleton<GameLoop>
{
    [Header("UI�R���e�i")]
    [SerializeField] private StatesContainer statesContainer;

    [Header("�C�x���g�i�s�V�X�e��")]
    [SerializeField] private PushdownStateMachine pushdownStack;
    [SerializeField] private MainStateMachine mainStateMachine;
    [SerializeField] private NovelEventScheduler novelEventScheduler;

    [Header("UI Settings")]
    [SerializeField] private Camera mainUICamera; // UI�\���p���C���J����

    // �v���p�e�B��`
    public PushdownStateMachine PushdownStack => pushdownStack;
    public MainStateMachine MainStateMachine => mainStateMachine;
    public NovelEventScheduler NovelEventScheduler => novelEventScheduler;
    public StatesContainer StatesContainer => statesContainer;

    private void Awake()
    {
        base.Awake();

        // �������̊m�F�iUIManagerProvider�̊m�F���폜�j
        if (statesContainer == null || pushdownStack == null ||
            mainStateMachine == null || novelEventScheduler == null)
        {
            Debug.LogError("GameLoop: �K�v�ȃR���|�[�l���g���ݒ肳��Ă��܂���");
        }
    }

    private void Start()
    {
        // ���C���X�e�[�g�̏������ƊJ�n
        mainStateMachine.Initialize(StateID.Day);

        // �e�X�e�[�g��UI�J������ݒ�
        SetUICameraToAllStates();

        // ProgressManager��StatusManager�̘A�g���m�F
        if (StatusManager.Instance != null)
        {
            ProgressManager progressManager = FindObjectOfType<ProgressManager>();
            if (progressManager != null && !StatusManager.Instance.GetProgressManager())
            {
                StatusManager.Instance.SetProgressManager(progressManager);
                Debug.Log("GameLoop: ProgressManager��StatusManager��A�g���܂���");
            }
        }
    }

    private void Update()
    {
        // 1. �X�^�b�N������΂܂��͂�������s�i�D��j
        if (!pushdownStack.IsEmpty)
        {
            // MainStateMachine�̃X�e�[�g���A�N�e�B�u��
            if (mainStateMachine.CurrentState != null &&
                mainStateMachine.CurrentState.gameObject.activeSelf)
            {
                mainStateMachine.CurrentState.gameObject.SetActive(false);
            }

            pushdownStack.Update();
        }
        // 2. �X�^�b�N����Ȃ�m�x���C�x���g���`�F�b�N
        else if (novelEventScheduler.CheckAndPushIfNeeded())
        {
            // CheckAndPushIfNeeded����Push�����s�ς�
            Debug.Log("GameLoop: �m�x���C�x���g���J�n���܂���");
        }
        // 3. �����Ȃ���΃��C���X�e�[�g���X�V
        else
        {
            // MainStateMachine�̃X�e�[�g���A�N�e�B�u�łȂ���΃A�N�e�B�u��
            if (mainStateMachine.CurrentState != null &&
                !mainStateMachine.CurrentState.gameObject.activeSelf)
            {
                mainStateMachine.CurrentState.gameObject.SetActive(true);
            }

            mainStateMachine.Update();
        }
    }

    /// <summary>
    /// �~�j�C�x���g���N�����郁�\�b�h
    /// </summary>
    /// <param name="stateID"></param>
    /// <param name="parameters"></param>
    public void PushMiniEvent(StateID stateID, Dictionary<string, object> parameters = null)
    {
        IState miniEventState = statesContainer.GetMiniEventState(stateID);

        if (miniEventState != null)
        {
            // ActionType���K�v��StateID�̏ꍇ�AStateID�Ɋ�Â���ActionType�������ݒ�
            if (parameters == null)
            {
                parameters = new Dictionary<string, object>();
            }

            if (stateID == StateID.Library && !parameters.ContainsKey("ActionType"))
            {
                parameters["ActionType"] = "Library";
            }
            else if (stateID == StateID.Cafe && !parameters.ContainsKey("ActionType"))
            {
                parameters["ActionType"] = "Cafe";
            }
            else if (stateID == StateID.PartJob && !parameters.ContainsKey("ActionType")) 
            {
                parameters["ActionType"] = "Work";
            }
            else if (stateID == StateID.Walk && !parameters.ContainsKey("ActionType"))
            {
                parameters["ActionType"] = "Walk";
            }
            else if (stateID == StateID.Game && !parameters.ContainsKey("ActionType"))
            {
                parameters["ActionType"] = "Game";
            }
            else if (stateID == StateID.Outing && !parameters.ContainsKey("ActionType"))
            {
                parameters["ActionType"] = "Outing";
            }
            else if (stateID == StateID.Talk && !parameters.ContainsKey("ActionType"))
            {
                parameters["ActionType"] = "Talk";
            }
            else if (stateID == StateID.Sleep && !parameters.ContainsKey("ActionType"))
            {
                parameters["ActionType"] = "Sleep";
            }

            // �p�����[�^������ꍇ�͐ݒ�
            if (parameters != null && miniEventState is MiniEventState miniEvent)
            {
                miniEvent.SetParameters(parameters);
            }

            // PushdownStack�Ƀv�b�V��
            pushdownStack.Push(miniEventState);

            Debug.Log($"GameLoop: �~�j�C�x���g {stateID} ���v�b�V�����܂���");
        }
        else
        {
            Debug.LogError($"GameLoop: �~�j�C�x���g {stateID} ��������܂���");
        }
    }

    // �S�X�e�[�g��UI�J������ݒ�
    private void SetUICameraToAllStates()
    {
        if (mainUICamera == null)
        {
            Debug.LogWarning("MainUICamera is not assigned in GameLoop");
            return;
        }

        // MainState�ɃJ������ݒ�
        SetUICameraToState(statesContainer.GetMainState(StateID.Day) as StateBase);
        SetUICameraToState(statesContainer.GetMainState(StateID.Evening) as StateBase);
        SetUICameraToState(statesContainer.GetMainState(StateID.Night) as StateBase);

        // �K�v�ɉ����ă~�j�C�x���g�X�e�[�g�ȂǑ��̃X�e�[�g�ɂ��ݒ�
    }

    // �ʃX�e�[�g�ւ̃J�����ݒ�w���p�[���\�b�h
    private void SetUICameraToState(StateBase state)
    {
        if (state != null)
        {
            // ���t���N�V�������g�p����uiRenderCamera�t�B�[���h��ݒ�
            var field = typeof(StateBase).GetField("uiRenderCamera",
                             System.Reflection.BindingFlags.Instance |
                             System.Reflection.BindingFlags.NonPublic);

            if (field != null)
            {
                field.SetValue(state, mainUICamera);
                // Debug.Log($"Set UI camera to {state.GetType().Name}");
            }
        }
    }
}