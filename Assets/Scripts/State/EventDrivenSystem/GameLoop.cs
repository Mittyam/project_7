using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // �V�[���Ǘ��p�ɒǉ�

public class GameLoop : Singleton<GameLoop>
{
    [Header("UI�R���e�i")]
    [SerializeField] private StatesContainer statesContainer;

    [Header("�C�x���g�i�s�V�X�e��")]
    [SerializeField] private PushdownStateMachine pushdownStack;
    [SerializeField] private MainStateMachine mainStateMachine;
    [SerializeField] private NovelEventScheduler novelEventScheduler;

    [Header("UI Settings")]
    [SerializeField] private Camera mainUICamera; // UI�\���p���C���J����

    // �v���p�e�B��`
    public PushdownStateMachine PushdownStack => pushdownStack;
    public MainStateMachine MainStateMachine => mainStateMachine;
    public NovelEventScheduler NovelEventScheduler => novelEventScheduler;
    public StatesContainer StatesContainer => statesContainer;
    public Camera MainUICamera => mainUICamera;

    // �V�[���Ǘ��p�̕ϐ�
    private string currentSceneName;
    private bool isInitialized = false;

    // �N���X�̃t�B�[���h�ɒǉ�
    private static bool isLoadedFromTitle = false;
    private static StateID loadedStateID = StateID.None;

    private void Awake()
    {
        base.Awake();

        // �V�[���J�ڃC�x���g���X�i�[��o�^
        SceneManager.sceneLoaded += OnSceneLoaded;

        // ���݂̃V�[�������L�^
        currentSceneName = SceneManager.GetActiveScene().name;

        // ���C���V�[���ȊO�ł͏������X�L�b�v
        if (currentSceneName != "MainScene" || !isInitialized)
        {
            return;
        }

        // StatesContainer�̎擾�i�����̃R�[�h�j
        if (statesContainer == null)
        {
            statesContainer = FindObjectOfType<StatesContainer>();
            if (statesContainer == null)
            {
                Debug.LogError("GameLoop: StatesContainer��������܂���");
            }
        }

        if (pushdownStack == null)
        {
            pushdownStack = FindObjectOfType<PushdownStateMachine>();
            if (pushdownStack == null)
            {
                Debug.LogError("GameLoop: StatesContainer��������܂���");
            }
        }

        if (mainStateMachine == null)
        {
            mainStateMachine = FindObjectOfType<MainStateMachine>();
            if (mainStateMachine == null)
            {
                Debug.LogError("GameLoop: StatesContainer��������܂���");
            }
        }

        if (novelEventScheduler == null)
        {
            novelEventScheduler = FindObjectOfType<NovelEventScheduler>();
            if (novelEventScheduler == null)
            {
                Debug.LogError("GameLoop: NovelEventScheduler��������܂���");
            }
        }

        // �������̊m�F
        if (statesContainer == null || pushdownStack == null ||
            mainStateMachine == null || novelEventScheduler == null)
        {
            Debug.LogError("GameLoop: �K�v�ȃR���|�[�l���g���ݒ肳��Ă��܂���");
        }

        // ���C���V�[���Ȃ珉���������s
        if (currentSceneName == "MainScene")
        {
            InitializeGameComponents();
        }
    }

