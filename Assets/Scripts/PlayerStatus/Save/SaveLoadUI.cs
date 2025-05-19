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

            Debug.Log($"SaveLoadUI: Canvas {slotPanel.name} �̃����_�[���[�h��ScreenSpaceCamera�A�J������ {mainCamera.name} �ɐݒ肵�܂���");
        }
        else if (slotPanelCanvas != null && mainCamera == null)
        {
            // �J������������Ȃ��ꍇ�͌x�����o���AScreenSpaceOverlay���[�h���g�p
            Debug.LogWarning("SaveLoadUI: MainCamera��������܂���B�����ScreenSpaceOverlay���[�h���g�p���܂��B");
            slotPanelCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }
    }

    // ���ڃZ�[�u�p�l�����J��
    public void OpenSavePanel()
    {
        // �p�l�����J���O�ɃJ�������m�F�i�O�̂��߁j
        if (slotPanelCanvas != null && slotPanelCanvas.worldCamera == null)
        {
            SetupCamera();
        }

        SaveSlotManager.Instance.isSaveMode = true;
        if (slotPanel != null) slotPanel.SetActive(true);
        SaveSlotManager.Instance.RefreshSlots();
    }

    // ���ڃ��[�h�p�l�����J��
    public void OpenLoadPanel()
    {
        // �p�l�����J���O�ɃJ�������m�F�i�O�̂��߁j
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