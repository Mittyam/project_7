
using UnityEngine;

/// <summary>
/// 各イベントステートの基本的なライフサイクルを定義するインターフェース
/// 昼（平日は大学/休日は自宅）、夕方、夜
/// </summary>
public interface IState
{
    GameObject gameObject { get; }

    // StateBaseで実装されているプロパティに合わせて追加
    IState NextState { get; set; }
    bool enabled { get; set; }

    void OnEnter();
    void OnUpdate();
    void OnExit();

    // オプション: すべてのステートが次のステートIDを取得できるようにする
    // StateID GetNextStateID();
}

/// <summary>
/// 一時停止と再開をサポートするステート用のインターフェース拡張
/// </summary>
public interface IPausableState : IState
{
    void OnPause();
    void OnResume();
}