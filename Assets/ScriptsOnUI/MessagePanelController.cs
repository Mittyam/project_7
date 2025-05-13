using UnityEngine;
using DG.Tweening;
using System;

public class MessagePanelController : MonoBehaviour
{
    [Header("�p�l���Q��")]
    [SerializeField] private RectTransform messagePanel;
    [SerializeField] private RectTransform selectPanel;

    [Header("�A�j���[�V�����ݒ�")]
    [SerializeField] private float animDuration = 0.3f;
    [SerializeField] private float choicePanelHideOffset = -300f; // ���ֈړ�������I�t�Z�b�g

    // �I�����p�l���̏����ʒu���L�^����ϐ�
    private Vector2 initialChoicePanelPos;

    // Tween�L�����Z���p�t���O
    private bool cancelMessageTween = false;

    private void Start()
    {
        // �����ʒu���L�^
        // initialChoicePanelPos = choicePanel.anchoredPosition;

        // MessagePanel�͏�����ԂŔ�\��
        messagePanel.gameObject.SetActive(false);
    }

    /// <summary>
    /// ���b�Z�[�W�p�l����\�����A�I�����p�l�������ɃA�j���[�V�����ŉB��
    /// </summary>
    public void ShowMessagePanel(Action onComplete = null)
    {
        // ���ɕ\������Ă���ꍇ�͉������Ȃ�
        if (messagePanel.gameObject.activeSelf) return;

        // �J�n���̓L�����Z����Ԃɂ��Ă��Ȃ�
        cancelMessageTween = false;

        // 1) �I�����p�l���������ʒu���牺�����ֈړ������A�������\���ɂ���
        selectPanel
            .DOAnchorPosY(initialChoicePanelPos.y + choicePanelHideOffset, animDuration)
            .SetEase(Ease.InQuad)
            .OnComplete(() =>
            {
                selectPanel.gameObject.SetActive(false);

                // �L�����Z������Ă���ꍇ�͏����𒆒f
                if (cancelMessageTween)
                    return;

                // 2) ���b�Z�[�W�p�l�����A�N�e�B�u�ɂ��āAY�X�P�[����0�ɐݒ�
                messagePanel.gameObject.SetActive(true);
                messagePanel.localScale = new Vector3(1, 0, 1);

                // 3) MessagePanel����ӂ���ɉ��֐L�т�i�X�P�[��Y��0��1�ɕ�ԁj�A�j���[�V�����ŕ\��
                messagePanel
                    .DOScaleY(1f, animDuration)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        // �L�����Z������Ă��Ȃ��ꍇ�̂�onComplete���Ă�
                        if (!cancelMessageTween)
                        {
                            onComplete?.Invoke();
                        }
                    });
            });
    }

    /// <summary>
    /// ���b�Z�[�W�p�l����������ɑI�����p�l�����ĕ\������
    /// </summary>
    public void ShowChoicePanel(Action onComplete = null)
    {
        // ���� choicePanel �����ɃA�N�e�B�u�ŁA�������������ʒu�ɂ���Ȃ�
        // �ǉ��̃A�j���[�V�����͕s�v�Ƃ��āA������ onComplete ���Ăяo��
        if (selectPanel.gameObject.activeSelf && Mathf.Abs(selectPanel.anchoredPosition.y - initialChoicePanelPos.y) < 0.1f)
        {
            onComplete?.Invoke();
            return;
        }

        cancelMessageTween = true;
        DOTween.Kill(messagePanel);
        DOTween.Kill(selectPanel);
        messagePanel.gameObject.SetActive(false);

        // choicePanel ���u������ԁv�̈ʒu�Ƀ��Z�b�g
        selectPanel.anchoredPosition = new Vector2(initialChoicePanelPos.x, initialChoicePanelPos.y + choicePanelHideOffset);
        selectPanel.gameObject.SetActive(true);

        // choicePanel �������ʒu�ɃA�j���[�V�����ňړ�����
        selectPanel
            .DOAnchorPosY(initialChoicePanelPos.y, animDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                onComplete?.Invoke();
            });
    }
}
