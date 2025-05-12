using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class HoliDayUIManager : MonoBehaviour
{
    [Header("UI参照")]
    [SerializeField] private DayState dayState;

    [Header("メッセージパネル参照")]
    [SerializeField] private AnimationTriggerState animationTriggerState;
    [SerializeField] private MessagePanelController messagePanel;
    [SerializeField] private MessagePrinter messagePrinter;

    [Header("選択肢パネルを表示")]
    [SerializeField] private ChoiceButtonGenerator choicePanel;

    [Header("ボタン参照")]
    [SerializeField] private Button gameButton;
    [SerializeField] private Button outingButton;
    [SerializeField] private Button workButton;
    [SerializeField] private Button talkButton;
    [SerializeField] private Button itemButton;
    [SerializeField] private Button memoryButton;

    [Header("InfoPanel参照")]
    [SerializeField] private InfoPanelController infoPanel;

    [Header("サブステートマシン")]
    [SerializeField] private SubStateMachine subStateMachine;

    [Header("サブイベントID")]
    [SerializeField] private int gameEventID;
    [SerializeField] private int outingEventID;
    [SerializeField] private int workEventID;
    [SerializeField] private int talkEventID;

    private void Start()
    {
        //gameButton.onClick.AddListener(OnGameButtonClicked);
        //outingButton.onClick.AddListener(OnOutingButtonClicked);
        //workButton.onClick.AddListener(OnWorkButtonClicked);
        //talkButton.onClick.AddListener(OnTalkButtonClicked);
        //itemButton.onClick.AddListener(OnItemButtonClicked);
        //memoryButton.onClick.AddListener(OnMemoryButtonClicked);

        // AnimationTriggerState からの完了通知を購読（OnAnimationFinished イベント）
        if (animationTriggerState != null)
        {
            animationTriggerState.OnAnimationFinished += OnAnimationFinishedHandler;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 選択肢を非表示にする
            choicePanel.ClosePanel();
            // メッセージパネルを非表示にする
            messagePanel.ShowChoicePanel(() =>
            {
                messagePrinter.ClearMessage();
            });
        }
    }

    private void OnGameButtonClicked()
    {
        // アイテムにゲームがあるか確認
        // ゲームがあるときとないときで分岐
        // ゲームが選択されたらアイテムからゲームを1つ消費

        infoPanel.ResetMenuPanel();

        messagePanel.ShowMessagePanel(() =>
        {
            messagePrinter.PrintMessage("この前おもちゃ屋で買ってきたゲーム、○○もやるかな。誘ってみるか。");
        });

        var choices = new List<ChoiceOption>();

        choices.Add(new ChoiceOption
        {
            choiceText = "○○と一緒にゲームで遊ぶ",
            onSelected = () =>
            {
                // ゲームを１個消費
                int itemId = 1; // gameはitemID=1と仮定
                StatusManager.Instance.DecreaseItem(itemId);
                // サブイベントを開始
                subStateMachine.
                    StartSubEvent(gameEventID, EventType.Animation);
                messagePanel.ShowChoicePanel(() =>
                {
                    messagePrinter.ClearMessage();
                });
            }
        });

        choices.Add(new ChoiceOption
        {
            choiceText = "やっぱりやめておく",
            onSelected = () =>
            {
                // サブイベントを開始
                messagePanel.ShowChoicePanel(() =>
                {
                    messagePrinter.ClearMessage();
                });
            }
        });

        // 選択肢を表示
        choicePanel.ShowChoices(choices);
    }

    private void OnOutingButtonClicked()
    {
        infoPanel.ResetMenuPanel();

        messagePanel.ShowMessagePanel(() =>
        {
            messagePrinter.PrintMessage("○○が学校以外で出かけてる所をみないな。外に連れてってみるか。");
        });

        var choices = new List<ChoiceOption>();

        choices.Add(new ChoiceOption
        {
            choiceText = "○○と一緒に出かける",
            onSelected = () =>
            {
                // サブイベントを開始
                subStateMachine.
                    StartSubEvent(outingEventID, EventType.Animation);
                messagePanel.ShowChoicePanel(() =>
                {
                    messagePrinter.ClearMessage();
                });
            }
        });

        choices.Add(new ChoiceOption
        {
            choiceText = "今は別のことをしよう",
            onSelected = () =>
            {
                // ゲームを消費しない
                // サブイベントを開始
                messagePanel.ShowChoicePanel(() =>
                {
                    messagePrinter.ClearMessage();
                });
            }
        });

        // 選択肢を表示
        choicePanel.ShowChoices(choices);
    }

    private void OnWorkButtonClicked()
    {
        infoPanel.ResetMenuPanel();

        messagePanel.ShowMessagePanel(() =>
        {
            messagePrinter.PrintMessage("今日のバイトの予定を確認してみよう。");
        });

        var choices = new List<ChoiceOption>();

        choices.Add(new ChoiceOption
        {
            choiceText = "バイトに行く",
            onSelected = () =>
            {
                // サブイベントを開始
                subStateMachine.
                    StartSubEvent(workEventID, EventType.Animation);
                messagePanel.ShowChoicePanel(() =>
                {
                    messagePrinter.ClearMessage();
                });
            }
        });

        choices.Add(new ChoiceOption
        {
            choiceText = "今日は入ってないみたいだ",
            onSelected = () =>
            {
                // ゲームを消費しない
                // サブイベントを開始
                messagePanel.ShowChoicePanel(() =>
                {
                    messagePrinter.ClearMessage();
                });
            }
        });

        // 選択肢を表示
        choicePanel.ShowChoices(choices);
    }

    private void OnTalkButtonClicked()
    {
        infoPanel.ResetMenuPanel();

        messagePanel.ShowMessagePanel(() =>
        {
            messagePrinter.PrintMessage("最近の学校の調子について聞いてみるか。");
        });

        var choices = new List<ChoiceOption>();

        choices.Add(new ChoiceOption
        {
            choiceText = "○○とお話する",
            onSelected = () =>
            {
                // サブイベントを開始
                subStateMachine.
                    StartSubEvent(talkEventID, EventType.Animation);
                messagePanel.ShowChoicePanel(() =>
                {
                    messagePrinter.ClearMessage();
                });
            }
        });

        choices.Add(new ChoiceOption
        {
            choiceText = "今はそっとしておこう",
            onSelected = () =>
            {
                // ゲームを消費しない
                // サブイベントを開始
                messagePanel.ShowChoicePanel(() =>
                {
                    messagePrinter.ClearMessage();
                });
            }
        });

        // 選択肢を表示
        choicePanel.ShowChoices(choices);
    }

    private void OnItemButtonClicked()
    {

    }

    private void OnMemoryButtonClicked()
    {

    }

    private void PlayAnimation(string spriteName)
    {

    }

    private void ShowItemUI()
    {

    }

    private void ShowMemoryList()
    {

    }

    // アニメーション終了時に呼ばれるコールバック
    private void OnAnimationFinishedHandler()
    {
        // AnimationTriggerState 側で設定した直近のステータス変更内容を取得
        var statusChange = animationTriggerState.LastStatusChange;
        if (statusChange != null)
        {
            // ここでは好感度 (lovelityChange) が 1 以上の上昇の場合の例
            if (statusChange.lovelityChange > 0)
            {
                string message = $"好感度が{statusChange.lovelityChange}上がりました。";
                // メッセージパネルを表示し、メッセージを印字
                messagePanel.ShowMessagePanel(() =>
                {
                    messagePrinter.PrintMessage(message);
                });

                // 2秒後にchoicePanelの表示に戻す処理を開始
                StartCoroutine(ShowChoiceAfterDelay(2.5f));
            }
        }
    }

    // 指定秒数待機後に choicePanel を再表示するコルーチン
    private IEnumerator ShowChoiceAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        messagePanel.ShowChoicePanel(() =>
        {
            // オプション: メッセージクリアなど必要ならばここで行う
            messagePrinter.ClearMessage();
        });
    }

    private void OnDestroy()
    {
        if (animationTriggerState != null)
        {
            animationTriggerState.OnAnimationFinished -= OnAnimationFinishedHandler;
        }
    }
}
