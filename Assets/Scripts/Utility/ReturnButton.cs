using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReturnButton : MonoBehaviour
{
    [SerializeField] private ShopPanelManager shopPanelManager;
    private Button button;

    private void Start()
    {
        // ボタンコンポーネントを取得
        button = GetComponent<Button>();

        // ShopPanelManagerが設定されていない場合は自動的に探す
        if (shopPanelManager == null)
        {
            shopPanelManager = FindObjectOfType<ShopPanelManager>();
        }

        // クリックイベントを登録
        if (button != null && shopPanelManager != null)
        {
            button.onClick.AddListener(shopPanelManager.ReturnToStoreSelection);
        }
    }
}