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
            slotInfoText.text =
                $"{data.saveDate}\n好感度: {data.affection}\nLove: {data.love}";
        }
        else
        {
            slotInfoText.text = "データがありません。";
        }
    }
}
