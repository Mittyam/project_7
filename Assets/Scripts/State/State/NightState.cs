using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static GameEvents;

// �����^�C�v��Enum�iLive2DController�Ƌ��L�j
public enum ClothingType
{
    Casual,     // ����
    Pajamas     // �p�W���}
}

/// <summary>
/// ��X�e�[�g
/// �E�X�e�[�^�X�X�V
/// �E��Ȃ�ł͂̃T�u�C�x���g�̔���Ȃ�
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

    [Header("Bath Button Settings")]
    [SerializeField] private string bathButtonTag = "BathButton"; // �����C�{�^���̃^�O
    [SerializeField] private int bathEventID = 7; // �����C�C�x���g��ID
    private GameObject bathButton; // �����C�{�^����GameObject�i���I�Ɏ擾�j

    [Header("Live2D Controller")]
    [SerializeField] private Live2DController live2DController; // Live2D���f���𐧌䂷��R���|�[�l���g

    // Live2D�֘A�t�B�[���h�ǉ�
    private GameObject activeLive2DModel; // �A�N�e�B�u��Live2D���f���ւ̎Q��
    private string currentModelID; // ���ݕ\�����̃��f��ID

    // UI������ԊǗ��p�t���O
    private bool isUICreated = false;

    // �����C�{�^���֘A�̃R���|�[�l���g�Q��
    private UIEventPublisher bathEventPublisher;
    private UISoundHandler bathSoundHandler;
    private Image bathButtonImage;


    public override void OnEnter()
    {
        Debug.Log("NightState�ɓ���܂��B");
        StatusManager.Instance.OnStateChanged();

        // StatusManager�ɕ�����ԊǗ���L����
        StatusManager.Instance.EnableClothingState();

        // UI�v�f�̃Z�b�g�A�b�v�ƕ\��
        SetupUI();
        ShowNightUI();

        // �����C�{�^���̏�Ԃ��X�V
        UpdateBathButtonState();

        // Index 2~4 ��BGM�������_���ɍĐ�
        int randomIndex = Random.Range(8, 11);
        SoundManager.Instance.PlayBGMWithFadeIn(randomIndex, 1f);

        // �C�x���g����
        nightStateEventSO.Raise();

        // Live2D���f�������������ĕ\��
        SetupLive2DModel();

        // StatusManager���畞����Ԃ��擾���ēK�p
        ApplyClothingFromStatusManager();

        // ProgressManager�̃C�x���g�X�V���Ď�
        ProgressManager.Instance.OnProgressUpdated += OnProgressUpdated;

        // StatusManager�̕����ύX�C�x���g���w��
        if (StatusManager.Instance != null)
        {
            StatusManager.Instance.OnClothingStateChanged += OnClothingStateChanged;
        }
    }

    public override void OnUpdate()
    {

    }

    public override void OnExit()
    {
        Debug.Log("NightState: �o�܂��B");

        // StatusManager�ɕ�����ԊǗ��𖳌����i�����Ƀ��Z�b�g�j
        StatusManager.Instance.DisableClothingState();

        // BGM���~
        SoundManager.Instance.StopBGM();

        // �^�b�`�n���h���[�𖳌���
        DisableLive2DTouchHandler();

        // Live2D���f���̃N���[���A�b�v
        CleanupLive2DModel();

        // UI�̔�\����
        HideAllUI();

        // �C�x���g�Ď�������
        ProgressManager.Instance.OnProgressUpdated -= OnProgressUpdated;

        // StatusManager�̕����ύX�C�x���g�̍w�ǉ���
        if (StatusManager.Instance != null)
        {
            StatusManager.Instance.OnClothingStateChanged -= OnClothingStateChanged;
        }

        // ���̓��ւ̑J�ڂȂ̂ŁA�����ł̃m�x���C�x���g�`�F�b�N
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

        // �����C�{�^���̏�Ԃ��X�V
        UpdateBathButtonState();

        // Index 2~4 ��BGM�������_���ɍĐ�
        int randomIndex = Random.Range(8, 11);
        SoundManager.Instance.PlayBGMWithFadeIn(randomIndex, 1f);

        // StatusManager���畞����Ԃ��擾���ēK�p�iBathState����߂����ꍇ�Ȃǁj
        ApplyClothingFromStatusManager();
    }

    // ���̃X�e�[�g�̎擾���\�b�h
    public StateID GetNextStateID()
    {
        return stateData != null ? stateData.nextStateID : StateID.Day;
    }

    #region --- �����C�{�^���֘A���\�b�h ---

    // �^�O���g�p���Ă����C�{�^��������
    private void FindBathButton()
    {
        // �^�O�ł����C�{�^��������
        bathButton = GameObject.FindWithTag(bathButtonTag);

        if (bathButton == null)
        {
            // �^�O�Ō�����Ȃ��ꍇ�́A�����҂��Ă���Ď��s����R���[�`�����J�n
            StartCoroutine(DelayedFindBathButton());
        }
        else
        {
            Debug.Log($"NightState: �����C�{�^���𔭌����܂��� - {bathButton.name}");
            // �R���|�[�l���g�Q�Ƃ�������
            InitializeBathButtonReferences();
        }
    }

    // �x�����Ă����C�{�^������������R���[�`��
    private IEnumerator DelayedFindBathButton()
    {
        float maxWaitTime = 2.0f; // �ő�ҋ@����
        float elapsedTime = 0f;
        float checkInterval = 0.1f; // �`�F�b�N�Ԋu

        while (elapsedTime < maxWaitTime)
        {
            yield return new WaitForSeconds(checkInterval);
            elapsedTime += checkInterval;

            bathButton = GameObject.FindWithTag(bathButtonTag);
            if (bathButton != null)
            {
                Debug.Log($"NightState: �����C�{�^����x�������Ŕ������܂��� - {bathButton.name}");
                InitializeBathButtonReferences();
                UpdateBathButtonState();
                yield break;
            }
        }

        Debug.LogWarning($"NightState: �^�O '{bathButtonTag}' �̂����C�{�^����������܂���ł���");
    }

    // �����C�{�^���̃R���|�[�l���g�Q�Ƃ��擾
    private void InitializeBathButtonReferences()
    {
        if (bathButton == null)
        {
            Debug.LogWarning("NightState: bathButton���ݒ肳��Ă��܂���");
            return;
        }

        // �e�R���|�[�l���g�̎Q�Ƃ��擾
        bathEventPublisher = bathButton.GetComponent<UIEventPublisher>();
        bathSoundHandler = bathButton.GetComponent<UISoundHandler>();

        // BathImage�Ƃ������O�̎q�I�u�W�F�N�g����Image�R���|�[�l���g���擾
        Transform bathImageTransform = bathButton.transform.Find("BathImage");
        if (bathImageTransform != null)
        {
            bathButtonImage = bathImageTransform.GetComponent<Image>();
        }
        else
        {
            // GetComponentInChildren�Ő[���K�w���܂߂Č���
            bathButtonImage = bathButton.GetComponentInChildren<Image>();
        }

        if (bathEventPublisher == null)
            Debug.LogWarning("NightState: UIEventPublisher��������܂���");
        if (bathSoundHandler == null)
            Debug.LogWarning("NightState: UISoundHandler��������܂���");
        if (bathButtonImage == null)
            Debug.LogWarning("NightState: Image�R���|�[�l���g��������܂���");
    }

    // �����C�{�^���̏�Ԃ��X�V
    private void UpdateBathButtonState()
    {
        // �����C�{�^�����܂��������Ă��Ȃ��ꍇ�͌��������݂�
        if (bathButton == null)
        {
            FindBathButton();
            return;
        }

        // �R���|�[�l���g�Q�Ƃ��������i�܂��擾���Ă��Ȃ��ꍇ�j
        if (bathEventPublisher == null || bathSoundHandler == null || bathButtonImage == null)
        {
            InitializeBathButtonReferences();
        }

        // ProgressManager����C�x���g7�̏�Ԃ��擾
        EventState eventState = ProgressManager.Instance.GetEventState(bathEventID);

        if (eventState == EventState.Completed)
        {
            // Completed�̏ꍇ�F�{�^����L����
            SetBathButtonCompleted();
        }
        else
        {
            // Complete�O�̏ꍇ�F�{�^���𖳌���
            SetBathButtonLocked();
        }

        Debug.Log($"NightState: �����C�{�^���̏�Ԃ��X�V - Event {bathEventID} : {eventState}");
    }

    // �����C�{�^����Completed��Ԃɐݒ�
    private void SetBathButtonCompleted()
    {
        // UIEventPublisher�ݒ�
        if (bathEventPublisher != null)
        {
            bathEventPublisher.SetProperties(
                UIEventPublisher.EventType.BathSelect,
                0, // itemID
                "", // itemName
                0, // actionPointCost
                "" // panelName
            );
        }

        // UISoundHandler�ݒ�
        if (bathSoundHandler != null)
        {
            bathSoundHandler.SetClickSoundType(UISoundHandler.SoundType.Minimal5);
            bathSoundHandler.SetHoverSoundType(UISoundHandler.SoundType.start); // �z�o�[�����ݒ�
        }

        // Image Color�ݒ�i���F�j
        if (bathButtonImage != null)
        {
            bathButtonImage.color = new Color(255f / 255f, 255f / 255f, 255f / 255f, 1f);
        }

        // �{�^���͏�ɗL���iinteractable�͕ύX���Ȃ��j
        Button buttonComponent = bathButton.GetComponent<Button>();
        if (buttonComponent != null)
        {
            buttonComponent.interactable = true;
        }
    }

    // �����C�{�^����Locked��Ԃɐݒ�
    private void SetBathButtonLocked()
    {
        // UIEventPublisher�ݒ�
        if (bathEventPublisher != null)
        {
            bathEventPublisher.SetProperties(
                UIEventPublisher.EventType.None,
                0, // itemID
                "", // itemName
                0, // actionPointCost
                "" // panelName
            );
        }

        // UISoundHandler�ݒ�
        if (bathSoundHandler != null)
        {
            bathSoundHandler.SetClickSoundType(UISoundHandler.SoundType.Cancel);
            bathSoundHandler.SetHoverSoundType(UISoundHandler.SoundType.start); // �z�o�[���͒ʏ�̉�
        }

        // Image Color�ݒ�i�O���[�j
        if (bathButtonImage != null)
        {
            bathButtonImage.color = new Color(180f / 255f, 180f / 255f, 180f / 255f, 1f);
        }

        // �{�^���͏�ɗL���ɂ��āA�����ڂ����O���[�ɂ���
        Button buttonComponent = bathButton.GetComponent<Button>();
        if (buttonComponent != null)
        {
            buttonComponent.interactable = true; // ���true�ɂ���
        }
    }

    // ProgressManager�̍X�V�C�x���g�n���h��
    private void OnProgressUpdated()
    {
        // �����C�{�^���̏�Ԃ��X�V
        UpdateBathButtonState();
    }

    // StatusManager���畞����Ԃ��擾���ēK�p
    private void ApplyClothingFromStatusManager()
    {
        if (StatusManager.Instance != null)
        {
            ClothingType currentClothing = StatusManager.Instance.GetClothingState();
            ApplyClothingToModel(currentClothing);
        }
    }

    // StatusManager�̕����ύX�C�x���g�n���h��
    private void OnClothingStateChanged(ClothingType newClothingType)
    {
        Debug.Log($"NightState: �����ύX�C�x���g����M - {newClothingType}");
        ApplyClothingToModel(newClothingType);
    }

    // ���f���ɕ�����K�p
    private void ApplyClothingToModel(ClothingType clothingType)
    {
        if (string.IsNullOrEmpty(currentModelID))
            return;

        if (live2DController != null)
        {
            // Live2DController�ɕ����ύX��ʒm
            live2DController.ApplyClothingToModel(currentModelID, clothingType);
            Debug.Log($"NightState: ���f���ɕ��� {clothingType} ��K�p���܂���");
        }
    }

    #endregion

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

        // UI���������ꂽ��A�����C�{�^��������
        FindBathButton();

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
            if (live2DController != null)
            {
                // ���f��ID��ۑ�
                currentModelID = live2DData.modelID;

                // Live2D���f����\��
                live2DController.ShowModel(
                    currentModelID,
                    live2DData.scale,
                    $"{live2DData.position.x},{live2DData.position.y}"
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

        if (live2DController != null)
        {
            // �R���g���[���[���猻�݂̃��f���I�u�W�F�N�g���擾����V�������\�b�h���g�p
            modelObject = live2DController.GetModelObject(currentModelID);

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

    // Live2D���f���̃N���[���A�b�v
    private void CleanupLive2DModel()
    {
        if (!string.IsNullOrEmpty(currentModelID))
        {
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

    // �^�b�`�n���h���[���A�^�b�`���郁�\�b�h - ������
    private void SetupLive2DTouchHandler(GameObject live2DModel)
    {
        if (live2DModel == null)
        {
            Debug.LogError("Cannot attach touch handler to null model");
            return;
        }

        // ���f���I�u�W�F�N�g�̏����ڍׂɃ��O�o��
        Debug.Log($"���f���̃^�b�`�n���h���[��ݒ�: {live2DModel.name}, " +
                  $"Active: {live2DModel.activeSelf}, " +
                  $"Path: {GetGameObjectPath(live2DModel)}");

        // ���ɃA�^�b�`���ꂽ�n���h���[���m���ɍ폜�i�d���h�~�j
        CharacterTouchHandler existingHandler = live2DModel.GetComponent<CharacterTouchHandler>();
        if (existingHandler != null)
        {
            DestroyImmediate(existingHandler);
        }

        // �V�K�Ƀn���h���[���A�^�b�`
        CharacterTouchHandler touchHandler = live2DModel.AddComponent<CharacterTouchHandler>();

        // �����ɏ����������s�i�R���[�`���̃^�C�~���O�Ɉˑ����Ȃ��j
        if (touchHandler != null)
        {
            touchHandler.Initialize(currentModelID);
            // Debug.Log("Touch handler successfully attached and initialized");
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