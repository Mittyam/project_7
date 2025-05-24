using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �m�x���C�x���g�̃f�[�^���`
/// </summary>
[CreateAssetMenu(menuName = "Game/NovelEventData")]
public class NovelEventData : ScriptableObject
{
    [Header("��{���")]
    public int eventID;                     // �C�x���gID
    public string eventName;                // �C�x���g��
    public string eventDescription;         // �C�x���g����

    [Header("�g���K�[�ݒ�")]
    public TriggerTiming triggerTiming;     // �����^�C�~���O
    public List<Condition> conditions;      // �����������X�g

    [Header("�J�ڐݒ�")]
    public StateID nextStateID;             // �C�x���g��ɑJ�ڂ���X�e�[�gID

    [Header("���\�[�X�ݒ�")]
    // public string scenarioPath;             // �V�i���I�t�@�C���ւ̃p�X
    public Sprite thumbnailImage;           // �T���l�C���摜�i�v���o�ꗗ�p�j
    public bool unlockAsMemory = true;      // �v���o�Ƃ��ĉ�����邩

    [Header("�X�e�[�^�X�ω�")]
    public int affectionChange;             // �D���x�ω���
    public int loveChange;                  // ����ω���
    public int moneyChange;                 // �����ω���
}

/// <summary>
/// �����̎�ނ��`����񋓌^
/// </summary>
public enum ConditionType
{
    CheckAffinity,      // �D���x�`�F�b�N
    CheckLove,          // ����`�F�b�N
    CheckDate,          // ���t�`�F�b�N
    CheckState,         // ��ԃ`�F�b�N
}

/// <summary>
/// �������`����N���X
/// </summary>
[System.Serializable]
public class Condition
{
    public ConditionType conditionType;     // �����̎��
    public int threshold;                   // 臒l
    public string paramName;                // �����ɕK�v�ȃp�����[�^��
    public bool isGreaterThanOrEqual = true; // true: �ȏ�Afalse: �ȉ�
}
