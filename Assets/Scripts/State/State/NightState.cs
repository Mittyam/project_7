using System.Collections;
using UnityEngine;
using static GameEvents;

/// <summary>
/// ��X�e�[�g
/// �E�X�e�[�^�X�X�V
/// �E���Ȃ�ł͂̃T�u�C�x���g�̔���Ȃ�
/// </summary>
public class NightState : StateBase, IPausableState
{
    [Header("State Data")]
    [SerializeField] private MainStateData stateData;

    [Header("UI�v�f")]
    [SerializeField] private GameObject nightUIContainer;
    [SerializeField] private GameObject commonUIContainer;

    [Header("Events")]
    [SerializeField] private GameEvent nightStateEventSO;

    // Live2D�֘A�t�B�[���h�ǉ�
    private GameObject activeLive2DModel; // �A�N�e�B�u��Live2D���f���ւ̎Q��
    private string currentModelID; // ���ݕ\�����̃��f��ID

    // UI������ԊǗ��p�t���O
    private bool isUICreated = false;

    public override void OnEnter()
    {
        Debug.Log("NightState�ɓ���܂��B");
        StatusManager.Instance.RecoverDailyActionPoints();

        // UI�v�f�̃Z�b�g�A�b�v�ƕ\��
        SetupUI();
        ShowNightUI();

        // Index 2~4 ��BGM�������_���ɍĐ�
        int randomIndex = Random.Range(8, 11);
        SoundManager.Instance.PlayBGMWithFadeIn(randomIndex, 1f);

        // �C�x���g����
        nightStateEventSO.Raise();

        // Live2D���f�������������ĕ\��
        SetupLive2DModel();
    }

    public override void OnUpdate()
    {
        
    }

    public override void OnExit()
    {
        Debug.Log("NightState: �o�܂��B");

        // BGM���~
        SoundManager.Instance.StopBGM();

        // �^�b�`�n���h���[�𖳌���
        DisableLive2DTouchHandler();

        // Live2D���f���̃N���[���A�b�v
        CleanupLive2DModel();

        // UI�̔�\����
        HideAllUI();

        // ���̓��ւ̈ڍs�Ȃ̂ŁA�����ł̃m�x���C�x���g�`�F�b�N
        EventTriggerChecker.Check(TriggerTiming.NightToDay);
    }

    public void OnPause()
    {
        Debug.Log("NightState: �ꎞ��~���܂�");

        // UI�v�f���\���ɂ���
        HideAllUI();

        SoundManager.Instance.StopBGM();
    }

    public void OnResume()
    {
        Debug.Log("NightState: �ĊJ���܂�");

        // UI�v�f���ĕ\������
        SetupUI();
        ShowNightUI();

        // Index 2~4 ��BGM�������_���ɍĐ�
        int randomIndex = Random.Range(8, 11);
        SoundManager.Instance.PlayBGMWithFadeIn(randomIndex, 1f);
    }

    // ���̃X�e�[�g�̎擾���\�b�h
    public StateID GetNextStateID()
    {
        return stateData != null ? stateData.nextStateID : StateID.Day;
    }

    #region --- UI�֘A���\�b�h ---

    // UI�̃Z�b�g�A�b�v
    private void SetupUI()
    {
        // UI�R���e�i�̏�����
        if (nightUIContainer == null)
        {
            nightUIContainer = new GameObject("NightUIContainer");
            nightUIContainer.transform.SetParent(transform);
            nightUIContainer.AddComponent<RectTransform>();
        }

        if (commonUIContainer == null)
        {
            commonUIContainer = new GameObject("CommonUIContainer");
            commonUIContainer.transform.SetParent(transform);
            commonUIContainer.AddComponent<RectTransform>();
        }

        // UI�v���n�u�̐����i�܂���������Ă��Ȃ��ꍇ�̂݁j
        if (!isUICreated && stateData != null && stateData.uiPrefab != null)
        {
            foreach (var prefab in stateData.uiPrefab)
            {
                if (prefab != null)
                {
                    // �J�����ݒ�t���̃C���X�^���X�����g�p
                    InstantiateUIWithCamera(prefab, nightUIContainer.transform);
                }
            }
            isUICreated = true;

            Debug.Log("NightState: UI�v���n�u�𐶐����A�J�����ݒ��K�p���܂���");
        }

        // ����L��UI�ݒ���ԍX�V������΂����Ŏ��s
        // ��: ��̕��͋C�G�t�F�N�g�K�p�Ȃ�
        // UpdateNightUIEffects();
    }

