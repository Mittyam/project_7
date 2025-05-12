using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI�v�f�𒼐ڊǗ�����~�j�C�x���g�X�e�[�g�̃v���g�^�C�v
/// </summary>
public class MiniEventStateDirectUI : StateBase, IPausableState
{
    [Header("State Data")]
    [SerializeField] protected MiniEventStateData stateData;

    [Header("UI References")]
    [SerializeField] protected GameObject uiRootContainer;
    [SerializeField] protected GameObject contentContainer;
    [SerializeField] protected Button closeButton;

    // StateData�̌��J
    public MiniEventStateData StateData => stateData;

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
            Debug.LogError($"MiniEventStateDirectUI: {gameObject.name}��StateData���ݒ肳��Ă��܂���");
            // �G���[���͈��S�ɏI��
            CompleteEvent();
            return;
        }

        Debug.Log($"MiniEventStateDirectUI: {stateData.displayName} ���J�n���܂�");

        // UI�v�f�̏����ݒ�
        SetupUI();

        // �C�x���g���s
        TypedEventManager.Instance.Publish(new GameEvents.MiniEventStarted
        {
            EventStateID = stateData.stateID,
            EventName = stateData.displayName
        });

        // �A�N�V�����|�C���g�̏���
        if (stateData.consumeActionPoint)
        {
            StatusManager.Instance.ConsumeActionPoint(stateData.actionPointCost);
        }

        // �X�e�[�g�ɉ�����UI�\������
        UpdateUIForState();
    }

    /// <summary>
    /// UI�v�f�̏����ݒ�
    /// </summary>
    protected virtual void SetupUI()
    {
        // UI�v�f�̗L����
        if (uiRootContainer != null)
        {
            uiRootContainer.SetActive(true);
        }

        // �R���e���c�R���e�i�̗L����
        if (contentContainer != null)
        {
            contentContainer.SetActive(true);
        }

        // ����{�^���̃C�x���g�o�^
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CompleteEvent);
        }
    }

    /// <summary>
    /// �X�e�[�g�ɉ�����UI�\������
    /// </summary>
    protected virtual void UpdateUIForState()
    {
        // StateData�Ɋ�Â�UI�\���̍X�V
        //if (stateData != null)
        //{
        //    // �J�n���v���n�u������Ε\��
        //    if (stateData.startPrefab != null && contentContainer != null)
        //    {
        //        GameObject startUI = Instantiate(stateData.startPrefab, contentContainer.transform);
        //        startUI.SetActive(true);
        //    }
        //}
    }

    public override void OnUpdate()
    {
        // �~�j�C�x���g���̍X�V����
    }

    public override void OnExit()
    {
        Debug.Log($"MiniEventStateDirectUI: {stateData?.displayName} ���I�����܂�");

        // �X�e�[�^�X�ω��̓K�p
        ApplyStatusChanges();

        // UI�\�����N���A
        ClearAllUI();

        // �C�x���g���s
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
    }

    public void OnPause()
    {
        Debug.Log($"MiniEventStateDirectUI: {stateData?.displayName} ���ꎞ��~���܂�");

        // UI�v�f���\���ɂ���
        if (uiRootContainer != null)
        {
            uiRootContainer.SetActive(false);
        }
    }

    public void OnResume()
    {
        Debug.Log($"MiniEventStateDirectUI: {stateData?.displayName} ���ĊJ���܂�");

        // UI�v�f���ĕ\��
        if (uiRootContainer != null)
        {
            uiRootContainer.SetActive(true);
        }

        // ��ԃf�[�^���ă��[�h
        UpdateUIForState();
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
        if (stateData != null)
        {
            StatusManager.Instance.UpdateStatus(
                    0,  //���t�ω��Ȃ�
                    stateData.affectionChange,
                    stateData.loveChange,
                    stateData.moneyChange
                );
        }
    }

    /// <summary>
    /// ���ׂĂ�UI�v�f���\���ɂ���
    /// </summary>
    protected virtual void ClearAllUI()
    {
        // �R���e���c�R���e�i���̎q�I�u�W�F�N�g�����ׂč폜
        if (contentContainer != null)
        {
            foreach (Transform child in contentContainer.transform)
            {
                Destroy(child.gameObject);
            }
        }

        // UI���[�g���\��
        if (uiRootContainer != null)
        {
            uiRootContainer.SetActive(false);
        }
    }

    /// <summary>
    /// �����UI�v�f���擾����w���p�[���\�b�h
    /// </summary>
    protected T GetUIComponent<T>(string relativePath) where T : Component
    {
        if (uiRootContainer == null)
            return null;

        Transform target = uiRootContainer.transform.Find(relativePath);
        if (target == null)
            return null;

        return target.GetComponent<T>();
    }
}
