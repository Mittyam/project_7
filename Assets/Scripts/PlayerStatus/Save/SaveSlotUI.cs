using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SaveSlotUI : MonoBehaviour
{
    public TextMeshProUGUI slotInfoText;
    public string slotName;

    // スロット情報を更新するメソッド
    public void UpdateSlotInfo()
    {
        if (ES3.KeyExists("playerStatus", slotName))
        {
            // 保存されたデータを読み込む
            StatusData data = ES3.Load<StatusData>("playerStatus", slotName);

            // ステートの表示名を取得
            string stateText = GetStateDisplayName(data.savedStateID);

            slotInfoText.text =
                $"{data.saveDate}\n好感度: {data.affection}\nLove: {data.love}\nステート: {stateText}";
        }
        else
        {
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
}
