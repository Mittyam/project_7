using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : Singleton<ShopManager>
{
    [Header("ショップ設定")]
    [SerializeField] private MessagePrinter messagePrinter;

    // 購入イベント (ItemID)
    public System.Action<int> OnItemPurchased;

    private void Start()
    {
        // MessagePrinterの取得
        if (messagePrinter == null)
        {
            messagePrinter = FindObjectOfType<MessagePrinter>();
        }
    }

    /// <summary>
    /// MessagePrinterをタグ検索で取得してコンポーネントを設定する
    /// </summary>
    public void SetupMessagePrinter()
    {
        // "MessagePrinter"タグでオブジェクトを検索
        GameObject messagePrinterObject = GameObject.FindWithTag("MessagePrinter");

        if (messagePrinterObject != null)
        {
            // MessagePrinterコンポーネントを取得
            messagePrinter = messagePrinterObject.GetComponent<MessagePrinter>();

            if (messagePrinter != null)
            {
                Debug.Log($"ShopManager: MessagePrinterを正常に取得しました (オブジェクト: {messagePrinterObject.name})");
            }
            else
            {
                Debug.LogError($"ShopManager: オブジェクト '{messagePrinterObject.name}' にMessagePrinterコンポーネントが見つかりません");
            }
        }
        else
        {
            Debug.LogError("ShopManager: 'MessagePrinter'タグを持つオブジェクトが見つかりません");
        }
    }


    // アイテムが購入済みかどうかをチェック
    public bool IsItemPurchased(int itemId)
    {
        // アイテムの情報を取得
        ItemData itemData = ItemDatabase.Instance.GetItemById(itemId);

        if (itemData == null)
        {
            return false;
        }

        // 一度だけ購入可能なアイテムの場合
        if (itemData.isUniquePurchase)
        {
            List<ItemData> ownedItems = StatusManager.Instance.GetOwnedItems();
            ItemData ownedItem = ownedItems.Find(item => item.itemId == itemId);

            return (ownedItem != null && ownedItem.quantity > 0);
        }

        return false;
    }

    // アイテム購入処理
    public bool PurchaseItem(int itemId)
    {
        // アイテムの情報を取得
        ItemData itemData = ItemDatabase.Instance.GetItemById(itemId);

        if (itemData == null)
        {
            ShowMessage("アイテムが見つかりません。");
            return false;
        }

        // 一度だけ購入可能なアイテムで、すでに所持している場合
        if (itemData.isUniquePurchase && IsItemPurchased(itemId))
        {
            ShowMessage($"{itemData.itemName}はすでに所持しています。");

            // 失敗音を鳴らす
            SoundManager.Instance.PlaySE(9);

            return false;
        }

        // 所持金のチェック
        if (StatusManager.Instance.GetStatus().money < itemData.price)
        {
            ShowMessage("所持金が足りません。");

            // 失敗音を鳴らす
            SoundManager.Instance.PlaySE(9);

            return false;
        }

        // 所持金を減らす
        StatusManager.Instance.UpdateStatus(0, 0, 0, -itemData.price);

        // アイテムを追加
        ItemData newItem = new ItemData(itemData);
        newItem.quantity = 1; // 購入時の数量は1
        StatusManager.Instance.AddItem(newItem);

        // 購入成功音を鳴らす
        SoundManager.Instance.PlaySE(8);

        // 購入イベントを発火
        OnItemPurchased?.Invoke(itemId);

        ShowMessage($"{itemData.itemName}を購入しました。");
        return true;
    }

    // メッセージ表示
    private void ShowMessage(string message)
    {
        if (messagePrinter != null)
        {
            messagePrinter.PrintMessage(message);
        }
        else
        {
            Debug.Log(message);
        }
    }
}