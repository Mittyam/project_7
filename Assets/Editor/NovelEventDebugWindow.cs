using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class NovelEventDebugWindow : EditorWindow
{
    private Vector2 scrollPosition;
    private NovelEventData selectedEvent;
    private List<NovelEventData> allEvents = new List<NovelEventData>();
    private bool isPlaying = false; // イベント再生中かどうかのフラグ
    private string searchFilter = ""; // イベント検索用フィルター

    // グルーピング用
    private Dictionary<TriggerTiming, List<NovelEventData>> eventsByTiming = new Dictionary<TriggerTiming, List<NovelEventData>>();
    private Dictionary<TriggerTiming, bool> groupFoldouts = new Dictionary<TriggerTiming, bool>();

    [MenuItem("Tools/Novel Event Debugger")]
    public static void ShowWindow()
    {
        GetWindow<NovelEventDebugWindow>("Novel Event Debugger");
    }

    private void OnEnable()
    {
        // イベントデータをロード
        LoadAllEvents();

        // EditorApplicationのupdateに登録して再生状態を監視
        EditorApplication.update += CheckPlayingState;
    }

    private void OnDisable()
    {
        // 登録解除
        EditorApplication.update -= CheckPlayingState;
    }

    // イベントを再読み込み
    private void LoadAllEvents()
    {
        allEvents.Clear();
        eventsByTiming.Clear();

        // すべてのイベントをロード
        allEvents.AddRange(Resources.LoadAll<NovelEventData>("Events"));

        // トリガータイミングでグルーピング
        foreach (var eventData in allEvents)
        {
            if (!eventsByTiming.ContainsKey(eventData.triggerTiming))
            {
                eventsByTiming[eventData.triggerTiming] = new List<NovelEventData>();
                // デフォルトで展開状態に
                groupFoldouts[eventData.triggerTiming] = true;
            }

            eventsByTiming[eventData.triggerTiming].Add(eventData);
        }
    }

    // 再生状態をチェック
    private void CheckPlayingState()
    {
        if (!Application.isPlaying) return;

        // GameLoopがあり、PushdownStackが空でない場合、現在のステートがNovelStateかチェック
        var gameLoop = GameLoop.Instance;
        if (gameLoop != null && !gameLoop.PushdownStack.IsEmpty)
        {
            isPlaying = gameLoop.PushdownStack.CurrentState is NovelState;
        }
        else
        {
            isPlaying = false;
        }

        // UIの更新を促す
        Repaint();
    }

    /// <summary>
    /// ウィンドウの描画
    /// </summary>
    private void OnGUI()
    {
        DrawToolbar();

        // 現在実行中のイベント情報
        DrawCurrentEventInfo();

        // 検索フィルター
        DrawSearchFilter();

        // 利用可能なイベント一覧
        DrawEventList();
    }

    // ツールバーの描画
    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        if (GUILayout.Button("イベント再読み込み", EditorStyles.toolbarButton))
        {
            LoadAllEvents();
        }

        EditorGUILayout.Space();

        // 再生中のみ表示するコントロール
        GUI.enabled = Application.isPlaying && !GameLoop.Instance.PushdownStack.IsEmpty && isPlaying;

        if (GUILayout.Button("現在のイベントを終了", EditorStyles.toolbarButton))
        {
            EndCurrentEvent();
        }

        if (GUILayout.Button("オートスキップ", EditorStyles.toolbarButton))
        {
            SetAutoSkipMode();
        }

        if (GUILayout.Button("クリックモード", EditorStyles.toolbarButton))
        {
            SetClickMode();
        }

        GUI.enabled = true;

        EditorGUILayout.EndHorizontal();
    }

    // 現在のイベント情報の描画
    private void DrawCurrentEventInfo()
    {
        if (!Application.isPlaying) return;

        EditorGUILayout.Space(5);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.LabelField("現在の状態:", EditorStyles.boldLabel);

        var gameLoop = GameLoop.Instance;
        if (gameLoop != null && !gameLoop.PushdownStack.IsEmpty)
        {
            var currentState = gameLoop.PushdownStack.CurrentState;
            EditorGUILayout.LabelField($"実行中ステート: {currentState.GetType().Name}");

            if (currentState is NovelState novelState)
            {
                EditorGUILayout.LabelField("ノベルイベント実行中", EditorStyles.boldLabel);

                // オプション: 現在のイベントIDなど詳細情報を表示
                // この部分はNovelStateの実装に合わせて調整が必要
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("現在のイベントを終了", GUILayout.Height(30)))
                {
                    EndCurrentEvent();
                }

                EditorGUILayout.EndHorizontal();
            }
        }
        else
        {
            EditorGUILayout.LabelField("実行中イベントなし");
        }

        EditorGUILayout.EndVertical();
    }

    // 検索フィルターの描画
    private void DrawSearchFilter()
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        EditorGUILayout.LabelField("検索フィルター:", GUILayout.Width(80));
        string newFilter = EditorGUILayout.TextField(searchFilter);

        if (newFilter != searchFilter)
        {
            searchFilter = newFilter;
        }

        if (GUILayout.Button("クリア", EditorStyles.toolbarButton, GUILayout.Width(50)))
        {
            searchFilter = "";
        }

        EditorGUILayout.EndHorizontal();
    }

    // イベントリストの描画
    private void DrawEventList()
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("利用可能なイベント:", EditorStyles.boldLabel);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // イベントをグループごとに表示
        foreach (var group in eventsByTiming)
        {
            var timing = group.Key;
            var events = group.Value;

            // 検索フィルターに一致するイベントがあるか確認
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

            // グループのフォールドアウト
            groupFoldouts[timing] = EditorGUILayout.Foldout(groupFoldouts[timing],
                $"{timing} ({events.Count}件)", true, EditorStyles.foldoutHeader);

            if (groupFoldouts[timing])
            {
                EditorGUI.indentLevel++;

                foreach (var eventData in events)
                {
                    // 検索フィルターが設定されている場合、一致するもののみ表示
                    if (!string.IsNullOrEmpty(searchFilter) &&
                        !eventData.eventName.ToLower().Contains(searchFilter.ToLower()) &&
                        !eventData.eventID.ToString().Contains(searchFilter))
                    {
                        continue;
                    }

                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                    // 再生中は新たなイベント実行ボタンを無効化
                    GUI.enabled = Application.isPlaying && (GameLoop.Instance.PushdownStack.IsEmpty ||
                                  !(GameLoop.Instance.PushdownStack.CurrentState is NovelState));

                    // イベント情報表示と実行ボタン
                    var buttonStyle = new GUIStyle(GUI.skin.button);
                    buttonStyle.alignment = TextAnchor.MiddleLeft;

                    if (GUILayout.Button($"ID:{eventData.eventID} - {eventData.eventName}",
                        buttonStyle, GUILayout.Height(30)))
                    {
                        selectedEvent = eventData;
                        ExecuteSelectedEvent();
                    }

                    GUI.enabled = true;

                    // イベント詳細表示ボタン
                    if (GUILayout.Button("詳細", GUILayout.Width(50), GUILayout.Height(30)))
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
    /// 選択されたイベントを実行する
    /// </summary>
    private void ExecuteSelectedEvent()
    {
        if (selectedEvent == null || !Application.isPlaying)
        {
            EditorUtility.DisplayDialog("エラー", "イベントが選択されていないか、再生モードではありません", "OK");
            return;
        }

        // GameLoopからNovelEventSchedulerを取得し、イベントをプッシュ
        var scheduler = GameLoop.Instance.NovelEventScheduler;
        scheduler.PushEvent(selectedEvent);

        Debug.Log($"NovelEventDebugger: イベント「{selectedEvent.eventName}」(ID:{selectedEvent.eventID})を実行開始");
    }

    /// <summary>
    /// 現在再生中のイベントを終了する
    /// </summary>
    private void EndCurrentEvent()
    {
        if (!Application.isPlaying) return;

        var gameLoop = GameLoop.Instance;
        if (gameLoop != null && !gameLoop.PushdownStack.IsEmpty)
        {
            // 現在のステートがNovelStateであれば終了処理を実行
            if (gameLoop.PushdownStack.CurrentState is NovelState novelState)
            {
                // 強制的に完了させる
                gameLoop.PushdownStack.Pop();
                Debug.Log("NovelEventDebugger: イベントを強制終了しました");
            }
        }
    }

    /// <summary>
    /// オートスキップモードを設定
    /// </summary>
    private void SetAutoSkipMode()
    {
        if (!Application.isPlaying) return;

        var gameLoop = GameLoop.Instance;
        if (gameLoop != null && !gameLoop.PushdownStack.IsEmpty)
        {
            // 現在のステートがNovelStateであればオートモードを設定
            if (gameLoop.PushdownStack.CurrentState is NovelState novelState)
            {
                // NovelStateにオートモード設定メソッドがあれば呼び出す
                if (novelState.GetType().GetMethod("SetPlaybackMode") != null)
                {
                    // WaitForAdvanceCommand.PlaybackMode.Autoを設定
                    object autoMode = System.Enum.Parse(
                        typeof(NovelState.PlaybackMode), "Auto");
                    novelState.GetType().GetMethod("SetPlaybackMode").Invoke(novelState, new[] { autoMode });

                    Debug.Log("NovelEventDebugger: オートスキップモードに設定しました");
                }
            }
        }
    }

    /// <summary>
    /// クリックモードを設定
    /// </summary>
    private void SetClickMode()
    {
        if (!Application.isPlaying) return;

        var gameLoop = GameLoop.Instance;
        if (gameLoop != null && !gameLoop.PushdownStack.IsEmpty)
        {
            // 現在のステートがNovelStateであればクリックモードを設定
            if (gameLoop.PushdownStack.CurrentState is NovelState novelState)
            {
                // NovelStateにモード設定メソッドがあれば呼び出す
                if (novelState.GetType().GetMethod("SetPlaybackMode") != null)
                {
                    // WaitForAdvanceCommand.PlaybackMode.Clickを設定
                    object clickMode = System.Enum.Parse(
                        typeof(NovelState.PlaybackMode), "Click");
                    novelState.GetType().GetMethod("SetPlaybackMode").Invoke(novelState, new[] { clickMode });

                    Debug.Log("NovelEventDebugger: クリックモードに設定しました");
                }
            }
        }
    }
}