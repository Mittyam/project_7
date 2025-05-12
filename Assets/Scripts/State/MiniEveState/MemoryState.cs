using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// �v���o�{���p�~�j�C�x���g�X�e�[�g
/// </summary>
public class MemoryState : MiniEventState
{
    [Header("Memory UI References")]
    [SerializeField] private Transform memoryListContainer;
    [SerializeField] private Button memoryButtonPrefab;
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject noMemoriesMessage;  // �v���o���Ȃ��ꍇ�ɕ\�����郁�b�Z�[�W

    [Header("ProgressManager")]
    [SerializeField] private ProgressManager progressManager;

    private List<Button> createButtons = new List<Button>();

    public override void OnEnter()
    {
        base.OnEnter();

        Debug.Log("MemoryState: �v���o�{�����J�n���܂��B����ς݃m�x���C�x���g���ꗗ�\�����܂��B");

        if (progressManager == null)
        {
            Debug.LogError("MemoryState: ProgressManager��������܂���");
            CompleteEvent();
            return;
        }

        // ����{�^���̃C�x���g�o�^
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CompleteEvent);
        }

        // ���b�Z�[�W�̏�����
        if (noMemoriesMessage != null)
        {
            noMemoriesMessage.SetActive(false);
        }

        // ����ς݃C�x���g�̃{�^���𓮓I����
        GenerateMemoryButtons();
    }

    public override void OnExit()
    {
        // �C�x���g���X�i�[�̉���
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CompleteEvent);
        }

        // ���������{�^���̃N���[���A�b�v
        foreach (var button in createButtons)
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
            }
        }
        createButtons.Clear();

        base.OnExit();
    }

    /// <summary>
    /// ����ς݃m�x���C�x���g�̃{�^���𓮓I����
    /// </summary>
    private void GenerateMemoryButtons()
    {
        if (memoryListContainer == null || memoryButtonPrefab == null)
        {
            Debug.LogError("MemoryState: �K�v��UI�v�f���������ݒ肳��Ă��܂���");
            return;
        }

        // �����̃{�^�����N���A
        foreach (Transform child in memoryListContainer)
        {
            Destroy(child.gameObject);
        }

        // Resource�t�H���_���炷�ׂẴC�x���g�f�[�^���擾
        NovelEventData[] allEvents = Resources.LoadAll<NovelEventData>("Events");
        int buttonCount = 0;

        foreach (var eventData in allEvents)
        {
            // �v���o�Ƃ��ĉ������ݒ肩��Completed�ɂȂ��Ă���C�x���g�̂ݕ\��
            if (eventData.unlockAsMemory &&
                progressManager.GetEventState(eventData.eventID) == EventState.Completed)
            {
                Button newButton = Instantiate(memoryButtonPrefab, memoryListContainer);
                createButtons.Add(newButton);
                buttonCount++;

                // �{�^���e�L�X�g�̐ݒ�
                Text buttonText = newButton.GetComponentInChildren<Text>();
                if (buttonText != null)
                {
                    buttonText.text = eventData.eventName;
                }

                // �T���l�C���摜�̐ݒ�i����΁j
                Image thumbnailImage = newButton.GetComponentInChildren<Image>();
                if (thumbnailImage != null && eventData.thumbnailImage != null)
                {
                    thumbnailImage.sprite = eventData.thumbnailImage;
                }

                // �C�x���g�f�[�^���ꎞ�ۑ����邽�߂Ƀ��[�J���ϐ��ɃR�s�[
                NovelEventData eventDataCopy = eventData;

                // �{�^���N���b�N���̏�����o�^
                newButton.onClick.AddListener(() => OnMemorySelected(eventDataCopy));
            }
        }

        // ����ς݃C�x���g���Ȃ��ꍇ�̃��b�Z�[�W�\��
        if (buttonCount == 0 && noMemoriesMessage != null)
        {
            noMemoriesMessage.SetActive(true);
            Debug.Log("����ς݃m�x���C�x���g�͂���܂���B");
        }
    }

    /// <summary>
    /// �v���o�C�x���g���I�����ꂽ���̏���
    /// </summary>
    /// <param name="eventData">�I�����ꂽ�C�x���g�f�[�^</param>
    private void OnMemorySelected(NovelEventData eventData)
    {
        Debug.Log($"�v���o�C�x���g�u{eventData.eventName}�v���I������܂����iID:{eventData.eventID}�j");

        // ���݂̃��C���X�e�[�gID���擾�i�߂��Ƃ��ĕۑ��j
        StateID currentMainStateID = MainStateMachine.CurrentStateID;

        // ��UMemoryState��pop
        PushdownStack.Pop();

        // NovelState�쐬�̏����͎��t���[���ɒx��
        StartCoroutine(CreateAndPushNovelStateNextFrame(eventData, currentMainStateID));
    }

    /// <summary>
    /// NovelState�̍쐬�Ɠo�^�����t���[���ɒx�����Ď��s
    /// �i����t���[������Pop��Push���s���ƃG���[�ɂȂ�ꍇ�ɑΉ��j
    /// </summary>
    private IEnumerator CreateAndPushNovelStateNextFrame(NovelEventData eventData, StateID returnStateID)
    {
        // 1�t���[���ҋ@
        yield return null;

        // StatesContainer����m�x���X�e�[�g���擾
        NovelState novelState = GameLoop.Instance.StatesContainer.GetNovelState();

        // �K�v�ȃf�[�^��ݒ�
        novelState.SetEventData(eventData);
        novelState.SetAsMemoryEvent(returnStateID); // �v���o����̍Đ��ł��邱�ƂƖ߂���ݒ�

        // PushdownStack�Ƀv�b�V��
        GameLoop.Instance.PushdownStack.Push(novelState);

        Debug.Log($"�v���o�u{eventData.eventName}�v��NovelState���擾���A�v�b�V�����܂����B�Đ����{returnStateID}�ɖ߂�܂��B");
    }
}
