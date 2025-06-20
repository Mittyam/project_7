﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class StatusManager : Singleton<StatusManager>
{
    [Header("Status Data")]
    public StatusData playerStatus;

    [Header("Action Point Settings")]
    [SerializeField] private int maxActionPoints = 3;
    [SerializeField] private int dailyActionPointRecovery = 3;

    [Header("Clothing State")]
    private ClothingType currentClothingState = ClothingType.Casual;
    private bool isClothingStateActive = false; // NightStateがアクティブかどうか

    // ステータス更新時に発火するイベント
    public event Action OnStatusUpdated;

    // 絶頂回数更新時に発火するイベント
    public event Action OnOrgCountUpdated;

    // 射精回数更新時に発火するイベント
    public event Action OnEjaCountUpdated;

    // アクションポイント更新時に発火するイベント
    public event Action<int, int> OnActionPointUpdated; // 現在値, 最大値

    // 服装変更時に発火するイベント
    public event Action<ClothingType> OnClothingStateChanged;

    // 現在のシーン名を取得
    private string currentSceneName;

    protected override void Awake()
    {
        base.Awake();

        SceneManager.sceneLoaded += OnSceneLoaded;

        currentSceneName = SceneManager.GetActiveScene().name;

        // 新規ゲーム開始時の初期化
        if (playerStatus == null)
        {
            Initialize();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // シーン遷移後にカメラを再取得
        currentSceneName = SceneManager.GetActiveScene().name;
    }

    // 初期化メソッドを拡張
    public void Initialize()
    {
        playerStatus = new StatusData
        {
            day = 0,
            affection = 0,
            love = 0,
            money = 10000,
            actionPoint = maxActionPoints,
            saveDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
            ownedItems = new List<ItemData>(),
            eventStates = new List<EventStateData>(),
            maxEjaCount = 1,    // デフォルト1回
            maxOrgCount = 2,    // デフォルト2回
            stateEjaCount = 0,
            stateOrgCount = 0
        };

        // 服装状態を初期化
        currentClothingState = ClothingType.Casual;
        isClothingStateActive = false;
    }

    #region 服装状態管理

    /// <summary>
    /// NightStateの開始を通知（服装状態管理を有効化）
    /// </summary>
    public void EnableClothingState()
    {
        isClothingStateActive = true;
        Debug.Log("StatusManager: 服装状態管理を有効化しました");
    }

    /// <summary>
    /// NightStateの終了を通知（服装状態管理を無効化し、私服にリセット）
    /// </summary>
    public void DisableClothingState()
    {
        isClothingStateActive = false;
        currentClothingState = ClothingType.Casual;
        Debug.Log("StatusManager: 服装状態管理を無効化し、私服にリセットしました");

        // 服装変更イベントを発火
        OnClothingStateChanged?.Invoke(currentClothingState);
    }

    /// <summary>
    /// 現在の服装状態を取得
    /// </summary>
    public ClothingType GetClothingState()
    {
        // 服装状態管理が無効の場合は常に私服を返す
        if (!isClothingStateActive)
        {
            return ClothingType.Casual;
        }
        return currentClothingState;
    }

    /// <summary>
    /// 服装状態を設定（NightStateアクティブ時のみ有効）
    /// </summary>
    public void SetClothingState(ClothingType clothingType)
    {
        if (!isClothingStateActive)
        {
            Debug.LogWarning("StatusManager: NightStateがアクティブでないため、服装変更は無効です");
            return;
        }

        if (currentClothingState != clothingType)
        {
            currentClothingState = clothingType;
            Debug.Log($"StatusManager: 服装を {clothingType} に変更しました");

            // 服装変更イベントを発火
            OnClothingStateChanged?.Invoke(currentClothingState);
        }
    }

    /// <summary>
    /// 服装状態管理が有効かどうかを取得
    /// </summary>
    public bool IsClothingStateActive()
    {
        return isClothingStateActive;
    }

    #endregion

    /// <summary>
    /// 十分なアクションポイントがあるかチェックする（消費はしない）
    /// </summary>
    /// <param name="amount">必要なアクションポイント量</param>
    /// <returns>十分なアクションポイントがあればtrue</returns>
    public bool HasEnoughActionPoints(int amount)
    {
        return playerStatus.actionPoint >= amount;
    }

    // 既存のUpdateStatusメソッド内または日付変更時に呼び出す
    public void OnStateChanged()
    {
        ResetStateCounts();         // State変更時に射精・絶頂回数をリセット
        RecoverStateActionPoints(); // State変更時にアクションポイントを回復
        // その他の日付変更時の処理
    }

    // Stateが変わったときにアクションポイントを回復する
    public void RecoverStateActionPoints()
    {
        playerStatus.actionPoint = Mathf.Min(playerStatus.actionPoint + dailyActionPointRecovery, maxActionPoints);
        OnActionPointUpdated?.Invoke(playerStatus.actionPoint, maxActionPoints);
    }

    // アクションポイントを消費する
    public bool ConsumeActionPoint(int amount)
    {
        if (playerStatus.actionPoint >= amount)
        {
            playerStatus.actionPoint -= amount;
            OnActionPointUpdated?.Invoke(playerStatus.actionPoint, maxActionPoints);
            return true;
        }

        Debug.Log($"アクションポイントが足りません。必要:{amount}, 現在:{playerStatus.actionPoint}");
        return false;
    }

    // ステータスをアップデートして、再表示する
    public void UpdateStatus(int day, int affection, int love, int money)
    {
        playerStatus.day += day;
        playerStatus.affection += affection;
        playerStatus.love += love;
        playerStatus.money += money;

        // affectionとloveを0〜100の範囲に制限
        // 体験版なので35までにする
        playerStatus.affection = Mathf.Clamp(playerStatus.affection, 0, 200);
        playerStatus.love = Mathf.Clamp(playerStatus.love, 0, 200);
        // moneyがマイナスにならないように制限
        playerStatus.money = Mathf.Max(playerStatus.money, 0);

        // ステータスが更新されたことを通知
        OnStatusUpdated?.Invoke();
    }

    // 射精可能かチェックして、可能なら回数を増やす
    public bool CheckAndUpdateEjaCount()
    {
        if (playerStatus.stateEjaCount < playerStatus.maxEjaCount)
        {
            playerStatus.stateEjaCount++;
            // 累計射精回数も増やす
            UpdateEjaCount();
            return true;
        }
        return false;
    }

    public void UpdateEjaCount()
    {
        playerStatus.ejaCount++;

        // 射精回数が更新されたことを通知
        OnEjaCountUpdated?.Invoke();
    }

    // 絶頂可能かのチェックのみ
    public bool CanAchieveOrgasm()
    {
        return playerStatus.stateOrgCount < playerStatus.maxOrgCount;
    }

    // 射精可能かのチェックのみ
    public bool CanAchieveEjaculation()
    {
        return playerStatus.stateEjaCount < playerStatus.maxEjaCount;
    }

    // 絶頂可能かチェックして、可能なら回数を増やす
    public bool CheckAndUpdateOrgCount()
    {
        if (playerStatus.stateOrgCount < playerStatus.maxOrgCount)
        {
            playerStatus.stateOrgCount++;
            // 累計絶頂回数も増やす
            UpdateOrgCount();
            return true;
        }
        return false;
    }

    public void UpdateOrgCount()
    {
        playerStatus.orgCount++;

        // 絶頂回数が更新されたことを通知
        OnOrgCountUpdated?.Invoke();
    }

    // 射精回数の上限を更新
    public void UpdateMaxEjaCount(int newMax)
    {
        playerStatus.maxEjaCount += newMax;
        OnEjaCountUpdated?.Invoke();
        Debug.Log($"射精回数の上限が{newMax}回に変更されました");
    }

    // 絶頂回数の上限を更新
    public void UpdateMaxOrgCount(int newMax)
    {
        playerStatus.maxOrgCount += newMax;
        OnOrgCountUpdated?.Invoke();
        Debug.Log($"絶頂回数の上限が{newMax}回に変更されました");
    }

    // 日付変更時に射精・絶頂回数をリセット
    public void ResetStateCounts()
    {
        playerStatus.stateEjaCount = 0;
        playerStatus.stateOrgCount = 0;
        playerStatus.maxEjaCount = 1;  // デフォルト値に戻す
        playerStatus.maxOrgCount = 2;  // デフォルト値に戻す
        Debug.Log("日付変更により射精・絶頂回数がリセットされました");
    }

    // 残りの射精回数を取得
    public int GetRemainingEjaCount()
    {
        return Mathf.Max(0, playerStatus.maxEjaCount - playerStatus.stateEjaCount);
    }

    // 残りの絶頂回数を取得
    public int GetRemainingOrgCount()
    {
        return Mathf.Max(0, playerStatus.maxOrgCount - playerStatus.stateOrgCount);
    }

    // アイテムをリストに追加、既に存在する場合は数量を増加させる
    public void AddItem(ItemData newItem)
    {
        // 同じItemIDを持つアイテムがすでに存在するかをチェックする
        ItemData existingItem = playerStatus.ownedItems.Find(item => item.itemId == newItem.itemId);

        // 一度だけ購入可能なアイテムの場合
        ItemData masterItem = ItemDatabase.Instance.GetItemById(newItem.itemId);
        if (masterItem != null && masterItem.isUniquePurchase)
        {
            // すでに持っている場合は何もしない
            if (existingItem != null)
            {
                Debug.Log($"一度だけ購入可能なアイテム {newItem.itemName} はすでに所持しています。");
                return;
            }
            // 新規追加の場合は数量を1に設定（安全のため）
            newItem.quantity = 1;
            playerStatus.ownedItems.Add(newItem);
        }
        // 通常アイテムの場合
        else
        {
            if (existingItem != null)
            {
                existingItem.quantity += newItem.quantity;
            }
            else
            {
                playerStatus.ownedItems.Add(newItem);
            }
        }

        OnStatusUpdated?.Invoke();
    }

    // アイテムを減少させる、数量が0になったらリストから削除する
    public void DecreaseItem(int itemId)
    {
        // 同じItemIDを持つアイテムがすでに存在するかをチェックする
        ItemData existingItem = playerStatus.ownedItems.Find(item => item.itemId == itemId);
        if (existingItem != null)
        {
            existingItem.quantity--;
            if (existingItem.quantity <= 0)
                playerStatus.ownedItems.Remove(existingItem);

            OnStatusUpdated?.Invoke();
        }
        else
        {
            Debug.LogWarning($"アイテムID {itemId} が見つかりません。");
        }
    }

    // ステータスを取得する
    public StatusData GetStatus()
    {
        return playerStatus;
    }

    // アイテムリストを取得する
    public List<ItemData> GetOwnedItems()
    {
        return playerStatus.ownedItems;
    }

    // 現在のアクションポイントを取得
    public int GetCurrentActionPoints()
    {
        return playerStatus.actionPoint;
    }

    // 最大アクションポイントを取得
    public int GetMaxActionPoints()
    {
        return maxActionPoints;
    }

    // セーブスロットを指定してステータスを保存する
    public void SaveStatus(string slotName)
    {
        // 保存時に現在日時を記録
        playerStatus.saveDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

        // 現在のステートIDを保存
        if (GameLoop.Instance != null && GameLoop.Instance.MainStateMachine != null)
        {
            playerStatus.savedStateID = GameLoop.Instance.MainStateMachine.CurrentStateID;
        }

        ProgressManager.Instance.SaveEventStatesToStatus(playerStatus);

        ES3.Save<StatusData>("playerStatus", playerStatus, slotName);
        Debug.Log($"ステータスをセーブスロット [{slotName}] に保存しました。保存ステート: {playerStatus.savedStateID}");
    }

    // セーブスロットを指定してステータスをロードする
    public void LoadStatus(string slotName)
    {
        if (ES3.KeyExists("playerStatus", slotName))
        {
            // いったん古いデータを保存
            StateID previousStateID = StateID.None;
            if (playerStatus != null && GameLoop.Instance != null && GameLoop.Instance.MainStateMachine != null)
            {
                previousStateID = GameLoop.Instance.MainStateMachine.CurrentStateID;
            }

            // データロード
            playerStatus = ES3.Load<StatusData>("playerStatus", slotName);

            // イベント状態を復元
            ProgressManager.Instance.LoadEventStatesFromStatus(playerStatus);

            // ロード後、保存されていたステートへ遷移
            StateID loadedStateID = playerStatus.savedStateID;

            // TitleSceneからロードした場合
            if (currentSceneName == "TitleScene" && loadedStateID != StateID.None)
            {
                // GameLoopにロードされたステートを通知
                GameLoop.SetLoadedStateFromTitle(loadedStateID);
                Debug.Log($"TitleSceneからのロード: ステート {loadedStateID} を記録しました!!!!!!!!!!!!!!!!");
            }
            // MainSceneでロードした場合（既存の処理）
            else if (loadedStateID != StateID.None && GameLoop.Instance != null && GameLoop.Instance.MainStateMachine != null)
            {
                // MainSceneの場合のみステート遷移を行う
                if (currentSceneName == "MainScene")
                {
                    try
                    {
                        // 現在のステートとロードしたステートが異なる場合のみ遷移
                        if (GameLoop.Instance.MainStateMachine.CurrentStateID != loadedStateID)
                        {
                            GameLoop.Instance.MainStateMachine.ChangeState(loadedStateID);
                            Debug.Log($"ロード後のステートを {loadedStateID} に変更しました");
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"ステート変更中にエラーが発生: {e.Message}");
                        // エラーが発生した場合は古いステートに戻す
                        if (previousStateID != StateID.None)
                        {
                            try
                            {
                                GameLoop.Instance.MainStateMachine.ChangeState(previousStateID);
                            }
                            catch
                            {
                                // 最終手段としてDayステートに
                                GameLoop.Instance.MainStateMachine.Initialize(StateID.Day);
                            }
                        }
                    }
                }
                else
                {
                    Debug.Log($"MainScene以外ではステート遷移しません。ロード後に自動的に適用されます。");
                }
            }

            OnStatusUpdated?.Invoke();
            OnActionPointUpdated?.Invoke(playerStatus.actionPoint, maxActionPoints);
            Debug.Log($"セーブスロット [{slotName}] からステータスをロードしました。");

            // ロードパネルを非表示
            if (SaveLoadUI.Instance != null)
            {
                SaveLoadUI.Instance.CloseAllPanels();
            }
        }
        else
        {
            Debug.LogWarning($"セーブスロット [{slotName}] にセーブデータが存在しません。");
        }
    }

    // 現在のステートでセーブが可能かどうかを判定するメソッド
    public bool CanSaveInCurrentState()
    {
        if (GameLoop.Instance != null && GameLoop.Instance.MainStateMachine != null)
        {
            StateID currentState = GameLoop.Instance.MainStateMachine.CurrentStateID;
            // 昼と夜のステートでのみセーブ可能
            return currentState == StateID.Day || currentState == StateID.Night;
        }
        return false;
    }

    private void OnDestroy()
    {
        // シーン遷移イベントの解除
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}