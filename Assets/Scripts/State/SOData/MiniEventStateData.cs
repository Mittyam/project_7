using UnityEngine;

/// <summary>
/// ミニイベント（会話・外出・思い出）のデータを定義するScriptableObject
/// </summary>
[CreateAssetMenu(menuName = "Game/MiniEventStateData")]
public class MiniEventStateData : ScriptableObject
{
    [Header("基本情報")]
    public StateID stateID;                 // ミニイベントの識別子
    public string displayName;              // 表示名

    [Header("UI設定")]
    public GameObject[] startPrefabs;          // 開始時のPrefab
    public GameObject[] endPrefabs;            // 終了演出Prefab

    [Header("イベント設定")]
    public bool consumeActionPoint = true;  // アクションポイントを消費するか
    public int actionPointCost = 1;         // 消費するアクションポイント数

    [Header("ステータス変化")]
    public int affectionChange;             // 好感度変化量
    public int loveChange;                  // 愛情変化量
    public int moneyChange;                 // お金変化量
}
