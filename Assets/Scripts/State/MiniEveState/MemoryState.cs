using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 思い出閲覧用ミニイベントステート
/// </summary>
public class MemoryState : MiniEventState
{
    [Header("Memory UI References")]
    [SerializeField] private Transform memoryListContainer;
    [SerializeField] private Button memoryButtonPrefab;
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject noMemoriesMessage;  // 思い出がない場合に表示するメッセージ

    [Header("ProgressManager")]
    [SerializeField] private ProgressManager progressManager;

    private List<Button> createButtons = new List<Button>();

    public override void OnEnter()
    {
        base.OnEnter();

        Debug.Log("MemoryState: 思い出閲覧を開始します。解放済みノベルイベントを一覧表示します。");

        if (progressManager == null)
        {
            Debug.LogError("MemoryState: ProgressManagerが見つかりません");
            CompleteEvent();
            return;
        }

        // 閉じるボタンのイベント登録
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CompleteEvent);
        }

        // メッセージの初期化
        if (noMemoriesMessage != null)
        {
            noMemoriesMessage.SetActive(false);
        }

        // 解放済みイベントのボタンを動的生成
        GenerateMemoryButtons();
    }

    public override void OnExit()
    {
        // イベントリスナーの解除
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CompleteEvent);
        }

        // 生成したボタンのクリーンアップ
        foreach (var button in createButtons)
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
            }
        }
        createButtons.Clear();

        base.OnExit();
    }

    /// <summary>
    /// 解放済みノベルイベントのボタンを動的生成
    /// </summary>
    private void GenerateMemoryButtons()
    {
        if (memoryListContainer == null || memoryButtonPrefab == null)
        {
            Debug.LogError("MemoryState: 必要なUI要素が正しく設定されていません");
            return;
        }

        // 既存のボタンをクリア
        foreach (Transform child in memoryListContainer)
        {
            Destroy(child.gameObject);
        }

        // Resourceフォルダからすべてのイベントデータを取得
        NovelEventData[] allEvents = Resources.LoadAll<NovelEventData>("Events");
        int buttonCount = 0;

        foreach (var eventData in allEvents)
        {
            // 思い出として解放する設定かつCompletedになっているイベントのみ表示
            if (eventData.unlockAsMemory &&
                progressManager.GetEventState(eventData.eventID) == EventState.Completed)
            {
                Button newButton = Instantiate(memoryButtonPrefab, memoryListContainer);
                createButtons.Add(newButton);
                buttonCount++;

                // ボタンテキストの設定
                Text buttonText = newButton.GetComponentInChildren<Text>();
                if (buttonText != null)
                {
                    buttonText.text = eventData.eventName;
                }

                // サムネイル画像の設定（あれば）
                Image thumbnailImage = newButton.GetComponentInChildren<Image>();
                if (thumbnailImage != null && eventData.thumbnailImage != null)
                {
                    thumbnailImage.sprite = eventData.thumbnailImage;
                }

                // イベントデータを一時保存するためにローカル変数にコピー
                NovelEventData eventDataCopy = eventData;

                // ボタンクリック時の処理を登録
                newButton.onClick.AddListener(() => OnMemorySelected(eventDataCopy));
            }
        }

        // 解放済みイベントがない場合のメッセージ表示
        if (buttonCount == 0 && noMemoriesMessage != null)
        {
            noMemoriesMessage.SetActive(true);
            Debug.Log("解放済みノベルイベントはありません。");
        }
    }

    /// <summary>
    /// 思い出イベントが選択された時の処理
    /// </summary>
    /// <param name="eventData">選択されたイベントデータ</param>
    private void OnMemorySelected(NovelEventData eventData)
    {
        Debug.Log($"思い出イベント「{eventData.eventName}」が選択されました（ID:{eventData.eventID}）");

        // 現在のメインステートIDを取得（戻り先として保存）
        StateID currentMainStateID = MainStateMachine.CurrentStateID;

        // 一旦MemoryStateをpop
        PushdownStack.Pop();

        // NovelState作成の処理は次フレームに遅延
        StartCoroutine(CreateAndPushNovelStateNextFrame(eventData, currentMainStateID));
    }

    /// <summary>
    /// NovelStateの作成と登録を次フレームに遅延して実行
    /// （同一フレーム内でPopとPushを行うとエラーになる場合に対応）
    /// </summary>
    private IEnumerator CreateAndPushNovelStateNextFrame(NovelEventData eventData, StateID returnStateID)
    {
        // 1フレーム待機
        yield return null;

        // StatesContainerからノベルステートを取得
        NovelState novelState = GameLoop.Instance.StatesContainer.GetNovelState();

        // 必要なデータを設定
        novelState.SetEventData(eventData);
        novelState.SetAsMemoryEvent(returnStateID); // 思い出からの再生であることと戻り先を設定

        // PushdownStackにプッシュ
        GameLoop.Instance.PushdownStack.Push(novelState);

        Debug.Log($"思い出「{eventData.eventName}」のNovelStateを取得し、プッシュしました。再生後は{returnStateID}に戻ります。");
    }
}
