using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 選択肢データ構造体
/// </summary>
public class ChoiceOption
{
    public string choiceText;
    public System.Action onSelected;
}

/// <summary>
/// 選択肢を動的に生成するパネル
/// </summary>
public class ChoiceButtonGenerator : MonoBehaviour
{
    [Header("選択肢パネル設定")]
    [SerializeField] private GameObject choicePanel;
    [SerializeField] private Button choiceButtonPrefab;
    [SerializeField] private Transform choiceContainer;

    private List<Button> activeButtons = new List<Button>();
    private bool isInitialized = false;

    // OnEnableで初期化
    private void OnEnable()
    {
        EnsureInitialized();
    }

    // 初期化確認メソッド
    private void EnsureInitialized()
    {
        if (isInitialized) return;

        // コンポーネントがない場合は検索
        if (choicePanel == null)
            choicePanel = gameObject;

        if (choiceContainer == null)
            choiceContainer = transform.Find("ChoiceContainer");

        if (choiceButtonPrefab == null)
        {
            Debug.LogWarning("ChoiceButtonGenerator: 選択肢ボタンプレハブが設定されていません");
            return;
        }

        isInitialized = true;
    }

    // 選択肢の表示
    public void ShowChoices(List<ChoiceOption> choices)
    {
        // 初期化確認
        EnsureInitialized();

        // 既存ボタンのクリア
        ClearButtons();

        // パネル表示
        if (choicePanel != null)
            choicePanel.SetActive(true);

        if (choiceContainer == null || choiceButtonPrefab == null)
        {
            Debug.LogError("ChoiceButtonGenerator: 必要なコンポーネントが見つかりません");
            return;
        }

        // 選択肢ボタンの生成
        foreach (var choice in choices)
        {
            Button newButton = Instantiate(choiceButtonPrefab, choiceContainer);

            // ボタンテキストの設定
            TextMeshProUGUI buttonText = newButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = choice.choiceText;
            }

            // クリックイベントの登録
            System.Action choiceAction = choice.onSelected;
            newButton.onClick.AddListener(() => {
                choiceAction?.Invoke();
            });

            activeButtons.Add(newButton);
        }
    }

    // パネルを閉じる
    public void ClosePanel()
    {
        if (choicePanel != null)
            choicePanel.SetActive(false);

        ClearButtons();
    }

    // ボタンのクリア
    private void ClearButtons()
    {
        foreach (var button in activeButtons)
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                Destroy(button.gameObject);
            }
        }
        activeButtons.Clear();
    }
}