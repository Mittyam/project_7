using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// アニメーションのステートを統括して扱う
/// 図書館、カフェ、バイト、散歩、ゲーム、お出かけ、お話、睡眠
/// </summary>
public class ActionState : MiniEventState
{
    [Header("Talk UI References")]
    [SerializeField] private List<Button> talkOptionButtons = new List<Button>();
    [SerializeField] private Button closeButton;

    private int selectedTopicID = -1;

    // アクションタイプ
    protected string actionType;

    public override void OnEnter()
    {
        // 親クラスのOnEnterを呼び出し
        base.OnEnter();

        Debug.Log("ActionState: 入ります。");

        // stateDataがnullの場合は早期リターン（親クラスでチェック済み）
        if (stateData == null) return;

        Debug.Log("ActionState: 会話オプションを表示します。");

        // パラメータからアクションタイプを取得
        actionType = GetParameter<string>("ActionType", "");

        // アクションタイプに基づいた処理
        switch (actionType)
        {
            case "Library":
                // 図書館アクション処理
                Debug.Log("図書館でのアクション開始");
                break;

            case "Cafe":
                // カフェアクション処理
                Debug.Log("カフェでのアクション開始");
                break;

            case "Work":
                // バイトアクション処理
                Debug.Log("バイトでのアクション開始");
                break;

            case "Walk":
                // 散歩アクション処理
                Debug.Log("散歩でのアクション開始");
                break;

            case "Game":
                // ゲームアクション処理
                Debug.Log("ゲームでのアクション開始");
                break;

            case "Talk":
                // お話アクション処理
                Debug.Log("お話アクション開始");
                break;

            case "Outing":
                // お出かけアクション処理
                Debug.Log("お出かけアクション開始");
                break;

            case "Sleep":
                // 睡眠アクション処理
                Debug.Log("睡眠アクション開始");
                break;

            default:
                Debug.LogWarning($"不明なアクションタイプ: {actionType}");
                break;
        }

        // ボタンイベントの登録
        SetupButtons();
    }

    public override void OnExit()
    {
        // イベントリスナーの解除
        //if (closeButton != null)
        //{
        //    closeButton.onClick.RemoveListener(CompleteEvent);
        //}

        //if (talkOptionButtons != null)
        //{
        //    for (int i = 0; i < talkOptionButtons.Length; i++)
        //    {
        //        talkOptionButtons[i].onClick.RemoveAllListeners();
        //    }
        //}

        base.OnExit();
    }

    /// <summary>
    /// 会話トピックの選択
    /// </summary>
    private void SelectTopic(int topicID)
    {
        selectedTopicID = topicID;
        Debug.Log($"会話トピック {topicID} が選択されました");

        // 選択したトピックに応じた処理
        // 例：　ランダムな好感度上昇など
        int affectionChange = Random.Range(1, 4);
        StatusManager.Instance.UpdateStatus(0, affectionChange, 0, 0);

        // TODO: 選択したトピックに応じた会話テキストの表示処理

        // 終了時UIの表示（もしあれば）
        ShowEndUI();

        // 選択後はすぐに閉じるか、もう一度選択可能にするかなど
        // ここでは単純化のため、選択したらイベントを終了する
        StartCoroutine(CompleteEventAfterDelay(2.0f));
    }

    /// <summary>
    /// 遅延後にイベントを完了させる
    /// </summary>
    private IEnumerator CompleteEventAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        CompleteEvent();
    }

    /// <summary>
    /// ボタンのセットアップ
    /// </summary>
    private void SetupButtons()
    {
        // closeButtonのチェック
        if (closeButton == null)
        {
            // ContentContainer内から探す
            //closeButton = contentContainer?.transform.Find("CloseButton")?.GetComponent<Button>();

            //if (closeButton == null)
            //{
            //    Debug.LogWarning("TalkState: CloseButtonが見つかりません");
            //}
            //else
            //{
            //    closeButton.onClick.AddListener(CompleteEvent);
            //}
        }
        else
        {
            //closeButton.onClick.AddListener(CompleteEvent);
        }

        // talkOptionButtonsのチェック
        if (talkOptionButtons == null || talkOptionButtons.Count == 0)
        {
            // ContentContainer内からボタンを探す
            //Button[] foundButtons = contentContainer?.GetComponentsInChildren<Button>();
            //if (foundButtons != null && foundButtons.Length > 0)
            //{
            //    // CloseButton以外のボタンをtalkOptionButtonsとして使用
            //    List<Button> options = new List<Button>();
            //    foreach (var button in foundButtons)
            //    {
            //        if (button != closeButton)
            //        {
            //            options.Add(button);
            //        }
            //    }
            //    talkOptionButtons = options.ToArray();
            //}
        }

        // 会話オプションボタンのセットアップ
        if (talkOptionButtons != null && talkOptionButtons.Count > 0)
        {
            //for (int i = 0; i < talkOptionButtons.Length; i++)
            //{
            //    if (talkOptionButtons[i] != null)
            //    {
            //        int topicID = i; // ローカル変数にコピー
            //        talkOptionButtons[i].onClick.RemoveAllListeners();
            //        talkOptionButtons[i].onClick.AddListener(() => SelectTopic(topicID));
            //    }
            //}
        }
        else
        {
            Debug.LogWarning("TalkState: 会話オプションボタンが設定されていません");
        }
    }
}
