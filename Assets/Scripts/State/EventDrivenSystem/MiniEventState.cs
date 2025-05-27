using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// �~�j�C�x���g�i���b/���o����/�A�C�e��/�v���o�Ȃǁj�̊�{�N���X
/// </summary>
public class MiniEventState : StateBase, IPausableState
{
    // �V���A���C�Y�t�B�[���h�Ƃ��ĊǗ�
    [Header("State Data")]
    [SerializeField] protected MiniEventStateData stateData;

    // stateData�̌��J
    public MiniEventStateData StateData => stateData;

    // ��������UI�v�f�̃��X�g
    protected List<GameObject> startUIs = new List<GameObject>();
    protected List<GameObject> endUIs = new List<GameObject>();

    // UI�v�f�̐e�ƂȂ�R���e�i
    protected GameObject uiContainer;

    // �p�����[�^�i�[�p�̃f�B�N�V���i��
    protected Dictionary<string, object> parameters = new Dictionary<string, object>();

    // �~�j�C�x���g�������Ƀ��C���X�e�[�g��J�ڂ����邩�ǂ����̃t���O
    // �f�t�H���g�ł͑J�ڂ����Ȃ�
    public bool ShouldAdvanceMainStateOnCompletion { get; set; } = false;

    /// <summary>
    /// �C�x���g�f�[�^�̐ݒ�
    /// </summary>
    public void SetStateData(MiniEventStateData data)
    {
        stateData = data;
    }

    public override void OnEnter()
    {
        // �f�[�^�`�F�b�N
        if (stateData == null)
        {
            Debug.LogError($"MiniEventState: {gameObject.name}��StateData���ݒ肳��܂���");
            // �G���[���͈��S�ɏI��
            CompleteEvent();
            return;
        }

        Debug.Log($"MiniEventState: {stateData.displayName} ���J�n���܂�");

        // UI�̃Z�b�g�A�b�v
        SetupUI();

        // �C�x���g����
        TypedEventManager.Instance.Publish(new GameEvents.MiniEventStarted
        {
            EventStateID = stateData.stateID,
            EventName = stateData.displayName
        });

        // �A�N�V�����|�C���g�̏���i�p�����[�^����擾�����l��D��j
        int actionPointCost = GetParameter<int>("ActionPointCost", stateData.actionPointCost);
        if (stateData.consumeActionPoint)
        {
            StatusManager.Instance.ConsumeActionPoint(actionPointCost);

            // �A�N�V�����|�C���g��0�ɂȂ����烁�C���X�e�[�g�J�ڃt���O�𗧂Ă�
            if (StatusManager.Instance.GetCurrentActionPoints() == 0)
            {
                Debug.Log("�A�N�V�����|�C���g��0�ɂȂ�܂����B���̃��C���X�e�[�g�ɐi�݂܂��B");
                ShouldAdvanceMainStateOnCompletion = true;
            }
        }
    }

    public override void OnUpdate()
    {
        // �~�j�C�x���g���̍X�V����
    }

    public override void OnExit()
    {
        Debug.Log($"MiniEventState: {stateData?.displayName} ���I�����܂�");

        ShouldAdvanceMainStateOnCompletion = false; // �I�����Ƀt���O�����Z�b�g

        // �X�e�[�^�X�ω��̓K�p
        ApplyStatusChanges();

        // �C�x���g����
        if (stateData != null)
        {
            TypedEventManager.Instance.Publish(new GameEvents.MiniEventCompleted
            {
                EventStateID = stateData.stateID,
                EventName = stateData.displayName,
                AffectionChange = stateData.affectionChange,
                LoveChange = stateData.loveChange,
                MoneyChange = stateData.moneyChange
            });
        }

        // ��������UI�v�f�̍폜
        DestroyEventUI();
    }

    public void OnPause()
    {
        Debug.Log($"MiniEventState: {stateData?.displayName} ���ꎞ��~���܂�");

        // UI���\��
        foreach (var ui in startUIs)
        {
            if (ui != null)
            {
                ui.SetActive(false);
            }
        }
    }

