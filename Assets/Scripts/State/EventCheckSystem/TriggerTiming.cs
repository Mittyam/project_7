/// <summary>
/// �C�x���g�����̃^�C�~���O���`
/// </summary>
public enum TriggerTiming
{
    None,           // �g���K�[�Ȃ�
    DayToEvening,   // �����[
    EveningToNight, // �[����
    NightToDay,     // �遨��

    OnLibrary,      // �}������
    OnCafe,         // �J�t�F��
    OnPartJob,      // �o�C�g��
    OnWalk,         // �U����
    OnGame,         // �Q�[����
    OnOuting,       // ���o������
    OnTalk,         // ��b��
    OnSleep,        // ������

    OnBath,         // ������
    OnTouch,        // �G�ꍇ����
    OnMemory,       // �v���o�{����
    
    Manual          // �蓮�g���K�[�i�f�o�b�O�p�j
}