    // ��UI�̕\��
    private void ShowNightUI()
    {
        if (nightUIContainer != null)
        {
            nightUIContainer.SetActive(true);
        }

        if (commonUIContainer != null)
        {
            commonUIContainer.SetActive(true);
        }
    }

    // ����L��UI�G�t�F�N�g�X�V�i�I�v�V���� - �K�v�ɉ����Ď����j
    private void UpdateNightUIEffects()
    {
        // ��: ���C�e�B���O�G�t�F�N�g��J���[�t�B���^�[�̓K�p
        // ��̕��͋C�����o����UI����
        if (nightUIContainer != null)
        {
            // ��: ������G�t�F�N�g�̓_�Őݒ�Ȃ�
            // �����ɖ���L�̃G�t�F�N�g������ǉ�
        }
    }

    // �SUI�̔�\��
    private void HideAllUI()
    {
        if (nightUIContainer != null)
        {
            nightUIContainer.SetActive(false);
        }

        if (commonUIContainer != null)
        {
            commonUIContainer.SetActive(false);
        }
    }

    #endregion

    #region --- Live2D�֘A���\�b�h ---

    // Live2D���f�����Z�b�g�A�b�v���郁�\�b�h
    private void SetupLive2DModel()
    {
        // Live2D�̕\���������̏ꍇ�͏������Ȃ�
        if (stateData == null || stateData.live2DData == null || !stateData.showLive2DModel)
        {
            return;
        }

        // Live2D���f���̃f�[�^���擾
        var live2DData = stateData.live2DData;

        // ���łɃ��f�������݂��Ă���ꍇ�͍폜
        CleanupLive2DModel();

        // ���f���v���n�u������ΐ���
        if (live2DData.modelPrefab != null)
        {
            // Live2DController�R���|�[�l���g���擾
            Live2DController live2DController = FindLive2DController();

            if (live2DController != null)
            {
                // ���f��ID��ۑ�
                currentModelID = live2DData.modelID;

                // Live2D���f����\��
                live2DController.ShowModel(
                    currentModelID,
                    live2DData.scale,
                    live2DData.position.ToString()
                );

                // �A�j���[�V�����Đ�
                if (!string.IsNullOrEmpty(live2DData.defaultAnimTrigger))
                {
                    live2DController.PlayAnimation(currentModelID, live2DData.defaultAnimTrigger);
                }

                // ���f���Q�Ǝ擾���m���ɒx�������邽�߂̃R���[�`�����J�n
                StartCoroutine(GetModelAndAttachHandler());

                Debug.Log($"Live2D model '{currentModelID}' displayed in NightState");
            }
            else
            {
                Debug.LogError("Live2DController not found");
            }
        }
    }

