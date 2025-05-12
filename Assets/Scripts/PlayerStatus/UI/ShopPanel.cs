using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopPanel : MonoBehaviour
{
    [Header("UI�v�f")]
    [SerializeField] private TextMeshProUGUI moneyText;     // �������\��
    [SerializeField] private Button backButton;             // �߂�{�^��
    [SerializeField] private ShopPanelManager shopPanelManager; // �V���b�v�p�l���}�l�[�W���[

    [Header("�V���b�v�A�C�e���{�^��")]
    [SerializeField] private ShopItemButton[] itemButtons;  // ���O�ɔz�u�����A�C�e���{�^��

    private void OnEnable()
    {
        // ��ʕ\�����ɏ��������X�V
        UpdateMoneyDisplay();

        // �X�e�[�^�X�X�V�C�x���g�Ƀ��X�i�[��ǉ�
        StatusManager.Instance.OnStatusUpdated += UpdateMoneyDisplay;
    }

    private void OnDisable()
    {
        // ���X�i�[�̉���
        if (StatusManager.Instance != null)
        {
            StatusManager.Instance.OnStatusUpdated -= UpdateMoneyDisplay;
        }
    }

    private void Start()
    {
        // �߂�{�^���̃C�x���g�o�^
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
        }

        // �����\���X�V
        UpdateMoneyDisplay();
    }

    // �������\���̍X�V
    private void UpdateMoneyDisplay()
    {
        if (moneyText != null)
        {
            moneyText.text = StatusManager.Instance.GetStatus().money.ToString();
        }
    }

    // �߂�{�^���N���b�N��
    private void OnBackButtonClicked()
    {
        shopPanelManager.ReturnToStoreSelection();
    }
}