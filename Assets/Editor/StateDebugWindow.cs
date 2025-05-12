using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class StateDebugWindow : EditorWindow
{
    private Vector2 scrollPosition;
    private GameLoop gameLoop;
    private StatesContainer statesContainer;
    private MainStateMachine mainStateMachine;
    private PushdownStateMachine pushdownStack;

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

        // �Q�Ƃ̎擾�iUIManagerProvider�̎Q�Ƃ��폜�j
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
    }
}