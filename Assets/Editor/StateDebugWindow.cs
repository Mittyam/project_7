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

    // メインステート表示用
    private Dictionary<StateID, string> stateDisplayNames = new Dictionary<StateID, string>()
    {
        { StateID.Day, "昼" },
        { StateID.Evening, "夕方" },
        { StateID.Night, "夜" }
    };

    // ミニイベント表示用
    private Dictionary<StateID, string> miniEventDisplayNames = new Dictionary<StateID, string>()
    {
        { StateID.Talk, "会話" },
        { StateID.Outing, "お出かけ" },
        { StateID.Memory, "思い出" },
        { StateID.Library, "図書館" },
        { StateID.Cafe, "カフェ" },
        { StateID.PartJob, "バイト" },
        { StateID.Walk, "散歩" },
        { StateID.Game, "ゲーム" },
    };

    [MenuItem("Tools/State Debug Window")]
    public static void ShowWindow()
    {
        GetWindow<StateDebugWindow>("ステートデバッグ");
    }

    private void OnGUI()
    {
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("ゲームプレイ中のみ使用できます。", MessageType.Info);
            return;
        }

        // 参照の取得（UIManagerProviderの参照を削除）
        if (gameLoop == null)
            gameLoop = FindObjectOfType<GameLoop>();
        if (gameLoop == null)
        {
            EditorGUILayout.HelpBox("GameLoopが見つかりません。", MessageType.Error);
            return;
        }

        if (mainStateMachine == null)
            mainStateMachine = gameLoop.MainStateMachine;
        if (pushdownStack == null)
            pushdownStack = gameLoop.PushdownStack;
        if (statesContainer == null)
            statesContainer = gameLoop.StatesContainer;

        // セクション：現在の状態
        GUILayout.Label("現在の状態", EditorStyles.boldLabel);
        GUILayout.BeginVertical("box");

        // メインステート状態
        string currentMainStateName = mainStateMachine.CurrentStateID.ToString();
        if (stateDisplayNames.TryGetValue(mainStateMachine.CurrentStateID, out string displayName))
            currentMainStateName = displayName;

        EditorGUILayout.LabelField("メインステート:", currentMainStateName);

        // PushdownStack状態
        bool isStackEmpty = pushdownStack.IsEmpty;
        EditorGUILayout.LabelField("スタック状態:", isStackEmpty ? "空" : "イベント実行中");
        if (!isStackEmpty && pushdownStack.CurrentState != null)
        {
            // 型名から取得
            string stateName = pushdownStack.CurrentState.GetType().Name;
            EditorGUILayout.LabelField("実行中ステート:", stateName);
        }

        GUILayout.EndVertical();

        // セクション：メインステート切り替え
        GUILayout.Space(10);
        GUILayout.Label("メインステート切り替え", EditorStyles.boldLabel);
        GUILayout.BeginVertical("box");

        // 各メインステートへの切り替えボタン
        GUILayout.BeginHorizontal();
        foreach (var state in stateDisplayNames)
        {
            // 現在のステートは強調表示
            bool isCurrentState = mainStateMachine.CurrentStateID == state.Key;
            GUI.enabled = !isCurrentState && isStackEmpty; // スタック実行中は無効化

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

        // 次のステートへボタン
        GUI.enabled = isStackEmpty;
        if (GUILayout.Button("次のステートへ進む", GUILayout.Height(30)))
        {
            mainStateMachine.AdvanceToNextState();
        }
        GUI.enabled = true;

        GUILayout.EndVertical();

        // セクション：ミニイベント起動
        GUILayout.Space(10);
        GUILayout.Label("ミニイベント起動", EditorStyles.boldLabel);
        GUILayout.BeginVertical("box");

        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(150));

        foreach (var miniEvent in miniEventDisplayNames)
        {
            if (GUILayout.Button(miniEvent.Value + "を起動", GUILayout.Height(25)))
            {
                gameLoop.PushMiniEvent(miniEvent.Key);
            }
        }

        GUILayout.EndScrollView();

        // PushdownStackの操作
        GUI.enabled = !isStackEmpty;
        if (GUILayout.Button("現在のイベントを終了", GUILayout.Height(30)))
        {
            pushdownStack.Pop();
        }
        GUI.enabled = true;

        GUILayout.EndVertical();

        // 更新ボタン
        GUILayout.Space(10);
        if (GUILayout.Button("ウィンドウ更新", GUILayout.Height(25)))
        {
            Repaint();
        }

        // 自動更新（毎フレーム）
        if (Application.isPlaying)
        {
            Repaint();
        }
    }
}