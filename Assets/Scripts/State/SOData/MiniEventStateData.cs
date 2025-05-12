using UnityEngine;

/// <summary>
/// �~�j�C�x���g�i��b�E�O�o�E�v���o�j�̃f�[�^���`����ScriptableObject
/// </summary>
[CreateAssetMenu(menuName = "Game/MiniEventStateData")]
public class MiniEventStateData : ScriptableObject
{
    [Header("��{���")]
    public StateID stateID;                 // �~�j�C�x���g�̎��ʎq
    public string displayName;              // �\����

    [Header("UI�ݒ�")]
    public GameObject[] startPrefabs;          // �J�n����Prefab
    public GameObject[] endPrefabs;            // �I�����oPrefab

    [Header("�C�x���g�ݒ�")]
    public bool consumeActionPoint = true;  // �A�N�V�����|�C���g������邩
    public int actionPointCost = 1;         // �����A�N�V�����|�C���g��

    [Header("�X�e�[�^�X�ω�")]
    public int affectionChange;             // �D���x�ω���
    public int loveChange;                  // ����ω���
    public int moneyChange;                 // �����ω���
}
