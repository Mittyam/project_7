using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 各ステートを識別するための列挙型
/// </summary>
public enum StateID
{
    // メインステート
    Day,
    Evening,
    Night,

    // ミニイベントステート
    Library,    // 図書室
    Cafe,       // カフェ
    PartJob,    // バイト
    Walk,       // 散歩
    Game,       // ゲーム
    Outing,     // お出かけ
    Talk,       // お話
    Sleep,      // 睡眠

    Bath,       // 入浴
    Touch,      // 触れ合い
    item,       // アイテム
    Memory,     // 思い出

    // その他
    Novel,

    Idle,
    Animation,
    Result,
    None,
}
