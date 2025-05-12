using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// アイテムマスターデータの管理クラス
/// </summary>
public class ItemDatabase : Singleton<ItemDatabase>
{
    [SerializeField] private List<ItemData> allItems = new List<ItemData>();
    private Dictionary<int, ItemData> itemDictionary = new Dictionary<int, ItemData>();

    protected override void Awake()
    {
        base.Awake();

        // 辞書の初期化
        foreach (ItemData item in allItems)
        {
            if (!itemDictionary.ContainsKey(item.itemId))
            {
                itemDictionary.Add(item.itemId, item);
            }
            else
            {
                Debug.LogError($"重複するアイテムID: {item.itemId} ({item.itemName})");
            }
        }

        // リストが空の場合は初期データを設定
        if (allItems.Count == 0)
        {
            InitializeItemDatabase();
        }
    }

    /// <summary>
    /// アイテムデータベースを初期化するメソッド
    /// </summary>
    private void InitializeItemDatabase()
    {
        // コップ
        allItems.Add(new ItemData(
            1,
            "コップ",
            4000,
            "かわいいコップ。飲み物を入れることができます。",
            false,   // 消費されない
            false,    // 一度だけ購入可能
            ItemData.ItemType.Drink
        ));

        // ぬいぐるみ
        allItems.Add(new ItemData(
            2,
            "うさぎのぬいぐるみ",
            10000,
            "かわいいうさぎのぬいぐるみ。大切にしてね。",
            false,   // 消費されない
            true,    // 一度だけ購入可能
            ItemData.ItemType.Toy
        ));

        // ヘアピン
        allItems.Add(new ItemData(
            3,
            "ヘアピン",
            2000,
            "かわいいヘアピン。身につけるとかわいさアップ！",
            false,   // 消費されない
            true,    // 一度だけ購入可能
            ItemData.ItemType.Accessory
        ));

        // ヘアゴム
        allItems.Add(new ItemData(
            4,
            "ヘアゴム",
            2000,
            "おしゃれなヘアゴム。髪をまとめるのに便利。",
            false,   // 消費されない
            true,    // 一度だけ購入可能
            ItemData.ItemType.Accessory
        ));

        // 辞書の更新
        itemDictionary.Clear();
        foreach (ItemData item in allItems)
        {
            itemDictionary.Add(item.itemId, item);
        }
    }

    // アイテムIDからアイテムを取得
    public ItemData GetItemById(int itemId)
    {
        if (itemDictionary.TryGetValue(itemId, out ItemData item))
        {
            return item;
        }
        return null;
    }

    // すべてのアイテムを取得
    public List<ItemData> GetAllItems()
    {
        return new List<ItemData>(allItems);
    }
}