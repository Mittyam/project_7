using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening; // アニメーション用（DOTween使用）

public class ShopItemButton : MonoBehaviour, IPointerClickHandler
{
    [Header("アイテム設定")]
    [SerializeField] private int itemId; // 固定のアイテムID

    [Header("UI要素")]
    [SerializeField] private Image itemImage;       // アイテム画像
    [SerializeField] private Sprite normalSprite;   // 通常時の画像
    [SerializeField] private Sprite soldOutSprite;  // 売り切れ時の画像

    [Header("ボタン設定")]
    [SerializeField] private bool interactable = true;  // ボタンが操作可能か

    private ItemData itemData;
    private bool isSoldOut = false;

    private void OnEnable()
    {
        // 画面表示時に状態を更新
        RefreshButtonState();

        // 購入イベントのリスナーを登録
        ShopManager.Instance.OnItemPurchased += OnAnyItemPurchased;
    }

    private void OnDisable()
    {
        // リスナーの解除
        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.OnItemPurchased -= OnAnyItemPurchased;
        }
    }

    private void Start()
    {
        // アイテムデータの取得
        itemData = ItemDatabase.Instance.GetItemById(itemId);

        if (itemData == null)
        {
            Debug.LogError($"アイテムID: {itemId} のデータが見つかりません。");
            return;
        }

        // 初期状態の設定
        RefreshButtonState();
    }

    // ボタンの状態を更新
    private void RefreshButtonState()
    {
        if (itemData == null) return;

        // 一度だけ購入可能で、既に購入済みかチェック
        isSoldOut = itemData.isUniquePurchase && ShopManager.Instance.IsItemPurchased(itemId);

        // 画像の更新
        if (itemImage != null)
        {
            if (isSoldOut && soldOutSprite != null)
            {
                itemImage.sprite = soldOutSprite;
            }
            else if (normalSprite != null)
            {
                itemImage.sprite = normalSprite;
            }
        }

        // 操作可能状態の更新
        interactable = !isSoldOut;
    }

    // いずれかのアイテムが購入された時のコールバック
    private void OnAnyItemPurchased(int purchasedItemId)
    {
        // 自分のアイテムが購入された場合のみ処理
        if (purchasedItemId == itemId)
        {
            RefreshButtonState();
        }
    }

    // クリック時の処理
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!interactable) return;

        // クリックアニメーション
        AnimateButtonClick();

        // アイテム購入処理
        ShopManager.Instance.PurchaseItem(itemId);
    }

    // クリック時のアニメーション
    private void AnimateButtonClick()
    {
        transform.DOScale(0.95f, 0.1f).SetEase(Ease.OutQuad).OnComplete(() => {
            transform.DOScale(1f, 0.1f).SetEase(Ease.OutQuad);
        });
    }
}