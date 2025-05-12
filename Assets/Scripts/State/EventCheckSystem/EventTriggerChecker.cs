using UnityEngine;

/// <summary>
/// ��ԑJ�ڂȂǂ̃^�C�~���O�ŃC�x���g�����`�F�b�N���Ăяo�����߂̃��[�e�B���e�B
/// </summary>
public static class EventTriggerChecker
{
    /// <summary>
    /// �w�肳�ꂽ�^�C�~���O�ł̃C�x���g�����`�F�b�N��v��
    /// </summary>
    /// <param name="timing"></param>
    public static void Check(TriggerTiming timing)
    {
        if (GameLoop.Instance == null)
        {
            Debug.LogError("EventTriggerChecker: GameLoop�̃C���X�^���X��������܂���");
            return;
        }

        // ���݂̏�Ԃ��擾
        StateID currentState = GameLoop.Instance.MainStateMachine.CurrentStateID;
        StateID nextState;

        // �^�C�~���O�ɉ��������̏�Ԃ𐄒�
        switch (timing)
        {
            case TriggerTiming.DayToEvening:
                nextState = StateID.Evening;
                break;

            case TriggerTiming.EveningToNight:
                nextState = StateID.Night;
                break;

            case TriggerTiming.NightToDay:
                nextState = StateID.Day;
                break;

            default:
                Debug.LogWarning($"EventTriggerChecker: �T�|�[�g����Ă��Ȃ��g���K�[�^�C�~���O {timing}");
                return;
        }

        // NovelEventScheduler�̃`�F�b�N��v��
        GameLoop.Instance.NovelEventScheduler.RequestCheck(currentState, nextState);

        // ���t���[����CheckAndPushIfNeeded��GameLoop����Ă΂��̂�҂�
    }
}