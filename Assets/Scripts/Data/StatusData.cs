// pixel8
// aaaatest
// test2
// minievent1
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StatusData
{
    public int day;         // 経過日数
    public int affection;   // 好感度
    public int love;        // H度
    public int orgCount;    // 絶頂回数
    public int ejaCount;    // 射精回数

    public int maxEjaCount = 1;
    public int maxOrgCount = 2;
    public int stateEjaCount = 0;   // 各ステートの射精回数
    public int stateOrgCount = 0;   // 各ステートの絶頂回数

    public int money;       // 所持金
    public int actionPoint; // 行動ポイント追加
    public string saveDate; // セーブ日時
    public List<ItemData> ownedItems = new List<ItemData>();
    public List<EventStateData> eventStates = new List<EventStateData>();

    public StateID savedStateID = StateID.None; // セーブ時のメインステートID
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

    // アイテム効果パラメータを追加
    public int affectionBonus;      // 好感度上昇ボーナス
    public int loveBonus;           // 愛情上昇ボーナス
    public int moneyBonus;          // お金上昇ボーナス

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
    /// <param name="affectionBonus">好感度上昇ボーナス</param>
    /// <param name="loveBonus">愛情上昇ボーナス</param>
    /// <param name="moneyBonus">お金上昇ボーナス</param>
    public ItemData(int id, string name, int price, string desc, bool consumable, bool uniquePurchase,
                    ItemType type, int affectionBonus = 0, int loveBonus = 0, int moneyBonus = 0)
    {
        this.itemId = id;
        this.itemName = name;
        this.price = price;
        this.description = desc;
        this.isConsumable = consumable;
        this.isUniquePurchase = uniquePurchase;
        this.itemType = type;
        this.quantity = 0;

        // ボーナスパラメータを設定
        this.affectionBonus = affectionBonus;
        this.loveBonus = loveBonus;
        this.moneyBonus = moneyBonus;
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

        // ボーナスパラメータもコピー
        this.affectionBonus = source.affectionBonus;
        this.loveBonus = source.loveBonus;
        this.moneyBonus = source.moneyBonus;
    }
}

// イベント状態をシリアライズするためのクラス
[System.Serializable]
public class EventStateData
{
    public int eventId;
    public EventState state;

    public EventStateData(int id, EventState eventState)
    {
        eventId = id;
        state = eventState;
    }
}