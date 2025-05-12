using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class NovelEventDebugWindow : EditorWindow
{
    private Vector2 scrollPosition;
    private NovelEventData selectedEvent;
    private List<NovelEventData> allEvents = new List<NovelEventData>();
    private bool isPlaying = false; // �C�x���g�Đ������ǂ����̃t���O
    private string searchFilter = ""; // �C�x���g�����p�t�B���^�[

    // �O���[�s���O�p
    private Dictionary<TriggerTiming, List<NovelEventData>> eventsByTiming = new Dictionary<TriggerTiming, List<NovelEventData>>();
    private Dictionary<TriggerTiming, bool> groupFoldouts = new Dictionary<TriggerTiming, bool>();

    [MenuItem("Tools/Novel Event Debugger")]
    public static void ShowWindow()
    {
        GetWindow<NovelEventDebugWindow>("Novel Event Debugger");
    }

    private void OnEnable()
    {
        // �C�x���g�f�[�^�����[�h
        LoadAllEvents();

        // EditorApplication��update�ɓo�^���čĐ���Ԃ��Ď�
        EditorApplication.update += CheckPlayingState;
    }

    private void OnDisable()
    {
        // �o�^����
        EditorApplication.update -= CheckPlayingState;
    }

    // �C�x���g���ēǂݍ���
    private void LoadAllEvents()
    {
        allEvents.Clear();
        eventsByTiming.Clear();

        // ���ׂẴC�x���g�����[�h
        allEvents.AddRange(Resources.LoadAll<NovelEventData>("Events"));

        // �g���K�[�^�C�~���O�ŃO���[�s���O
        foreach (var eventData in allEvents)
        {
            if (!eventsByTiming.ContainsKey(eventData.triggerTiming))
            {
                eventsByTiming[eventData.triggerTiming] = new List<NovelEventData>();
                // �f�t�H���g�œW�J��Ԃ�
                groupFoldouts[eventData.triggerTiming] = true;
            }

            eventsByTiming[eventData.triggerTiming].Add(eventData);
        }
    }

    // �Đ���Ԃ��`�F�b�N
    private void CheckPlayingState()
    {
        if (!Application.isPlaying) return;

        // GameLoop������APushdownStack����łȂ��ꍇ�A���݂̃X�e�[�g��NovelState���`�F�b�N
        var gameLoop = GameLoop.Instance;
        if (gameLoop != null && !gameLoop.PushdownStack.IsEmpty)
        {
            isPlaying = gameLoop.PushdownStack.CurrentState is NovelState;
        }
        else
        {
            isPlaying = false;
        }

        // UI�̍X�V�𑣂�
        Repaint();
    }

    /// <summary>
    /// �E�B���h�E�̕`��
    /// </summary>
    private void OnGUI()
    {
        DrawToolbar();

        // ���ݎ��s���̃C�x���g���
        DrawCurrentEventInfo();

        // �����t�B���^�[
        DrawSearchFilter();

        // ���p�\�ȃC�x���g�ꗗ
        DrawEventList();
    }

    // �c�[���o�[�̕`��
    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        if (GUILayout.Button("�C�x���g�ēǂݍ���", EditorStyles.toolbarButton))
        {
            LoadAllEvents();
        }

        EditorGUILayout.Space();

        // �Đ����̂ݕ\������R���g���[��
        GUI.enabled = Application.isPlaying && !GameLoop.Instance.PushdownStack.IsEmpty && isPlaying;

        if (GUILayout.Button("���݂̃C�x���g���I��", EditorStyles.toolbarButton))
        {
            EndCurrentEvent();
        }

        if (GUILayout.Button("�I�[�g�X�L�b�v", EditorStyles.toolbarButton))
        {
            SetAutoSkipMode();
        }

        if (GUILayout.Button("�N���b�N���[�h", EditorStyles.toolbarButton))
        {
            SetClickMode();
        }

        GUI.enabled = true;

        EditorGUILayout.EndHorizontal();
    }

    // ���݂̃C�x���g���̕`��
    private void DrawCurrentEventInfo()
    {
        if (!Application.isPlaying) return;

        EditorGUILayout.Space(5);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.LabelField("���݂̏��:", EditorStyles.boldLabel);

        var gameLoop = GameLoop.Instance;
        if (gameLoop != null && !gameLoop.PushdownStack.IsEmpty)
        {
            var currentState = gameLoop.PushdownStack.CurrentState;
            EditorGUILayout.LabelField($"���s���X�e�[�g: {currentState.GetType().Name}");

            if (currentState is NovelState novelState)
            {
                EditorGUILayout.LabelField("�m�x���C�x���g���s��", EditorStyles.boldLabel);

                // �I�v�V����: ���݂̃C�x���gID�ȂǏڍ׏���\��
                // ���̕�����NovelState�̎����ɍ��킹�Ē������K�v
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("���݂̃C�x���g���I��", GUILayout.Height(30)))
                {
                    EndCurrentEvent();
                }

                EditorGUILayout.EndHorizontal();
            }
        }
        else
        {
            EditorGUILayout.LabelField("���s���C�x���g�Ȃ�");
        }

        EditorGUILayout.EndVertical();
    }

    // �����t�B���^�[�̕`��
    private void DrawSearchFilter()
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        EditorGUILayout.LabelField("�����t�B���^�[:", GUILayout.Width(80));
        string newFilter = EditorGUILayout.TextField(searchFilter);

        if (newFilter != searchFilter)
        {
            searchFilter = newFilter;
        }

        if (GUILayout.Button("�N���A", EditorStyles.toolbarButton, GUILayout.Width(50)))
        {
            searchFilter = "";
        }

        EditorGUILayout.EndHorizontal();
    }

    // �C�x���g���X�g�̕`��
    private void DrawEventList()
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("���p�\�ȃC�x���g:", EditorStyles.boldLabel);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // �C�x���g���O���[�v���Ƃɕ\��
        foreach (var group in eventsByTiming)
        {
            var timing = group.Key;
            var events = group.Value;

            // �����t�B���^�[�Ɉ�v����C�x���g�����邩�m�F
            if (!string.IsNullOrEmpty(searchFilter))
            {
                bool hasMatchingEvent = false;
                foreach (var eventData in events)
                {
                    if (eventData.eventName.ToLower().Contains(searchFilter.ToLower()) ||
                        eventData.eventID.ToString().Contains(searchFilter))
                    {
                        hasMatchingEvent = true;
                        break;
                    }
                }

                if (!hasMatchingEvent) continue;
            }

            // �O���[�v�̃t�H�[���h�A�E�g
            groupFoldouts[timing] = EditorGUILayout.Foldout(groupFoldouts[timing],
                $"{timing} ({events.Count}��)", true, EditorStyles.foldoutHeader);

            if (groupFoldouts[timing])
            {
                EditorGUI.indentLevel++;

                foreach (var eventData in events)
                {
                    // �����t�B���^�[���ݒ肳��Ă���ꍇ�A��v������̂̂ݕ\��
                    if (!string.IsNullOrEmpty(searchFilter) &&
                        !eventData.eventName.ToLower().Contains(searchFilter.ToLower()) &&
                        !eventData.eventID.ToString().Contains(searchFilter))
                    {
                        continue;
                    }

                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                    // �Đ����͐V���ȃC�x���g���s�{�^���𖳌���
                    GUI.enabled = Application.isPlaying && (GameLoop.Instance.PushdownStack.IsEmpty ||
                                  !(GameLoop.Instance.PushdownStack.CurrentState is NovelState));

                    // �C�x���g���\���Ǝ��s�{�^��
                    var buttonStyle = new GUIStyle(GUI.skin.button);
                    buttonStyle.alignment = TextAnchor.MiddleLeft;

                    if (GUILayout.Button($"ID:{eventData.eventID} - {eventData.eventName}",
                        buttonStyle, GUILayout.Height(30)))
                    {
                        selectedEvent = eventData;
                        ExecuteSelectedEvent();
                    }

                    GUI.enabled = true;

                    // �C�x���g�ڍו\���{�^��
                    if (GUILayout.Button("�ڍ�", GUILayout.Width(50), GUILayout.Height(30)))
                    {
                        Selection.activeObject = eventData;
                        EditorGUIUtility.PingObject(eventData);
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUI.indentLevel--;
            }
        }

        EditorGUILayout.EndScrollView();
    }

    /// <summary>
    /// �I�����ꂽ�C�x���g�����s����
    /// </summary>
    private void ExecuteSelectedEvent()
    {
        if (selectedEvent == null || !Application.isPlaying)
        {
            EditorUtility.DisplayDialog("�G���[", "�C�x���g���I������Ă��Ȃ����A�Đ����[�h�ł͂���܂���", "OK");
            return;
        }

        // GameLoop����NovelEventScheduler���擾���A�C�x���g���v�b�V��
        var scheduler = GameLoop.Instance.NovelEventScheduler;
        scheduler.PushEvent(selectedEvent);

        Debug.Log($"NovelEventDebugger: �C�x���g�u{selectedEvent.eventName}�v(ID:{selectedEvent.eventID})�����s�J�n");
    }

    /// <summary>
    /// ���ݍĐ����̃C�x���g���I������
    /// </summary>
    private void EndCurrentEvent()
    {
        if (!Application.isPlaying) return;

        var gameLoop = GameLoop.Instance;
        if (gameLoop != null && !gameLoop.PushdownStack.IsEmpty)
        {
            // ���݂̃X�e�[�g��NovelState�ł���ΏI�����������s
            if (gameLoop.PushdownStack.CurrentState is NovelState novelState)
            {
                // �����I�Ɋ���������
                gameLoop.PushdownStack.Pop();
                Debug.Log("NovelEventDebugger: �C�x���g�������I�����܂���");
            }
        }
    }

    /// <summary>
    /// �I�[�g�X�L�b�v���[�h��ݒ�
    /// </summary>
    private void SetAutoSkipMode()
    {
        if (!Application.isPlaying) return;

        var gameLoop = GameLoop.Instance;
        if (gameLoop != null && !gameLoop.PushdownStack.IsEmpty)
        {
            // ���݂̃X�e�[�g��NovelState�ł���΃I�[�g���[�h��ݒ�
            if (gameLoop.PushdownStack.CurrentState is NovelState novelState)
            {
                // NovelState�ɃI�[�g���[�h�ݒ胁�\�b�h������ΌĂяo��
                if (novelState.GetType().GetMethod("SetPlaybackMode") != null)
                {
                    // WaitForAdvanceCommand.PlaybackMode.Auto��ݒ�
                    object autoMode = System.Enum.Parse(
                        typeof(NovelState.PlaybackMode), "Auto");
                    novelState.GetType().GetMethod("SetPlaybackMode").Invoke(novelState, new[] { autoMode });

                    Debug.Log("NovelEventDebugger: �I�[�g�X�L�b�v���[�h�ɐݒ肵�܂���");
                }
            }
        }
    }

    /// <summary>
    /// �N���b�N���[�h��ݒ�
    /// </summary>
    private void SetClickMode()
    {
        if (!Application.isPlaying) return;

        var gameLoop = GameLoop.Instance;
        if (gameLoop != null && !gameLoop.PushdownStack.IsEmpty)
        {
            // ���݂̃X�e�[�g��NovelState�ł���΃N���b�N���[�h��ݒ�
            if (gameLoop.PushdownStack.CurrentState is NovelState novelState)
            {
                // NovelState�Ƀ��[�h�ݒ胁�\�b�h������ΌĂяo��
                if (novelState.GetType().GetMethod("SetPlaybackMode") != null)
                {
                    // WaitForAdvanceCommand.PlaybackMode.Click��ݒ�
                    object clickMode = System.Enum.Parse(
                        typeof(NovelState.PlaybackMode), "Click");
                    novelState.GetType().GetMethod("SetPlaybackMode").Invoke(novelState, new[] { clickMode });

                    Debug.Log("NovelEventDebugger: �N���b�N���[�h�ɐݒ肵�܂���");
                }
            }
        }
    }
}