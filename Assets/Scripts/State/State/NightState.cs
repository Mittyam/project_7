using System.Collections;
using UnityEngine;
using static GameEvents;

/// <summary>
/// 夜ステート
/// ・ステータス更新
/// ・昼ならではのサブイベントの判定など
/// </summary>
public class NightState : StateBase, IPausableState
{
    [Header("State Data")]
    [SerializeField] private MainStateData stateData;

    [Header("UI要素")]
    [SerializeField] private GameObject nightUIContainer;
    [SerializeField] private GameObject commonUIContainer;

    [Header("Events")]
    [SerializeField] private GameEvent nightStateEventSO;

    // Live2D関連フィールド追加
    private GameObject activeLive2DModel; // アクティブなLive2Dモデルへの参照
    private string currentModelID; // 現在表示中のモデルID

    // UI生成状態管理用フラグ
    private bool isUICreated = false;

    public override void OnEnter()
    {
        Debug.Log("NightStateに入ります。");
        StatusManager.Instance.RecoverDailyActionPoints();

        // UI要素のセットアップと表示
        SetupUI();
        ShowNightUI();

        // Index 2~4 のBGMをランダムに再生
        int randomIndex = Random.Range(8, 11);
        SoundManager.Instance.PlayBGMWithFadeIn(randomIndex, 1f);

        // イベント発火
        nightStateEventSO.Raise();

        // Live2Dモデルを初期化して表示
        SetupLive2DModel();
    }

    public override void OnUpdate()
    {
        
    }

    public override void OnExit()
    {
        Debug.Log("NightState: 出ます。");

        // BGMを停止
        SoundManager.Instance.StopBGM();

        // タッチハンドラーを無効化
        DisableLive2DTouchHandler();

        // Live2Dモデルのクリーンアップ
        CleanupLive2DModel();

        // UIの非表示化
        HideAllUI();

        // 次の日への移行なので、ここでのノベルイベントチェック
        EventTriggerChecker.Check(TriggerTiming.NightToDay);
    }

    public void OnPause()
    {
        Debug.Log("NightState: 一時停止します");

        // UI要素を非表示にする
        HideAllUI();

        SoundManager.Instance.StopBGM();
    }

    public void OnResume()
    {
        Debug.Log("NightState: 再開します");

        // UI要素を再表示する
        SetupUI();
        ShowNightUI();

        // Index 2~4 のBGMをランダムに再生
        int randomIndex = Random.Range(8, 11);
        SoundManager.Instance.PlayBGMWithFadeIn(randomIndex, 1f);
    }

    // 次のステートの取得メソッド
    public StateID GetNextStateID()
    {
        return stateData != null ? stateData.nextStateID : StateID.Day;
    }

    #region --- UI関連メソッド ---

    // UIのセットアップ
    private void SetupUI()
    {
        // UIコンテナの初期化
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

        // UIプレハブの生成（まだ生成されていない場合のみ）
        if (!isUICreated && stateData != null && stateData.uiPrefab != null)
        {
            foreach (var prefab in stateData.uiPrefab)
            {
                if (prefab != null)
                {
                    // カメラ設定付きのインスタンス化を使用
                    InstantiateUIWithCamera(prefab, nightUIContainer.transform);
                }
            }
            isUICreated = true;

            Debug.Log("NightState: UIプレハブを生成し、カメラ設定を適用しました");
        }

        // 夜特有のUI設定や状態更新があればここで実行
        // 例: 夜の雰囲気エフェクト適用など
        // UpdateNightUIEffects();
    }

    // 夜UIの表示
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

    // 夜特有のUIエフェクト更新（オプション - 必要に応じて実装）
    private void UpdateNightUIEffects()
    {
        // 例: ライティングエフェクトやカラーフィルターの適用
        // 夜の雰囲気を演出するUI調整
        if (nightUIContainer != null)
        {
            // 例: 明かりエフェクトの点滅設定など
            // ここに夜特有のエフェクト処理を追加
        }
    }

    // 全UIの非表示
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

    #region --- Live2D関連メソッド ---

