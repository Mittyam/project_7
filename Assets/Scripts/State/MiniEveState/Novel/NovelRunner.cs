using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NovelRunner : MonoBehaviour
{
    [SerializeField] private NovelCommandExecutor commandExecutor;

    public IEnumerator RunEvent(List<EventEntity> entities, NovelState.PlaybackMode mode)
    {
        if (commandExecutor == null)
        {
            Debug.LogError("NovelRunner: CommandExecutor ���ݒ肳��Ă��܂���B");
            yield break;
        }

        // �R�}���h�����s����O�ɁA�K�v�ȏ��������s��

        // �R�}���h�����s
        yield return commandExecutor.ExecuteCommands(entities, mode);

        yield return commandExecutor.Initialize();

        // �C�x���g�����ʒm
        OnEventCompleted?.Invoke();

        Debug.Log("NovelRunner: �C�x���g�Đ�����");
    }

    // �����ʒm�p�C�x���g
    public event System.Action OnEventCompleted;
}
