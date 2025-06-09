using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �A�C�e�����g�p���邽�߂̃}�l�[�W���[�N���X
/// </summary>
public class ItemManager : Singleton<ItemManager>
{
    // �A�C�e���g�p�C�x���g
    public System.Action<int> OnItemUsed;

    // �A�C�e�����g�p����
    public bool UseItem(int itemId)
    {
        // �A�C�e�����������Ă��邩�m�F
        ItemData ownedItem = GetOwnedItemById(itemId);

        if (ownedItem == null || ownedItem.quantity <= 0)
        {
            Debug.Log($"�A�C�e��ID {itemId} �͏������Ă��Ȃ����A���ʂ�0�ł��B");
            return false;
        }

        // ����A�C�e���̏ꍇ�A���ʂ����炷
        if (ownedItem.isConsumable)
        {
            StatusManager.Instance.DecreaseItem(itemId);
        }

        // �A�C�e���̌��ʂ�K�p�i���ۂ̃Q�[�����W�b�N�ɍ��킹�Ď����j
        ApplyItemEffect(itemId);

        // �A�C�e���g�p�C�x���g�𔭉�
        OnItemUsed?.Invoke(itemId);

        return true;
    }

    // �����A�C�e������ID�Ō���
    private ItemData GetOwnedItemById(int itemId)
    {
        List<ItemData> ownedItems = StatusManager.Instance.GetOwnedItems();
        return ownedItems.Find(item => item.itemId == itemId);
    }

    // �A�C�e���̌��ʂ�K�p�i�Q�[���ɍ��킹�Ď����j
    private void ApplyItemEffect(int itemId)
    {
        // �A�C�e���f�[�^��������擾
        ItemData itemData = ItemDatabase.Instance.GetItemById(itemId);

        if (itemData == null)
        {
            Debug.LogWarning($"�A�C�e��ID {itemId} �̃f�[�^��������܂���B");
            return;
        }

        // �A�C�e���̎�ނɂ���Č��ʂ𕪊�
        switch (itemData.itemType)
        {
            case ItemData.ItemType.Medicine:
                if (itemId == 6)
                {
                    StatusManager.Instance.UpdateMaxEjaCount(2);
                    Debug.Log($"{itemData.itemName}���g�p���܂����B�ː��񐔏����2�񑝂₵�܂����B");
                }
                else if (itemId == 7)
                {
                    StatusManager.Instance.UpdateMaxOrgCount(3);
                    Debug.Log($"{itemData.itemName}���g�p���܂����B�Ⓒ�񐔏����3�񑝂₵�܂����B");
                }
                break;
            case ItemData.ItemType.Accessory:
                // �A�N�Z�T���[�̌��ʗ�F�D���x���グ��
                StatusManager.Instance.UpdateStatus(0, 5, 0, 0);
                Debug.Log($"{itemData.itemName}�𑕒����܂����B�D���x��5�オ��܂����B");
                break;

            case ItemData.ItemType.Toy:
                // ��������̌��ʗ�F������グ��
                StatusManager.Instance.UpdateStatus(0, 0, 10, 0);
                Debug.Log($"{itemData.itemName}�ŗV�т܂����B���10�オ��܂����B");
                break;

            case ItemData.ItemType.Drink:
                // ���ݕ��̌��ʗ�F�D���x�ƈ���������グ��
                StatusManager.Instance.UpdateStatus(0, 3, 3, 0);
                Debug.Log($"{itemData.itemName}���g���܂����B�D���x�ƈ��3���オ��܂����B");
                break;

            case ItemData.ItemType.Special:
                // ���ʃA�C�e���̌��ʗ�F�傫�����ʂ��グ��
                StatusManager.Instance.UpdateStatus(0, 10, 15, 0);
                Debug.Log($"{itemData.itemName}���g�p���܂����B�D���x��10�A���15�オ��܂����B");
                break;
        }
    }
}