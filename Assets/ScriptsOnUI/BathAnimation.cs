using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Pointer �C�x���g�������̂ɕK�v
using DG.Tweening;

public class BathAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image panelImage;      // UI��Image�R���|�[�l���g�i�p�l���j
    public Sprite[] frames;       // �؂�ւ���X�v���C�g�Q
    public float frameRate = 0.1f;  // �e�t���[���̕\������

    // Inspector �Őݒ�\�ȕϐ�
    [Header("Slide Settings")]
    [Tooltip("�J�[�\����������Ƃ��̍������̂���o���ʁi�s�N�Z���A���̒l�ō��Ɉړ��j")]
    public float slideOffset = 50f;
    [Tooltip("�J�[�\����������Ƃ��E�O�ꂽ�Ƃ��̃A�j���[�V�������ԁi�b�j")]
    public float slideDuration = 0.3f;

    private Sequence animationSequence;
    private RectTransform panelRect;   // �p�l����RectTransform
    private Vector2 originalPos;       // �p�l���̌��̈ʒu

    void Start()
    {
        // �p�l������A�N�e�B�u�Ȃ�A�N�e�B�u�ɂ���
        if (!panelImage.gameObject.activeSelf)
        {
            panelImage.gameObject.SetActive(true);
        }

        // RectTransform �ւ̎Q�Ƃ��擾
        panelRect = panelImage.GetComponent<RectTransform>();
        // ���̍��W���o���Ă���
        originalPos = panelRect.anchoredPosition;

        // �X�v���C�g�����[�v�Đ�����A�j���[�V�������쐬
        animationSequence = DOTween.Sequence();
        foreach (Sprite frame in frames)
        {
            animationSequence.AppendCallback(() => panelImage.sprite = frame);
            animationSequence.AppendInterval(frameRate);
        }
        animationSequence.SetLoops(-1); // �������[�v
    }

    // �}�E�X�J�[�\�����p�l����ɓ������Ƃ�
    public void OnPointerEnter(PointerEventData eventData)
    {
        // ���̈ʒu���獶�� slideOffset �s�N�Z�������ړ�������
        panelRect.DOAnchorPos(originalPos + new Vector2(-slideOffset, 0f), slideDuration);
    }

    // �}�E�X�J�[�\�����p�l���ォ��O�ꂽ�Ƃ�
    public void OnPointerExit(PointerEventData eventData)
    {
        // ���̈ʒu�ɖ߂�
        panelRect.DOAnchorPos(originalPos, slideDuration);
    }
}
