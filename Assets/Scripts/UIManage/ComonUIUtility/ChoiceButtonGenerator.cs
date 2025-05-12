using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ChoiceOption
{
    public string choiceText;              // ボタンに表示する文章
    public System.Action onSelected;       // ボタンを押した時の処理
}

/// <summary>
/// 選択肢を動的に生成するパネル
/// </summary>
public class ChoiceButtonGenerator : MonoBehaviour
{
    [SerializeField] private Transform content; // ボタンを生成する親オブジェクト
    [SerializeField] private GameObject choiceButtonPrefab;

    // 選択肢のボタンを動的に生成
    public void ShowChoices(List<ChoiceOption> options)
    {
        // すでにボタンが残っていたら削除
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        // パネル自体をアクティブ化
        gameObject.SetActive(true);

        // 選択肢のボタンを生成
        foreach (var option in options)
        {
            var buttonObj = Instantiate(choiceButtonPrefab, content);
            var button = buttonObj.GetComponent<Button>();

            // ボタンのテキストを設定
            var textComp = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            textComp.text = option.choiceText;

            // ボタンが押された時の処理を設定
            button.onClick.AddListener(() =>
            {
                // コールバックを実行
                option.onSelected?.Invoke();
                // 選択後はパネルを閉じるなど
                ClosePanel();
            });
        }
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }
}
