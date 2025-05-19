using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ノベル再生用ステート
/// CommandExecutorパターンを採用
/// </summary>
public class NovelState : StateBase
{
    /// <summary>
    /// イベント起動元の種類を定義
    /// </summary>
    private enum EventSource
    {
        TimingTrigger,  // 通常トリガー（昼→夕方など）
        MemoryBrowser,  // 思い出閲覧から
        ManualTrigger,  // 手動トリガー（デバッグ用など）
    }

    public enum PlaybackMode { Click, Auto, Skip }

    [Header("UI コンテナ")]
    [SerializeField] private GameObject novelUIRoot;

    [Header("UI コンポーネント")]
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image characterImage;

    [Header("Live2D")]
    [SerializeField] private GameObject live2DContainer;
    [SerializeField] private Live2DController live2DController;

    [Header("Components")]
    [SerializeField] private NovelRunner novelRunner;
    [SerializeField] private MessagePrinter messagePrinter;

    [Header("Data Assets")]
    [SerializeField] private NovelData novelDataAsset;

    [Header("Default Playback Mode")]
    [SerializeField]
    private PlaybackMode defaultPlaybackMode = PlaybackMode.Click;

    // --- ランタイムフィールド ---
    private NovelEventData currentEventData;
    private bool isPlaybackCompleted = false;
    private EventSource eventSource = EventSource.TimingTrigger;
    private StateID returnToStateID = StateID.None;

    // backgroundImageをpublicにして、他のスクリプトからアクセスできるようにする
    public Image BackgroundImage => backgroundImage;
    public Live2DController Live2DController => live2DController;

    /// <summary> イベントデータをセット </summary>
    public void SetEventData(NovelEventData data)
    {
        currentEventData = data;
    }

    /// <summary>
    /// 思い出閲覧から起動されたことを設定
    /// </summary>
    /// <param name="returnState">再生終了後に戻るステートID</param>
    public void SetAsMemoryEvent(StateID returnState)
    {
        eventSource = EventSource.MemoryBrowser;
        returnToStateID = returnState;
        Debug.Log($"NovelState: 思い出閲覧からの再生として設定。終了後は{returnState}に戻ります");
    }

    /// <summary>
    /// 手動トリガーからの起動として設定（デバッグ用）
    /// </summary>
    public void SetAsManualTrigger()
    {
        eventSource = EventSource.ManualTrigger;
    }

