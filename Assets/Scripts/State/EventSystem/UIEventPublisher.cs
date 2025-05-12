using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI�{�^������C�x���g�𔭍s����R���|�[�l���g
/// �eUI�{�^���ɃA�^�b�`���Ďg�p����
/// </summary>
public class UIEventPublisher : MonoBehaviour
{
    /// <summary>
    /// ���s����C�x���g�̎��
    /// </summary>
    public enum EventType
    {
        LibraryButton,      // �}���ك{�^��
        CafeButton,         // �J�t�F�{�^��
        WorkButton,         // �o�C�g�{�^��
        WalkButton,         // �U���{�^��
        GameButton,         // �Q�[���{�^��
        OutingButton,       // ���o�����{�^��
        TalkButton,         // ���b�{�^��
        SleepButton,        // �����{�^��
        CloseButton,        // ����{�^��

        BathSelect,         // �����C�I���{�^��
        TouchSelect,        // �^�b�`�I���{�^��
        ItemSelect,         // �A�C�e���I���{�^��
        MemoryEventSelect,  // �v���o�C�x���g�I���{�^��

        // �ǉ��̃C�x���g�^�C�v�������ɒǉ�
    }

    [Header("�C�x���g�ݒ�")]
    [SerializeField] private EventType eventType; // ���s����C�x���g�̎��
    [SerializeField] private string panelName; // CloseButton�C�x���g�p
    [SerializeField] private int itemID;       // �I���n�C�x���g�p
    [SerializeField] private string itemName;  // �I���n�C�x���g�p
    [SerializeField] private int actionPointCost; // �{�^���C�x���g�p

    [Header("�{�^���Q��")]
    [SerializeField] private Button targetButton; // �{�^���̎Q��

    private void Awake()
    {
        // �^�[�Q�b�g�{�^�����w�肳��Ă��Ȃ��ꍇ�͎��g��Button�R���|�[�l���g���擾
        if (targetButton == null)
        {
            targetButton = GetComponent<Button>();
        }

        if (targetButton == null)
        {
            Debug.LogError($"UIEventPublisher: {gameObject.name} �Ƀ{�^���R���|�[�l���g��������܂���");
            return;
        }

        // �{�^���N���b�N���̃C�x���g�n���h����o�^
        targetButton.onClick.AddListener(PublishEvent);
    }

    private void OnDestroy()
    {
        if (targetButton != null)
        {
            // �{�^���N���b�N���̃C�x���g�n���h��������
            targetButton.onClick.RemoveListener(PublishEvent);
        }
    }

    /// <summary>
    /// �{�^���N���b�N���ɃC�x���g�𔭍s����
    /// </summary>
    private void PublishEvent()
    {
        // �e�C�x���g�^�C�v�ɉ���������
        switch (eventType)
        {
            // �}���ك{�^��
            case EventType.LibraryButton:
                TypedEventManager.Instance.Publish(new GameEvents.LibraryButtonClicked
                {
                    ActionPointCost = actionPointCost
                });
                break;

            // �J�t�F�{�^��
            case EventType.CafeButton:
                TypedEventManager.Instance.Publish(new GameEvents.CafeButtonClicked
                {
                    ActionPointCost = actionPointCost
                });
                break;

            // �o�C�g�{�^��
            case EventType.WorkButton:
                TypedEventManager.Instance.Publish(new GameEvents.WorkButtonClicked
                {
                    ActionPointCost = actionPointCost
                });
                break;

            // �U���{�^��
            case EventType.WalkButton:
                TypedEventManager.Instance.Publish(new GameEvents.WalkButtonClicked
                {
                    ActionPointCost = actionPointCost
                });
                break;

            // �Q�[���{�^��
            case EventType.GameButton:
                TypedEventManager.Instance.Publish(new GameEvents.GameButtonClicked
                {
                    ActionPointCost = actionPointCost
                });
                break;

            // ���o�����{�^��
            case EventType.OutingButton:
                TypedEventManager.Instance.Publish(new GameEvents.OutingButtonClicked
                {
                    ActionPointCost = actionPointCost
                });
                break;

            // ���b�{�^��
            case EventType.TalkButton:
                TypedEventManager.Instance.Publish(new GameEvents.TalkButtonClicked
                {
                    ActionPointCost = actionPointCost
                });
                break;
            
            // �����{�^��
            case EventType.SleepButton:
                TypedEventManager.Instance.Publish(new GameEvents.SleepButtonClicked
                {
                    ActionPointCost = actionPointCost
                });
                break;

            // �����C�I���{�^��
            case EventType.BathSelect:
                TypedEventManager.Instance.Publish(new GameEvents.BathButtonClicked
                {
                    ActionPointCost = actionPointCost
                });
                break;

            // �G�ꍇ���I���{�^��
            case EventType.TouchSelect:
                TypedEventManager.Instance.Publish(new GameEvents.TouchButtonClicked
                {
                    ActionPointCost = actionPointCost
                });
                break;

            // �A�C�e���I���{�^��
            case EventType.ItemSelect:
                TypedEventManager.Instance.Publish(new GameEvents.ItemButtonClicked
                {
                    ActionPointCost = actionPointCost
                });
                break;

            // �v���o�{�^��
            case EventType.MemoryEventSelect:
                TypedEventManager.Instance.Publish(new GameEvents.MemoryButtonClicked());
                break;

            // ����{�^��
            case EventType.CloseButton:
                TypedEventManager.Instance.Publish(new GameEvents.CloseButtonClicked());
                break;
        }
    }

    /// <summary>
    /// �C���X�y�N�^�Œl��ύX�����Ƃ��̏���
    /// ��ɓ��I�ɐ��������{�^���̃v���p�e�B��ݒ肷�邽��
    /// </summary>
    public void SetProperties(EventType type, int id, string name, int cost = 0, string panel = "")
    {
        eventType = type;
        itemID = id;
        itemName = name;
        actionPointCost = cost;
        panelName = panel;
    }
}
