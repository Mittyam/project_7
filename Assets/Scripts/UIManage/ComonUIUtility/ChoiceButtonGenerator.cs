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

    // 選択肢のボタンを動的に生成
    public void ShowChoices(List<ChoiceOption> choices)
    {
        // 既存ボタンのクリア
        ClearButtons();

        // パネル表示
        choicePanel.SetActive(true);

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

    public void ClosePanel()
    {
        choicePanel.SetActive(false);
        ClearButtons();
    }

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