    /// <summary>
    /// UIのセットアップ処理
    /// </summary>
    protected override void SetupUI()
    {
        base.SetupUI();

        // Live2Dコンテナの初期化
        if (live2DContainer == null)
        {
            live2DContainer = new GameObject("Live2DContainer");
            live2DContainer.transform.SetParent(transform);
            live2DContainer.AddComponent<RectTransform>();

            // RectTransformの設定
            RectTransform rectTransform = live2DContainer.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        // Live2Dコントローラの確認
        if (live2DController == null)
        {
            live2DController = live2DContainer.GetComponent<Live2DController>();
            if (live2DController == null)
            {
                live2DController = live2DContainer.AddComponent<Live2DController>();
            }
        }
    }

    /// <summary>
    /// すべてのUIを表示
    /// </summary>
    private void ShowAllUI()
    {
        if (novelUIRoot != null)
        {
            novelUIRoot.SetActive(true);
        }
    }

    /// <summary>
    /// すべてのUIを非表示
    /// </summary>
    private void HideAllUI()
    {
        if (novelUIRoot != null)
        {
            novelUIRoot.SetActive(false);
        }
    }

    /// <summary>
    /// イベントデータからUIを読み込む
    /// </summary>
    private void LoadUIFromEventData(NovelEventData eventData)
    {
        if (eventData == null) return;

        // 背景画像の設定
        SetBackground(eventData.thumbnailImage);

        // すべてのUIを表示
        ShowAllUI();
    }

    /// <summary>
    /// 背景画像を設定する
    /// </summary>
    private void SetBackground(Sprite backgroundSprite)
    {
        if (backgroundImage != null && backgroundSprite != null)
        {
            backgroundImage.sprite = backgroundSprite;
            backgroundImage.gameObject.SetActive(true);
        }
    }

    public override void OnEnter()
    {
        Debug.Log("NovelState: OnEnter");
        isPlaybackCompleted = false; // 再生完了フラグを初期化

        // UIのセットアップ
        SetupUI();
        ShowAllUI();

        // イベント発火
        if (currentEventData != null)
        {
            TypedEventManager.Instance.Publish(new GameEvents.NovelEventStarted
            {
                EventID = currentEventData.eventID,
                EventName = currentEventData.eventName
            });

            // イベントデータからUIをロード
            LoadUIFromEventData(currentEventData);
        }
        else
        {
            Debug.LogError("NovelState: EventData が設定されていません。");
            CompletePlayback();
            return;
        }

        // イベントID からセクションリストを取得
        List<EventEntity> sectionList = GetSectionListByEventID(currentEventData.eventID);
        if (sectionList == null || sectionList.Count == 0)
        {
            Debug.LogError($"NovelState: EventID {currentEventData.eventID} のデータが見つかりません。");
            CompletePlayback();
            return;
        }

        // NovelRunner でイベント再生開始
        if (novelRunner != null)
        {
            StartCoroutine(RunNovelEvent(sectionList));
        }
        else
        {
            Debug.LogError("NovelState: NovelRunner がアタッチされていません。");
            CompletePlayback();
        }
    }

    public override void OnUpdate()
    {
        // 再生終了していたらPop
        if (isPlaybackCompleted)
        {
            CompletePlayback();
        }
    }

    public override void OnExit()
    {
        Debug.Log("NovelState: OnExit");
        StopAllCoroutines();

        // Live2Dモデルをクリア
        if (live2DController != null)
        {
            live2DController.DeleteAllModels();
        }

        // 全ての音声を停止
        SoundManager.Instance.StopVoice();
        SoundManager.Instance.StopAllSE();
        SoundManager.Instance.StopBGMWithFadeOut(2f);

        // ステータスの変化を適用
        if (currentEventData != null)
        {
            // イベント発行
            TypedEventManager.Instance.Publish(new GameEvents.NovelEventCompleted
            {
                EventID = currentEventData.eventID,
                EventName = currentEventData.eventName,
                ReturnStateID = returnToStateID
            });

            // 思い出として解放する設定なら
            if (currentEventData.unlockAsMemory)
            {
                // すでにCompletedになっているのでここで特に処理不要
            }
        }

        // すべてのUIを非表示
        HideAllUI();
    }

    /// <summary>
    /// ノベルイベントの再生処理
    /// </summary>
    private IEnumerator RunNovelEvent(List<EventEntity> sectionList)
    {
        yield return StartCoroutine(novelRunner.RunEvent(sectionList, defaultPlaybackMode));

        // 再生完了
        isPlaybackCompleted = true;
    }

    /// <summary>
    /// ノベルイベント再生完了時の処理
    /// </summary>
    private void CompletePlayback()
    {
        // ステータス変化を適用
        ApplyStatusChanges();

        // イベントをCompletedに
        if (currentEventData != null)
        {
            ProgressManager.Instance.CompleteEvent(currentEventData.eventID);
            Debug.Log($"NovelState: イベントID {currentEventData.eventID} を完了済みにしました");
        }

        // ノベルイベント自体をpop
        PushdownStack.Pop();

        // 遷移先指定ロジック（イベント起動元によって異なる）
        switch (eventSource)
        {
            case EventSource.MemoryBrowser:
                // 思い出閲覧からの場合は保存していた元のステートに戻る
                if (returnToStateID != StateID.None)
                {
                    Debug.Log($"NovelState: 思い出閲覧からの再生が完了しました。{returnToStateID} に戻ります");
                    MainStateMachine.ChangeState(returnToStateID);
                }
                break;

            case EventSource.ManualTrigger:
                // 手動トリガーの場合は特に何もしない
                Debug.Log("NovelState: 手動トリガーからの再生が完了しました");
                break;

            case EventSource.TimingTrigger:
            default:
                // 定期トリガーからの場合はイベントデータの設定に従う
                if (currentEventData != null && currentEventData.nextStateID != StateID.None)
                {
                    Debug.Log($"NovelState: 定期イベントの再生が完了しました。{currentEventData.nextStateID} に遷移します");
                    MainStateMachine.ChangeState(currentEventData.nextStateID);
                }
                break;
        }
    }

    /// <summary>
    /// イベントIDからセクションリストを取得
    /// </summary>
    private List<EventEntity> GetSectionListByEventID(int id)
    {
        // リファクタリング後：EventEntityのリストを直接返す
        return id switch
        {
            1 => novelDataAsset.Eve1,
            2 => novelDataAsset.Eve2,
            3 => novelDataAsset.Eve3,
            4 => novelDataAsset.Eve4,
            5 => novelDataAsset.Eve5,
            6 => novelDataAsset.Eve6,
            7 => novelDataAsset.Eve7,
            8 => novelDataAsset.Eve8,
            9 => novelDataAsset.Eve9,
            10 => novelDataAsset.Eve10,
            _ => null
        };
    }

    /// <summary>
    /// ステータス変化の適用
    /// </summary>
    protected void ApplyStatusChanges()
    {
        if (currentEventData != null)
        {
            StatusManager.Instance.UpdateStatus(
                0,  // 日付変化なし
                currentEventData.affectionChange,
                currentEventData.loveChange,
                currentEventData.moneyChange
            );

            Debug.Log($"NovelState: ステータス変化を適用 - 好感度:{currentEventData.affectionChange}, 恋情:{currentEventData.loveChange}, お金:{currentEventData.moneyChange}");
        }
    }

    /// <summary>
    /// 再生モード（自動/スキップ）の変更
    /// </summary>
    public void SetPlaybackMode(NovelState.PlaybackMode mode)
    {
        // UI要素を更新（トグルボタンの状態など）
        switch (mode)
        {
            case NovelState.PlaybackMode.Click:
                // クリック待ちモードのUIセットアップ
                break;
            case NovelState.PlaybackMode.Auto:
                // 自動モードのUIセットアップ
                break;
            case NovelState.PlaybackMode.Skip:
                // スキップモードのUIセットアップ
                break;
        }
    }

    /// <summary>
    /// イベントを強制的に終了する（デバッグ用）
    /// </summary>
    public void ForceComplete()
    {
        // 実行中のコルーチンを停止
        StopAllCoroutines();

        // 再生完了フラグをセット
        isPlaybackCompleted = true;

        // 終了処理を呼び出し
        CompletePlayback();

        Debug.Log("NovelState: イベントを強制終了しました");
    }
}