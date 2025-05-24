using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ノベルイベントのデータを定義
/// </summary>
[CreateAssetMenu(menuName = "Game/NovelEventData")]
public class NovelEventData : ScriptableObject
{
    [Header("基本情報")]
    public int eventID;                     // イベントID
    public string eventName;                // イベント名
    public string eventDescription;         // イベント説明

    [Header("トリガー設定")]
    public TriggerTiming triggerTiming;     // 発生タイミング
    public List<Condition> conditions;      // 発生条件リスト

    [Header("遷移設定")]
    public StateID nextStateID;             // イベント後に遷移するステートID

    [Header("リソース設定")]
    // public string scenarioPath;             // シナリオファイルへのパス
    public Sprite thumbnailImage;           // サムネイル画像（思い出一覧用）
    public bool unlockAsMemory = true;      // 思い出として解放するか

    [Header("ステータス変化")]
    public int affectionChange;             // 好感度変化量
    public int loveChange;                  // 愛情変化量
    public int moneyChange;                 // お金変化量
}

/// <summary>
/// 条件の種類を定義する列挙型
/// </summary>
public enum ConditionType
{
    CheckAffinity,      // 好感度チェック
    CheckLove,          // 愛情チェック
    CheckDate,          // 日付チェック
    CheckState,         // 状態チェック
}

/// <summary>
/// 条件を定義するクラス
/// </summary>
[System.Serializable]
public class Condition
{
    public ConditionType conditionType;     // 条件の種類
    public int threshold;                   // 閾値
    public string paramName;                // 条件に必要なパラメータ名
    public bool isGreaterThanOrEqual = true; // true: 以上、false: 以下
}