    public void OnResume()
    {
        Debug.Log($"MiniEventState: {stateData?.displayName} ���ĊJ���܂�");

        // UI���ĕ\��
        foreach (var ui in startUIs)
        {
            if (ui != null)
            {
                ui.SetActive(true);
            }
        }
    }

    /// <summary>
    /// �p�����[�^��ݒ�
    /// </summary>
    public void SetParameters(Dictionary<string, object> newParameters)
    {
        if (newParameters != null)
        {
            // �p�����[�^���R�s�[
            foreach (var param in newParameters)
            {
                parameters[param.Key] = param.Value;
            }
        }
    }

    /// <summary>
    /// ����̃p�����[�^���擾
    /// </summary>
    protected T GetParameter<T>(string key, T defaultValue = default)
    {
        if (parameters.TryGetValue(key, out object value) && value is T typedValue)
        {
            return typedValue;
        }

        return defaultValue;
    }

    /// <summary>
    /// �C�x���g�I�������i���[�U�[�A�N�V��������Ă΂��j
    /// </summary>
    public void CompleteEvent()
    {
        // �������g��PushdownStack����pop
        PushdownStack.Pop();
    }

    /// <summary>
    /// �X�e�[�^�X�ω��̓K�p
    /// </summary>
    protected virtual void ApplyStatusChanges()
    {
        StatusManager.Instance.UpdateStatus(
                0,  //���t�ω��Ȃ�
                stateData.affectionChange,
                stateData.loveChange,
                stateData.moneyChange
            );
    }

    /// <summary>
    /// UI���Z�b�g�A�b�v����
    /// </summary>
    protected override void SetupUI()
    {
        base.SetupUI();

        // UI�R���e�i���Ȃ���΍쐬
        if (uiContainer == null)
        {
            uiContainer = new GameObject("UIContainer");
            uiContainer.transform.SetParent(transform);

            // ���N�g�g�����X�t�H�[����ǉ��iUI�z�u�̂��߁j
            if (uiContainer.GetComponent<RectTransform>() == null)
            {
                uiContainer.AddComponent<RectTransform>();
            }
        }

        // StartPrefabs������ΐ���
        if (stateData.startPrefabs != null && stateData.startPrefabs.Length > 0)
        {
            foreach (var prefab in stateData.startPrefabs)
            {
                if (prefab != null)
                {
                    // �J�����ݒ�t���̃C���X�^���X�����g�p
                    GameObject startUI = InstantiateUIWithCamera(prefab, uiContainer.transform);
                    if (startUI != null)
                    {
                        startUIs.Add(startUI);
                    }
                }
            }

            // �K�v�ȃ{�^���C�x���g�������œo�^
            SetupButtons();
        }
    }


    /// <summary>
    /// �{�^���C�x���g�̓o�^
    /// </summary>
    protected virtual void SetupButtons()
    {
        // �q�N���X�ŃI�[�o�[���C�h
    }

    /// <summary>
    /// ��������UI�v�f�̍폜
    /// </summary>
    protected virtual void DestroyEventUI()
    {
        // �J�n��UI�v�f�̍폜
        foreach (var ui in startUIs)
        {
            if (ui != null)
            {
                Destroy(ui);
            }
        }
        startUIs.Clear();

        // �I����UI�v�f�̍폜
        foreach (var ui in endUIs)
        {
            if (ui != null)
            {
                Destroy(ui);
            }
        }
        endUIs.Clear();
    }

    /// <summary>
    /// �I����UI�̕\��
    /// </summary>
    protected virtual void ShowEndUI()
    {
        // ���łɏI����UI����������Ă���ꍇ�͏������Ȃ�
        if (endUIs.Count > 0) return;

        // EndPrefabs������ΐ���
        if (stateData.endPrefabs != null && stateData.endPrefabs.Length > 0)
        {
            foreach (var prefab in stateData.endPrefabs)
            {
                if (prefab != null)
                {
                    // �J�����ݒ�t���̃C���X�^���X�����g�p
                    GameObject endUI = InstantiateUIWithCamera(prefab, uiContainer.transform);
                    if (endUI != null)
                    {
                        endUIs.Add(endUI);
                    }
                }
            }
        }
    }
}
