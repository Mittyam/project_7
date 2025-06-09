// pixel8
// aaaatest
// test2
// minievent1
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StatusData
{
    public int day;         // �o�ߓ���
    public int affection;   // �D���x
    public int love;        // H�x
    public int orgCount;    // �Ⓒ��
    public int ejaCount;    // �ː���

    public int maxEjaCount = 1;
    public int maxOrgCount = 2;
    public int stateEjaCount = 0;   // �e�X�e�[�g�̎ː���
    public int stateOrgCount = 0;   // �e�X�e�[�g�̐Ⓒ��

    public int money;       // ������
    public int actionPoint; // �s���|�C���g�ǉ�
    public string saveDate; // �Z�[�u����
    public List<ItemData> ownedItems = new List<ItemData>();
    public List<EventStateData> eventStates = new List<EventStateData>();

    public StateID savedStateID = StateID.None; // �Z�[�u���̃��C���X�e�[�gID
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

    // �A�C�e�����ʃp�����[�^��ǉ�
    public int affectionBonus;      // �D���x�㏸�{�[�i�X
    public int loveBonus;           // ����㏸�{�[�i�X
    public int moneyBonus;          // �����㏸�{�[�i�X

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
    /// <param name="affectionBonus">�D���x�㏸�{�[�i�X</param>
    /// <param name="loveBonus">����㏸�{�[�i�X</param>
    /// <param name="moneyBonus">�����㏸�{�[�i�X</param>
    public ItemData(int id, string name, int price, string desc, bool consumable, bool uniquePurchase,
                    ItemType type, int affectionBonus = 0, int loveBonus = 0, int moneyBonus = 0)
    {
        this.itemId = id;
        this.itemName = name;
        this.price = price;
        this.description = desc;
        this.isConsumable = consumable;
        this.isUniquePurchase = uniquePurchase;
        this.itemType = type;
        this.quantity = 0;

        // �{�[�i�X�p�����[�^��ݒ�
        this.affectionBonus = affectionBonus;
        this.loveBonus = loveBonus;
        this.moneyBonus = moneyBonus;
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

        // �{�[�i�X�p�����[�^���R�s�[
        this.affectionBonus = source.affectionBonus;
        this.loveBonus = source.loveBonus;
        this.moneyBonus = source.moneyBonus;
    }
}

// �C�x���g��Ԃ��V���A���C�Y���邽�߂̃N���X
[System.Serializable]
public class EventStateData
{
    public int eventId;
    public EventState state;

    public EventStateData(int id, EventState eventState)
    {
        eventId = id;
        state = eventState;
    }
}