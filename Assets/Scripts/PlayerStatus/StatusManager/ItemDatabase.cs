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
        // 本屋
        allItems.Add(new ItemData(
            1,
            "ちょっとエッチな本",
            1000,
            "図書館で一緒に読むとH度が１上がります。",
            false,   // 消費される
            false,    // 再度購入可能
            ItemData.ItemType.Book,
            0,    // 好感度上昇ボーナス
            1,    // H度上昇ボーナス
            0     // お金上昇ボーナス
        ));

        // 本屋
        allItems.Add(new ItemData(
            2,
            "凄くエッチな本",
            2500,
            "図書館で一緒に読むとH度が３上がります。",
            false,   // 消費される
            false,    // 再度購入可能
            ItemData.ItemType.Book,
            0,    // 好感度上昇ボーナス
            3,    // H度上昇ボーナス
            0     // お金上昇ボーナス
        ));

        // 本屋
        allItems.Add(new ItemData(
            3,
            "恋愛小説",
            2000,
            "図書館で一緒に読むと好感度が１、H度が２上がります。",
            false,   // 消費される
            false,    // 再度購入可能
            ItemData.ItemType.Book,
            1,    // 好感度上昇ボーナス
            2,    // H度上昇ボーナス
            0     // お金上昇ボーナス
        ));

        // おもちゃ屋
        allItems.Add(new ItemData(
            4,
            "エッチなゲーム",
            6000,
            "使用すると好感度が１、H度が３上がります。",
            true,   // 消費されない
            true,    // 一度だけ購入可能
            ItemData.ItemType.Game,
            1,    // 好感度上昇ボーナス
            3,    // H度上昇ボーナス
            0     // お金上昇ボーナス
        ));

        // おもちゃ屋
        allItems.Add(new ItemData(
            5,
            "パーティーゲーム",
            4000,
            "使用すると好感度が３上がります。",
            true,   // 消費されない
            true,    // 一度だけ購入可能
            ItemData.ItemType.Game,
            3,    // 好感度上昇ボーナス
            0,    // H度上昇ボーナス
            0     // お金上昇ボーナス
        ));

        allItems.Add(new ItemData(
            6,
            "精力剤",
            3000,
            "使用すると射精までの時間が短縮されます。",
            false,   // 消費される
            false,    // 再度購入可能
            ItemData.ItemType.Medicine,  // 薬
            0,    // 好感度上昇ボーナス
            0,    // H度上昇ボーナス
            0     // お金上昇ボーナス
        ));

        allItems.Add(new ItemData(
            7,
            "媚薬",
            3000,
            "使用すると絶頂までの時間が短縮されます。",
            false,   // 消費される
            false,    // 再度購入可能
            ItemData.ItemType.Medicine,  // 薬
            1,    // 好感度上昇ボーナス
            0,    // H度上昇ボーナス
            0     // お金上昇ボーナス
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