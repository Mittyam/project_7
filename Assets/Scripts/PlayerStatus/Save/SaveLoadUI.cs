using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveLoadUI : MonoBehaviour
{
    [Header("�Z�[�u/���[�h�I���|�b�v�A�b�v")]
    public GameObject selectionPanel;
    public Button saveButton;
    public Button loadButton;

    [Header("�X���b�g���j���[")]
    public GameObject slotPanel;

    private void Start()
    {
        // ������Ԃł͗�����\��
        selectionPanel.SetActive(false);
        slotPanel.SetActive(false);

        // �{�^���̃N���b�N�C�x���g�Ƀ��X�i�[��o�^
        saveButton.onClick.AddListener(() => OnSelection(true));
        loadButton.onClick.AddListener(() => OnSelection(false));
    }

    private void Update()
    {
        // �G�X�P�[�v�L�[����������A�\�����̃p�l�������
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (selectionPanel.activeSelf)
            {
                selectionPanel.SetActive(false);
            }
            if (slotPanel.activeSelf)
            {
                slotPanel.SetActive(false);
            }
        }
    }

    public void OpenSelectionPopup()
    {
        selectionPanel.SetActive(true);
    }

    private void OnSelection(bool isSave)
    {
        SaveSlotManager.Instance.isSaveMode = isSave;
        selectionPanel.SetActive(false);
        slotPanel.SetActive(true);
        SaveSlotManager.Instance.RefreshSlots();
    }
}
