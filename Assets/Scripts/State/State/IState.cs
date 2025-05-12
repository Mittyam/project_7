
using UnityEngine;

/// <summary>
/// �e�C�x���g�X�e�[�g�̊�{�I�ȃ��C�t�T�C�N�����`����C���^�[�t�F�[�X
/// ���i�����͑�w/�x���͎���j�A�[���A��
/// </summary>
public interface IState
{
    GameObject gameObject { get; }

    // StateBase�Ŏ�������Ă���v���p�e�B�ɍ��킹�Ēǉ�
    IState NextState { get; set; }
    bool enabled { get; set; }

    void OnEnter();
    void OnUpdate();
    void OnExit();

    // �I�v�V����: ���ׂẴX�e�[�g�����̃X�e�[�gID���擾�ł���悤�ɂ���
    // StateID GetNextStateID();
}

/// <summary>
/// �ꎞ��~�ƍĊJ���T�|�[�g����X�e�[�g�p�̃C���^�[�t�F�[�X�g��
/// </summary>
public interface IPausableState : IState
{
    void OnPause();
    void OnResume();
}