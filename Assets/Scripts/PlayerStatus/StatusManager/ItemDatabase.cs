using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �A�C�e���}�X�^�[�f�[�^�̊Ǘ��N���X
/// </summary>
public class ItemDatabase : Singleton<ItemDatabase>
{
    [SerializeField] private List<ItemData> allItems = new List<ItemData>();
    private Dictionary<int, ItemData> itemDictionary = new Dictionary<int, ItemData>();

    protected override void Awake()
    {
        base.Awake();

        // �����̏�����
        foreach (ItemData item in allItems)
        {
            if (!itemDictionary.ContainsKey(item.itemId))
            {
                itemDictionary.Add(item.itemId, item);
            }
            else
            {
                Debug.LogError($"�d������A�C�e��ID: {item.itemId} ({item.itemName})");
            }
        }

        // ���X�g����̏ꍇ�͏����f�[�^��ݒ�
        if (allItems.Count == 0)
        {
            InitializeItemDatabase();
        }
    }

    /// <summary>
    /// �A�C�e���f�[�^�x�[�X�����������郁�\�b�h
    /// </summary>
    private void InitializeItemDatabase()
    {
        // �R�b�v
        allItems.Add(new ItemData(
            1,
            "�R�b�v",
            4000,
            "���킢���R�b�v�B���ݕ������邱�Ƃ��ł��܂��B",
            false,   // �����Ȃ�
            false,    // ��x�����w���\
            ItemData.ItemType.Drink
        ));

        // �ʂ������
        allItems.Add(new ItemData(
            2,
            "�������̂ʂ������",
            10000,
            "���킢���������̂ʂ�����݁B��؂ɂ��ĂˁB",
            false,   // �����Ȃ�
            true,    // ��x�����w���\
            ItemData.ItemType.Toy
        ));

        // �w�A�s��
        allItems.Add(new ItemData(
            3,
            "�w�A�s��",
            2000,
            "���킢���w�A�s���B�g�ɂ���Ƃ��킢���A�b�v�I",
            false,   // �����Ȃ�
            true,    // ��x�����w���\
            ItemData.ItemType.Accessory
        ));

        // �w�A�S��
        allItems.Add(new ItemData(
            4,
            "�w�A�S��",
            2000,
            "�������ȃw�A�S���B�����܂Ƃ߂�̂ɕ֗��B",
            false,   // �����Ȃ�
            true,    // ��x�����w���\
            ItemData.ItemType.Accessory
        ));

        // �����̍X�V
        itemDictionary.Clear();
        foreach (ItemData item in allItems)
        {
            itemDictionary.Add(item.itemId, item);
        }
    }

    // �A�C�e��ID����A�C�e�����擾
    public ItemData GetItemById(int itemId)
    {
        if (itemDictionary.TryGetValue(itemId, out ItemData item))
        {
            return item;
        }
        return null;
    }

    // ���ׂẴA�C�e�����擾
    public List<ItemData> GetAllItems()
    {
        return new List<ItemData>(allItems);
    }
}