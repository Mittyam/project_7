using UnityEngine;
using static GameEvents;

/// <summary>
/// �[���X�e�[�g
/// ���i�̍w���Ȃ�
/// </summary>
public class EveningState : StateBase, IPausableState
{
    [Header("State Data")]
    [SerializeField] private MainStateData stateData;

    // UI�֘A�t�B�[���h
    [Header("UI�v�f")]
    [SerializeField] private GameObject eveningUIContainer;
    [SerializeField] private GameObject commonUIContainer;

    [Header("Events")]
    [SerializeField] private GameEvent eveningStateEventSO;

    private float eveningProgress; // �[���̐i�s�x

    // UI������ԊǗ��p�t���O
    private bool isUICreated = false;

    public override void OnEnter()
    {
        Debug.Log("EveningState: ����܂��B");

        // UI�v�f�̃Z�b�g�A�b�v�ƕ\��
        SetupUI();
        ShowEveningUI();

        eveningProgress = 0f;

        // �C�x���g����
        eveningStateEventSO?.Raise();

        // �C�x���g�w��
        SubscribeToEvents();
    }

    public override void OnUpdate()
    {
        // �[���̐i�s�x���X�V
        if (eveningProgress >= 1.0f)
        {
            MainStateMachine.AdvanceToNextState();
        }
    }

    public override void OnExit()
    {
        Debug.Log("EveningState: �����܂��B");

        // �C�x���g�w�ǉ���
        UnsubscribeFromEvents();

        // UI�̔�\����
        HideAllUI();

        // ���̃X�e�[�g�Ɉڍs����O�Ƀm�x���C�x���g�̔��΃`�F�b�N
        EventTriggerChecker.Check(TriggerTiming.EveningToNight);
    }

    public void OnPause()
    {
        Debug.Log("EveningState: �ꎞ��~���܂�");

        // UI�v�f���\���ɂ���
        HideAllUI();
    }

    public void OnResume()
    {
        Debug.Log("EveningState: �ĊJ���܂�");

        // UI�v�f���ĕ\������
        SetupUI();
        ShowEveningUI();
    }

    // ���̃X�e�[�g�̎擾���\�b�h
    public StateID GetNextStateID()
    {
        return stateData != null ? stateData.nextStateID : StateID.Night;
    }

    #region --- �������牺��UI�֘A�̐V���\�b�h ---

    // UI�̃Z�b�g�A�b�v
    private void SetupUI()
    {
        // UI�R���e�i�̏������i�����̃R�[�h�j
        if (eveningUIContainer == null)
        {
            eveningUIContainer = new GameObject("EveningUIContainer");
            eveningUIContainer.transform.SetParent(transform);
            eveningUIContainer.AddComponent<RectTransform>();
        }

        if (commonUIContainer == null)
        {
            commonUIContainer = new GameObject("CommonUIContainer");
            commonUIContainer.transform.SetParent(transform);
            commonUIContainer.AddComponent<RectTransform>();
        }

        // UI�v���n�u�̐����i�܂���������Ă��Ȃ��ꍇ�̂݁j
        if (!isUICreated && stateData != null && stateData.uiPrefab != null)
        {
            foreach (var prefab in stateData.uiPrefab)
            {
                if (prefab != null)
                {
                    // �J�����ݒ�t���̃C���X�^���X�����\�b�h���g�p
                    InstantiateUIWithCamera(prefab, eveningUIContainer.transform);
                }
            }
            isUICreated = true;

            Debug.Log("EveningState: UI�v���n�u�𐶐����A�J�����ݒ��K�p���܂���");
        }

        // �{�^����Ԃ̍X�V��ǉ���UI�ݒ肪����΂����Ŏ��s
        // UpdateButtonStates();
    }

    // �[��UI�̕\��
    private void ShowEveningUI()
    {
        if (eveningUIContainer != null)
        {
            eveningUIContainer.SetActive(true);
        }

        if (commonUIContainer != null)
        {
            commonUIContainer.SetActive(true);
        }
    }

    // �SUI�̔�\��
    private void HideAllUI()
    {
        if (eveningUIContainer != null)
        {
            eveningUIContainer.SetActive(false);
        }

        if (commonUIContainer != null)
        {
            commonUIContainer.SetActive(false);
        }
    }

    #endregion

    #region --- �C�x���g�w�ǃ��\�b�h ---

    // �C�x���g�w�ǂ̐ݒ� - �ύX
    private void SubscribeToEvents()
    {
        // �i�s�x�X�V�ƃ{�^����ԍX�V�݂̂��w��
        TypedEventManager.Instance.Subscribe<GameEvents.EveningProgressUpdated>(OnEveningProgressUpdated);
        TypedEventManager.Instance.Subscribe<GameEvents.ButtonStateUpdateRequested>(OnButtonStateUpdateRequested);
    }

    // �C�x���g�w�ǂ̉��� - �ύX
    private void UnsubscribeFromEvents()
    {
        if (TypedEventManager.Instance == null) return;

        TypedEventManager.Instance.Unsubscribe<GameEvents.EveningProgressUpdated>(OnEveningProgressUpdated);
        TypedEventManager.Instance.Unsubscribe<GameEvents.ButtonStateUpdateRequested>(OnButtonStateUpdateRequested);
    }

    // �V�����C�x���g�n���h���[
    private void OnEveningProgressUpdated(GameEvents.EveningProgressUpdated eventData)
    {
        UpdateEveningProgress(eventData.ProgressValue);
    }

    private void OnButtonStateUpdateRequested(GameEvents.ButtonStateUpdateRequested eventData)
    {
        // ���̃X�e�[�g��ID�ƈ�v����ꍇ�̂ݏ���
        if (eventData.CurrentStateID == StateID.Evening)
        {
            UpdateButtonStates();
        }
    }

    // �[���̐i�s�x���X�V
    private void UpdateEveningProgress(float progressValue)
    {
        eveningProgress += progressValue;
    }

    // �{�^����Ԃ̍X�V�i�����̃��\�b�h���ێ��j
    private void UpdateButtonStates()
    {
        // �����ȗ��i�����̃{�^����ԍX�V�������g�p�j
    }

    // ����{�^���Ȃǂ̃C�x���g�n���h��
    private void OnCloseButtonClicked(GameEvents.CloseButtonClicked eventData)
    {
        // �p�l�����̃`�F�b�N�ȂǁA�K�v�ɉ����ď�������
        //if (eventData.SourcePanelName == "EveningPanel")
        //{
        //    eveningProgress += 0.5f;  // �i�s�x�𑝉�
        //}
    }

    // �w���{�^���C�x���g�n���h��
    //private void OnPurchaseButtonClicked(GameEvents.PurchaseButtonClicked eventData)
    //{
    //    // �w������
    //    bool purchaseSuccess = true; // ���ۂɂ͍w���������`�F�b�N����

    //    if (purchaseSuccess)
    //    {
    //        Debug.Log("�w�����������܂���");
    //        eveningProgress += 1.0f;  // �i�s�x��1����

    //        // �K�v�ɉ����ăX�e�[�^�X�X�V
    //        // StatusManager.Instance.UpdateStatus(0, 0, 0, -eventData.Cost);
    //    }
    //}

    #endregion
}