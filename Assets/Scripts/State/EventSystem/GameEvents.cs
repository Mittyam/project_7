using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ゲーム内で使用される全てのイベントデータ型を定義
/// このファイルに集約することで、イベントの全体像を把握しやすくする
/// </summary>
public static class GameEvents
{
    #region UI Events

    /// <summary>
    /// 「図書館」ボタンがクリックされたときのイベント
    /// </summary>
    public struct LibraryButtonClicked
    {
        public int ActionPointCost;
    }

    /// <summary>
    /// 「カフェ」ボタンがクリックされたときのイベント
    /// </summary>
    public struct CafeButtonClicked
    {
        public int ActionPointCost;
    }

    /// <summary>
    /// 「バイト」ボタンがクリックされたときのイベント
    /// </summary>
    public struct WorkButtonClicked
    {
        public int ActionPointCost;
    }

    /// <summary>
    /// 「散歩」ボタンがクリックされたときのイベント
    /// </summary>
    public struct WalkButtonClicked
    {
        public int ActionPointCost;
    }

    /// <summary>
    /// 「ゲーム」ボタンがクリックされたときのイベント
    /// </summary>
    public struct GameButtonClicked
    {
        public int ActionPointCost;
    }

    /// <summary>
    /// 「お出かけ」ボタンがクリックされたときのイベント
    /// </summary>
    public struct OutingButtonClicked
    {
        public int ActionPointCost;
    }

    /// <summary>
    /// 「お話」ボタンがクリックされたときのイベント
    /// </summary>
    public struct TalkButtonClicked
    {
        public int ActionPointCost;
    }

    /// <summary>
    /// 「睡眠」ボタンがクリックされたときのイベント
    /// </summary>
    public struct SleepButtonClicked
    {
        public int ActionPointCost;
    }

    /// <summary>
    /// 「お風呂」ボタンがクリックされたときのイベント
    /// </summary>
    public struct BathButtonClicked
    {
        public int ActionPointCost;
    }

    /// <summary>
    /// 「お触り」ボタンがクリックされたときのイベント
    /// </summary>
    public struct TouchButtonClicked
    {
        public int ActionPointCost;
    }

    /// <summary>
    /// 「アイテム」ボタンがクリックされたときのイベント
    /// </summary>
    public struct ItemButtonClicked
    {
        public int ActionPointCost;
    }

    /// <summary>
    /// 「思い出」ボタンがクリックされたときのイベント
    /// </summary>
    public struct MemoryButtonClicked
    {
        public int ActionPointCost;
    }

    /// <summary>
    /// 「閉じる」ボタンがクリックされたときのイベント
    /// </summary>
    public struct CloseButtonClicked
    {
        public string PanelName; // 閉じるボタンがクリックされたときのイベント
    }

    /// <summary>
    /// 日の進行度が更新されたときのイベント
    /// </summary>
    public struct DayProgressUpdated
    {
        public float ProgressValue; // 進行度の値(0.0〜1.0)
        public StateID SourceStateID; // イベント発生元のステートID（任意）
    }

    /// <summary>
    /// 夜の進行度が更新されたときのイベント
    /// </summary>
    public struct NightProgressUpdated
    {
        public float ProgressValue; // 進行度の値(0.0〜1.0)
    }

    /// <summary>
    /// 夕方の進行度が更新されたときのイベント
    /// </summary>
    public struct EveningProgressUpdated
    {
        public float ProgressValue; // 進行度の値(0.0〜1.0)
    }

    /// <summary>
    /// ボタン状態の更新が必要なときのイベント
    /// </summary>
    public struct ButtonStateUpdateRequested
    {
        public StateID CurrentStateID; // 現在のメインステートID
    }

    #endregion

    #region System Events

    /// <summary>
    /// ステータスが更新されたときのイベント
    /// </summary>
    public struct StatusUpdated
    {
        public int AffectionChange;
        public int LoveChange;
        public int MoneyChange;
        public int DayChange;
    }

    /// <summary>
    /// アクションポイントが更新されたときのイベント
    /// </summary>
    public struct ActionPointUpdated
    {
        public int CurrentPoints;
        public int MaxPoints;
        public int UsedPoints; // 今回使用した量（負の値も可能）
    }

    /// <summary>
    /// 日付が変わったときのイベント
    /// </summary>
    public struct DayChanged
    {
        public int Day;
        public bool IsWeekday;
    }

    /// <summary>
    /// イベントの状態が変化したときのイベント
    /// </summary>
    public struct EventStateChanged
    {
        public int EventID;
        public EventState NewState;
    }

    /// <summary>
    /// ノベルイベントの再生が開始されたときのイベント
    /// </summary>
    public struct NovelEventStarted
    {
        public int EventID;
        public string EventName;
    }

    /// <summary>
    /// ノベルイベントの再生が完了したときのイベント
    /// </summary>
    public struct NovelEventCompleted
    {
        public int EventID;
        public string EventName;
        public StateID ReturnStateID; // 遷移先ステートID
    }

    #endregion

    #region State Events

    /// <summary>
    /// メインステートが変更されたときのイベント
    /// </summary>
    public struct MainStateChanged
    {
        public StateID PreviousStateID;
        public StateID CurrentStateID;
    }

    /// <summary>
    /// ミニイベントが開始されたときのイベント
    /// </summary>
    public struct MiniEventStarted
    {
        public StateID EventStateID;
        public string EventName;
    }

    /// <summary>
    /// ミニイベントが完了したときのイベント
    /// </summary>
    public struct MiniEventCompleted
    {
        public StateID EventStateID;
        public string EventName;
        public int AffectionChange;
        public int LoveChange;
        public int MoneyChange;
    }

    #endregion

    #region Animation Events

    /// <summary>
    /// キャラクターアニメーションが開始されたときのイベント
    /// </summary>
    public struct CharacterAnimationStarted
    {
        public string CharacterID;
        public string AnimationName;
    }

    /// <summary>
    /// キャラクターアニメーションが完了したときのイベント
    /// </summary>
    public struct CharacterAnimationCompleted
    {
        public string CharacterID;
        public string AnimationName;
    }

    #endregion
}
