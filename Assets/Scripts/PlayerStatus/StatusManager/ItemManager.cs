using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// アイテムを使用するためのマネージャークラス
/// </summary>
public class ItemManager : Singleton<ItemManager>
{
    // アイテム使用イベント
    public System.Action<int> OnItemUsed;

    // アイテムを使用する
    public bool UseItem(int itemId)
    {
        // アイテムが所持しているか確認
        ItemData ownedItem = GetOwnedItemById(itemId);

        if (ownedItem == null || ownedItem.quantity <= 0)
        {
            Debug.Log($"アイテムID {itemId} は所持していないか、数量が0です。");
            return false;
        }

        // 消費アイテムの場合、数量を減らす
        if (ownedItem.isConsumable)
        {
            StatusManager.Instance.DecreaseItem(itemId);
        }

        // アイテムの効果を適用（実際のゲームロジックに合わせて実装）
        ApplyItemEffect(itemId);

        // アイテム使用イベントを発火
        OnItemUsed?.Invoke(itemId);

        return true;
    }

    // 所持アイテムからIDで検索
    private ItemData GetOwnedItemById(int itemId)
    {
        List<ItemData> ownedItems = StatusManager.Instance.GetOwnedItems();
        return ownedItems.Find(item => item.itemId == itemId);
    }

    // アイテムの効果を適用（ゲームに合わせて実装）
    private void ApplyItemEffect(int itemId)
    {
        // アイテムデータから情報を取得
        ItemData itemData = ItemDatabase.Instance.GetItemById(itemId);

        if (itemData == null)
        {
            Debug.LogWarning($"アイテムID {itemId} のデータが見つかりません。");
            return;
        }

        // アイテムの種類によって効果を分岐
        switch (itemData.itemType)
        {
            case ItemData.ItemType.Medicine:
                if (itemId == 6)
                {
                    StatusManager.Instance.UpdateMaxEjaCount(2);
                    Debug.Log($"{itemData.itemName}を使用しました。射精回数上限を2回増やしました。");
                }
                else if (itemId == 7)
                {
                    StatusManager.Instance.UpdateMaxOrgCount(3);
                    Debug.Log($"{itemData.itemName}を使用しました。絶頂回数上限を3回増やしました。");
                }
                break;
            case ItemData.ItemType.Accessory:
                // アクセサリーの効果例：好感度を上げる
                StatusManager.Instance.UpdateStatus(0, 5, 0, 0);
                Debug.Log($"{itemData.itemName}を装着しました。好感度が5上がりました。");
                break;

            case ItemData.ItemType.Toy:
                // おもちゃの効果例：愛情を上げる
                StatusManager.Instance.UpdateStatus(0, 0, 10, 0);
                Debug.Log($"{itemData.itemName}で遊びました。愛情が10上がりました。");
                break;

            case ItemData.ItemType.Drink:
                // 飲み物の効果例：好感度と愛情を少し上げる
                StatusManager.Instance.UpdateStatus(0, 3, 3, 0);
                Debug.Log($"{itemData.itemName}を使いました。好感度と愛情が3ずつ上がりました。");
                break;

            case ItemData.ItemType.Special:
                // 特別アイテムの効果例：大きく効果を上げる
                StatusManager.Instance.UpdateStatus(0, 10, 15, 0);
                Debug.Log($"{itemData.itemName}を使用しました。好感度が10、愛情が15上がりました。");
                break;
        }
    }
}