    // ���f���Q�Ƃ��擾���ăn���h���[���A�^�b�`����R���[�`��
    private IEnumerator GetModelAndAttachHandler()
    {
        // Live2D���f�������������܂ŏ����ҋ@
        yield return new WaitForSeconds(0.1f);

        // Live2D�R���g���[���[���猻�݂̃��f���𒼐ڎ擾
        GameObject modelObject = null;
        Live2DController controller = FindLive2DController();

        if (controller != null)
        {
            // �R���g���[���[���猻�݂̃��f���I�u�W�F�N�g���擾����V�������\�b�h���g�p
            modelObject = controller.GetModelObject(currentModelID);

            if (modelObject != null)
            {
                // �擾�������f����ۑ�
                activeLive2DModel = modelObject;

                // �^�b�`�n���h�����A�^�b�`�iStateData��null�łȂ��ALive2D�\�����L���ŁA�^�b�`�@�\���L���ȏꍇ�j
                if (stateData != null && stateData.showLive2DModel &&
                    stateData.live2DData != null && stateData.live2DData.enableTouch)
                {
                    SetupLive2DTouchHandler(activeLive2DModel);
                    Debug.Log($"Model object found and handler attached: {modelObject.name}");
                }
            }
            else
            {
                Debug.LogWarning("Could not find Live2D model object after delay");
            }
        }
    }

    // Live2DController���������Ď擾
    private Live2DController FindLive2DController()
    {
        // �܂���GameLoop����擾�����݂�
        if (GameLoop.Instance != null)
        {
            // 1. ��p��Live2DContainer������ꍇ
            GameObject container = GameObject.Find("Live2DContainer");
            if (container != null)
            {
                Live2DController controller = container.GetComponent<Live2DController>();
                if (controller != null) return controller;
            }

            // 2. �V�[����������
            return FindObjectOfType<Live2DController>();
        }

        return null;
    }

    // Live2D���f���̃N���[���A�b�v
    private void CleanupLive2DModel()
    {
        if (!string.IsNullOrEmpty(currentModelID))
        {
            // Live2DController���擾
            Live2DController live2DController = FindLive2DController();
            if (live2DController != null)
            {
                // ���f���̔�\��
                live2DController.HideModel(currentModelID);
            }

            // �Q�Ƃ��N���A
            activeLive2DModel = null;
            currentModelID = string.Empty;
        }
    }

    // �^�b�`�n���h���[���A�^�b�`���郁�\�b�h
    private void SetupLive2DTouchHandler(GameObject live2DModel)
    {
        if (live2DModel == null)
        {
            Debug.LogError("Cannot attach touch handler to null model");
            return;
        }

        // ���f���I�u�W�F�N�g�̏����ڍׂɃ��O�o��
        Debug.Log($"Setting up touch handler for model: {live2DModel.name}, " +
                  $"Active: {live2DModel.activeSelf}, " +
                  $"Path: {GetGameObjectPath(live2DModel)}");

        // ���ɃA�^�b�`���ꂽ�n���h���[���m���ɍ폜�i�d���h�~�j
        CharacterTouchHandler existingHandler = live2DModel.GetComponent<CharacterTouchHandler>();
        if (existingHandler != null)
        {
            Debug.Log("Removing existing touch handler");
            DestroyImmediate(existingHandler);
        }

        // �V�K�Ƀn���h���[���A�^�b�`
        CharacterTouchHandler touchHandler = live2DModel.AddComponent<CharacterTouchHandler>();

        // �����ɏ����������s
        if (touchHandler != null)
        {
            touchHandler.Initialize(currentModelID);
            Debug.Log("Touch handler successfully attached and initialized");
        }
        else
        {
            Debug.LogError("Failed to attach CharacterTouchHandler component");
        }
    }

    // �^�b�`�n���h���[�𖳌������郁�\�b�h
    private void DisableLive2DTouchHandler()
    {
        if (activeLive2DModel != null)
        {
            CharacterTouchHandler handler = activeLive2DModel.GetComponent<CharacterTouchHandler>();
            if (handler != null)
            {
                handler.DisableTouchDetection();
                Debug.Log("Live2D touch handler disabled in NightState");
            }
        }
    }

    // GameObject�K�w�p�X���擾����w���p�[���\�b�h�i�f�o�b�O�p�j
    private string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        while (obj.transform.parent != null)
        {
            obj = obj.transform.parent.gameObject;
            path = obj.name + "/" + path;
        }
        return path;
    }

    #endregion
}
