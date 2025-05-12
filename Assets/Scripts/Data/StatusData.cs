// pixel8
// test2
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StatusData
{
    public int day;
    public int affection;
    public int love;
    public int money;
    public int actionPoint; // 行動ポイント追加
    public string saveDate;
    public List<ItemData> ownedItems = new List<ItemData>();
    public List<ToyData> ownedToys = new List<ToyData>();
}

[System.Serializable]
public class ItemData
{
    public int itemId;              // アイテムID
    public string itemName;         // アイテム名
    public int quantity;            // 所持数
    public string description;      // 説明文
    public int price;               // 価格
    public bool isConsumable;       // 使用するごとに減少するか
    public bool isUniquePurchase;   // 一度だけ購入可能か（売り切れになるか）
    public ItemType itemType;       // アイテムの種類

    // アイテムの種類を定義するenum
    public enum ItemType
    {
        Book,      // 本
        Toy,       // おもちゃ
        Game,      // ゲーム
        Medicine,  // 薬

        Accessory,  // アクセサリー（ヘアピン、ヘアゴムなど）
        Drink,      // 飲み物（コップなど）
        Special     // 特別アイテム
    }

    /// <summary>
    /// アイテムデータのコンストラクタ
    /// </summary>
    /// <param name="id">アイテムID</param>
    /// <param name="name">アイテムネーム</param>
    /// <param name="price">アイテムの価格</param>
    /// <param name="desc">アイテムの説明</param>
    /// <param name="consumable">消費されないかどうか</param>
    /// <param name="uniquePurchase">一度だけ購入可能かどうか</param>
    /// <param name="type">アイテムタイプ</param>
    public ItemData(int id, string name, int price, string desc, bool consumable, bool uniquePurchase, ItemType type)
    {
        this.itemId = id;
        this.itemName = name;
        this.price = price;
        this.description = desc;
        this.isConsumable = consumable;
        this.isUniquePurchase = uniquePurchase;
        this.itemType = type;
        this.quantity = 0;
    }

    /// <summary>
    /// マスターデータを保護しながら
    /// プレイヤーの所持アイテムとして同じ内容のデータを作成
    /// </summary>
    /// <param name="source"></param>
    public ItemData(ItemData source)
    {
        this.itemId = source.itemId;
        this.itemName = source.itemName;
        this.price = source.price;
        this.description = source.description;
        this.isConsumable = source.isConsumable;
        this.isUniquePurchase = source.isUniquePurchase;
        this.itemType = source.itemType;
        this.quantity = source.quantity;
    }
}

[System.Serializable]
public class ToyData
{
    public int toyId;
    public string toyName;
    public bool isOwned;
    public string description; // 説明文追加
    public Sprite icon; // アイコン追加
}