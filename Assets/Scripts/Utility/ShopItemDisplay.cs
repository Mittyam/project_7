using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShopItemDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private MessagePrinter messagePrinter;
    [SerializeField] private ShopPanelManager shopPanelManager;

    // �V���b�v�^�C�v��\��enum
    public enum ShopType
    {
        Pharmacy,   // ���
        ToyStore,   // �������ቮ
        BookStore,  // �{��
        None,       // ���ݒ�
    }

    [Header("�V���b�v�ݒ�")]
    [SerializeField] private ShopType shopType;
    [TextArea(3, 5)]
    [SerializeField] private string shopDescription;

    private void Start()
    {
        // MessagePrinter���ݒ肳��Ă��Ȃ��ꍇ�͎����I�ɒT��
        if (messagePrinter == null)
        {
            messagePrinter = FindObjectOfType<MessagePrinter>();
            if (messagePrinter == null)
            {
                Debug.LogError("MessagePrinter��������܂���B�C���X�y�N�^�[�Őݒ肵�Ă��������B");
            }
        }

        // ShopPanelManager���ݒ肳��Ă��Ȃ��ꍇ�͎����I�ɒT��
        if (shopPanelManager == null)
        {
            shopPanelManager = FindObjectOfType<ShopPanelManager>();
            if (shopPanelManager == null)
            {
                Debug.LogError("ShopPanelManager��������܂���B�C���X�y�N�^�[�Őݒ肵�Ă��������B");
            }
        }
    }

    // �}�E�X����������̃C�x���g
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (messagePrinter != null)
        {
            // �����N���A�@�\���ꎞ�I�ɖ�����
            bool originalAutoClear = messagePrinter.autoClearMessage;
            messagePrinter.autoClearMessage = false;

            // ��������\��
            messagePrinter.ShowMessage(shopDescription);

            // �ݒ�����ɖ߂�
            messagePrinter.autoClearMessage = originalAutoClear;
        }
    }

    // �}�E�X�����ꂽ���̃C�x���g
    public void OnPointerExit(PointerEventData eventData)
    {
        if (messagePrinter != null)
        {
            // ���b�Z�[�W���N���A
            messagePrinter.ClearMessage();
        }
    }

    // �N���b�N���̃C�x���g�i�V�K�ǉ��j
    public void OnPointerClick(PointerEventData eventData)
    {
        if (shopPanelManager != null)
        {
            // shopType��None�̏ꍇ�͉������Ȃ�
            if (shopType == ShopType.None)
            {
                Debug.LogWarning("ShopType��None�ɐݒ肳��Ă��܂��B�V���b�v���J�����Ƃ͂ł��܂���B");
                return;
            }

            // �N���b�N���ꂽ�V���b�v�̃p�l�����J��
            shopPanelManager.OpenShopPanel(shopType);

            // �N���b�N���ɐ������N���A
            if (messagePrinter != null)
            {
                messagePrinter.ClearMessage();
            }
        }
    }

    // �V���b�v�̃^�C�v���擾
    public ShopType GetShopType()
    {
        return shopType;
    }
}