    // Live2Dモデルをセットアップするメソッド
    private void SetupLive2DModel()
    {
        // Live2Dの表示が無効の場合は処理しない
        if (stateData == null || stateData.live2DData == null || !stateData.showLive2DModel)
        {
            return;
        }

        // Live2Dモデルのデータを取得
        var live2DData = stateData.live2DData;

        // すでにモデルが存在している場合は削除
        CleanupLive2DModel();

        // モデルプレハブがあれば生成
        if (live2DData.modelPrefab != null)
        {
            // Live2DControllerコンポーネントを取得
            Live2DController live2DController = FindLive2DController();

            if (live2DController != null)
            {
                // モデルIDを保存
                currentModelID = live2DData.modelID;

                // Live2Dモデルを表示
                live2DController.ShowModel(
                    currentModelID,
                    live2DData.scale,
                    live2DData.position.ToString()
                );

                // アニメーション再生
                if (!string.IsNullOrEmpty(live2DData.defaultAnimTrigger))
                {
                    live2DController.PlayAnimation(currentModelID, live2DData.defaultAnimTrigger);
                }

                // モデル参照取得を確実に遅延させるためのコルーチンを開始
                StartCoroutine(GetModelAndAttachHandler());

                Debug.Log($"Live2D model '{currentModelID}' displayed in NightState");
            }
            else
            {
                Debug.LogError("Live2DController not found");
            }
        }
    }

    // モデル参照を取得してハンドラーをアタッチするコルーチン
    private IEnumerator GetModelAndAttachHandler()
    {
        // Live2Dモデルが生成されるまで少し待機
        yield return new WaitForSeconds(0.1f);

        // Live2Dコントローラーから現在のモデルを直接取得
        GameObject modelObject = null;
        Live2DController controller = FindLive2DController();

        if (controller != null)
        {
            // コントローラーから現在のモデルオブジェクトを取得する新しいメソッドを使用
            modelObject = controller.GetModelObject(currentModelID);

            if (modelObject != null)
            {
                // 取得したモデルを保存
                activeLive2DModel = modelObject;

                // タッチハンドラをアタッチ（StateDataがnullでなく、Live2D表示が有効で、タッチ機能が有効な場合）
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

    // Live2DControllerを検索して取得
    private Live2DController FindLive2DController()
    {
        // まずはGameLoopから取得を試みる
        if (GameLoop.Instance != null)
        {
            // 1. 専用のLive2DContainerがある場合
            GameObject container = GameObject.Find("Live2DContainer");
            if (container != null)
            {
                Live2DController controller = container.GetComponent<Live2DController>();
                if (controller != null) return controller;
            }

            // 2. シーン内を検索
            return FindObjectOfType<Live2DController>();
        }

        return null;
    }

    // Live2Dモデルのクリーンアップ
    private void CleanupLive2DModel()
    {
        if (!string.IsNullOrEmpty(currentModelID))
        {
            // Live2DControllerを取得
            Live2DController live2DController = FindLive2DController();
            if (live2DController != null)
            {
                // モデルの非表示
                live2DController.HideModel(currentModelID);
            }

            // 参照をクリア
            activeLive2DModel = null;
            currentModelID = string.Empty;
        }
    }

    // タッチハンドラーをアタッチするメソッド
    private void SetupLive2DTouchHandler(GameObject live2DModel)
    {
        if (live2DModel == null)
        {
            Debug.LogError("Cannot attach touch handler to null model");
            return;
        }

        // モデルオブジェクトの情報を詳細にログ出力
        Debug.Log($"Setting up touch handler for model: {live2DModel.name}, " +
                  $"Active: {live2DModel.activeSelf}, " +
                  $"Path: {GetGameObjectPath(live2DModel)}");

        // 既にアタッチされたハンドラーを確実に削除（重複防止）
        CharacterTouchHandler existingHandler = live2DModel.GetComponent<CharacterTouchHandler>();
        if (existingHandler != null)
        {
            Debug.Log("Removing existing touch handler");
            DestroyImmediate(existingHandler);
        }

        // 新規にハンドラーをアタッチ
        CharacterTouchHandler touchHandler = live2DModel.AddComponent<CharacterTouchHandler>();

        // 即座に初期化を実行
        if (touchHandler != null)
        {
            touchHandler.Initialize(currentModelID);
            Debug.Log("Touch handler successfully attached and initialized");
        }
        else
        {
            Debug.LogError("Failed to attach CharacterTouchHandler component");
        }
    }

    // タッチハンドラーを無効化するメソッド
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

    // GameObject階層パスを取得するヘルパーメソッド（デバッグ用）
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
