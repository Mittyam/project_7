using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// �m�x���Đ��p�X�e�[�g
/// CommandExecutor�p�^�[�����̗p
/// </summary>
public class NovelState : StateBase
{
    /// <summary>
    /// �C�x���g�N�����̎�ނ��`
    /// </summary>
    private enum EventSource
    {
        TimingTrigger,  // �ʏ�g���K�[�i�����[���Ȃǁj
        MemoryBrowser,  // �v���o�{������
        ManualTrigger,  // �蓮�g���K�[�i�f�o�b�O�p�Ȃǁj
    }

    public enum PlaybackMode { Click, Auto, Skip }

    [Header("UI �R���e�i")]
    [SerializeField] private GameObject novelUIRoot;

    [Header("UI �R���|�[�l���g")]
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image characterImage;

    [Header("Live2D")]
    [SerializeField] private GameObject live2DContainer;
    [SerializeField] private Live2DController live2DController;

    [Header("Components")]
    [SerializeField] private NovelRunner novelRunner;
    [SerializeField] private MessagePrinter messagePrinter;

    [Header("Data Assets")]
    [SerializeField] private NovelData novelDataAsset;

    [Header("Default Playback Mode")]
    [SerializeField]
    private PlaybackMode defaultPlaybackMode = PlaybackMode.Click;

    // --- �����^�C���t�B�[���h ---
    private NovelEventData currentEventData;
    private bool isPlaybackCompleted = false;
    private EventSource eventSource = EventSource.TimingTrigger;
    private StateID returnToStateID = StateID.None;

    // backgroundImage��public�ɂ��āA���̃X�N���v�g����A�N�Z�X�ł���悤�ɂ���
    public Image BackgroundImage => backgroundImage;
    public Live2DController Live2DController => live2DController;

    /// <summary> �C�x���g�f�[�^���Z�b�g </summary>
    public void SetEventData(NovelEventData data)
    {
        currentEventData = data;
    }

    /// <summary>
    /// �v���o�{������N�����ꂽ���Ƃ�ݒ�
    /// </summary>
    /// <param name="returnState">�Đ��I����ɖ߂�X�e�[�gID</param>
    public void SetAsMemoryEvent(StateID returnState)
    {
        eventSource = EventSource.MemoryBrowser;
        returnToStateID = returnState;
        Debug.Log($"NovelState: �v���o�{������̍Đ��Ƃ��Đݒ�B�I�����{returnState}�ɖ߂�܂�");
    }

    /// <summary>
    /// �蓮�g���K�[����̋N���Ƃ��Đݒ�i�f�o�b�O�p�j
    /// </summary>
    public void SetAsManualTrigger()
    {
        eventSource = EventSource.ManualTrigger;
    }

