using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// メインステート（昼・夕・夜）のデータを定義
/// </summary>
[CreateAssetMenu(menuName = "Game/MainStateData")]
public class MainStateData : ScriptableObject
{
    [Header("基本情報")]
    public StateID stateID;                 // ステートの識別子（Day/Evening/Night）
    public string displayName;              // 表示名

    [Header("UI設定")]
    public GameObject[] uiPrefab;           // 表示用Prefab

    [Header("Live2D設定")]
    public Live2DModelData live2DData;      // Live2Dモデルデータ
    public bool showLive2DModel = true;     // Live2Dモデルを表示するか

    [Header("遷移設定")]
    public StateID nextStateID;             // 次のステートID（昼→夕→夜ループ）
    public TriggerTiming exitTriggrTiming;  // このステートから出るときのトリガータイミング

    [Header("消費ポイント設定")]
    public int talkActionPointCost = 1;     // 会話に必要なアクションポイント
    public int outingActionPointCost = 2;   // 外出に必要なアクションポイント
    public int gameActionPointCost = 1;     // ゲームに必要なアクションポイント

    [Header("アクションボタン設定")]
    public bool enableTalkButton = true;    // お話ボタンを有効にするか
    public bool enableOutingButton = true;  // お出かけボタンを有効にするか
    public bool enableGameButton = true;    // ゲームボタンを有効にするか
    public bool enableMemoryButton = true;  // 思い出ボタンを有効にするか
}

/// <summary>
/// Live2Dモデルの基本データを定義する構造体
/// </summary>
[System.Serializable]
public class Live2DModelData
{
    [Header("基本情報")]
    public string modelID;             // モデルのID（必須）
    public GameObject modelPrefab;     // モデルのプレハブ

    [Header("表示設定")]
    public Vector2 position = Vector2.zero;  // 表示位置
    public float scale = 1.0f;               // 表示スケール

    [Header("タッチ反応設定")]
    public bool enableTouch = true;          // タッチ機能の有効/無効

    [Header("デフォルトアニメーション")]
    public string defaultAnimTrigger = "idle"; // 初期表示時のアニメーション
}