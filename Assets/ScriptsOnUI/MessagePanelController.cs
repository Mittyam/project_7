using UnityEngine;
using DG.Tweening;
using System;

/// <summary>
/// ���b�Z�[�W�p�l������N���X
/// </summary>
public class MessagePanelController : MonoBehaviour
{
    [Header("�p�l���Q��")]
    [SerializeField] private RectTransform messagePanel;
    [SerializeField] private RectTransform selectPanel;

    [Header("�A�j���[�V�����ݒ�")]
    [SerializeField] private float animDuration = 0.3f;
    [SerializeField] private float choicePanelHideOffset = -300f;

    // �����ʒu�̕ۑ�
    private Vector2 initialMessagePanelPos;
    private Vector2 initialChoicePanelPos;
    private bool isInitialized = false;

    // Tween�L�����Z���p�t���O
    private bool cancelMessageTween = false;

    private void Awake()
    {
        InitializePanels();
    }

    private void OnEnable()
    {
        InitializePanels();
    }

    // �p�l���̏�����
    private void InitializePanels()
    {
        if (isInitialized) return;

        // �����ʒu���L�^
        if (messagePanel != null)
            initialMessagePanelPos = messagePanel.anchoredPosition;

        if (selectPanel != null)
            initialChoicePanelPos = selectPanel.anchoredPosition;

        // MessagePanel�͏�����ԂŔ�\��
        if (messagePanel != null)
            messagePanel.gameObject.SetActive(false);

        isInitialized = true;
    }

    // ���b�Z�[�W�p�l����\��
    public void ShowMessagePanel(Action onComplete = null)
    {
        // �������m�F
        if (!isInitialized)
            InitializePanels();

        // ���łɕ\������Ă���ꍇ�͉������Ȃ�
        if (messagePanel != null && messagePanel.gameObject.activeSelf)
        {
            onComplete?.Invoke();
            return;
        }

        cancelMessageTween = false;

        // �I�����p�l�����\��
        if (selectPanel != null)
        {
            // �ʒu�����Z�b�g
            selectPanel.anchoredPosition = initialChoicePanelPos;

            // �A�j���[�V�����ŉ��Ɉړ�
            selectPanel
                .DOAnchorPosY(initialChoicePanelPos.y + choicePanelHideOffset, animDuration)
                .SetEase(Ease.InQuad)
                .OnComplete(() => {
                    selectPanel.gameObject.SetActive(false);

                    // �L�����Z�����ꂽ�ꍇ�͏������f
                    if (cancelMessageTween)
                        return;

                    // ���b�Z�[�W�p�l����\��
                    if (messagePanel != null)
                    {
                        // �ʒu�����Z�b�g
                        messagePanel.anchoredPosition = initialMessagePanelPos;
                        messagePanel.gameObject.SetActive(true);
                        messagePanel.localScale = new Vector3(1, 0, 1);

                        // �A�j���[�V�����ŕ\��
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

    // �I�����p�l����\��
    public void ShowChoicePanel(Action onComplete = null)
    {
        // �������m�F
        if (!isInitialized)
            InitializePanels();

        // ���łɕ\����ԂŐ������ʒu�ɂ���ꍇ�͉������Ȃ�
        if (selectPanel != null && selectPanel.gameObject.activeSelf &&
            Mathf.Abs(selectPanel.anchoredPosition.y - initialChoicePanelPos.y) < 0.1f)
        {
            onComplete?.Invoke();
            return;
        }

        cancelMessageTween = true;
        DOTween.Kill(messagePanel);
        DOTween.Kill(selectPanel);

        // ���b�Z�[�W�p�l�����\��
        if (messagePanel != null)
            messagePanel.gameObject.SetActive(false);

        // �I�����p�l�������Z�b�g
        if (selectPanel != null)
        {
            selectPanel.anchoredPosition = new Vector2(initialChoicePanelPos.x, initialChoicePanelPos.y + choicePanelHideOffset);
            selectPanel.gameObject.SetActive(true);

            // �A�j���[�V�����Ō��̈ʒu�ɖ߂�
            selectPanel
                .DOAnchorPosY(initialChoicePanelPos.y, animDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => {
                    onComplete?.Invoke();
                });
        }
    }
}
