using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI; // ButtonコンポーネントのためとImageコンポーネントのため

public class SaveSlotUI : MonoBehaviour
{
    [Header("スロット情報表示用テキスト")]
    public TextMeshProUGUI slotNameText;
    public TextMeshProUGUI slotInfoText;
    public string slotName;

    [Header("スロットボタン")]
    public Button slotButton; // スロットのボタンコンポーネント

    [Header("スロット背景")]
    public Image slotBackground; // スロットの背景画像
    public Color normalColor = Color.white; // データがある場合の色
    public Color emptyColor = new Color(0.5f, 0.5f, 0.5f, 1f); // データがない場合の色（少し暗く）

    private void Start()
    {
        // ボタンがnullの場合は自身から取得を試みる
        if (slotButton == null)
        {
            slotButton = GetComponent<Button>();
        }

        // 背景がnullの場合は自身から取得を試みる
        if (slotBackground == null)
        {
            slotBackground = GetComponent<Image>();

            // それでも見つからない場合は子オブジェクトから検索
            if (slotBackground == null)
            {
                slotBackground = GetComponentInChildren<Image>();
            }
        }

        // ボタンにクリックイベントを登録
        if (slotButton != null)
        {
            slotButton.onClick.AddListener(OnButtonClicked);
        }
        else
        {
            Debug.LogError($"SaveSlotUI: {gameObject.name} にButtonコンポーネントが見つかりません");
        }

        // 初期表示を更新
        UpdateSlotInfo();
    }

    // ボタンクリック時の処理
    private void OnButtonClicked()
    {
        // SaveSlotManagerのOnSlotSelectedメソッドを呼び出す
        if (SaveSlotManager.Instance != null)
        {
            SaveSlotManager.Instance.OnSlotSelected(slotName);
        }
        else
        {
            Debug.LogError("SaveSlotManager.Instance が見つかりません");
        }
    }

    // スロット情報を更新するメソッド
    public void UpdateSlotInfo()
    {
        bool hasData = ES3.KeyExists("playerStatus", slotName);

        // 背景色を設定
        if (slotBackground != null)
        {
            slotBackground.color = hasData ? normalColor : emptyColor;
        }

        if (hasData)
        {
            // 保存されたデータを読み込む
            StatusData data = ES3.Load<StatusData>("playerStatus", slotName);

            // ステートの表示名を取得
            string stateText = GetStateDisplayName(data.savedStateID);

            slotNameText.text = data.saveDate;

            slotInfoText.text =
                $"好感度: {data.affection}\nLove: {data.love}\nステート: {stateText}";
        }
        else
        {
            slotNameText.text = "";
            slotInfoText.text = "データがありません。";
        }
    }

    private string GetStateDisplayName(StateID stateID)
    {
        switch (stateID)
        {
            case StateID.Day: return "昼";
            case StateID.Evening: return "夕方";
            case StateID.Night: return "夜";
            default: return stateID.ToString();
        }
    }

    // コンポーネント破壊時にイベントリスナーを解除
    private void OnDestroy()
    {
        if (slotButton != null)
        {
            slotButton.onClick.RemoveListener(OnButtonClicked);
        }
    }
}