    /// <summary>
    /// UI�̃Z�b�g�A�b�v����
    /// </summary>
    protected override void SetupUI()
    {
        base.SetupUI();

        // Live2D�R���e�i�̏�����
        if (live2DContainer == null)
        {
            live2DContainer = new GameObject("Live2DContainer");
            live2DContainer.transform.SetParent(transform);
            live2DContainer.AddComponent<RectTransform>();

            // RectTransform�̐ݒ�
            RectTransform rectTransform = live2DContainer.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        // Live2D�R���g���[���̊m�F
        if (live2DController == null)
        {
            live2DController = live2DContainer.GetComponent<Live2DController>();
            if (live2DController == null)
            {
                live2DController = live2DContainer.AddComponent<Live2DController>();
            }
        }
    }

    /// <summary>
    /// ���ׂĂ�UI��\��
    /// </summary>
    private void ShowAllUI()
    {
        if (novelUIRoot != null)
        {
            novelUIRoot.SetActive(true);
        }
    }

    /// <summary>
    /// ���ׂĂ�UI���\��
    /// </summary>
    private void HideAllUI()
    {
        if (novelUIRoot != null)
        {
            novelUIRoot.SetActive(false);
        }
    }

    /// <summary>
    /// �C�x���g�f�[�^����UI��ǂݍ���
    /// </summary>
    private void LoadUIFromEventData(NovelEventData eventData)
    {
        if (eventData == null) return;

        // �w�i�摜�̐ݒ�
        SetBackground(eventData.thumbnailImage);

        // ���ׂĂ�UI��\��
        ShowAllUI();
    }

    /// <summary>
    /// �w�i�摜��ݒ肷��
    /// </summary>
    private void SetBackground(Sprite backgroundSprite)
    {
        if (backgroundImage != null && backgroundSprite != null)
        {
            backgroundImage.sprite = backgroundSprite;
            backgroundImage.gameObject.SetActive(true);
        }
    }

    public override void OnEnter()
    {
        Debug.Log("NovelState: OnEnter");
        isPlaybackCompleted = false; // �Đ������t���O��������

        // UI�̃Z�b�g�A�b�v
        SetupUI();
        ShowAllUI();

        // �C�x���g����
        if (currentEventData != null)
        {
            TypedEventManager.Instance.Publish(new GameEvents.NovelEventStarted
            {
                EventID = currentEventData.eventID,
                EventName = currentEventData.eventName
            });

            // �C�x���g�f�[�^����UI�����[�h
            LoadUIFromEventData(currentEventData);
        }
        else
        {
            Debug.LogError("NovelState: EventData ���ݒ肳��Ă��܂���B");
            CompletePlayback();
            return;
        }

        // �C�x���gID ����Z�N�V�������X�g���擾
        List<EventEntity> sectionList = GetSectionListByEventID(currentEventData.eventID);
        if (sectionList == null || sectionList.Count == 0)
        {
            Debug.LogError($"NovelState: EventID {currentEventData.eventID} �̃f�[�^��������܂���B");
            CompletePlayback();
            return;
        }

        // NovelRunner �ŃC�x���g�Đ��J�n
        if (novelRunner != null)
        {
            StartCoroutine(RunNovelEvent(sectionList));
        }
        else
        {
            Debug.LogError("NovelState: NovelRunner ���A�^�b�`����Ă��܂���B");
            CompletePlayback();
        }
    }

    public override void OnUpdate()
    {
        // �Đ��I�����Ă�����Pop
        if (isPlaybackCompleted)
        {
            CompletePlayback();
        }
    }

    public override void OnExit()
    {
        Debug.Log("NovelState: OnExit");
        StopAllCoroutines();

        // Live2D���f�����N���A
        if (live2DController != null)
        {
            live2DController.DeleteAllModels();
        }

        // �S�Ẳ������~
        SoundManager.Instance.StopVoice();
        SoundManager.Instance.StopAllSE();
        SoundManager.Instance.StopBGMWithFadeOut(2f);

        // �X�e�[�^�X�̕ω���K�p
        if (currentEventData != null)
        {
            // �C�x���g���s
            TypedEventManager.Instance.Publish(new GameEvents.NovelEventCompleted
            {
                EventID = currentEventData.eventID,
                EventName = currentEventData.eventName,
                ReturnStateID = returnToStateID
            });

            // �v���o�Ƃ��ĉ������ݒ�Ȃ�
            if (currentEventData.unlockAsMemory)
            {
                // ���ł�Completed�ɂȂ��Ă���̂ł����œ��ɏ����s�v
            }
        }

        // ���ׂĂ�UI���\��
        HideAllUI();
    }

    /// <summary>
    /// �m�x���C�x���g�̍Đ�����
    /// </summary>
    private IEnumerator RunNovelEvent(List<EventEntity> sectionList)
    {
        yield return StartCoroutine(novelRunner.RunEvent(sectionList, defaultPlaybackMode));

        // �Đ�����
        isPlaybackCompleted = true;
    }

    /// <summary>
    /// �m�x���C�x���g�Đ��������̏���
    /// </summary>
    private void CompletePlayback()
    {
        // �X�e�[�^�X�ω���K�p
        ApplyStatusChanges();

        // �C�x���g��Completed��
        if (currentEventData != null)
        {
            ProgressManager.Instance.CompleteEvent(currentEventData.eventID);
            Debug.Log($"NovelState: �C�x���gID {currentEventData.eventID} �������ς݂ɂ��܂���");
        }

        // �m�x���C�x���g���̂�pop
        PushdownStack.Pop();

        // �J�ڐ�w�胍�W�b�N�i�C�x���g�N�����ɂ���ĈقȂ�j
        switch (eventSource)
        {
            case EventSource.MemoryBrowser:
                // �v���o�{������̏ꍇ�͕ۑ����Ă������̃X�e�[�g�ɖ߂�
                if (returnToStateID != StateID.None)
                {
                    Debug.Log($"NovelState: �v���o�{������̍Đ����������܂����B{returnToStateID} �ɖ߂�܂�");
                    MainStateMachine.ChangeState(returnToStateID);
                }
                break;

            case EventSource.ManualTrigger:
                // �蓮�g���K�[�̏ꍇ�͓��ɉ������Ȃ�
                Debug.Log("NovelState: �蓮�g���K�[����̍Đ����������܂���");
                break;

            case EventSource.TimingTrigger:
            default:
                // ����g���K�[����̏ꍇ�̓C�x���g�f�[�^�̐ݒ�ɏ]��
                if (currentEventData != null && currentEventData.nextStateID != StateID.None)
                {
                    Debug.Log($"NovelState: ����C�x���g�̍Đ����������܂����B{currentEventData.nextStateID} �ɑJ�ڂ��܂�");
                    MainStateMachine.ChangeState(currentEventData.nextStateID);
                }
                break;
        }
    }

    /// <summary>
    /// �C�x���gID����Z�N�V�������X�g���擾
    /// </summary>
    private List<EventEntity> GetSectionListByEventID(int id)
    {
        // ���t�@�N�^�����O��FEventEntity�̃��X�g�𒼐ڕԂ�
        return id switch
        {
            1 => novelDataAsset.Eve1,
            2 => novelDataAsset.Eve2,
            3 => novelDataAsset.Eve3,
            4 => novelDataAsset.Eve4,
            5 => novelDataAsset.Eve5,
            6 => novelDataAsset.Eve6,
            7 => novelDataAsset.Eve7,
            8 => novelDataAsset.Eve8,
            9 => novelDataAsset.Eve9,
            10 => novelDataAsset.Eve10,
            _ => null
        };
    }

    /// <summary>
    /// �X�e�[�^�X�ω��̓K�p
    /// </summary>
    protected void ApplyStatusChanges()
    {
        if (currentEventData != null)
        {
            StatusManager.Instance.UpdateStatus(
                0,  // ���t�ω��Ȃ�
                currentEventData.affectionChange,
                currentEventData.loveChange,
                currentEventData.moneyChange
            );

            Debug.Log($"NovelState: �X�e�[�^�X�ω���K�p - �D���x:{currentEventData.affectionChange}, ����:{currentEventData.loveChange}, ����:{currentEventData.moneyChange}");
        }
    }

    /// <summary>
    /// �Đ����[�h�i����/�X�L�b�v�j�̕ύX
    /// </summary>
    public void SetPlaybackMode(NovelState.PlaybackMode mode)
    {
        // UI�v�f���X�V�i�g�O���{�^���̏�ԂȂǁj
        switch (mode)
        {
            case NovelState.PlaybackMode.Click:
                // �N���b�N�҂����[�h��UI�Z�b�g�A�b�v
                break;
            case NovelState.PlaybackMode.Auto:
                // �������[�h��UI�Z�b�g�A�b�v
                break;
            case NovelState.PlaybackMode.Skip:
                // �X�L�b�v���[�h��UI�Z�b�g�A�b�v
                break;
        }
    }

    /// <summary>
    /// �C�x���g�������I�ɏI������i�f�o�b�O�p�j
    /// </summary>
    public void ForceComplete()
    {
        // ���s���̃R���[�`�����~
        StopAllCoroutines();

        // �Đ������t���O���Z�b�g
        isPlaybackCompleted = true;

        // �I���������Ăяo��
        CompletePlayback();

        Debug.Log("NovelState: �C�x���g�������I�����܂���");
    }
}