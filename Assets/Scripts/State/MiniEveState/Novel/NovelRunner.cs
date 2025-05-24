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
            Debug.LogError("NovelRunner: CommandExecutor が設定されていません。");
            yield break;
        }

        // コマンドを実行する前に、必要な初期化を行う

        // コマンドを実行
        yield return commandExecutor.ExecuteCommands(entities, mode);

        yield return commandExecutor.Initialize();

        // イベント完了通知
        OnEventCompleted?.Invoke();

        Debug.Log("NovelRunner: イベント再生完了");
    }

    // 完了通知用イベント
    public event System.Action OnEventCompleted;
}
