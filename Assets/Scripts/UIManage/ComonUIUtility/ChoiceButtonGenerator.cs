using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ChoiceOption
{
    public string choiceText;              // �{�^���ɕ\�����镶��
    public System.Action onSelected;       // �{�^�������������̏���
}

/// <summary>
/// �I�����𓮓I�ɐ�������p�l��
/// </summary>
public class ChoiceButtonGenerator : MonoBehaviour
{
    [SerializeField] private Transform content; // �{�^���𐶐�����e�I�u�W�F�N�g
    [SerializeField] private GameObject choiceButtonPrefab;

    // �I�����̃{�^���𓮓I�ɐ���
    public void ShowChoices(List<ChoiceOption> options)
    {
        // ���łɃ{�^�����c���Ă�����폜
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        // �p�l�����̂��A�N�e�B�u��
        gameObject.SetActive(true);

        // �I�����̃{�^���𐶐�
        foreach (var option in options)
        {
            var buttonObj = Instantiate(choiceButtonPrefab, content);
            var button = buttonObj.GetComponent<Button>();

            // �{�^���̃e�L�X�g��ݒ�
            var textComp = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            textComp.text = option.choiceText;

            // �{�^���������ꂽ���̏�����ݒ�
            button.onClick.AddListener(() =>
            {
                // �R�[���o�b�N�����s
                option.onSelected?.Invoke();
                // �I����̓p�l�������Ȃ�
                ClosePanel();
            });
        }
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }
}
