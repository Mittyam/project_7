using UnityEngine;
using DG.Tweening;
using System;

public class MessagePanelController : MonoBehaviour
{
    [Header("パネル参照")]
    [SerializeField] private RectTransform messagePanel;
    [SerializeField] private RectTransform selectPanel;

    [Header("アニメーション設定")]
    [SerializeField] private float animDuration = 0.3f;
    [SerializeField] private float choicePanelHideOffset = -300f; // 下へ移動させるオフセット

    // 選択肢パネルの初期位置を記録する変数
    private Vector2 initialChoicePanelPos;

    // Tweenキャンセル用フラグ
    private bool cancelMessageTween = false;

    private void Start()
    {
        // 初期位置を記録
        // initialChoicePanelPos = choicePanel.anchoredPosition;

        // MessagePanelは初期状態で非表示
        messagePanel.gameObject.SetActive(false);
    }

    /// <summary>
    /// メッセージパネルを表示し、選択肢パネルを下にアニメーションで隠す
    /// </summary>
    public void ShowMessagePanel(Action onComplete = null)
    {
        // 既に表示されている場合は何もしない
        if (messagePanel.gameObject.activeSelf) return;

        // 開始時はキャンセル状態にしていない
        cancelMessageTween = false;

        // 1) 選択肢パネルを初期位置から下方向へ移動させ、完了後非表示にする
        selectPanel
            .DOAnchorPosY(initialChoicePanelPos.y + choicePanelHideOffset, animDuration)
            .SetEase(Ease.InQuad)
            .OnComplete(() =>
            {
                selectPanel.gameObject.SetActive(false);

                // キャンセルされている場合は処理を中断
                if (cancelMessageTween)
                    return;

                // 2) メッセージパネルをアクティブにして、Yスケールを0に設定
                messagePanel.gameObject.SetActive(true);
                messagePanel.localScale = new Vector3(1, 0, 1);

                // 3) MessagePanelを上辺を基準に下へ伸びる（スケールYを0→1に補間）アニメーションで表示
                messagePanel
                    .DOScaleY(1f, animDuration)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        // キャンセルされていない場合のみonCompleteを呼ぶ
                        if (!cancelMessageTween)
                        {
                            onComplete?.Invoke();
                        }
                    });
            });
    }

    /// <summary>
    /// メッセージパネルが閉じた後に選択肢パネルを再表示する
    /// </summary>
    public void ShowChoicePanel(Action onComplete = null)
    {
        // もし choicePanel が既にアクティブで、かつ正しい初期位置にあるなら
        // 追加のアニメーションは不要として、即座に onComplete を呼び出す
        if (selectPanel.gameObject.activeSelf && Mathf.Abs(selectPanel.anchoredPosition.y - initialChoicePanelPos.y) < 0.1f)
        {
            onComplete?.Invoke();
            return;
        }

        cancelMessageTween = true;
        DOTween.Kill(messagePanel);
        DOTween.Kill(selectPanel);
        messagePanel.gameObject.SetActive(false);

        // choicePanel を「閉じた状態」の位置にリセット
        selectPanel.anchoredPosition = new Vector2(initialChoicePanelPos.x, initialChoicePanelPos.y + choicePanelHideOffset);
        selectPanel.gameObject.SetActive(true);

        // choicePanel を初期位置にアニメーションで移動する
        selectPanel
            .DOAnchorPosY(initialChoicePanelPos.y, animDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                onComplete?.Invoke();
            });
    }
}
