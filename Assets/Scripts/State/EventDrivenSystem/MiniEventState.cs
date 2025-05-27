using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// ミニイベント（お話/お出かけ/アイテム/思い出など）の基本クラス
/// </summary>
public class MiniEventState : StateBase, IPausableState
{
    // シリアライズフィールドとして管理
    [Header("State Data")]
    [SerializeField] protected MiniEventStateData stateData;

    // stateDataの公開
    public MiniEventStateData StateData => stateData;

    // 生成したUI要素のリスト
    protected List<GameObject> startUIs = new List<GameObject>();
    protected List<GameObject> endUIs = new List<GameObject>();

    // UI要素の親となるコンテナ
    protected GameObject uiContainer;

    // パラメータ格納用のディクショナリ
    protected Dictionary<string, object> parameters = new Dictionary<string, object>();

    // ミニイベント完了時にメインステートを遷移させるかどうかのフラグ
    // デフォルトでは遷移させない
    public bool ShouldAdvanceMainStateOnCompletion { get; set; } = false;

    /// <summary>
    /// イベントデータの設定
    /// </summary>
    public void SetStateData(MiniEventStateData data)
    {
        stateData = data;
    }

    public override void OnEnter()
    {
        // データチェック
        if (stateData == null)
        {
            Debug.LogError($"MiniEventState: {gameObject.name}にStateDataが設定されません");
            // エラー時は安全に終了
            CompleteEvent();
            return;
        }

        Debug.Log($"MiniEventState: {stateData.displayName} を開始します");

        // UIのセットアップ
        SetupUI();

        // イベント発火
        TypedEventManager.Instance.Publish(new GameEvents.MiniEventStarted
        {
            EventStateID = stateData.stateID,
            EventName = stateData.displayName
        });

        // アクションポイントの消費（パラメータから取得した値を優先）
        int actionPointCost = GetParameter<int>("ActionPointCost", stateData.actionPointCost);
        if (stateData.consumeActionPoint)
        {
            StatusManager.Instance.ConsumeActionPoint(actionPointCost);

            // アクションポイントが0になったらメインステート遷移フラグを立てる
            if (StatusManager.Instance.GetCurrentActionPoints() == 0)
            {
                Debug.Log("アクションポイントが0になりました。次のメインステートに進みます。");
                ShouldAdvanceMainStateOnCompletion = true;
            }
        }
    }

    public override void OnUpdate()
    {
        // ミニイベント中の更新処理
    }

    public override void OnExit()
    {
        Debug.Log($"MiniEventState: {stateData?.displayName} を終了します");

        ShouldAdvanceMainStateOnCompletion = false; // 終了時にフラグをリセット

        // ステータス変化の適用
        ApplyStatusChanges();

        // イベント発火
        if (stateData != null)
        {
            TypedEventManager.Instance.Publish(new GameEvents.MiniEventCompleted
            {
                EventStateID = stateData.stateID,
                EventName = stateData.displayName,
                AffectionChange = stateData.affectionChange,
                LoveChange = stateData.loveChange,
                MoneyChange = stateData.moneyChange
            });
        }

        // 生成したUI要素の削除
        DestroyEventUI();
    }

    public void OnPause()
    {
        Debug.Log($"MiniEventState: {stateData?.displayName} を一時停止します");

        // UIを非表示
        foreach (var ui in startUIs)
        {
            if (ui != null)
            {
                ui.SetActive(false);
            }
        }
    }

    public void OnResume()
    {
        Debug.Log($"MiniEventState: {stateData?.displayName} を再開します");

        // UIを再表示
        foreach (var ui in startUIs)
        {
            if (ui != null)
            {
                ui.SetActive(true);
            }
        }
    }

    /// <summary>
    /// パラメータを設定
    /// </summary>
    public void SetParameters(Dictionary<string, object> newParameters)
    {
        if (newParameters != null)
        {
            // パラメータをコピー
            foreach (var param in newParameters)
            {
                parameters[param.Key] = param.Value;
            }
        }
    }

    /// <summary>
    /// 特定のパラメータを取得
    /// </summary>
    protected T GetParameter<T>(string key, T defaultValue = default)
    {
        if (parameters.TryGetValue(key, out object value) && value is T typedValue)
        {
            return typedValue;
        }

        return defaultValue;
    }

    /// <summary>
    /// イベント終了処理（ユーザーアクションから呼ばれる）
    /// </summary>
    public void CompleteEvent()
    {
        // 自分自身をPushdownStackからpop
        PushdownStack.Pop();
    }

    /// <summary>
    /// ステータス変化の適用
    /// </summary>
    protected virtual void ApplyStatusChanges()
    {
        StatusManager.Instance.UpdateStatus(
                0,  //日付変化なし
                stateData.affectionChange,
                stateData.loveChange,
                stateData.moneyChange
            );
    }

    /// <summary>
    /// UIをセットアップする
    /// </summary>
    protected override void SetupUI()
    {
        base.SetupUI();

        // UIコンテナがなければ作成
        if (uiContainer == null)
        {
            uiContainer = new GameObject("UIContainer");
            uiContainer.transform.SetParent(transform);

            // レクトトランスフォームを追加（UI配置のため）
            if (uiContainer.GetComponent<RectTransform>() == null)
            {
                uiContainer.AddComponent<RectTransform>();
            }
        }

        // StartPrefabsがあれば生成
        if (stateData.startPrefabs != null && stateData.startPrefabs.Length > 0)
        {
            foreach (var prefab in stateData.startPrefabs)
            {
                if (prefab != null)
                {
                    // カメラ設定付きのインスタンス化を使用
                    GameObject startUI = InstantiateUIWithCamera(prefab, uiContainer.transform);
                    if (startUI != null)
                    {
                        startUIs.Add(startUI);
                    }
                }
            }

            // 必要なボタンイベントをここで登録
            SetupButtons();
        }
    }


    /// <summary>
    /// ボタンイベントの登録
    /// </summary>
    protected virtual void SetupButtons()
    {
        // 子クラスでオーバーライド
    }

    /// <summary>
    /// 生成したUI要素の削除
    /// </summary>
    protected virtual void DestroyEventUI()
    {
        // 開始時UI要素の削除
        foreach (var ui in startUIs)
        {
            if (ui != null)
            {
                Destroy(ui);
            }
        }
        startUIs.Clear();

        // 終了時UI要素の削除
        foreach (var ui in endUIs)
        {
            if (ui != null)
            {
                Destroy(ui);
            }
        }
        endUIs.Clear();
    }

    /// <summary>
    /// 終了時UIの表示
    /// </summary>
    protected virtual void ShowEndUI()
    {
        // すでに終了時UIが生成されている場合は処理しない
        if (endUIs.Count > 0) return;

        // EndPrefabsがあれば生成
        if (stateData.endPrefabs != null && stateData.endPrefabs.Length > 0)
        {
            foreach (var prefab in stateData.endPrefabs)
            {
                if (prefab != null)
                {
                    // カメラ設定付きのインスタンス化を使用
                    GameObject endUI = InstantiateUIWithCamera(prefab, uiContainer.transform);
                    if (endUI != null)
                    {
                        endUIs.Add(endUI);
                    }
                }
            }
        }
    }
}
