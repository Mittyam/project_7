using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// �I�����f�[�^�\����
/// </summary>
public class ChoiceOption
{
    public string choiceText;
    public System.Action onSelected;
}

/// <summary>
/// �I�����𓮓I�ɐ�������p�l��
/// </summary>
public class ChoiceButtonGenerator : MonoBehaviour
{
    [Header("�I�����p�l���ݒ�")]
    [SerializeField] private GameObject choicePanel;
    [SerializeField] private Button choiceButtonPrefab;
    [SerializeField] private Transform choiceContainer;

    private List<Button> activeButtons = new List<Button>();

    // �I�����̃{�^���𓮓I�ɐ���
    public void ShowChoices(List<ChoiceOption> choices)
    {
        // �����{�^���̃N���A
        ClearButtons();

        // �p�l���\��
        choicePanel.SetActive(true);

        // �I�����{�^���̐���
        foreach (var choice in choices)
        {
            Button newButton = Instantiate(choiceButtonPrefab, choiceContainer);

            // �{�^���e�L�X�g�̐ݒ�
            TextMeshProUGUI buttonText = newButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = choice.choiceText;
            }

            // �N���b�N�C�x���g�̓o�^
            System.Action choiceAction = choice.onSelected;
            newButton.onClick.AddListener(() => {
                choiceAction?.Invoke();
            });

            activeButtons.Add(newButton);
        }
    }

    public void ClosePanel()
    {
        choicePanel.SetActive(false);
        ClearButtons();
    }

    private void ClearButtons()
    {
        foreach (var button in activeButtons)
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                Destroy(button.gameObject);
            }
        }
        activeButtons.Clear();
    }
}
