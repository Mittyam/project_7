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
    private bool isInitialized = false;

    // OnEnable�ŏ�����
    private void OnEnable()
    {
        EnsureInitialized();
    }

    // �������m�F���\�b�h
    private void EnsureInitialized()
    {
        if (isInitialized) return;

        // �R���|�[�l���g���Ȃ��ꍇ�͌���
        if (choicePanel == null)
            choicePanel = gameObject;

        if (choiceContainer == null)
            choiceContainer = transform.Find("ChoiceContainer");

        if (choiceButtonPrefab == null)
        {
            Debug.LogWarning("ChoiceButtonGenerator: �I�����{�^���v���n�u���ݒ肳��Ă��܂���");
            return;
        }

        isInitialized = true;
    }

    // �I�����̕\��
    public void ShowChoices(List<ChoiceOption> choices)
    {
        // �������m�F
        EnsureInitialized();

        // �����{�^���̃N���A
        ClearButtons();

        // �p�l���\��
        if (choicePanel != null)
            choicePanel.SetActive(true);

        if (choiceContainer == null || choiceButtonPrefab == null)
        {
            Debug.LogError("ChoiceButtonGenerator: �K�v�ȃR���|�[�l���g��������܂���");
            return;
        }

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

    // �p�l�������
    public void ClosePanel()
    {
        if (choicePanel != null)
            choicePanel.SetActive(false);

        ClearButtons();
    }

    // �{�^���̃N���A
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