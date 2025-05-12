using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UIボタンからイベントを発行するコンポーネント
/// 各UIボタンにアタッチして使用する
/// </summary>
public class UIEventPublisher : MonoBehaviour
{
    /// <summary>
    /// 発行するイベントの種類
    /// </summary>
    public enum EventType
    {
        LibraryButton,      // 図書館ボタン
        CafeButton,         // カフェボタン
        WorkButton,         // バイトボタン
        WalkButton,         // 散歩ボタン
        GameButton,         // ゲームボタン
        OutingButton,       // お出かけボタン
        TalkButton,         // お話ボタン
        SleepButton,        // 睡眠ボタン
        CloseButton,        // 閉じるボタン

        BathSelect,         // お風呂選択ボタン
        TouchSelect,        // タッチ選択ボタン
        ItemSelect,         // アイテム選択ボタン
        MemoryEventSelect,  // 思い出イベント選択ボタン

        // 追加のイベントタイプをここに追加
    }

    [Header("イベント設定")]
    [SerializeField] private EventType eventType; // 発行するイベントの種類
    [SerializeField] private string panelName; // CloseButtonイベント用
    [SerializeField] private int itemID;       // 選択系イベント用
    [SerializeField] private string itemName;  // 選択系イベント用
    [SerializeField] private int actionPointCost; // ボタンイベント用

    [Header("ボタン参照")]
    [SerializeField] private Button targetButton; // ボタンの参照

    private void Awake()
    {
        // ターゲットボタンが指定されていない場合は自身のButtonコンポーネントを取得
        if (targetButton == null)
        {
            targetButton = GetComponent<Button>();
        }

        if (targetButton == null)
        {
            Debug.LogError($"UIEventPublisher: {gameObject.name} にボタンコンポーネントが見つかりません");
            return;
        }

        // ボタンクリック時のイベントハンドラを登録
        targetButton.onClick.AddListener(PublishEvent);
    }

    private void OnDestroy()
    {
        if (targetButton != null)
        {
            // ボタンクリック時のイベントハンドラを解除
            targetButton.onClick.RemoveListener(PublishEvent);
        }
    }

    /// <summary>
    /// ボタンクリック時にイベントを発行する
    /// </summary>
    private void PublishEvent()
    {
        // 各イベントタイプに応じた処理
        switch (eventType)
        {
            // 図書館ボタン
            case EventType.LibraryButton:
                TypedEventManager.Instance.Publish(new GameEvents.LibraryButtonClicked
                {
                    ActionPointCost = actionPointCost
                });
                break;

            // カフェボタン
            case EventType.CafeButton:
                TypedEventManager.Instance.Publish(new GameEvents.CafeButtonClicked
                {
                    ActionPointCost = actionPointCost
                });
                break;

            // バイトボタン
            case EventType.WorkButton:
                TypedEventManager.Instance.Publish(new GameEvents.WorkButtonClicked
                {
                    ActionPointCost = actionPointCost
                });
                break;

            // 散歩ボタン
            case EventType.WalkButton:
                TypedEventManager.Instance.Publish(new GameEvents.WalkButtonClicked
                {
                    ActionPointCost = actionPointCost
                });
                break;

            // ゲームボタン
            case EventType.GameButton:
                TypedEventManager.Instance.Publish(new GameEvents.GameButtonClicked
                {
                    ActionPointCost = actionPointCost
                });
                break;

            // お出かけボタン
            case EventType.OutingButton:
                TypedEventManager.Instance.Publish(new GameEvents.OutingButtonClicked
                {
                    ActionPointCost = actionPointCost
                });
                break;

            // お話ボタン
            case EventType.TalkButton:
                TypedEventManager.Instance.Publish(new GameEvents.TalkButtonClicked
                {
                    ActionPointCost = actionPointCost
                });
                break;
            
            // 睡眠ボタン
            case EventType.SleepButton:
                TypedEventManager.Instance.Publish(new GameEvents.SleepButtonClicked
                {
                    ActionPointCost = actionPointCost
                });
                break;

            // お風呂選択ボタン
            case EventType.BathSelect:
                TypedEventManager.Instance.Publish(new GameEvents.BathButtonClicked
                {
                    ActionPointCost = actionPointCost
                });
                break;

            // 触れ合い選択ボタン
            case EventType.TouchSelect:
                TypedEventManager.Instance.Publish(new GameEvents.TouchButtonClicked
                {
                    ActionPointCost = actionPointCost
                });
                break;

            // アイテム選択ボタン
            case EventType.ItemSelect:
                TypedEventManager.Instance.Publish(new GameEvents.ItemButtonClicked
                {
                    ActionPointCost = actionPointCost
                });
                break;

            // 思い出ボタン
            case EventType.MemoryEventSelect:
                TypedEventManager.Instance.Publish(new GameEvents.MemoryButtonClicked());
                break;

            // 閉じるボタン
            case EventType.CloseButton:
                TypedEventManager.Instance.Publish(new GameEvents.CloseButtonClicked());
                break;
        }
    }

    /// <summary>
    /// インスペクタで値を変更したときの処理
    /// 主に動的に生成したボタンのプロパティを設定するため
    /// </summary>
    public void SetProperties(EventType type, int id, string name, int cost = 0, string panel = "")
    {
        eventType = type;
        itemID = id;
        itemName = name;
        actionPointCost = cost;
        panelName = panel;
    }
}
