using System;
using System.Collections.Generic;
using UnityEngine;

public class StatusManager : Singleton<StatusManager>
{
    [Header("Status Data")]
    public StatusData playerStatus;

    [Header("Action Point Settings")]
    [SerializeField] private int maxActionPoints = 3;
    [SerializeField] private int dailyActionPointRecovery = 3;

    // ステータス更新時に発火するイベント
    public event Action OnStatusUpdated;
    // アクションポイント更新時に発火するイベント
    public event Action<int, int> OnActionPointUpdated; // 現在値, 最大値

    protected override void Awake()
    {
        base.Awake();

        // 新規ゲーム開始時の初期化
        if (playerStatus == null)
        {
            playerStatus = new StatusData
            {
                day = 1,
                affection = 0,
                love = 0,
                money = 1000,
                actionPoint = maxActionPoints,
                saveDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                ownedItems = new List<ItemData>(),
                ownedToys = new List<ToyData>()
            };
        }
    }

    // 日付が変わったときにアクションポイントを回復する
    public void RecoverDailyActionPoints()
    {
        playerStatus.actionPoint = Mathf.Min(playerStatus.actionPoint + dailyActionPointRecovery, maxActionPoints);
        OnActionPointUpdated?.Invoke(playerStatus.actionPoint, maxActionPoints);
    }

    // アクションポイントを消費する（成功したらtrue）
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

        // 日付が変わった場合は行動ポイントを回復
        if (day > 0)
        {
            RecoverDailyActionPoints();
        }

        // affectionとloveを0〜100の範囲に制限
        playerStatus.affection = Mathf.Clamp(playerStatus.affection, 0, 100);
        playerStatus.love = Mathf.Clamp(playerStatus.love, 0, 100);

        // ステータスが更新されたことを通知
        OnStatusUpdated?.Invoke();
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

    // おもちゃをリストに追加、既に存在する場合は所有状態を更新する
    public void AddToy(ToyData newToy)
    {
        // 新しく追加されたら、isOwnedをtrueにする
        ToyData existingToy = playerStatus.ownedToys.Find(toy => toy.toyId == newToy.toyId);
        if (existingToy == null)
        {
            newToy.isOwned = true;                  // 新しいおもちゃを追加する
            playerStatus.ownedToys.Add(newToy);     // リストに追加
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

    // おもちゃリストを取得する
    public List<ToyData> GetOwnedToys()
    {
        return playerStatus.ownedToys;
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

        ES3.Save<StatusData>("playerStatus", playerStatus, slotName);
        Debug.Log($"ステータスをセーブスロット [{slotName}] に保存しました。");
    }

    // セーブスロットを指定してステータスをロードする
    public void LoadStatus(string slotName)
    {
        if (ES3.KeyExists("playerStatus", slotName))
        {
            playerStatus = ES3.Load<StatusData>("playerStatus", slotName);
            OnStatusUpdated?.Invoke();
            OnActionPointUpdated?.Invoke(playerStatus.actionPoint, maxActionPoints);
            Debug.Log($"セーブスロット [{slotName}] からステータスをロードしました。");
        }
        else
        {
            Debug.LogWarning($"セーブスロット [{slotName}] にセーブデータが存在しません。");
        }
    }
}