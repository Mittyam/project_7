using UnityEngine;
using DG.Tweening;
using System;

/// <summary>
/// メッセージパネル制御クラス
/// </summary>
public class MessagePanelController : MonoBehaviour
{
    [Header("パネル参照")]
    [SerializeField] private RectTransform messagePanel;
    [SerializeField] private RectTransform selectPanel;

    [Header("アニメーション設定")]
    [SerializeField] private float animDuration = 0.3f;
    [SerializeField] private float choicePanelHideOffset = -300f;

    // 初期位置の保存
    private Vector2 initialMessagePanelPos;
    private Vector2 initialChoicePanelPos;
    private bool isInitialized = false;

    // Tweenキャンセル用フラグ
    private bool cancelMessageTween = false;

    private void Awake()
    {
        InitializePanels();
    }

    private void OnEnable()
    {
        InitializePanels();
    }

    // パネルの初期化
    private void InitializePanels()
    {
        if (isInitialized) return;

        // 初期位置を記録
        if (messagePanel != null)
            initialMessagePanelPos = messagePanel.anchoredPosition;

        if (selectPanel != null)
            initialChoicePanelPos = selectPanel.anchoredPosition;

        // MessagePanelは初期状態で非表示
        if (messagePanel != null)
            messagePanel.gameObject.SetActive(false);

        isInitialized = true;
    }

    // メッセージパネルを表示
    public void ShowMessagePanel(Action onComplete = null)
    {
        // 初期化確認
        if (!isInitialized)
            InitializePanels();

        // すでに表示されている場合は何もしない
        if (messagePanel != null && messagePanel.gameObject.activeSelf)
        {
            onComplete?.Invoke();
            return;
        }

        cancelMessageTween = false;

        // 選択肢パネルを非表示
        if (selectPanel != null)
        {
            // 位置をリセット
            selectPanel.anchoredPosition = initialChoicePanelPos;

            // アニメーションで下に移動
            selectPanel
                .DOAnchorPosY(initialChoicePanelPos.y + choicePanelHideOffset, animDuration)
                .SetEase(Ease.InQuad)
                .OnComplete(() => {
                    selectPanel.gameObject.SetActive(false);

                    // キャンセルされた場合は処理中断
                    if (cancelMessageTween)
                        return;

                    // メッセージパネルを表示
                    if (messagePanel != null)
                    {
                        // 位置をリセット
                        messagePanel.anchoredPosition = initialMessagePanelPos;
                        messagePanel.gameObject.SetActive(true);
                        messagePanel.localScale = new Vector3(1, 0, 1);

                        // アニメーションで表示
                        messagePanel
                            .DOScaleY(1f, animDuration)
                            .SetEase(Ease.OutQuad)
                            .OnComplete(() => {
                                if (!cancelMessageTween)
                                {
                                    onComplete?.Invoke();
                                }
                            });
                    }
                });
        }
    }

    // 選択肢パネルを表示
    public void ShowChoicePanel(Action onComplete = null)
    {
        // 初期化確認
        if (!isInitialized)
            InitializePanels();

        // すでに表示状態で正しい位置にある場合は何もしない
        if (selectPanel != null && selectPanel.gameObject.activeSelf &&
            Mathf.Abs(selectPanel.anchoredPosition.y - initialChoicePanelPos.y) < 0.1f)
        {
            onComplete?.Invoke();
            return;
        }

        cancelMessageTween = true;
        DOTween.Kill(messagePanel);
        DOTween.Kill(selectPanel);

        // メッセージパネルを非表示
        if (messagePanel != null)
            messagePanel.gameObject.SetActive(false);

        // 選択肢パネルをリセット
        if (selectPanel != null)
        {
            selectPanel.anchoredPosition = new Vector2(initialChoicePanelPos.x, initialChoicePanelPos.y + choicePanelHideOffset);
            selectPanel.gameObject.SetActive(true);

            // アニメーションで元の位置に戻す
            selectPanel
                .DOAnchorPosY(initialChoicePanelPos.y, animDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => {
                    onComplete?.Invoke();
                });
        }
    }
}
