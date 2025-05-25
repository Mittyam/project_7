using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // �V�[���Ǘ��p�̖��O��Ԃ�ǉ�

public class SaveLoadUI : Singleton<SaveLoadUI>
{
    [Header("�X���b�g���j���[")]
    public GameObject slotPanel;

    [Header("����{�^��")]
    public Button closeButton;

    [Header("UI�ݒ�")]
    [SerializeField] private float planeDistance = 10f; // �L�����o�X�̃v���[������

    private Canvas slotPanelCanvas; // �X���b�g�p�l���̃L�����o�X�Q��
    private Camera mainCamera; // ���C���J�����̎Q��

    protected override void Awake()
    {
        base.Awake(); // �V���O���g���̏���������

        // ������Ԃł͗�����\��
        if (slotPanel != null) slotPanel.SetActive(false);

        // �L�����o�X�R���|�[�l���g���擾
        if (slotPanel != null)
        {
            slotPanelCanvas = slotPanel.GetComponent<Canvas>();
            if (slotPanelCanvas == null)
            {
                Debug.LogWarning("SaveLoadUI: �X���b�g�p�l����Canvas�R���|�[�l���g��������܂���");
            }
        }

        // �J�������Z�b�g�A�b�v����
        SetupCamera();
    }

    private void OnEnable()
    {
        // �V�[���ǂݍ��ݎ��̃C�x���g��o�^
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // �C�x���g�o�^����
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // ����{�^���̃N���b�N�C�x���g�Ƀ��\�b�h��o�^
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseAllPanels);
        }
    }

    private void Update()
    {
        // �G�X�P�[�v�L�[����������A�\�����̃p�l�������
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseAllPanels();
        }
    }

    // �V�[���ǂݍ��ݎ��̃C�x���g�n���h��
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"SaveLoadUI: �V�[�� '{scene.name}' ���ǂݍ��܂�܂����B�J�������Đݒ肵�܂�");
        // �V�[�����ς������J�������Đݒ�
        SetupCamera();
    }

    // �J�������Z�b�g�A�b�v����
    private void SetupCamera()
    {
        // ���C���J����������
        mainCamera = Camera.main;

        // �X���b�g�p�l���̃L�����o�X�����݂��A���C���J���������������ꍇ�ɐݒ�
        if (slotPanelCanvas != null && mainCamera != null)
        {
            slotPanelCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            slotPanelCanvas.worldCamera = mainCamera;
            slotPanelCanvas.planeDistance = planeDistance;
        }
        else if (slotPanelCanvas != null && mainCamera == null)
        {
            // �J������������Ȃ��ꍇ�͌x�����o���AScreenSpaceOverlay���[�h���g�p
            Debug.LogWarning("SaveLoadUI: MainCamera��������܂���B�����ScreenSpaceOverlay���[�h���g�p���܂��B");
            slotPanelCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }
    }

    // �Z�[�u�p�l�����J��
    public void OpenSavePanel()
    {
        // MainScene�̏ꍇ�A�Z�[�u���\�ȃX�e�[�g���`�F�b�N
        if (SceneManager.GetActiveScene().name == "MainScene" &&
            !StatusManager.Instance.CanSaveInCurrentState())
        {
            // �Z�[�u�s�\�ȃX�e�[�g�̏ꍇ�̓��b�Z�[�W��\��
            Debug.LogWarning("�Z�[�u�͒��܂��͖�̃X�e�[�g�ł̂݉\�ł��B");

            // �I�v�V�����F���[�U�[�ɒʒm���郁�b�Z�[�W��\��
            if (ConfirmationDialogManager.Instance != null)
            {
                ConfirmationDialogManager.Instance.ShowConfirmation(
                    "�Z�[�u�͒��܂��͖�̃X�e�[�g�ł̂݉\�ł��B",
                    "�ʒm",
                    null,
                    null
                );
            }
            return;
        }

        // �p�l�����J���O�ɃJ�������m�F
        if (slotPanelCanvas != null && slotPanelCanvas.worldCamera == null)
        {
            SetupCamera();
        }

        SaveSlotManager.Instance.isSaveMode = true;
        if (slotPanel != null) slotPanel.SetActive(true);
        SaveSlotManager.Instance.RefreshSlots();
    }

    // ���[�h�p�l�����J���iTitleScene�ł͏�ɊJ����AMainScene�ł͐����j
    public void OpenLoadPanel()
    {
        // MainScene�ŁA���[�h���\�ȃX�e�[�g���`�F�b�N
        if (SceneManager.GetActiveScene().name == "MainScene" &&
            !StatusManager.Instance.CanSaveInCurrentState())
        {
            // ���[�h�s�\�ȃX�e�[�g�̏ꍇ�̓��b�Z�[�W��\��
            Debug.LogWarning("���[�h�͒��܂��͖�̃X�e�[�g�ł̂݉\�ł��B");

            // �I�v�V�����F���[�U�[�ɒʒm���郁�b�Z�[�W��\��
            if (ConfirmationDialogManager.Instance != null)
            {
                ConfirmationDialogManager.Instance.ShowConfirmation(
                    "���[�h�͒��܂��͖�̃X�e�[�g�ł̂݉\�ł��B",
                    "�ʒm",
                    null,
                    null
                );
            }
            return;
        }

        // �p�l�����J���O�ɃJ�������m�F
        if (slotPanelCanvas != null && slotPanelCanvas.worldCamera == null)
        {
            SetupCamera();
        }

        SaveSlotManager.Instance.isSaveMode = false;
        if (slotPanel != null) slotPanel.SetActive(true);
        SaveSlotManager.Instance.RefreshSlots();
    }

    // ���ׂẴp�l�������
    public void CloseAllPanels()
    {
        if (slotPanel != null && slotPanel.activeSelf)
        {
            slotPanel.SetActive(false);
        }
    }
}