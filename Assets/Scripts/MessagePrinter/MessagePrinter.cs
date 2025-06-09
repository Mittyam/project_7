using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class MessagePrinter : MonoBehaviour
{
    public TextMeshProUGUI messageText;       // メッセージ表示用 TextMeshPro
    // public float characterDelay = 0.05f;   // 固定値ではなくConfigUIManagerから取得
    public float messageDisplayTime = 2.0f;   // 表示完了後にメッセージを消すまでの時間
    public bool autoClearMessage = true;      // 一定時間後にメッセージをクリアするか

    private Coroutine currentCoroutine = null;
    private Coroutine clearCoroutine = null;

    /// <summary>
    /// テキストのフル表示（最後の文字まで表示）完了時に呼び出されるイベント
    /// NovelEventManager のオート進行がこのイベントを利用している
    /// </summary>
    public event Action OnTextFullDisplayed;

    /// <summary>
    /// 新しいメッセージを表示する
    /// </summary>
    /// <param name="message">表示したい文章</param>
    public void PrintMessage(string message)
    {
        // すでに表示中のコルーチンがあれば停止
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        // メッセージクリア待ちコルーチンも停止
        if (clearCoroutine != null)
        {
            StopCoroutine(clearCoroutine);
            clearCoroutine = null;
        }

        // 新しいコルーチン開始
        currentCoroutine = StartCoroutine(DisplayMessageRoutine(message));
    }


    /// <summary>
    /// メッセージを表示する（コルーチンを使用しない）
    /// </summary>
    /// <param name="message"></param>
    public void ShowMessage(string message)
    {
        // すでに表示中のコルーチンがあれば停止
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }
        // メッセージクリア待ちコルーチンも停止
        if (clearCoroutine != null)
        {
            StopCoroutine(clearCoroutine);
            clearCoroutine = null;
        }
        // メッセージを即座に表示
        messageText.text = message;
        // 自動でメッセージクリアする設定なら一定時間後にクリア
        if (autoClearMessage)
        {
            clearCoroutine = StartCoroutine(ClearMessageAfterDelay());
        }
    }

    /// <summary>
    /// 1文字ずつ表示していくコルーチン
    /// </summary>
    private IEnumerator DisplayMessageRoutine(string message)
    {
        messageText.text = ""; // 表示をリセット

        int seCounter = 0; // SEカウンター初期化

        // ConfigUIManagerから文字送り速度を取得
        float characterDelay = ConfigUIManager.Instance.TextSpeed;

        // 1文字ずつ追加して表示
        foreach (char c in message)
        {
            messageText.text += c;

            if (seCounter % 2 == 0)
            { // 2文字ごとにSEを鳴らす
                SoundManager.Instance.PlaySE(11);
            }
            seCounter++;

            yield return new WaitForSeconds(characterDelay);
        }

        // 全文字表示完了を通知
        OnTextFullDisplayed?.Invoke();

        // 自動でメッセージクリアする設定なら一定時間後にクリア
        if (autoClearMessage)
        {
            clearCoroutine = StartCoroutine(ClearMessageAfterDelay());
        }

        currentCoroutine = null;
    }

    /// <summary>
    /// 指定時間後にメッセージをクリアする
    /// </summary>
    private IEnumerator ClearMessageAfterDelay()
    {
        yield return new WaitForSeconds(messageDisplayTime);
        messageText.text = "";
        clearCoroutine = null;
    }

    /// <summary>
    /// テキストが最後まで表示されているかどうかを返す
    /// （コルーチンが回っていない状態 = 全表示完了 or 何も表示していない）
    /// </summary>
    public bool IsTextFullyDisplayed()
    {
        // currentCoroutine が null であれば「文字送りが終わっている」状態
        return (currentCoroutine == null);
    }

    /// <summary>
    /// メッセージをクリアする
    /// </summary>
    public void ClearMessage()
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
            clearCoroutine = null;
        }
        messageText.text = "";
    }
}
