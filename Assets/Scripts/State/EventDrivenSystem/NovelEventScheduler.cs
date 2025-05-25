using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// ノベルイベントの発火条件をチェックし、条件を満たしたらPushDownStackにPushするクラス
/// </summary>
public class NovelEventScheduler : MonoBehaviour
{
    // 登録されたイベントデータ
    private Dictionary<TriggerTiming, List<NovelEventData>> eventTable;

    // 現在チェック中のタイミング
    private TriggerTiming currentCheckTiming = TriggerTiming.None;
    private StateID prevStateID;
    private StateID nextStateID;

    private void Awake()
    {
        // イベントテーブルの初期化
        eventTable = new Dictionary<TriggerTiming, List<NovelEventData>>();

        // Resource/Events フォルダからイベントデータを読み込む
        LoadNovelEvents();
    }

    /// <summary>
    /// Resource/Events フォルダからイベントデータを読み込み、eventTableに登録する
    /// </summary>
    private void LoadNovelEvents()
    {
        foreach (var eventData in Resources.LoadAll<NovelEventData>("Events"))
        {
            // タイミングごとにグループ化
            if (!eventTable.ContainsKey(eventData.triggerTiming))
            {
                eventTable[eventData.triggerTiming] = new List<NovelEventData>();
            }

            eventTable[eventData.triggerTiming].Add(eventData);
        }
    }

    /// <summary>
    /// MainStateMachineからの遷移通知を受け取る
    /// </summary>
    public void RequestCheck(StateID prev, StateID next)
    {
        prevStateID = prev;
        nextStateID = next;

        // ステート遷移からトリガータイミングを判定
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
            // マッチするタイミングがなければ何もしない
            currentCheckTiming = TriggerTiming.None;
        }

        Debug.Log($"NovelEventScheduler: {prev} → {next} への遷移でチェックをリクエスト（タイミング：{currentCheckTiming}）");
    }

    // <summary>
    /// イベント発火条件を確認し、条件を満たしていればPushdownStackにPushする
    /// GameLoopから毎フレーム呼ばれる
    /// </summary>
    public bool CheckAndPushIfNeeded()
    {
        // チェック対象がなければ何もしない
        if (currentCheckTiming == TriggerTiming.None ||
            !eventTable.ContainsKey(currentCheckTiming) ||
            eventTable[currentCheckTiming].Count == 0)
        {
            return false;
        }

        // 現在のタイミングに対応するイベントをすべて確認
        foreach (var eventData in eventTable[currentCheckTiming])
        {
            // 既に完了済みのイベントはスキップ（追加）
            if (ProgressManager.Instance.GetEventState(eventData.eventID) == EventState.Completed) continue;

            // 既に解放済みのイベントはスキップ
            if (ProgressManager.Instance.GetEventState(eventData.eventID) == EventState.Unlocked) continue;

            // 条件を満たしていないイベントはスキュー
            if (!ConditionEvaluator.Evaluate(eventData.conditions)) continue;

            // 条件を満たしたイベントを発火
            ProgressManager.Instance.UnlockEvent(eventData.eventID);

            // NovelStateを取得してPush
            NovelState novelState = GameLoop.Instance.StatesContainer.GetNovelState();
            novelState.gameObject.SetActive(true);
            novelState.SetEventData(eventData);
            GameLoop.Instance.PushdownStack.Push(novelState);

            // 現在のチェックをリセット（1度に１つのみ発火）
            currentCheckTiming = TriggerTiming.None;

            Debug.Log($"NovelEventScheduler: イベント '{eventData.eventName}' (ID:{eventData.eventID}) を発火しました");
            return true;
        }

        // 条件を満たすイベントが見つからなかった
        currentCheckTiming = TriggerTiming.None;
        return false;
    }

    /// <summary>
    /// イベントデータを直接指定してPushdownStackにPushする（デバッグ用）
    /// </summary>
    public void PushEvent(NovelEventData eventData)
    {
        if (eventData == null)
        {
            Debug.LogError("NovelEventScheduler: イベントデータがnullです");
            return;
        }

        // ここでノベルステートを生成してPushdownStackにPush
        NovelState novelState = GameLoop.Instance.StatesContainer.GetNovelState();

        // ***修正ポイント3: デバッグPush時もステートを有効化***
        novelState.gameObject.SetActive(true);

        // イベントデータを設定
        novelState.SetEventData(eventData);
        novelState.SetAsManualTrigger();  // 手動トリガーとして設定

        // PushdownStackにプッシュ
        GameLoop.Instance.PushdownStack.Push(novelState);
        Debug.Log($"NovelEventScheduler: イベント'{eventData.eventName}'を手動でプッシュしました");
    }
}