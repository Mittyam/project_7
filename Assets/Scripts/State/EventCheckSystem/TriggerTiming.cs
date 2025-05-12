/// <summary>
/// イベント発生のタイミングを定義
/// </summary>
public enum TriggerTiming
{
    None,           // トリガーなし
    DayToEvening,   // 昼→夕
    EveningToNight, // 夕→夜
    NightToDay,     // 夜→昼

    OnLibrary,      // 図書室時
    OnCafe,         // カフェ時
    OnPartJob,      // バイト時
    OnWalk,         // 散歩時
    OnGame,         // ゲーム時
    OnOuting,       // お出かけ時
    OnTalk,         // 会話時
    OnSleep,        // 睡眠時

    OnBath,         // 入浴時
    OnTouch,        // 触れ合い時
    OnMemory,       // 思い出閲覧時
    
    Manual          // 手動トリガー（デバッグ用）
}