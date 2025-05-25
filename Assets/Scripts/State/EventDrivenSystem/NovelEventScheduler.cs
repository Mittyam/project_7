using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// �m�x���C�x���g�̔��Ώ������`�F�b�N���A�����𖞂�������PushDownStack��Push����N���X
/// </summary>
public class NovelEventScheduler : MonoBehaviour
{
    // �o�^���ꂽ�C�x���g�f�[�^
    private Dictionary<TriggerTiming, List<NovelEventData>> eventTable;

    // ���݃`�F�b�N���̃^�C�~���O
    private TriggerTiming currentCheckTiming = TriggerTiming.None;
    private StateID prevStateID;
    private StateID nextStateID;

    private void Awake()
    {
        // �C�x���g�e�[�u���̏�����
        eventTable = new Dictionary<TriggerTiming, List<NovelEventData>>();

        // Resource/Events �t�H���_����C�x���g�f�[�^��ǂݍ���
        LoadNovelEvents();
    }

    /// <summary>
    /// Resource/Events �t�H���_����C�x���g�f�[�^��ǂݍ��݁AeventTable�ɓo�^����
    /// </summary>
    private void LoadNovelEvents()
    {
        foreach (var eventData in Resources.LoadAll<NovelEventData>("Events"))
        {
            // �^�C�~���O���ƂɃO���[�v��
            if (!eventTable.ContainsKey(eventData.triggerTiming))
            {
                eventTable[eventData.triggerTiming] = new List<NovelEventData>();
            }

            eventTable[eventData.triggerTiming].Add(eventData);
        }
    }

    /// <summary>
    /// MainStateMachine����̑J�ڒʒm���󂯎��
    /// </summary>
    public void RequestCheck(StateID prev, StateID next)
    {
        prevStateID = prev;
        nextStateID = next;

        // �X�e�[�g�J�ڂ���g���K�[�^�C�~���O�𔻒�
        if (prev == StateID.Day && next == StateID.Evening)
        {
            currentCheckTiming = TriggerTiming.DayToEvening;
        }
        else if (prev == StateID.Evening && next == StateID.Night)
        {
            currentCheckTiming = TriggerTiming.EveningToNight;
        }
        else if (prev == StateID.Night && next == StateID.Day)
        {
            currentCheckTiming = TriggerTiming.NightToDay;
        }
        else
        {
            // �}�b�`����^�C�~���O���Ȃ���Ή������Ȃ�
            currentCheckTiming = TriggerTiming.None;
        }

        Debug.Log($"NovelEventScheduler: {prev} �� {next} �ւ̑J�ڂŃ`�F�b�N�����N�G�X�g�i�^�C�~���O�F{currentCheckTiming}�j");
    }

    // <summary>
    /// �C�x���g���Ώ������m�F���A�����𖞂����Ă����PushdownStack��Push����
    /// GameLoop���疈�t���[���Ă΂��
    /// </summary>
    public bool CheckAndPushIfNeeded()
    {
        // �`�F�b�N�Ώۂ��Ȃ���Ή������Ȃ�
        if (currentCheckTiming == TriggerTiming.None ||
            !eventTable.ContainsKey(currentCheckTiming) ||
            eventTable[currentCheckTiming].Count == 0)
        {
            return false;
        }

        // ���݂̃^�C�~���O�ɑΉ�����C�x���g�����ׂĊm�F
        foreach (var eventData in eventTable[currentCheckTiming])
        {
            // ���Ɋ����ς݂̃C�x���g�̓X�L�b�v�i�ǉ��j
            if (ProgressManager.Instance.GetEventState(eventData.eventID) == EventState.Completed) continue;

            // ���ɉ���ς݂̃C�x���g�̓X�L�b�v
            if (ProgressManager.Instance.GetEventState(eventData.eventID) == EventState.Unlocked) continue;

            // �����𖞂����Ă��Ȃ��C�x���g�̓X�L���[
            if (!ConditionEvaluator.Evaluate(eventData.conditions)) continue;

            // �����𖞂������C�x���g�𔭉�
            ProgressManager.Instance.UnlockEvent(eventData.eventID);

            // NovelState���擾����Push
            NovelState novelState = GameLoop.Instance.StatesContainer.GetNovelState();
            novelState.gameObject.SetActive(true);
            novelState.SetEventData(eventData);
            GameLoop.Instance.PushdownStack.Push(novelState);

            // ���݂̃`�F�b�N�����Z�b�g�i1�x�ɂP�̂ݔ��΁j
            currentCheckTiming = TriggerTiming.None;

            Debug.Log($"NovelEventScheduler: �C�x���g '{eventData.eventName}' (ID:{eventData.eventID}) �𔭉΂��܂���");
            return true;
        }

        // �����𖞂����C�x���g��������Ȃ�����
        currentCheckTiming = TriggerTiming.None;
        return false;
    }

    /// <summary>
    /// �C�x���g�f�[�^�𒼐ڎw�肵��PushdownStack��Push����i�f�o�b�O�p�j
    /// </summary>
    public void PushEvent(NovelEventData eventData)
    {
        if (eventData == null)
        {
            Debug.LogError("NovelEventScheduler: �C�x���g�f�[�^��null�ł�");
            return;
        }

        // �����Ńm�x���X�e�[�g�𐶐�����PushdownStack��Push
        NovelState novelState = GameLoop.Instance.StatesContainer.GetNovelState();

        // ***�C���|�C���g3: �f�o�b�OPush�����X�e�[�g��L����***
        novelState.gameObject.SetActive(true);

        // �C�x���g�f�[�^��ݒ�
        novelState.SetEventData(eventData);
        novelState.SetAsManualTrigger();  // �蓮�g���K�[�Ƃ��Đݒ�

        // PushdownStack�Ƀv�b�V��
        GameLoop.Instance.PushdownStack.Push(novelState);
        Debug.Log($"NovelEventScheduler: �C�x���g'{eventData.eventName}'���蓮�Ńv�b�V�����܂���");
    }
}