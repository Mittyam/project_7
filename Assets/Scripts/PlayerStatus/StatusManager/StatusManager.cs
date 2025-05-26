using System;
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

    // ステータス更新時に発火するイベント
    public event Action OnStatusUpdated;

    // 絶頂回数更新時に発火するイベント
    public event Action OnOrgCountUpdated;

    // 射精回数更新時に発火するイベント
    public event Action OnEjaCountUpdated;

    // アクションポイント更新時に発火するイベント
    public event Action<int, int> OnActionPointUpdated; // 現在値, 最大値

    // 現在のシーン名を取得
    private string currentSceneName; //SceneManager.GetActiveScene().name

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

    // 初期化
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
            eventStates = new List<EventStateData>()
        };
    }

    /// <summary>
    /// 十分なアクションポイントがあるかチェックする（消費はしない）
    /// </summary>
    /// <param name="amount">必要なアクションポイント量</param>
    /// <returns>十分なアクションポイントがあればtrue</returns>
    public bool HasEnoughActionPoints(int amount)
    {
        return playerStatus.actionPoint >= amount;
    }

    // 日付が変わったときにアクションポイントを回復する
    public void RecoverDailyActionPoints()
    {
        playerStatus.actionPoint = Mathf.Min(playerStatus.actionPoint + dailyActionPointRecovery, maxActionPoints);
        OnActionPointUpdated?.Invoke(playerStatus.actionPoint, maxActionPoints);
    }

    // 既存のConsumeActionPointメソッドも残しておく
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
        playerStatus.affection = Mathf.Clamp(playerStatus.affection, 0, 100);
        playerStatus.love = Mathf.Clamp(playerStatus.love, 0, 100);
        // moneyがマイナスにならないように制限
        playerStatus.money = Mathf.Max(playerStatus.money, 0);

        // ステータスが更新されたことを通知
        OnStatusUpdated?.Invoke();
    }

    public void UpdateOrgCount()
    {
        playerStatus.orgCount++;

        // 絶頂回数が更新されたことを通知
        OnOrgCountUpdated?.Invoke();
    }

    public void UpdateEjaCount()
    {
        playerStatus.ejaCount++;

        // 射精回数が更新されたことを通知
        OnEjaCountUpdated?.Invoke();
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