// pixel8
// test2
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StatusData
{
    public int day;
    public int affection;
    public int love;
    public int money;
    public int actionPoint; // �s���|�C���g�ǉ�
    public string saveDate;
    public List<ItemData> ownedItems = new List<ItemData>();
    public List<ToyData> ownedToys = new List<ToyData>();
}

[System.Serializable]
public class ItemData
{
    public int itemId;              // �A�C�e��ID
    public string itemName;         // �A�C�e����
    public int quantity;            // ������
    public string description;      // ������
    public int price;               // ���i
    public bool isConsumable;       // �g�p���邲�ƂɌ������邩
    public bool isUniquePurchase;   // ��x�����w���\���i����؂�ɂȂ邩�j
    public ItemType itemType;       // �A�C�e���̎��

    // �A�C�e���̎�ނ��`����enum
    public enum ItemType
    {
        Book,      // �{
        Toy,       // ��������
        Game,      // �Q�[��
        Medicine,  // ��

        Accessory,  // �A�N�Z�T���[�i�w�A�s���A�w�A�S���Ȃǁj
        Drink,      // ���ݕ��i�R�b�v�Ȃǁj
        Special     // ���ʃA�C�e��
    }

    /// <summary>
    /// �A�C�e���f�[�^�̃R���X�g���N�^
    /// </summary>
    /// <param name="id">�A�C�e��ID</param>
    /// <param name="name">�A�C�e���l�[��</param>
    /// <param name="price">�A�C�e���̉��i</param>
    /// <param name="desc">�A�C�e���̐���</param>
    /// <param name="consumable">�����Ȃ����ǂ���</param>
    /// <param name="uniquePurchase">��x�����w���\���ǂ���</param>
    /// <param name="type">�A�C�e���^�C�v</param>
    public ItemData(int id, string name, int price, string desc, bool consumable, bool uniquePurchase, ItemType type)
    {
        this.itemId = id;
        this.itemName = name;
        this.price = price;
        this.description = desc;
        this.isConsumable = consumable;
        this.isUniquePurchase = uniquePurchase;
        this.itemType = type;
        this.quantity = 0;
    }

    /// <summary>
    /// �}�X�^�[�f�[�^��ی삵�Ȃ���
    /// �v���C���[�̏����A�C�e���Ƃ��ē������e�̃f�[�^���쐬
    /// </summary>
    /// <param name="source"></param>
    public ItemData(ItemData source)
    {
        this.itemId = source.itemId;
        this.itemName = source.itemName;
        this.price = source.price;
        this.description = source.description;
        this.isConsumable = source.isConsumable;
        this.isUniquePurchase = source.isUniquePurchase;
        this.itemType = source.itemType;
        this.quantity = source.quantity;
    }
}

[System.Serializable]
public class ToyData
{
    public int toyId;
    public string toyName;
    public bool isOwned;
    public string description; // �������ǉ�
    public Sprite icon; // �A�C�R���ǉ�
}