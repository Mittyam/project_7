using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �e�X�e�[�g�����ʂ��邽�߂̗񋓌^
/// </summary>
public enum StateID
{
    // ���C���X�e�[�g
    Day,
    Evening,
    Night,

    // �~�j�C�x���g�X�e�[�g
    Library,    // �}����
    Cafe,       // �J�t�F
    PartJob,    // �o�C�g
    Walk,       // �U��
    Game,       // �Q�[��
    Outing,     // ���o����
    Talk,       // ���b
    Sleep,      // ����

    Bath,       // ����
    Touch,      // �G�ꍇ��
    item,       // �A�C�e��
    Memory,     // �v���o

    // ���̑�
    Novel,

    Idle,
    Animation,
    Result,
    None,
}
