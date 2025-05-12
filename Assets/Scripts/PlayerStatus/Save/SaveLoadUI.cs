using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveLoadUI : MonoBehaviour
{
    [Header("セーブ/ロード選択ポップアップ")]
    public GameObject selectionPanel;
    public Button saveButton;
    public Button loadButton;

    [Header("スロットメニュー")]
    public GameObject slotPanel;

    private void Start()
    {
        // 初期状態では両方非表示
        selectionPanel.SetActive(false);
        slotPanel.SetActive(false);

        // ボタンのクリックイベントにリスナーを登録
        saveButton.onClick.AddListener(() => OnSelection(true));
        loadButton.onClick.AddListener(() => OnSelection(false));
    }

    private void Update()
    {
        // エスケープキーを押したら、表示中のパネルを閉じる
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
