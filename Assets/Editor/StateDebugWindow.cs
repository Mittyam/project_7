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

    // セーブ・ロード関連の変数
    private string saveSlotName = "default"; // デフォルトのセーブスロット名
    private StatusManager statusManager; // StatusManagerの参照
    private Vector2 saveListScrollPosition; // セーブリスト用のスクロール位置
    private bool showSaveLoadSection = true; // セーブ・ロードセクションの折りたたみ状態

    // ProgressManager関連の変数
    private bool showProgressSection = true; // ProgressManagerセクションの折りたたみ状態
    private Vector2 progressScrollPosition; // イベントリスト用のスクロール位置
    private Dictionary<int, string> eventNames = new Dictionary<int, string>(); // イベントID to 名前のマッピング

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
        { StateID.Bath, "お風呂" },
        { StateID.Touch, "お触り" },
        { StateID.item, "アイテム" },
        { StateID.Sleep, "睡眠" }
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

        // 参照の取得（StatusManagerを追加）
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
        if (statusManager == null)
            statusManager = FindObjectOfType<StatusManager>();

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

        // セーブ・ロードセクションを追加
        DrawSaveLoadSection();

        // ProgressManagerセクションを追加
        DrawProgressManagerSection();
    }

    // セーブ・ロードセクションの描画
    private void DrawSaveLoadSection()
    {
        GUILayout.Space(10);

        // 折りたたみヘッダー
        showSaveLoadSection = EditorGUILayout.Foldout(showSaveLoadSection, "セーブ・ロード機能", true, EditorStyles.foldoutHeader);

        if (!showSaveLoadSection)
            return;

        GUILayout.BeginVertical("box");

        // StatusManagerの存在確認
        if (statusManager == null)
        {
            EditorGUILayout.HelpBox("StatusManagerが見つかりません。", MessageType.Error);
            GUILayout.EndVertical();
            return;
        }

        // セーブスロット名の入力
        GUILayout.BeginHorizontal();
        GUILayout.Label("セーブスロット名:", GUILayout.Width(100));
        saveSlotName = EditorGUILayout.TextField(saveSlotName, GUILayout.Width(150));
        GUILayout.EndHorizontal();

        // セーブ・ロード・削除ボタン
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("セーブ", GUILayout.Height(30)))
        {
            SaveGame();
        }
        if (GUILayout.Button("ロード", GUILayout.Height(30)))
        {
            LoadGame();
        }
        if (GUILayout.Button("削除", GUILayout.Height(30)))
        {
            DeleteSaveData();
        }
        GUILayout.EndHorizontal();

        // 現在のセーブデータの一覧表示
        DrawSaveSlotList();

        GUILayout.EndVertical();
    }

    // セーブスロット一覧の表示
    private void DrawSaveSlotList()
    {
        GUILayout.Space(5);
        GUILayout.Label("利用可能なセーブデータ:", EditorStyles.boldLabel);

        // Easy Save 3のファイル命名規則に合わせたパスを構築
        string saveDirectory = Application.persistentDataPath;
        if (Directory.Exists(saveDirectory))
        {
            // ES3のファイルを検索
            string[] files = Directory.GetFiles(saveDirectory, "*.es3");

            if (files.Length == 0)
            {
                EditorGUILayout.HelpBox("セーブデータが存在しません。", MessageType.Info);
                return;
            }

            // スクロールビューで一覧表示
            saveListScrollPosition = EditorGUILayout.BeginScrollView(saveListScrollPosition, GUILayout.Height(150));

            foreach (string file in files)
            {
                // ファイル名からスロット名を取得
                string fileName = Path.GetFileNameWithoutExtension(file);

                GUILayout.BeginHorizontal("box");

                // スロット名とファイルサイズを表示
                FileInfo fileInfo = new FileInfo(file);
                string fileSize = (fileInfo.Length / 1024f).ToString("F2") + " KB";
                GUILayout.Label(fileName, GUILayout.ExpandWidth(true));
                GUILayout.Label(fileSize, GUILayout.Width(70));

                // スロット操作ボタン
                if (GUILayout.Button("選択", GUILayout.Width(60)))
                {
                    saveSlotName = fileName;
                }

                if (GUILayout.Button("ロード", GUILayout.Width(60)))
                {
                    saveSlotName = fileName;
                    LoadGame();
                }

                if (GUILayout.Button("削除", GUILayout.Width(60)))
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
            EditorGUILayout.HelpBox("セーブディレクトリが存在しません。", MessageType.Warning);
        }
    }

    // セーブ処理
    private void SaveGame()
    {
        if (string.IsNullOrEmpty(saveSlotName))
        {
            Debug.LogError("セーブスロット名が空です。");
            return;
        }

        // 昼と夜のステートでのみセーブ可能にする
        if (!statusManager.CanSaveInCurrentState())
        {
            string message = "セーブは昼または夜のステートでのみ可能です。";
            Debug.LogWarning(message);
            EditorUtility.DisplayDialog("セーブ不可", message, "OK");
            return;
        }

        statusManager.SaveStatus(saveSlotName);
        Debug.Log($"ゲームをスロット「{saveSlotName}」にセーブしました。");
        EditorUtility.DisplayDialog("セーブ完了", $"ゲームをスロット「{saveSlotName}」にセーブしました。", "OK");
    }

    // ロード処理
    private void LoadGame()
    {
        if (string.IsNullOrEmpty(saveSlotName))
        {
            Debug.LogError("セーブスロット名が空です。");
            return;
        }

        // ES3でファイルの存在確認
        if (ES3.FileExists(saveSlotName))
        {
            statusManager.LoadStatus(saveSlotName);
            Debug.Log($"スロット「{saveSlotName}」からゲームをロードしました。");
            EditorUtility.DisplayDialog("ロード完了", $"スロット「{saveSlotName}」からゲームをロードしました。", "OK");
        }
        else
        {
            Debug.LogError($"スロット「{saveSlotName}」のセーブデータが見つかりません。");
            EditorUtility.DisplayDialog("エラー", $"スロット「{saveSlotName}」のセーブデータが見つかりません。", "OK");
        }
    }

    // セーブデータ削除処理
    private void DeleteSaveData()
    {
        if (string.IsNullOrEmpty(saveSlotName))
        {
            Debug.LogError("セーブスロット名が空です。");
            return;
        }

        // 確認ダイアログ
        if (EditorUtility.DisplayDialog("セーブデータの削除",
            $"スロット「{saveSlotName}」のセーブデータを削除しますか？",
            "削除", "キャンセル"))
        {
            // ES3を使用してファイルを削除
            if (ES3.FileExists(saveSlotName))
            {
                ES3.DeleteFile(saveSlotName);
                Debug.Log($"スロット「{saveSlotName}」のセーブデータを削除しました。");
                EditorUtility.DisplayDialog("削除完了", $"スロット「{saveSlotName}」のセーブデータを削除しました。", "OK");
            }
            else
            {
                Debug.LogWarning($"スロット「{saveSlotName}」のセーブデータが見つかりません。");
                EditorUtility.DisplayDialog("エラー", $"スロット「{saveSlotName}」のセーブデータが見つかりません。", "OK");
            }
        }
    }

    // ProgressManagerセクションの描画
    private void DrawProgressManagerSection()
    {
        GUILayout.Space(10);

        // 折りたたみヘッダー
        showProgressSection = EditorGUILayout.Foldout(showProgressSection, "イベント進行管理", true, EditorStyles.foldoutHeader);

        if (!showProgressSection)
            return;

        GUILayout.BeginVertical("box");

        // ProgressManagerの存在確認
        ProgressManager progressManager = ProgressManager.Instance;
        if (progressManager == null)
        {
            EditorGUILayout.HelpBox("ProgressManagerが見つかりません。", MessageType.Error);
            GUILayout.EndVertical();
            return;
        }

        // 全体の統計情報
        int totalEvents = progressManager.GetTotalEventCount();
        int unlockedEvents = progressManager.GetUnlockedEventCount();
        int completedEvents = progressManager.GetCompletedEventCount();

        EditorGUILayout.LabelField("イベント統計:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"総イベント数: {totalEvents}");
        EditorGUILayout.LabelField($"解放済み: {unlockedEvents}");
        EditorGUILayout.LabelField($"完了済み: {completedEvents}");

        GUILayout.Space(5);

        // 全イベントを一括操作するボタン
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("全イベントを解放", GUILayout.Height(25)))
        {
            UnlockAllEvents();
        }
        if (GUILayout.Button("全イベントを完了", GUILayout.Height(25)))
        {
            CompleteAllEvents();
        }
        if (GUILayout.Button("全イベントをリセット", GUILayout.Height(25)))
        {
            ResetAllEvents();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        // 個別イベントの制御
        EditorGUILayout.LabelField("個別イベント制御:", EditorStyles.boldLabel);

        progressScrollPosition = EditorGUILayout.BeginScrollView(progressScrollPosition, GUILayout.Height(200));

        // 既知のイベントID（1-20まで表示、必要に応じて拡張可能）
        for (int eventId = 1; eventId <= 20; eventId++)
        {
            DrawEventControl(eventId);
        }

        EditorGUILayout.EndScrollView();

        GUILayout.EndVertical();
    }

    // 個別イベントのコントロールを描画
    private void DrawEventControl(int eventId)
    {
        EventState currentState = ProgressManager.Instance.GetEventState(eventId);

        GUILayout.BeginHorizontal("box");

        // イベントID表示
        EditorGUILayout.LabelField($"イベント {eventId}:", GUILayout.Width(80));

        // 現在の状態を色付きで表示
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

        // 状態変更ボタン
        GUI.enabled = currentState != EventState.Locked;
        if (GUILayout.Button("ロック", GUILayout.Width(60)))
        {
            ProgressManager.Instance.SetEventState(eventId, EventState.Locked);
        }

        GUI.enabled = currentState != EventState.Unlocked;
        if (GUILayout.Button("解放", GUILayout.Width(60)))
        {
            ProgressManager.Instance.SetEventState(eventId, EventState.Unlocked);
        }

        GUI.enabled = currentState != EventState.Completed;
        if (GUILayout.Button("完了", GUILayout.Width(60)))
        {
            ProgressManager.Instance.SetEventState(eventId, EventState.Completed);
        }

        GUI.enabled = true;

        GUILayout.EndHorizontal();
    }

    // 全イベントを解放
    private void UnlockAllEvents()
    {
        if (EditorUtility.DisplayDialog("確認",
            "すべてのイベントを解放状態にしますか？",
            "解放", "キャンセル"))
        {
            for (int i = 1; i <= 20; i++)
            {
                ProgressManager.Instance.SetEventState(i, EventState.Unlocked);
            }
            Debug.Log("StateDebugWindow: すべてのイベントを解放しました");
        }
    }

    // 全イベントを完了
    private void CompleteAllEvents()
    {
        if (EditorUtility.DisplayDialog("確認",
            "すべてのイベントを完了状態にしますか？",
            "完了", "キャンセル"))
        {
            for (int i = 1; i <= 20; i++)
            {
                ProgressManager.Instance.SetEventState(i, EventState.Completed);
            }
            Debug.Log("StateDebugWindow: すべてのイベントを完了しました");
        }
    }

    // 全イベントをリセット
    private void ResetAllEvents()
    {
        if (EditorUtility.DisplayDialog("確認",
            "すべてのイベントをロック状態に戻しますか？",
            "リセット", "キャンセル"))
        {
            ProgressManager.Instance.ResetAllEvents();
            Debug.Log("StateDebugWindow: すべてのイベントをリセットしました");
        }
    }
}