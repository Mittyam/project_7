using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopPanel : MonoBehaviour
{
    [Header("UI要素")]
    [SerializeField] private TextMeshProUGUI moneyText;     // 所持金表示
    [SerializeField] private Button backButton;             // 戻るボタン
    [SerializeField] private ShopPanelManager shopPanelManager; // ショップパネルマネージャー

    [Header("ショップアイテムボタン")]
    [SerializeField] private ShopItemButton[] itemButtons;  // 事前に配置したアイテムボタン

    private void OnEnable()
    {
        // 画面表示時に所持金を更新
        UpdateMoneyDisplay();

        // ステータス更新イベントにリスナーを追加
        StatusManager.Instance.OnStatusUpdated += UpdateMoneyDisplay;
    }

    private void OnDisable()
    {
        // リスナーの解除
        if (StatusManager.Instance != null)
        {
            StatusManager.Instance.OnStatusUpdated -= UpdateMoneyDisplay;
        }
    }

    private void Start()
    {
        // 戻るボタンのイベント登録
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
        }

        // 初期表示更新
        UpdateMoneyDisplay();
    }

    // 所持金表示の更新
    private void UpdateMoneyDisplay()
    {
        if (moneyText != null)
        {
            moneyText.text = StatusManager.Instance.GetStatus().money.ToString();
        }
    }

    // 戻るボタンクリック時
    private void OnBackButtonClicked()
    {
        shopPanelManager.ReturnToStoreSelection();
    }
}