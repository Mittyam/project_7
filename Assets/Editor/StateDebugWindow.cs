using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class StateDebugWindow : EditorWindow
{
    private Vector2 scrollPosition;
    private GameLoop gameLoop;
    private StatesContainer statesContainer;
    private MainStateMachine mainStateMachine;
    private PushdownStateMachine pushdownStack;

    // �Z�[�u�E���[�h�֘A�̕ϐ�
    private string saveSlotName = "default"; // �f�t�H���g�̃Z�[�u�X���b�g��
    private StatusManager statusManager; // StatusManager�̎Q��
    private Vector2 saveListScrollPosition; // �Z�[�u���X�g�p�̃X�N���[���ʒu
    private bool showSaveLoadSection = true; // �Z�[�u�E���[�h�Z�N�V�����̐܂肽���ݏ��

    // ProgressManager�֘A�̕ϐ�
    private bool showProgressSection = true; // ProgressManager�Z�N�V�����̐܂肽���ݏ��
    private Vector2 progressScrollPosition; // �C�x���g���X�g�p�̃X�N���[���ʒu
    private Dictionary<int, string> eventNames = new Dictionary<int, string>(); // �C�x���gID to ���O�̃}�b�s���O

    // ���C���X�e�[�g�\���p
    private Dictionary<StateID, string> stateDisplayNames = new Dictionary<StateID, string>()
    {
        { StateID.Day, "��" },
        { StateID.Evening, "�[��" },
        { StateID.Night, "��" }
    };

    // �~�j�C�x���g�\���p
    private Dictionary<StateID, string> miniEventDisplayNames = new Dictionary<StateID, string>()
    {
        { StateID.Talk, "��b" },
        { StateID.Outing, "���o����" },
        { StateID.Memory, "�v���o" },
        { StateID.Library, "�}����" },
        { StateID.Cafe, "�J�t�F" },
        { StateID.PartJob, "�o�C�g" },
        { StateID.Walk, "�U��" },
        { StateID.Game, "�Q�[��" },
        { StateID.Bath, "�����C" },
        { StateID.Touch, "���G��" },
        { StateID.item, "�A�C�e��" },
        { StateID.Sleep, "����" }
    };

    [MenuItem("Tools/State Debug Window")]
    public static void ShowWindow()
    {
        GetWindow<StateDebugWindow>("�X�e�[�g�f�o�b�O");
    }

    private void OnGUI()
    {
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("�Q�[���v���C���̂ݎg�p�ł��܂��B", MessageType.Info);
            return;
        }

        // �Q�Ƃ̎擾�iStatusManager��ǉ��j
        if (gameLoop == null)
            gameLoop = FindObjectOfType<GameLoop>();
        if (gameLoop == null)
        {
            EditorGUILayout.HelpBox("GameLoop��������܂���B", MessageType.Error);
            return;
        }

        if (mainStateMachine == null)
            mainStateMachine = gameLoop.MainStateMachine;
        if (pushdownStack == null)
            pushdownStack = gameLoop.PushdownStack;
        if (statesContainer == null)
            statesContainer = gameLoop.StatesContainer;
        if (statusManager == null)
            statusManager = FindObjectOfType<StatusManager>();

        // �Z�N�V�����F���݂̏��
        GUILayout.Label("���݂̏��", EditorStyles.boldLabel);
        GUILayout.BeginVertical("box");

        // ���C���X�e�[�g���
        string currentMainStateName = mainStateMachine.CurrentStateID.ToString();
        if (stateDisplayNames.TryGetValue(mainStateMachine.CurrentStateID, out string displayName))
            currentMainStateName = displayName;

        EditorGUILayout.LabelField("���C���X�e�[�g:", currentMainStateName);

        // PushdownStack���
        bool isStackEmpty = pushdownStack.IsEmpty;
        EditorGUILayout.LabelField("�X�^�b�N���:", isStackEmpty ? "��" : "�C�x���g���s��");
        if (!isStackEmpty && pushdownStack.CurrentState != null)
        {
            // �^������擾
            string stateName = pushdownStack.CurrentState.GetType().Name;
            EditorGUILayout.LabelField("���s���X�e�[�g:", stateName);
        }

        GUILayout.EndVertical();

        // �Z�N�V�����F���C���X�e�[�g�؂�ւ�
        GUILayout.Space(10);
        GUILayout.Label("���C���X�e�[�g�؂�ւ�", EditorStyles.boldLabel);
        GUILayout.BeginVertical("box");

        // �e���C���X�e�[�g�ւ̐؂�ւ��{�^��
        GUILayout.BeginHorizontal();
        foreach (var state in stateDisplayNames)
        {
            // ���݂̃X�e�[�g�͋����\��
            bool isCurrentState = mainStateMachine.CurrentStateID == state.Key;
            GUI.enabled = !isCurrentState && isStackEmpty; // �X�^�b�N���s���͖�����

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            if (isCurrentState)
            {
                buttonStyle.normal.textColor = Color.green;
                buttonStyle.fontStyle = FontStyle.Bold;
            }

            if (GUILayout.Button(state.Value, buttonStyle, GUILayout.Height(30)))
            {
                mainStateMachine.ChangeState(state.Key);
            }
        }
        GUILayout.EndHorizontal();

        // ���̃X�e�[�g�փ{�^��
        GUI.enabled = isStackEmpty;
        if (GUILayout.Button("���̃X�e�[�g�֐i��", GUILayout.Height(30)))
        {
            mainStateMachine.AdvanceToNextState();
        }
        GUI.enabled = true;

        GUILayout.EndVertical();

        // �Z�N�V�����F�~�j�C�x���g�N��
        GUILayout.Space(10);
        GUILayout.Label("�~�j�C�x���g�N��", EditorStyles.boldLabel);
        GUILayout.BeginVertical("box");

        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(150));

        foreach (var miniEvent in miniEventDisplayNames)
        {
            if (GUILayout.Button(miniEvent.Value + "���N��", GUILayout.Height(25)))
            {
                gameLoop.PushMiniEvent(miniEvent.Key);
            }
        }

        GUILayout.EndScrollView();

        // PushdownStack�̑���
        GUI.enabled = !isStackEmpty;
        if (GUILayout.Button("���݂̃C�x���g���I��", GUILayout.Height(30)))
        {
            pushdownStack.Pop();
        }
        GUI.enabled = true;

        GUILayout.EndVertical();

        // �X�V�{�^��
        GUILayout.Space(10);
        if (GUILayout.Button("�E�B���h�E�X�V", GUILayout.Height(25)))
        {
            Repaint();
        }

        // �����X�V�i���t���[���j
        if (Application.isPlaying)
        {
            Repaint();
        }

        // �Z�[�u�E���[�h�Z�N�V������ǉ�
        DrawSaveLoadSection();

        // ProgressManager�Z�N�V������ǉ�
        DrawProgressManagerSection();
    }

    // �Z�[�u�E���[�h�Z�N�V�����̕`��
    private void DrawSaveLoadSection()
    {
        GUILayout.Space(10);

        // �܂肽���݃w�b�_�[
        showSaveLoadSection = EditorGUILayout.Foldout(showSaveLoadSection, "�Z�[�u�E���[�h�@�\", true, EditorStyles.foldoutHeader);

        if (!showSaveLoadSection)
            return;

        GUILayout.BeginVertical("box");

        // StatusManager�̑��݊m�F
        if (statusManager == null)
        {
            EditorGUILayout.HelpBox("StatusManager��������܂���B", MessageType.Error);
            GUILayout.EndVertical();
            return;
        }

        // �Z�[�u�X���b�g���̓���
        GUILayout.BeginHorizontal();
        GUILayout.Label("�Z�[�u�X���b�g��:", GUILayout.Width(100));
        saveSlotName = EditorGUILayout.TextField(saveSlotName, GUILayout.Width(150));
        GUILayout.EndHorizontal();

        // �Z�[�u�E���[�h�E�폜�{�^��
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("�Z�[�u", GUILayout.Height(30)))
        {
            SaveGame();
        }
        if (GUILayout.Button("���[�h", GUILayout.Height(30)))
        {
            LoadGame();
        }
        if (GUILayout.Button("�폜", GUILayout.Height(30)))
        {
            DeleteSaveData();
        }
        GUILayout.EndHorizontal();

        // ���݂̃Z�[�u�f�[�^�̈ꗗ�\��
        DrawSaveSlotList();

        GUILayout.EndVertical();
    }

    // �Z�[�u�X���b�g�ꗗ�̕\��
    private void DrawSaveSlotList()
    {
        GUILayout.Space(5);
        GUILayout.Label("���p�\�ȃZ�[�u�f�[�^:", EditorStyles.boldLabel);

        // Easy Save 3�̃t�@�C�������K���ɍ��킹���p�X���\�z
        string saveDirectory = Application.persistentDataPath;
        if (Directory.Exists(saveDirectory))
        {
            // ES3�̃t�@�C��������
            string[] files = Directory.GetFiles(saveDirectory, "*.es3");

            if (files.Length == 0)
            {
                EditorGUILayout.HelpBox("�Z�[�u�f�[�^�����݂��܂���B", MessageType.Info);
                return;
            }

            // �X�N���[���r���[�ňꗗ�\��
            saveListScrollPosition = EditorGUILayout.BeginScrollView(saveListScrollPosition, GUILayout.Height(150));

            foreach (string file in files)
            {
                // �t�@�C��������X���b�g�����擾
                string fileName = Path.GetFileNameWithoutExtension(file);

                GUILayout.BeginHorizontal("box");

                // �X���b�g���ƃt�@�C���T�C�Y��\��
                FileInfo fileInfo = new FileInfo(file);
                string fileSize = (fileInfo.Length / 1024f).ToString("F2") + " KB";
                GUILayout.Label(fileName, GUILayout.ExpandWidth(true));
                GUILayout.Label(fileSize, GUILayout.Width(70));

                // �X���b�g����{�^��
                if (GUILayout.Button("�I��", GUILayout.Width(60)))
                {
                    saveSlotName = fileName;
                }

                if (GUILayout.Button("���[�h", GUILayout.Width(60)))
                {
                    saveSlotName = fileName;
                    LoadGame();
                }

                if (GUILayout.Button("�폜", GUILayout.Width(60)))
                {
                    saveSlotName = fileName;
                    DeleteSaveData();
                }

                GUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }
        else
        {
            EditorGUILayout.HelpBox("�Z�[�u�f�B���N�g�������݂��܂���B", MessageType.Warning);
        }
    }

    // �Z�[�u����
    private void SaveGame()
    {
        if (string.IsNullOrEmpty(saveSlotName))
        {
            Debug.LogError("�Z�[�u�X���b�g������ł��B");
            return;
        }

        // ���Ɩ�̃X�e�[�g�ł̂݃Z�[�u�\�ɂ���
        if (!statusManager.CanSaveInCurrentState())
        {
            string message = "�Z�[�u�͒��܂��͖�̃X�e�[�g�ł̂݉\�ł��B";
            Debug.LogWarning(message);
            EditorUtility.DisplayDialog("�Z�[�u�s��", message, "OK");
            return;
        }

        statusManager.SaveStatus(saveSlotName);
        Debug.Log($"�Q�[�����X���b�g�u{saveSlotName}�v�ɃZ�[�u���܂����B");
        EditorUtility.DisplayDialog("�Z�[�u����", $"�Q�[�����X���b�g�u{saveSlotName}�v�ɃZ�[�u���܂����B", "OK");
    }

    // ���[�h����
    private void LoadGame()
    {
        if (string.IsNullOrEmpty(saveSlotName))
        {
            Debug.LogError("�Z�[�u�X���b�g������ł��B");
            return;
        }

        // ES3�Ńt�@�C���̑��݊m�F
        if (ES3.FileExists(saveSlotName))
        {
            statusManager.LoadStatus(saveSlotName);
            Debug.Log($"�X���b�g�u{saveSlotName}�v����Q�[�������[�h���܂����B");
            EditorUtility.DisplayDialog("���[�h����", $"�X���b�g�u{saveSlotName}�v����Q�[�������[�h���܂����B", "OK");
        }
        else
        {
            Debug.LogError($"�X���b�g�u{saveSlotName}�v�̃Z�[�u�f�[�^��������܂���B");
            EditorUtility.DisplayDialog("�G���[", $"�X���b�g�u{saveSlotName}�v�̃Z�[�u�f�[�^��������܂���B", "OK");
        }
    }

    // �Z�[�u�f�[�^�폜����
    private void DeleteSaveData()
    {
        if (string.IsNullOrEmpty(saveSlotName))
        {
            Debug.LogError("�Z�[�u�X���b�g������ł��B");
            return;
        }

        // �m�F�_�C�A���O
        if (EditorUtility.DisplayDialog("�Z�[�u�f�[�^�̍폜",
            $"�X���b�g�u{saveSlotName}�v�̃Z�[�u�f�[�^���폜���܂����H",
            "�폜", "�L�����Z��"))
        {
            // ES3���g�p���ăt�@�C�����폜
            if (ES3.FileExists(saveSlotName))
            {
                ES3.DeleteFile(saveSlotName);
                Debug.Log($"�X���b�g�u{saveSlotName}�v�̃Z�[�u�f�[�^���폜���܂����B");
                EditorUtility.DisplayDialog("�폜����", $"�X���b�g�u{saveSlotName}�v�̃Z�[�u�f�[�^���폜���܂����B", "OK");
            }
            else
            {
                Debug.LogWarning($"�X���b�g�u{saveSlotName}�v�̃Z�[�u�f�[�^��������܂���B");
                EditorUtility.DisplayDialog("�G���[", $"�X���b�g�u{saveSlotName}�v�̃Z�[�u�f�[�^��������܂���B", "OK");
            }
        }
    }

    // ProgressManager�Z�N�V�����̕`��
    private void DrawProgressManagerSection()
    {
        GUILayout.Space(10);

        // �܂肽���݃w�b�_�[
        showProgressSection = EditorGUILayout.Foldout(showProgressSection, "�C�x���g�i�s�Ǘ�", true, EditorStyles.foldoutHeader);

        if (!showProgressSection)
            return;

        GUILayout.BeginVertical("box");

        // ProgressManager�̑��݊m�F
        ProgressManager progressManager = ProgressManager.Instance;
        if (progressManager == null)
        {
            EditorGUILayout.HelpBox("ProgressManager��������܂���B", MessageType.Error);
            GUILayout.EndVertical();
            return;
        }

        // �S�̂̓��v���
        int totalEvents = progressManager.GetTotalEventCount();
        int unlockedEvents = progressManager.GetUnlockedEventCount();
        int completedEvents = progressManager.GetCompletedEventCount();

        EditorGUILayout.LabelField("�C�x���g���v:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"���C�x���g��: {totalEvents}");
        EditorGUILayout.LabelField($"����ς�: {unlockedEvents}");
        EditorGUILayout.LabelField($"�����ς�: {completedEvents}");

        GUILayout.Space(5);

        // �S�C�x���g���ꊇ���삷��{�^��
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("�S�C�x���g�����", GUILayout.Height(25)))
        {
            UnlockAllEvents();
        }
        if (GUILayout.Button("�S�C�x���g������", GUILayout.Height(25)))
        {
            CompleteAllEvents();
        }
        if (GUILayout.Button("�S�C�x���g�����Z�b�g", GUILayout.Height(25)))
        {
            ResetAllEvents();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        // �ʃC�x���g�̐���
        EditorGUILayout.LabelField("�ʃC�x���g����:", EditorStyles.boldLabel);

        progressScrollPosition = EditorGUILayout.BeginScrollView(progressScrollPosition, GUILayout.Height(200));

        // ���m�̃C�x���gID�i1-20�܂ŕ\���A�K�v�ɉ����Ċg���\�j
        for (int eventId = 1; eventId <= 20; eventId++)
        {
            DrawEventControl(eventId);
        }

        EditorGUILayout.EndScrollView();

        GUILayout.EndVertical();
    }

    // �ʃC�x���g�̃R���g���[����`��
    private void DrawEventControl(int eventId)
    {
        EventState currentState = ProgressManager.Instance.GetEventState(eventId);

        GUILayout.BeginHorizontal("box");

        // �C�x���gID�\��
        EditorGUILayout.LabelField($"�C�x���g {eventId}:", GUILayout.Width(80));

        // ���݂̏�Ԃ�F�t���ŕ\��
        Color originalColor = GUI.color;
        switch (currentState)
        {
            case EventState.Locked:
                GUI.color = Color.gray;
                break;
            case EventState.Unlocked:
                GUI.color = Color.yellow;
                break;
            case EventState.Completed:
                GUI.color = Color.green;
                break;
        }

        EditorGUILayout.LabelField(currentState.ToString(), GUILayout.Width(80));
        GUI.color = originalColor;

        // ��ԕύX�{�^��
        GUI.enabled = currentState != EventState.Locked;
        if (GUILayout.Button("���b�N", GUILayout.Width(60)))
        {
            ProgressManager.Instance.SetEventState(eventId, EventState.Locked);
        }

        GUI.enabled = currentState != EventState.Unlocked;
        if (GUILayout.Button("���", GUILayout.Width(60)))
        {
            ProgressManager.Instance.SetEventState(eventId, EventState.Unlocked);
        }

        GUI.enabled = currentState != EventState.Completed;
        if (GUILayout.Button("����", GUILayout.Width(60)))
        {
            ProgressManager.Instance.SetEventState(eventId, EventState.Completed);
        }

        GUI.enabled = true;

        GUILayout.EndHorizontal();
    }

    // �S�C�x���g�����
    private void UnlockAllEvents()
    {
        if (EditorUtility.DisplayDialog("�m�F",
            "���ׂẴC�x���g�������Ԃɂ��܂����H",
            "���", "�L�����Z��"))
        {
            for (int i = 1; i <= 20; i++)
            {
                ProgressManager.Instance.SetEventState(i, EventState.Unlocked);
            }
            Debug.Log("StateDebugWindow: ���ׂẴC�x���g��������܂���");
        }
    }

    // �S�C�x���g������
    private void CompleteAllEvents()
    {
        if (EditorUtility.DisplayDialog("�m�F",
            "���ׂẴC�x���g��������Ԃɂ��܂����H",
            "����", "�L�����Z��"))
        {
            for (int i = 1; i <= 20; i++)
            {
                ProgressManager.Instance.SetEventState(i, EventState.Completed);
            }
            Debug.Log("StateDebugWindow: ���ׂẴC�x���g���������܂���");
        }
    }

    // �S�C�x���g�����Z�b�g
    private void ResetAllEvents()
    {
        if (EditorUtility.DisplayDialog("�m�F",
            "���ׂẴC�x���g�����b�N��Ԃɖ߂��܂����H",
            "���Z�b�g", "�L�����Z��"))
        {
            ProgressManager.Instance.ResetAllEvents();
            Debug.Log("StateDebugWindow: ���ׂẴC�x���g�����Z�b�g���܂���");
        }
    }
}