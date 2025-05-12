using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SaveSlotUI : MonoBehaviour
{
    public TextMeshProUGUI slotInfoText;
    public string slotName;

    // �X���b�g�����X�V���郁�\�b�h
    public void UpdateSlotInfo()
    {
        if (ES3.KeyExists("playerStatus", slotName))
        {
            // �ۑ����ꂽ�f�[�^��ǂݍ���
            StatusData data = ES3.Load<StatusData>("playerStatus", slotName);
            slotInfoText.text =
                $"{data.saveDate}\n�D���x: {data.affection}\nLove: {data.love}";
        }
        else
        {
            slotInfoText.text = "�f�[�^������܂���B";
        }
    }
}
