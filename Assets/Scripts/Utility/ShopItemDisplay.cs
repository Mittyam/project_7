using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShopItemDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private MessagePrinter messagePrinter;
    [SerializeField] private ShopPanelManager shopPanelManager;

    // ショップタイプを表すenum
    public enum ShopType
    {
        Pharmacy,   // 薬局
        ToyStore,   // おもちゃ屋
        BookStore,  // 本屋
        None,       // 未設定
    }

    [Header("ショップ設定")]
    [SerializeField] private ShopType shopType;
    [TextArea(3, 5)]
    [SerializeField] private string shopDescription;

    private void Start()
    {
        // MessagePrinterが設定されていない場合は自動的に探す
        if (messagePrinter == null)
        {
            messagePrinter = FindObjectOfType<MessagePrinter>();
            if (messagePrinter == null)
            {
                Debug.LogError("MessagePrinterが見つかりません。インスペクターで設定してください。");
            }
        }

        // ShopPanelManagerが設定されていない場合は自動的に探す
        if (shopPanelManager == null)
        {
            shopPanelManager = FindObjectOfType<ShopPanelManager>();
            if (shopPanelManager == null)
            {
                Debug.LogError("ShopPanelManagerが見つかりません。インスペクターで設定してください。");
            }
        }
    }

    // マウスが乗った時のイベント
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (messagePrinter != null)
        {
            // 自動クリア機能を一時的に無効化
            bool originalAutoClear = messagePrinter.autoClearMessage;
            messagePrinter.autoClearMessage = false;

            // 説明文を表示
            messagePrinter.ShowMessage(shopDescription);

            // 設定を元に戻す
            messagePrinter.autoClearMessage = originalAutoClear;
        }
    }

    // マウスが離れた時のイベント
    public void OnPointerExit(PointerEventData eventData)
    {
        if (messagePrinter != null)
        {
            // メッセージをクリア
            messagePrinter.ClearMessage();
        }
    }

    // クリック時のイベント（新規追加）
    public void OnPointerClick(PointerEventData eventData)
    {
        if (shopPanelManager != null)
        {
            // shopTypeがNoneの場合は何もしない
            if (shopType == ShopType.None)
            {
                Debug.LogWarning("ShopTypeがNoneに設定されています。ショップを開くことはできません。");
                return;
            }

            // クリックされたショップのパネルを開く
            shopPanelManager.OpenShopPanel(shopType);

            // クリック時に説明をクリア
            if (messagePrinter != null)
            {
                messagePrinter.ClearMessage();
            }
        }
    }

    // ショップのタイプを取得
    public ShopType GetShopType()
    {
        return shopType;
    }
}