    // �V�[���ǂݍ��݊������ɌĂ΂��C�x���g�n���h��
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"GameLoop: �V�[���u{scene.name}�v���ǂݍ��܂�܂���");

        // �V�[�������ς�����ꍇ�̂ݏ���
        if (currentSceneName != scene.name)
        {
            currentSceneName = scene.name;

            // MainScene�ɑJ�ڂ����ꍇ
            if (scene.name == "MainScene")
            {
                Debug.Log("GameLoop: MainScene�ɑJ�ڂ��܂����B�K�v�ȃR���|�[�l���g���Ď擾���܂��B");
                InitializeGameComponents();

                // �V�K�Q�[���J�n�`�F�b�N�������Ŏ��s
                CheckAndStartNewGame();
            }
        }

        currentSceneName = SceneManager.GetActiveScene().name;
    }

    // �^�C�g�����烍�[�h���ꂽ���Ƃ��L�^���郁�\�b�h��ǉ�
    public static void SetLoadedStateFromTitle(StateID stateID)
    {
        isLoadedFromTitle = true;
        loadedStateID = stateID;
        Debug.Log($"GameLoop: �^�C�g������̃��[�h��Ԃ��L�^ - StateID: {stateID}");
    }

    // �Q�[���R���|�[�l���g�̏������i���C���V�[����p�j
    private void InitializeGameComponents()
    {
        Debug.Log("GameLoop: �Q�[���R���|�[�l���g�̏��������J�n���܂�");

        // StatesContainer�̍Ď擾
        if (statesContainer == null)
        {
            // �܂��̓I�u�W�F�N�g���Ō���
            GameObject statesContainerObj = GameObject.Find("StatesContainer");
            if (statesContainerObj != null)
            {
                statesContainer = statesContainerObj.GetComponent<StatesContainer>();
                Debug.Log("GameLoop: StatesContainer���I�u�W�F�N�g���ōĎ擾���܂���");
            }
            else
            {
                // �I�u�W�F�N�g���Ō�����Ȃ��ꍇ�͌^�Ō���
                statesContainer = FindObjectOfType<StatesContainer>();
                if (statesContainer != null)
                {
                    Debug.Log("GameLoop: StatesContainer���^�ōĎ擾���܂���");
                }
                else
                {
                    Debug.LogError("GameLoop: StatesContainer��������܂���");
                }
            }
        }

        // PushdownStack�̍Ď擾
        if (pushdownStack == null)
        {
            // �܂��̓I�u�W�F�N�g���Ō���
            GameObject pushdownStackObj = GameObject.Find("PushdownStateMachine");
            if (pushdownStackObj != null)
            {
                pushdownStack = pushdownStackObj.GetComponent<PushdownStateMachine>();
                Debug.Log("GameLoop: PushdownStateMachine���I�u�W�F�N�g���ōĎ擾���܂���");
            }
            else
            {
                // �I�u�W�F�N�g���Ō�����Ȃ��ꍇ�͌^�Ō���
                pushdownStack = FindObjectOfType<PushdownStateMachine>();
                if (pushdownStack != null)
                {
                    Debug.Log("GameLoop: PushdownStateMachine���^�ōĎ擾���܂���");
                }
                else
                {
                    Debug.LogError("GameLoop: PushdownStateMachine��������܂���");
                }
            }
        }

        // MainStateMachine�̍Ď擾
        if (mainStateMachine == null)
        {
            // �܂��̓I�u�W�F�N�g���Ō���
            GameObject mainStateMachineObj = GameObject.Find("MainStateMachine");
            if (mainStateMachineObj != null)
            {
                mainStateMachine = mainStateMachineObj.GetComponent<MainStateMachine>();
                Debug.Log("GameLoop: MainStateMachine���I�u�W�F�N�g���ōĎ擾���܂���");
            }
            else
            {
                // �I�u�W�F�N�g���Ō�����Ȃ��ꍇ�͌^�Ō���
                mainStateMachine = FindObjectOfType<MainStateMachine>();
                if (mainStateMachine != null)
                {
                    Debug.Log("GameLoop: MainStateMachine���^�ōĎ擾���܂���");
                }
                else
                {
                    Debug.LogError("GameLoop: MainStateMachine��������܂���");
                }
            }
        }

        // NovelEventScheduler�̍Ď擾
        if (novelEventScheduler == null)
        {
            // �܂��̓I�u�W�F�N�g���Ō���
            GameObject novelEventSchedulerObj = GameObject.Find("NovelEventScheduler");
            if (novelEventSchedulerObj != null)
            {
                novelEventScheduler = novelEventSchedulerObj.GetComponent<NovelEventScheduler>();
                Debug.Log("GameLoop: NovelEventScheduler���I�u�W�F�N�g���ōĎ擾���܂���");
            }
            else
            {
                // �I�u�W�F�N�g���Ō�����Ȃ��ꍇ�͌^�Ō���
                novelEventScheduler = FindObjectOfType<NovelEventScheduler>();
                if (novelEventScheduler != null)
                {
                    Debug.Log("GameLoop: NovelEventScheduler���^�ōĎ擾���܂���");
                }
                else
                {
                    Debug.LogError("GameLoop: NovelEventScheduler��������܂���");
                }
            }
        }

        // MainCamera�̍Ď擾
        if (mainUICamera == null)
        {
            // �J�����̌����i���O�Ō����j
            GameObject mainCameraObj = GameObject.Find("MainCamera");
            if (mainCameraObj != null)
            {
                mainUICamera = mainCameraObj.GetComponent<Camera>();
            }
            else
            {
                // Tag�Ō���
                mainCameraObj = GameObject.FindWithTag("MainCamera");
                if (mainCameraObj != null)
                {
                    mainUICamera = mainCameraObj.GetComponent<Camera>();
                }
                else
                {
                    Debug.LogWarning("GameLoop: MainCamera��������܂���");
                }
            }
        }

        if (mainStateMachine != null && !isInitialized && statesContainer != null)
        {
            isInitialized = true;

            // ���[�h���ꂽ�f�[�^������ꍇ�͂��̃X�e�[�g�ŏ�����
            StateID initialStateID = StateID.Day; // �f�t�H���g��Day

            if (isLoadedFromTitle && loadedStateID != StateID.None)
            {
                initialStateID = loadedStateID;
                Debug.Log($"GameLoop: �^�C�g�����烍�[�h���ꂽ�X�e�[�g {loadedStateID} �ŏ��������܂�");

                // �t���O�����Z�b�g
                isLoadedFromTitle = false;
                loadedStateID = StateID.None;
            }
            else if (StatusManager.Instance != null && StatusManager.Instance.GetStatus() != null)
            {
                // StatusManager�ɕۑ����ꂽ�X�e�[�gID������΂�����g�p
                var savedState = StatusManager.Instance.GetStatus().savedStateID;
                if (savedState != StateID.None)
                {
                    initialStateID = savedState;
                    Debug.Log($"GameLoop: �ۑ����ꂽ�X�e�[�g {savedState} �ŏ��������܂�");
                }
            }

            mainStateMachine.Initialize(initialStateID);

            // UI�J�������擾�ł��Ă���Ίe�X�e�[�g�ɐݒ�
            if (mainUICamera != null)
            {
                SetUICameraToAllStates();
            }
        }
    }

    private void Start()
    {
        // �^�C�g���V�[���̏ꍇ�͂����ŏ��������Ȃ��i���C���V�[���ւ̑J�ڎ��ɍs���j
        if (currentSceneName != "MainScene")
        {
            Debug.Log($"GameLoop: ����{currentSceneName}�V�[���ł��B���C���V�[���J�ڎ��ɏ��������s���܂��B");
            return;
        }

        // ���ɏ������ς݂Ȃ�X�L�b�v
        if (isInitialized)
        {
            return;
        }

        // ���C���V�[���ł̏���������
        // ���C���X�e�[�g�̏������ƊJ�n
        mainStateMachine.Initialize(StateID.Day);
        isInitialized = true;

        // �e�X�e�[�g��UI�J������ݒ�
        SetUICameraToAllStates();

        // �V�K�Q�[���J�n�`�F�b�N
        CheckAndStartNewGame();
    }

    // �V�������\�b�h��ǉ�
    private void CheckAndStartNewGame()
    {
        // PlayerPrefs����V�K�Q�[���J�n�t���O���擾
        int isNewGameStart = PlayerPrefs.GetInt("IsNewGameStart", 0);

        if (isNewGameStart == 1 && currentSceneName == "MainScene")
        {
            // �t���O�����Z�b�g
            PlayerPrefs.SetInt("IsNewGameStart", 0);
            PlayerPrefs.Save();

            // �����x�����Ă���ŏ��̃C�x���g���J�n�i����������������̂�҂j
            StartCoroutine(StartFirstNovelEvent());
        }
    }

    // �ŏ���NovelEvent���J�n����R���[�`����ǉ�
    private IEnumerator StartFirstNovelEvent()
    {
        // ����������������܂őҋ@
        yield return new WaitForEndOfFrame();

        // EventID = 1�̃C�x���g�f�[�^���擾
        NovelEventData firstEvent = Resources.Load<NovelEventData>("Events/1.�o�");

        if (firstEvent == null)
        {
            Debug.LogError("GameLoop: �ŏ��̃C�x���g(EventID=1)��������܂���");
            yield break;
        }

        // NovelEventScheduler���g���Ď蓮�ŃC�x���g���v�b�V��
        if (novelEventScheduler != null)
        {
            novelEventScheduler.PushEvent(firstEvent);
            Debug.Log("GameLoop: �ŏ���NovelEvent���J�n���܂���");
        }
        else
        {
            Debug.LogError("GameLoop: NovelEventScheduler��������܂���");
        }
    }

    private void Update()
    {
        // ���C���V�[���ȊO�ł͏������X�L�b�v
        if (currentSceneName != "MainScene" || !isInitialized)
        {
            return;
        }

        // 1. �X�^�b�N������΂܂��͂�������s�i�D��j
        if (!pushdownStack.IsEmpty)
        {
            // MainStateMachine�̃X�e�[�g���A�N�e�B�u���inull�`�F�b�N�ǉ��j
            if (mainStateMachine != null &&
                mainStateMachine.CurrentState != null &&
                mainStateMachine.CurrentState.gameObject != null &&
                mainStateMachine.CurrentState.gameObject.activeSelf)
            {
                mainStateMachine.CurrentState.gameObject.SetActive(false);
            }

            pushdownStack.Update();
        }
        // 2. �X�^�b�N����Ȃ�m�x���C�x���g���`�F�b�N
        else if (novelEventScheduler != null && novelEventScheduler.CheckAndPushIfNeeded())
        {
            // CheckAndPushIfNeeded����Push�����s�ς�
            Debug.Log("GameLoop: �m�x���C�x���g���J�n���܂���");
        }
        // 3. �����Ȃ���΃��C���X�e�[�g���X�V
        else
        {
            // MainStateMachine��CurrentState�̑��݂��m�F
            if (mainStateMachine != null && mainStateMachine.CurrentState != null)
            {
                // MainStateMachine�̃X�e�[�g���A�N�e�B�u�łȂ���΃A�N�e�B�u��
                if (mainStateMachine.CurrentState.gameObject != null &&
                    !mainStateMachine.CurrentState.gameObject.activeSelf)
                {
                    mainStateMachine.CurrentState.gameObject.SetActive(true);
                }

                mainStateMachine.Update();
            }
        }
    }

    /// <summary>
    /// �~�j�C�x���g���N�����郁�\�b�h
    /// </summary>
    /// <param name="stateID"></param>
    /// <param name="parameters"></param>
    public void PushMiniEvent(StateID stateID, Dictionary<string, object> parameters = null)
    {
        // �����̃R�[�h�i�ύX�Ȃ��j
        IState miniEventState = statesContainer.GetMiniEventState(stateID);

        if (miniEventState != null)
        {
            // ActionType���K�v��StateID�̏ꍇ�AStateID�Ɋ�Â���ActionType�������ݒ�
            if (parameters == null)
            {
                parameters = new Dictionary<string, object>();
            }

            if (stateID == StateID.Library && !parameters.ContainsKey("ActionType"))
            {
                parameters["ActionType"] = "Library";
            }
            else if (stateID == StateID.Cafe && !parameters.ContainsKey("ActionType"))
            {
                parameters["ActionType"] = "Cafe";
            }
            else if (stateID == StateID.PartJob && !parameters.ContainsKey("ActionType"))
            {
                parameters["ActionType"] = "Work";
            }
            else if (stateID == StateID.Walk && !parameters.ContainsKey("ActionType"))
            {
                parameters["ActionType"] = "Walk";
            }
            else if (stateID == StateID.Game && !parameters.ContainsKey("ActionType"))
            {
                parameters["ActionType"] = "Game";
            }
            else if (stateID == StateID.Outing && !parameters.ContainsKey("ActionType"))
            {
                parameters["ActionType"] = "Outing";
            }
            else if (stateID == StateID.Talk && !parameters.ContainsKey("ActionType"))
            {
                parameters["ActionType"] = "Talk";
            }
            else if (stateID == StateID.Sleep && !parameters.ContainsKey("ActionType"))
            {
                parameters["ActionType"] = "Sleep";
            }

            // �p�����[�^������ꍇ�͐ݒ�
            if (parameters != null && miniEventState is MiniEventState miniEvent)
            {
                miniEvent.SetParameters(parameters);
            }

            // PushdownStack�Ƀv�b�V��
            pushdownStack.Push(miniEventState);

            Debug.Log($"GameLoop: �~�j�C�x���g {stateID} ���v�b�V�����܂���");
        }
        else
        {
            Debug.LogError($"GameLoop: �~�j�C�x���g {stateID} ��������܂���");
        }
    }

    // �S�X�e�[�g��UI�J������ݒ�
    private void SetUICameraToAllStates()
    {
        if (mainUICamera == null)
        {
            Debug.LogWarning("MainUICamera is not assigned in GameLoop");
            return;
        }

        if (statesContainer == null)
        {
            Debug.LogWarning("StatesContainer is not assigned in GameLoop");
            return;
        }

        // MainState�ɃJ������ݒ�
        SetUICameraToState(statesContainer.GetMainState(StateID.Day) as StateBase);
        SetUICameraToState(statesContainer.GetMainState(StateID.Evening) as StateBase);
        SetUICameraToState(statesContainer.GetMainState(StateID.Night) as StateBase);
    }

    // �ʃX�e�[�g�ւ̃J�����ݒ�w���p�[���\�b�h
    private void SetUICameraToState(StateBase state)
    {
        if (state != null)
        {
            // ���t���N�V�������g�p����uiRenderCamera�t�B�[���h��ݒ�
            var field = typeof(StateBase).GetField("uiRenderCamera",
                             System.Reflection.BindingFlags.Instance |
                             System.Reflection.BindingFlags.NonPublic);

            if (field != null)
            {
                field.SetValue(state, mainUICamera);
                // Debug.Log($"Set UI camera to {state.GetType().Name}");
            }
        }
    }
}