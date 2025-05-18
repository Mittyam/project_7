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

            // �X�e�[�g�̕\�������擾
            string stateText = GetStateDisplayName(data.savedStateID);

            slotInfoText.text =
                $"{data.saveDate}\n�D���x: {data.affection}\nLove: {data.love}\n�X�e�[�g: {stateText}";
        }
        else
        {
            slotInfoText.text = "�f�[�^������܂���B";
        }
    }

    private string GetStateDisplayName(StateID stateID)
    {
        switch (stateID)
        {
            case StateID.Day: return "��";
            case StateID.Evening: return "�[��";
            case StateID.Night: return "��";
            default: return stateID.ToString();
        }
    }
}
