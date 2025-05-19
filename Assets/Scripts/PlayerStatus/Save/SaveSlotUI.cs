using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI; // Button�R���|�[�l���g�̂��߂ɒǉ�

public class SaveSlotUI : MonoBehaviour
{
    [Header("�X���b�g���\���p�e�L�X�g")]
    public TextMeshProUGUI slotNameText;
    public TextMeshProUGUI slotInfoText;
    public string slotName;

    [Header("�X���b�g�{�^��")]
    public Button slotButton; // �X���b�g�̃{�^���R���|�[�l���g

    private void Start()
    {
        // �{�^����null�̏ꍇ�͎��g����擾�����݂�
        if (slotButton == null)
        {
            slotButton = GetComponent<Button>();
        }

        // �{�^���ɃN���b�N�C�x���g��o�^
        if (slotButton != null)
        {
            slotButton.onClick.AddListener(OnButtonClicked);
        }
        else
        {
            Debug.LogError($"SaveSlotUI: {gameObject.name} ��Button�R���|�[�l���g��������܂���");
        }

        // �����\�����X�V
        UpdateSlotInfo();
    }

    // �{�^���N���b�N���̏���
    private void OnButtonClicked()
    {
        // SaveSlotManager��OnSlotSelected���\�b�h���Ăяo��
        if (SaveSlotManager.Instance != null)
        {
            SaveSlotManager.Instance.OnSlotSelected(slotName);
        }
        else
        {
            Debug.LogError("SaveSlotManager.Instance ��������܂���");
        }
    }

    // �X���b�g�����X�V���郁�\�b�h (�����R�[�h)
    public void UpdateSlotInfo()
    {
        if (ES3.KeyExists("playerStatus", slotName))
        {
            // �ۑ����ꂽ�f�[�^��ǂݍ���
            StatusData data = ES3.Load<StatusData>("playerStatus", slotName);

            // �X�e�[�g�̕\�������擾
            string stateText = GetStateDisplayName(data.savedStateID);

            slotNameText.text = data.saveDate;

            slotInfoText.text =
                $"�D���x: {data.affection}\nLove: {data.love}\n�X�e�[�g: {stateText}";
        }
        else
        {
            slotNameText.text = "";
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

    // �R���|�[�l���g�j�����ɃC�x���g���X�i�[������
    private void OnDestroy()
    {
        if (slotButton != null)
        {
            slotButton.onClick.RemoveListener(OnButtonClicked);
        }
    }
}