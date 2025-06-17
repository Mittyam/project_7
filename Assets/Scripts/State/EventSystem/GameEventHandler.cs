using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameEvents;

public class GameEventHandler : MonoBehaviour
{
    private void OnEnable()
    {
        SubscribeToEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    private void SubscribeToEvents()
    {
        // 特殊ボタン
        TypedEventManager.Instance.Subscribe<GameEvents.BathButtonClicked>(OnBathButtonClicked);
        // TypedEventManager.Instance.Subscribe<GameEvents.TouchButtonClicked>(OnTouchButtonClicked);
        TypedEventManager.Instance.Subscribe<GameEvents.ItemButtonClicked>(OnItemButtonClicked);
        TypedEventManager.Instance.Subscribe<GameEvents.MemoryButtonClicked>(OnMemoryButtonClicked);

        // 閉じるボタン
        TypedEventManager.Instance.Subscribe<GameEvents.CloseButtonClicked>(OnCloseButtonClicked);
    }

    private void UnsubscribeFromEvents()
    {
        if (TypedEventManager.Instance == null) return;

        // 特殊ボタン
        TypedEventManager.Instance.Unsubscribe<GameEvents.BathButtonClicked>(OnBathButtonClicked);
        // TypedEventManager.Instance.Unsubscribe<GameEvents.TouchButtonClicked>(OnTouchButtonClicked);
        TypedEventManager.Instance.Unsubscribe<GameEvents.ItemButtonClicked>(OnItemButtonClicked);
        TypedEventManager.Instance.Unsubscribe<GameEvents.MemoryButtonClicked>(OnMemoryButtonClicked);

        // 閉じるボタン
        TypedEventManager.Instance.Unsubscribe<GameEvents.CloseButtonClicked>(OnCloseButtonClicked);
    }

    #region イベントハンドラー
    // 特殊ステート系

    // お風呂ボタン押下時の処理
    private void OnBathButtonClicked(GameEvents.BathButtonClicked eventData)
    {
        // お風呂はBathStateに直接遷移（アクションポイント消費なし）
        GameLoop.Instance.PushMiniEvent(StateID.Bath);

        // ボタン状態更新リクエスト
        TypedEventManager.Instance.Publish(new GameEvents.ButtonStateUpdateRequested
        {
            CurrentStateID = GameLoop.Instance.MainStateMachine.CurrentStateID
        });
    }

    // お触りボタン押下時の処理
    private void OnTouchButtonClicked(GameEvents.TouchButtonClicked eventData)
    {
        // 触れ合いはTouchStateに直接遷移（アクションポイント消費なし）
        GameLoop.Instance.PushMiniEvent(StateID.Touch);
    }

    // アイテムボタン押下時の処理
    private void OnItemButtonClicked(GameEvents.ItemButtonClicked eventData)
    {
        // アイテムはItemStateに直接遷移（アクションポイント消費なし）
        GameLoop.Instance.PushMiniEvent(StateID.item);

        // ボタン状態更新リクエスト
        TypedEventManager.Instance.Publish(new GameEvents.ButtonStateUpdateRequested
        {
            CurrentStateID = GameLoop.Instance.MainStateMachine.CurrentStateID
        });
    }

    // 思い出ボタン押下時の処理
    private void OnMemoryButtonClicked(GameEvents.MemoryButtonClicked eventData)
    {
        // 思い出はMemoryStateに直接遷移（アクションポイント消費なし）
        GameLoop.Instance.PushMiniEvent(StateID.Memory);

        // ボタン状態更新リクエスト
        TypedEventManager.Instance.Publish(new GameEvents.ButtonStateUpdateRequested
        {
            CurrentStateID = GameLoop.Instance.MainStateMachine.CurrentStateID
        });
    }

    // 閉じるボタン押下時の処理
    private void OnCloseButtonClicked(GameEvents.CloseButtonClicked eventData)
    {
    }

    #endregion
}
