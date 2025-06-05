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
        // �{��
        allItems.Add(new ItemData(
            1,
            "������ƃG�b�`�Ȗ{",
            1000,
            "�}���قňꏏ�ɓǂނ�H�x���P�オ��܂��B",
            false,   // ������
            false,    // �ēx�w���\
            ItemData.ItemType.Book,
            0,    // �D���x�㏸�{�[�i�X
            1,    // H�x�㏸�{�[�i�X
            0     // �����㏸�{�[�i�X
        ));

        // �{��
        allItems.Add(new ItemData(
            2,
            "�����G�b�`�Ȗ{",
            2500,
            "�}���قňꏏ�ɓǂނ�H�x���R�オ��܂��B",
            false,   // ������
            false,    // �ēx�w���\
            ItemData.ItemType.Book,
            0,    // �D���x�㏸�{�[�i�X
            3,    // H�x�㏸�{�[�i�X
            0     // �����㏸�{�[�i�X
        ));

        // �{��
        allItems.Add(new ItemData(
            3,
            "��������",
            2000,
            "�}���قňꏏ�ɓǂނƍD���x���P�AH�x���Q�オ��܂��B",
            false,   // ������
            false,    // �ēx�w���\
            ItemData.ItemType.Book,
            1,    // �D���x�㏸�{�[�i�X
            2,    // H�x�㏸�{�[�i�X
            0     // �����㏸�{�[�i�X
        ));

        // �������ቮ
        allItems.Add(new ItemData(
            4,
            "�G�b�`�ȃQ�[��",
            6000,
            "�g�p����ƍD���x���P�AH�x���R�オ��܂��B",
            true,   // �����Ȃ�
            true,    // ��x�����w���\
            ItemData.ItemType.Game,
            1,    // �D���x�㏸�{�[�i�X
            3,    // H�x�㏸�{�[�i�X
            0     // �����㏸�{�[�i�X
        ));

        // �������ቮ
        allItems.Add(new ItemData(
            5,
            "�p�[�e�B�[�Q�[��",
            4000,
            "�g�p����ƍD���x���R�オ��܂��B",
            true,   // �����Ȃ�
            true,    // ��x�����w���\
            ItemData.ItemType.Game,
            3,    // �D���x�㏸�{�[�i�X
            0,    // H�x�㏸�{�[�i�X
            0     // �����㏸�{�[�i�X
        ));

        allItems.Add(new ItemData(
            6,
            "���͍�",
            3000,
            "�g�p����Ǝː��܂ł̎��Ԃ��Z�k����܂��B",
            false,   // ������
            false,    // �ēx�w���\
            ItemData.ItemType.Medicine,  // ��
            0,    // �D���x�㏸�{�[�i�X
            0,    // H�x�㏸�{�[�i�X
            0     // �����㏸�{�[�i�X
        ));

        allItems.Add(new ItemData(
            7,
            "�Z��",
            3000,
            "�g�p����ƐⒸ�܂ł̎��Ԃ��Z�k����܂��B",
            false,   // ������
            false,    // �ēx�w���\
            ItemData.ItemType.Medicine,  // ��
            1,    // �D���x�㏸�{�[�i�X
            0,    // H�x�㏸�{�[�i�X
            0     // �����㏸�{�[�i�X
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