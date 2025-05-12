using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening; // �A�j���[�V�����p�iDOTween�g�p�j

public class ShopItemButton : MonoBehaviour, IPointerClickHandler
{
    [Header("�A�C�e���ݒ�")]
    [SerializeField] private int itemId; // �Œ�̃A�C�e��ID

    [Header("UI�v�f")]
    [SerializeField] private Image itemImage;       // �A�C�e���摜
    [SerializeField] private Sprite normalSprite;   // �ʏ펞�̉摜
    [SerializeField] private Sprite soldOutSprite;  // ����؂ꎞ�̉摜

    [Header("�{�^���ݒ�")]
    [SerializeField] private bool interactable = true;  // �{�^��������\��

    private ItemData itemData;
    private bool isSoldOut = false;

    private void OnEnable()
    {
        // ��ʕ\�����ɏ�Ԃ��X�V
        RefreshButtonState();

        // �w���C�x���g�̃��X�i�[��o�^
        ShopManager.Instance.OnItemPurchased += OnAnyItemPurchased;
    }

    private void OnDisable()
    {
        // ���X�i�[�̉���
        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.OnItemPurchased -= OnAnyItemPurchased;
        }
    }

    private void Start()
    {
        // �A�C�e���f�[�^�̎擾
        itemData = ItemDatabase.Instance.GetItemById(itemId);

        if (itemData == null)
        {
            Debug.LogError($"�A�C�e��ID: {itemId} �̃f�[�^��������܂���B");
            return;
        }

        // ������Ԃ̐ݒ�
        RefreshButtonState();
    }

    // �{�^���̏�Ԃ��X�V
    private void RefreshButtonState()
    {
        if (itemData == null) return;

        // ��x�����w���\�ŁA���ɍw���ς݂��`�F�b�N
        isSoldOut = itemData.isUniquePurchase && ShopManager.Instance.IsItemPurchased(itemId);

        // �摜�̍X�V
        if (itemImage != null)
        {
            if (isSoldOut && soldOutSprite != null)
            {
                itemImage.sprite = soldOutSprite;
            }
            else if (normalSprite != null)
            {
                itemImage.sprite = normalSprite;
            }
        }

        // ����\��Ԃ̍X�V
        interactable = !isSoldOut;
    }

    // �����ꂩ�̃A�C�e�����w�����ꂽ���̃R�[���o�b�N
    private void OnAnyItemPurchased(int purchasedItemId)
    {
        // �����̃A�C�e�����w�����ꂽ�ꍇ�̂ݏ���
        if (purchasedItemId == itemId)
        {
            RefreshButtonState();
        }
    }

    // �N���b�N���̏���
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!interactable) return;

        // �N���b�N�A�j���[�V����
        AnimateButtonClick();

        // �A�C�e���w������
        ShopManager.Instance.PurchaseItem(itemId);
    }

    // �N���b�N���̃A�j���[�V����
    private void AnimateButtonClick()
    {
        transform.DOScale(0.95f, 0.1f).SetEase(Ease.OutQuad).OnComplete(() => {
            transform.DOScale(1f, 0.1f).SetEase(Ease.OutQuad);
        });
    }
}