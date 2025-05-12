using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : Singleton<ShopManager>
{
    [Header("�V���b�v�ݒ�")]
    [SerializeField] private MessagePrinter messagePrinter;

    // �w���C�x���g (ItemID)
    public System.Action<int> OnItemPurchased;

    private void Start()
    {
        // MessagePrinter�̎擾
        if (messagePrinter == null)
        {
            messagePrinter = FindObjectOfType<MessagePrinter>();
        }
    }

    // �A�C�e�����w���ς݂��ǂ������`�F�b�N
    public bool IsItemPurchased(int itemId)
    {
        // �A�C�e���̏����擾
        ItemData itemData = ItemDatabase.Instance.GetItemById(itemId);

        if (itemData == null)
        {
            return false;
        }

        // ��x�����w���\�ȃA�C�e���̏ꍇ
        if (itemData.isUniquePurchase)
        {
            List<ItemData> ownedItems = StatusManager.Instance.GetOwnedItems();
            ItemData ownedItem = ownedItems.Find(item => item.itemId == itemId);

            return (ownedItem != null && ownedItem.quantity > 0);
        }

        return false;
    }

    // �A�C�e���w������
    public bool PurchaseItem(int itemId)
    {
        // �A�C�e���̏����擾
        ItemData itemData = ItemDatabase.Instance.GetItemById(itemId);

        if (itemData == null)
        {
            ShowMessage("�A�C�e����������܂���B");
            return false;
        }

        // ��x�����w���\�ȃA�C�e���ŁA���łɏ������Ă���ꍇ
        if (itemData.isUniquePurchase && IsItemPurchased(itemId))
        {
            ShowMessage($"{itemData.itemName}�͂��łɏ������Ă��܂��B");

            // ���s����炷
            // SoundManager.Instance.PlaySE(purchaseFailSound);

            return false;
        }

        // �������̃`�F�b�N
        if (StatusManager.Instance.GetStatus().money < itemData.price)
        {
            ShowMessage("������������܂���B");

            // ���s����炷
            // SoundManager.Instance.PlaySE(purchaseFailSound);

            return false;
        }

        // �����������炷
        StatusManager.Instance.UpdateStatus(0, 0, 0, -itemData.price);

        // �A�C�e����ǉ�
        ItemData newItem = new ItemData(itemData);
        newItem.quantity = 1; // �w�����̐��ʂ�1
        StatusManager.Instance.AddItem(newItem);

        // �w����������炷
        // SoundManager.Instance.PlaySE(purchaseSuccessSound);

        // �w���C�x���g�𔭉�
        OnItemPurchased?.Invoke(itemId);

        ShowMessage($"{itemData.itemName}���w�����܂����B");
        return true;
    }

    // ���b�Z�[�W�\��
    private void ShowMessage(string message)
    {
        if (messagePrinter != null)
        {
            messagePrinter.PrintMessage(message);
        }
        else
        {
            Debug.Log(message);
        }
    }
}