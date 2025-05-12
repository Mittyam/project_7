using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���C���X�e�[�g�i���E�[�E��j�̃f�[�^���`
/// </summary>
[CreateAssetMenu(menuName = "Game/MainStateData")]
public class MainStateData : ScriptableObject
{
    [Header("��{���")]
    public StateID stateID;                 // �X�e�[�g�̎��ʎq�iDay/Evening/Night�j
    public string displayName;              // �\����

    [Header("UI�ݒ�")]
    public GameObject[] uiPrefab;           // �\���pPrefab

    [Header("Live2D�ݒ�")]
    public Live2DModelData live2DData;      // Live2D���f���f�[�^
    public bool showLive2DModel = true;     // Live2D���f����\�����邩

    [Header("�J�ڐݒ�")]
    public StateID nextStateID;             // ���̃X�e�[�gID�i�����[���郋�[�v�j
    public TriggerTiming exitTriggrTiming;  // ���̃X�e�[�g����o��Ƃ��̃g���K�[�^�C�~���O

    [Header("����|�C���g�ݒ�")]
    public int talkActionPointCost = 1;     // ��b�ɕK�v�ȃA�N�V�����|�C���g
    public int outingActionPointCost = 2;   // �O�o�ɕK�v�ȃA�N�V�����|�C���g
    public int gameActionPointCost = 1;     // �Q�[���ɕK�v�ȃA�N�V�����|�C���g

    [Header("�A�N�V�����{�^���ݒ�")]
    public bool enableTalkButton = true;    // ���b�{�^����L���ɂ��邩
    public bool enableOutingButton = true;  // ���o�����{�^����L���ɂ��邩
    public bool enableGameButton = true;    // �Q�[���{�^����L���ɂ��邩
    public bool enableMemoryButton = true;  // �v���o�{�^����L���ɂ��邩
}

/// <summary>
/// Live2D���f���̊�{�f�[�^���`����\����
/// </summary>
[System.Serializable]
public class Live2DModelData
{
    [Header("��{���")]
    public string modelID;             // ���f����ID�i�K�{�j
    public GameObject modelPrefab;     // ���f���̃v���n�u

    [Header("�\���ݒ�")]
    public Vector2 position = Vector2.zero;  // �\���ʒu
    public float scale = 1.0f;               // �\���X�P�[��

    [Header("�^�b�`�����ݒ�")]
    public bool enableTouch = true;          // �^�b�`�@�\�̗L��/����

    [Header("�f�t�H���g�A�j���[�V����")]
    public string defaultAnimTrigger = "idle"; // �����\�����̃A�j���[�V����
}