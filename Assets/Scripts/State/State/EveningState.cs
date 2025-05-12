using UnityEngine;
using static GameEvents;

/// <summary>
/// 夕方ステート
/// 商品の購入など
/// </summary>
public class EveningState : StateBase, IPausableState
{
    [Header("State Data")]
    [SerializeField] private MainStateData stateData;

    // UI関連フィールド
    [Header("UI要素")]
    [SerializeField] private GameObject eveningUIContainer;
    [SerializeField] private GameObject commonUIContainer;

    [Header("Events")]
    [SerializeField] private GameEvent eveningStateEventSO;

    private float eveningProgress; // 夕方の進行度

    // UI生成状態管理用フラグ
    private bool isUICreated = false;

    public override void OnEnter()
    {
        Debug.Log("EveningState: 入ります。");

        // UI要素のセットアップと表示
        SetupUI();
        ShowEveningUI();

        eveningProgress = 0f;

        // イベント発火
        eveningStateEventSO?.Raise();

        // イベント購読
        SubscribeToEvents();
    }

    public override void OnUpdate()
    {
        // 夕方の進行度を更新
        if (eveningProgress >= 1.0f)
        {
            MainStateMachine.AdvanceToNextState();
        }
    }

    public override void OnExit()
    {
        Debug.Log("EveningState: 抜けます。");

        // イベント購読解除
        UnsubscribeFromEvents();

        // UIの非表示化
        HideAllUI();

        // 次のステートに移行する前にノベルイベントの発火チェック
        EventTriggerChecker.Check(TriggerTiming.EveningToNight);
    }

    public void OnPause()
    {
        Debug.Log("EveningState: 一時停止します");

        // UI要素を非表示にする
        HideAllUI();
    }

    public void OnResume()
    {
        Debug.Log("EveningState: 再開します");

        // UI要素を再表示する
        SetupUI();
        ShowEveningUI();
    }

    // 次のステートの取得メソッド
    public StateID GetNextStateID()
    {
        return stateData != null ? stateData.nextStateID : StateID.Night;
    }

    #region --- ここから下はUI関連の新メソッド ---

    // UIのセットアップ
    private void SetupUI()
    {
        // UIコンテナの初期化（既存のコード）
        if (eveningUIContainer == null)
        {
            eveningUIContainer = new GameObject("EveningUIContainer");
            eveningUIContainer.transform.SetParent(transform);
            eveningUIContainer.AddComponent<RectTransform>();
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
                    // カメラ設定付きのインスタンス化メソッドを使用
                    InstantiateUIWithCamera(prefab, eveningUIContainer.transform);
                }
            }
            isUICreated = true;

            Debug.Log("EveningState: UIプレハブを生成し、カメラ設定を適用しました");
        }

        // ボタン状態の更新や追加のUI設定があればここで実行
        // UpdateButtonStates();
    }

    // 夕方UIの表示
    private void ShowEveningUI()
    {
        if (eveningUIContainer != null)
        {
            eveningUIContainer.SetActive(true);
        }

        if (commonUIContainer != null)
        {
            commonUIContainer.SetActive(true);
        }
    }

    // 全UIの非表示
    private void HideAllUI()
    {
        if (eveningUIContainer != null)
        {
            eveningUIContainer.SetActive(false);
        }

        if (commonUIContainer != null)
        {
            commonUIContainer.SetActive(false);
        }
    }

    #endregion

    #region --- イベント購読メソッド ---

    // イベント購読の設定 - 変更
    private void SubscribeToEvents()
    {
        // 進行度更新とボタン状態更新のみを購読
        TypedEventManager.Instance.Subscribe<GameEvents.EveningProgressUpdated>(OnEveningProgressUpdated);
        TypedEventManager.Instance.Subscribe<GameEvents.ButtonStateUpdateRequested>(OnButtonStateUpdateRequested);
    }

    // イベント購読の解除 - 変更
    private void UnsubscribeFromEvents()
    {
        if (TypedEventManager.Instance == null) return;

        TypedEventManager.Instance.Unsubscribe<GameEvents.EveningProgressUpdated>(OnEveningProgressUpdated);
        TypedEventManager.Instance.Unsubscribe<GameEvents.ButtonStateUpdateRequested>(OnButtonStateUpdateRequested);
    }

    // 新しいイベントハンドラー
    private void OnEveningProgressUpdated(GameEvents.EveningProgressUpdated eventData)
    {
        UpdateEveningProgress(eventData.ProgressValue);
    }

    private void OnButtonStateUpdateRequested(GameEvents.ButtonStateUpdateRequested eventData)
    {
        // このステートのIDと一致する場合のみ処理
        if (eventData.CurrentStateID == StateID.Evening)
        {
            UpdateButtonStates();
        }
    }

    // 夕方の進行度を更新
    private void UpdateEveningProgress(float progressValue)
    {
        eveningProgress += progressValue;
    }

    // ボタン状態の更新（既存のメソッドを維持）
    private void UpdateButtonStates()
    {
        // 実装省略（既存のボタン状態更新処理を使用）
    }

    // 閉じるボタンなどのイベントハンドラ
    private void OnCloseButtonClicked(GameEvents.CloseButtonClicked eventData)
    {
        // パネル名のチェックなど、必要に応じて条件分岐
        //if (eventData.SourcePanelName == "EveningPanel")
        //{
        //    eveningProgress += 0.5f;  // 進行度を増加
        //}
    }

    // 購入ボタンイベントハンドラ
    //private void OnPurchaseButtonClicked(GameEvents.PurchaseButtonClicked eventData)
    //{
    //    // 購入処理
    //    bool purchaseSuccess = true; // 実際には購入条件をチェックする

    //    if (purchaseSuccess)
    //    {
    //        Debug.Log("購入が完了しました");
    //        eveningProgress += 1.0f;  // 進行度を1増加

    //        // 必要に応じてステータス更新
    //        // StatusManager.Instance.UpdateStatus(0, 0, 0, -eventData.Cost);
    //    }
    //}

    #endregion
}