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

    /// <summary>
    /// MessagePrinter���^�O�����Ŏ擾���ăR���|�[�l���g��ݒ肷��
    /// </summary>
    public void SetupMessagePrinter()
    {
        // "MessagePrinter"�^�O�ŃI�u�W�F�N�g������
        GameObject messagePrinterObject = GameObject.FindWithTag("MessagePrinter");

        if (messagePrinterObject != null)
        {
            // MessagePrinter�R���|�[�l���g���擾
            messagePrinter = messagePrinterObject.GetComponent<MessagePrinter>();

            if (messagePrinter != null)
            {
                Debug.Log($"ShopManager: MessagePrinter�𐳏�Ɏ擾���܂��� (�I�u�W�F�N�g: {messagePrinterObject.name})");
            }
            else
            {
                Debug.LogError($"ShopManager: �I�u�W�F�N�g '{messagePrinterObject.name}' ��MessagePrinter�R���|�[�l���g��������܂���");
            }
        }
        else
        {
            Debug.LogError("ShopManager: 'MessagePrinter'�^�O�����I�u�W�F�N�g��������܂���");
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
            SoundManager.Instance.PlaySE(9);

            return false;
        }

        // �������̃`�F�b�N
        if (StatusManager.Instance.GetStatus().money < itemData.price)
        {
            ShowMessage("������������܂���B");

            // ���s����炷
            SoundManager.Instance.PlaySE(9);

            return false;
        }

        // �����������炷
        StatusManager.Instance.UpdateStatus(0, 0, 0, -itemData.price);

        // �A�C�e����ǉ�
        ItemData newItem = new ItemData(itemData);
        newItem.quantity = 1; // �w�����̐��ʂ�1
        StatusManager.Instance.AddItem(newItem);

        // �w����������炷
        SoundManager.Instance.PlaySE(8);

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