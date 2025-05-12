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
        // 各ボタンイベントのサブスクライブ
        TypedEventManager.Instance.Subscribe<GameEvents.TalkButtonClicked>(OnTalkButtonClicked);
        TypedEventManager.Instance.Subscribe<GameEvents.OutingButtonClicked>(OnOutingButtonClicked);
        TypedEventManager.Instance.Subscribe<GameEvents.GameButtonClicked>(OnGameButtonClicked);
        TypedEventManager.Instance.Subscribe<GameEvents.LibraryButtonClicked>(OnLibraryButtonClicked);
        TypedEventManager.Instance.Subscribe<GameEvents.CafeButtonClicked>(OnCafeButtonClicked);
        TypedEventManager.Instance.Subscribe<GameEvents.WorkButtonClicked>(OnWorkButtonClicked);
        TypedEventManager.Instance.Subscribe<GameEvents.WalkButtonClicked>(OnWalkButtonClicked);
        TypedEventManager.Instance.Subscribe<GameEvents.SleepButtonClicked>(OnSleepButtonClicked);

        // 特殊ボタン
        TypedEventManager.Instance.Subscribe<GameEvents.BathButtonClicked>(OnBathButtonClicked);
        TypedEventManager.Instance.Subscribe<GameEvents.TouchButtonClicked>(OnTouchButtonClicked);
        TypedEventManager.Instance.Subscribe<GameEvents.ItemButtonClicked>(OnItemButtonClicked);
        TypedEventManager.Instance.Subscribe<GameEvents.MemoryButtonClicked>(OnMemoryButtonClicked);

        // 閉じるボタン
        TypedEventManager.Instance.Subscribe<GameEvents.CloseButtonClicked>(OnCloseButtonClicked);
    }

    private void UnsubscribeFromEvents()
    {
        if (TypedEventManager.Instance == null) return;

        // 各ボタンイベントのサブスクライブ解除
        TypedEventManager.Instance.Unsubscribe<GameEvents.TalkButtonClicked>(OnTalkButtonClicked);
        TypedEventManager.Instance.Unsubscribe<GameEvents.OutingButtonClicked>(OnOutingButtonClicked);
        TypedEventManager.Instance.Unsubscribe<GameEvents.GameButtonClicked>(OnGameButtonClicked);
        TypedEventManager.Instance.Unsubscribe<GameEvents.LibraryButtonClicked>(OnLibraryButtonClicked);
        TypedEventManager.Instance.Unsubscribe<GameEvents.CafeButtonClicked>(OnCafeButtonClicked);
        TypedEventManager.Instance.Unsubscribe<GameEvents.WorkButtonClicked>(OnWorkButtonClicked);
        TypedEventManager.Instance.Unsubscribe<GameEvents.WalkButtonClicked>(OnWalkButtonClicked);
        TypedEventManager.Instance.Unsubscribe<GameEvents.SleepButtonClicked>(OnSleepButtonClicked);

        // 特殊ボタン
        TypedEventManager.Instance.Unsubscribe<GameEvents.BathButtonClicked>(OnBathButtonClicked);
        TypedEventManager.Instance.Unsubscribe<GameEvents.TouchButtonClicked>(OnTouchButtonClicked);
        TypedEventManager.Instance.Unsubscribe<GameEvents.ItemButtonClicked>(OnItemButtonClicked);
        TypedEventManager.Instance.Unsubscribe<GameEvents.MemoryButtonClicked>(OnMemoryButtonClicked);

        // 閉じるボタン
        TypedEventManager.Instance.Unsubscribe<GameEvents.CloseButtonClicked>(OnCloseButtonClicked);
    }

    #region イベントハンドラー

    // アクションステート系

    // 図書館ボタン押下時の処理
    private void OnLibraryButtonClicked(GameEvents.LibraryButtonClicked eventData)
    {
        // アクションポイント消費チェック
        if (StatusManager.Instance.ConsumeActionPoint(eventData.ActionPointCost))
        {
            // ActionStateに遷移するためのパラメータを設定
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "ActionType", "Library" }
            };

            // GameLoopを通じてミニイベントをプッシュ
            GameLoop.Instance.PushMiniEvent(StateID.Library, parameters);

            // 進行度更新通知
            TypedEventManager.Instance.Publish(new GameEvents.DayProgressUpdated
            {
                ProgressValue = 0.5f,
                SourceStateID = GameLoop.Instance.MainStateMachine.CurrentStateID
            });

            // ボタン状態更新リクエスト
            TypedEventManager.Instance.Publish(new GameEvents.ButtonStateUpdateRequested
            {
                CurrentStateID = GameLoop.Instance.MainStateMachine.CurrentStateID
            });
        }
    }

    // カフェボタン押下時の処理
    private void OnCafeButtonClicked(GameEvents.CafeButtonClicked eventData)
    {
        if (StatusManager.Instance.ConsumeActionPoint(eventData.ActionPointCost))
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "ActionType", "Cafe" }
            };

            GameLoop.Instance.PushMiniEvent(StateID.Cafe, parameters);

            // 進行度更新通知
            TypedEventManager.Instance.Publish(new GameEvents.DayProgressUpdated
            {
                ProgressValue = 0.5f,
                SourceStateID = GameLoop.Instance.MainStateMachine.CurrentStateID
            });

            // ボタン状態更新リクエスト
            TypedEventManager.Instance.Publish(new GameEvents.ButtonStateUpdateRequested
            {
                CurrentStateID = GameLoop.Instance.MainStateMachine.CurrentStateID
            });
        }
    }

    // バイトボタン押下時の処理
    private void OnWorkButtonClicked(GameEvents.WorkButtonClicked eventData)
    {
        if (StatusManager.Instance.ConsumeActionPoint(eventData.ActionPointCost))
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "ActionType", "Work" }
            };

            GameLoop.Instance.PushMiniEvent(StateID.PartJob, parameters);

            // 進行度更新通知
            TypedEventManager.Instance.Publish(new GameEvents.DayProgressUpdated
            {
                ProgressValue = 0.5f,
                SourceStateID = GameLoop.Instance.MainStateMachine.CurrentStateID
            });

            // ボタン状態更新リクエスト
            TypedEventManager.Instance.Publish(new GameEvents.ButtonStateUpdateRequested
            {
                CurrentStateID = GameLoop.Instance.MainStateMachine.CurrentStateID
            });
        }
    }

    // 散歩ボタン押下時の処理
    private void OnWalkButtonClicked(GameEvents.WalkButtonClicked eventData)
    {
        if (StatusManager.Instance.ConsumeActionPoint(eventData.ActionPointCost))
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "ActionType", "Walk" }
            };

            GameLoop.Instance.PushMiniEvent(StateID.Walk, parameters);

            // 進行度更新通知
            TypedEventManager.Instance.Publish(new GameEvents.DayProgressUpdated
            {
                ProgressValue = 0.5f,
                SourceStateID = GameLoop.Instance.MainStateMachine.CurrentStateID
            });

            // ボタン状態更新リクエスト
            TypedEventManager.Instance.Publish(new GameEvents.ButtonStateUpdateRequested
            {
                CurrentStateID = GameLoop.Instance.MainStateMachine.CurrentStateID
            });
        }
    }

    // ゲームボタン押下時の処理
    private void OnGameButtonClicked(GameEvents.GameButtonClicked eventData)
    {
        if (StatusManager.Instance.ConsumeActionPoint(eventData.ActionPointCost))
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "ActionType", "Game" }
            };

            GameLoop.Instance.PushMiniEvent(StateID.Game, parameters);

            // 進行度更新通知
            TypedEventManager.Instance.Publish(new GameEvents.DayProgressUpdated
            {
                ProgressValue = 0.5f,
                SourceStateID = GameLoop.Instance.MainStateMachine.CurrentStateID
            });

            // ボタン状態更新リクエスト
            TypedEventManager.Instance.Publish(new GameEvents.ButtonStateUpdateRequested
            {
                CurrentStateID = GameLoop.Instance.MainStateMachine.CurrentStateID
            });
        }
    }

    // お話ボタン押下時の処理
    private void OnTalkButtonClicked(GameEvents.TalkButtonClicked eventData)
    {
        if (StatusManager.Instance.ConsumeActionPoint(eventData.ActionPointCost))
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "ActionType", "Talk" }
            };

            GameLoop.Instance.PushMiniEvent(StateID.Talk, parameters);

            // 進行度更新通知
            TypedEventManager.Instance.Publish(new GameEvents.DayProgressUpdated
            {
                ProgressValue = 0.5f,
                SourceStateID = GameLoop.Instance.MainStateMachine.CurrentStateID
            });

            // ボタン状態更新リクエスト
            TypedEventManager.Instance.Publish(new GameEvents.ButtonStateUpdateRequested
            {
                CurrentStateID = GameLoop.Instance.MainStateMachine.CurrentStateID
            });
        }
    }

    // お出かけボタン押下時の処理
    private void OnOutingButtonClicked(GameEvents.OutingButtonClicked eventData)
    {
        if (StatusManager.Instance.ConsumeActionPoint(eventData.ActionPointCost))
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "ActionType", "Outing" }
            };

            GameLoop.Instance.PushMiniEvent(StateID.Outing, parameters);

            // 進行度更新通知（お出かけは進行度が1.0になる＝一日終了）
            TypedEventManager.Instance.Publish(new GameEvents.DayProgressUpdated
            {
                ProgressValue = 1.0f,
                SourceStateID = GameLoop.Instance.MainStateMachine.CurrentStateID
            });

            // ボタン状態更新リクエスト
            TypedEventManager.Instance.Publish(new GameEvents.ButtonStateUpdateRequested
            {
                CurrentStateID = GameLoop.Instance.MainStateMachine.CurrentStateID
            });
        }
    }

    // 睡眠ボタン押下時の処理
    private void OnSleepButtonClicked(GameEvents.SleepButtonClicked eventData)
    {
        // 睡眠は時間経過のみでアクションポイント消費なし
        Dictionary<string, object> parameters = new Dictionary<string, object>
        {
            { "ActionType", "Sleep" }
        };

        GameLoop.Instance.PushMiniEvent(StateID.Sleep, parameters);

        // 進行度更新通知（睡眠は進行度が1.0になる＝一日終了）
        StateID currentState = GameLoop.Instance.MainStateMachine.CurrentStateID;

        if (currentState == StateID.Day)
        {
            TypedEventManager.Instance.Publish(new GameEvents.DayProgressUpdated
            {
                ProgressValue = 1.0f,
                SourceStateID = currentState
            });
        }
        else if (currentState == StateID.Evening)
        {
            TypedEventManager.Instance.Publish(new GameEvents.EveningProgressUpdated
            {
                ProgressValue = 1.0f
            });
        }
        else if (currentState == StateID.Night)
        {
            TypedEventManager.Instance.Publish(new GameEvents.NightProgressUpdated
            {
                ProgressValue = 1.0f
            });
        }

        // ボタン状態更新リクエスト
        TypedEventManager.Instance.Publish(new GameEvents.ButtonStateUpdateRequested
        {
            CurrentStateID = currentState
        });
    }

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

    // 触れ合いボタン押下時の処理
    private void OnTouchButtonClicked(GameEvents.TouchButtonClicked eventData)
    {
        // 触れ合いはTouchStateに直接遷移（アクションポイント消費なし）
        GameLoop.Instance.PushMiniEvent(StateID.Touch);

        // ボタン状態更新リクエスト
        TypedEventManager.Instance.Publish(new GameEvents.ButtonStateUpdateRequested
        {
            CurrentStateID = GameLoop.Instance.MainStateMachine.CurrentStateID
        });
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
        // 閉じるボタンの処理（必要に応じて実装）
        // Debug.Log($"閉じるボタンが押されました: {eventData.SourcePanelName}");
    }

    #endregion
}
