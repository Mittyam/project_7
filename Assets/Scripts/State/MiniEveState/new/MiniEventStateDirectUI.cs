using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI要素を直接管理するミニイベントステートのプロトタイプ
/// </summary>
public class MiniEventStateDirectUI : StateBase, IPausableState
{
    [Header("State Data")]
    [SerializeField] protected MiniEventStateData stateData;

    [Header("UI References")]
    [SerializeField] protected GameObject uiRootContainer;
    [SerializeField] protected GameObject contentContainer;
    [SerializeField] protected Button closeButton;

    // StateDataの公開
    public MiniEventStateData StateData => stateData;

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
            Debug.LogError($"MiniEventStateDirectUI: {gameObject.name}にStateDataが設定されていません");
            // エラー時は安全に終了
            CompleteEvent();
            return;
        }

        Debug.Log($"MiniEventStateDirectUI: {stateData.displayName} を開始します");

        // UI要素の初期設定
        SetupUI();

        // イベント発行
        TypedEventManager.Instance.Publish(new GameEvents.MiniEventStarted
        {
            EventStateID = stateData.stateID,
            EventName = stateData.displayName
        });

        // アクションポイントの消費
        if (stateData.consumeActionPoint)
        {
            StatusManager.Instance.ConsumeActionPoint(stateData.actionPointCost);
        }

        // ステートに応じたUI表示処理
        UpdateUIForState();
    }

    /// <summary>
    /// UI要素の初期設定
    /// </summary>
    protected virtual void SetupUI()
    {
        // UI要素の有効化
        if (uiRootContainer != null)
        {
            uiRootContainer.SetActive(true);
        }

        // コンテンツコンテナの有効化
        if (contentContainer != null)
        {
            contentContainer.SetActive(true);
        }

        // 閉じるボタンのイベント登録
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CompleteEvent);
        }
    }

    /// <summary>
    /// ステートに応じたUI表示処理
    /// </summary>
    protected virtual void UpdateUIForState()
    {
        // StateDataに基づくUI表示の更新
        //if (stateData != null)
        //{
        //    // 開始時プレハブがあれば表示
        //    if (stateData.startPrefab != null && contentContainer != null)
        //    {
        //        GameObject startUI = Instantiate(stateData.startPrefab, contentContainer.transform);
        //        startUI.SetActive(true);
        //    }
        //}
    }

    public override void OnUpdate()
    {
        // ミニイベント中の更新処理
    }

    public override void OnExit()
    {
        Debug.Log($"MiniEventStateDirectUI: {stateData?.displayName} を終了します");

        // ステータス変化の適用
        ApplyStatusChanges();

        // UI表示をクリア
        ClearAllUI();

        // イベント発行
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
    }

    public void OnPause()
    {
        Debug.Log($"MiniEventStateDirectUI: {stateData?.displayName} を一時停止します");

        // UI要素を非表示にする
        if (uiRootContainer != null)
        {
            uiRootContainer.SetActive(false);
        }
    }

    public void OnResume()
    {
        Debug.Log($"MiniEventStateDirectUI: {stateData?.displayName} を再開します");

        // UI要素を再表示
        if (uiRootContainer != null)
        {
            uiRootContainer.SetActive(true);
        }

        // 状態データを再ロード
        UpdateUIForState();
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
        if (stateData != null)
        {
            StatusManager.Instance.UpdateStatus(
                    0,  //日付変化なし
                    stateData.affectionChange,
                    stateData.loveChange,
                    stateData.moneyChange
                );
        }
    }

    /// <summary>
    /// すべてのUI要素を非表示にする
    /// </summary>
    protected virtual void ClearAllUI()
    {
        // コンテンツコンテナ内の子オブジェクトをすべて削除
        if (contentContainer != null)
        {
            foreach (Transform child in contentContainer.transform)
            {
                Destroy(child.gameObject);
            }
        }

        // UIルートを非表示
        if (uiRootContainer != null)
        {
            uiRootContainer.SetActive(false);
        }
    }

    /// <summary>
    /// 特定のUI要素を取得するヘルパーメソッド
    /// </summary>
    protected T GetUIComponent<T>(string relativePath) where T : Component
    {
        if (uiRootContainer == null)
            return null;

        Transform target = uiRootContainer.transform.Find(relativePath);
        if (target == null)
            return null;

        return target.GetComponent<T>();
    }
}
