using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class InfoPanelController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("���j���[�p�l����RectTransform")]
    [SerializeField] private RectTransform menuPanel;

    [Header("���j���[�p�l���摜 (����Image�Ƃ��ēǂݍ���)")]
    [SerializeField] private Image closedMenuImage; // ������� (�u>�v�̉摜)
    [SerializeField] private Image openMenuImage;   // �J������� (�u<�v�̉摜)

    [Header("�z�o�[���ɏ����������� (px)")]
    [SerializeField] private float hoverOffset = 10f;

    [Header("�N���b�N���̃��j���[�J�� (px)")]
    [SerializeField] private float openOffset = 200f;

    [Header("�A�j���[�V�������� (�b)")]
    [SerializeField] private float animationDuration = 0.2f;

    // �e��Ԃ̈ʒu���
    private Vector2 defaultPos;   // ������Ԃ̈ʒu
    private Vector2 hoveredPos;   // �z�o�[���̈ʒu
    private Vector2 openPos;      // �J������Ԃ̈ʒu

    // ���j���[�̊J��ԃt���O
    private bool isOpen = false;

    private void Start()
    {
        // �����ʒu���L�^
        defaultPos = menuPanel.anchoredPosition;
        hoveredPos = defaultPos + new Vector2(hoverOffset, 0);
        openPos = defaultPos + new Vector2(openOffset, 0);

        // ������Ԃ͕��Ă���̂ŁA������Ԃ̉摜��\��
        if (closedMenuImage != null) closedMenuImage.gameObject.SetActive(true);
        if (openMenuImage != null) openMenuImage.gameObject.SetActive(false);
    }

    /// <summary>
    /// �O������Ăяo���A���j���[�p�l�������̈ʒu�Ƀ��Z�b�g����
    /// </summary>
    public void ResetMenuPanel()
    {
        // Tween ���~���āA�������ʒu�ɖ߂�
        menuPanel.DOKill();
        menuPanel.DOAnchorPos(defaultPos, animationDuration);
        if (closedMenuImage != null) closedMenuImage.gameObject.SetActive(true);
        if (openMenuImage != null) openMenuImage.gameObject.SetActive(false);
        isOpen = false;
    }

    /// <summary>
    /// �z�o�[���Ƀ��j���[�p�l���������E�ɓ���
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isOpen)
        {
            menuPanel.DOAnchorPos(hoveredPos, animationDuration);
        }
    }

    /// <summary>
    /// �z�o�[���O�ꂽ�Ƃ��Ƀ��j���[�p�l�������̈ʒu�ɖ߂�
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isOpen)
        {
            menuPanel.DOAnchorPos(defaultPos, animationDuration);
        }
    }

    /// <summary>
    /// �N���b�N�Ń��j���[�̊J��؂�ւ��A�摜��ύX����
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isOpen)
        {
            // ���j���[���J���F�p�l�����E�Ɉړ����A������ɉ摜�ؑ�
            menuPanel.DOAnchorPos(openPos, animationDuration)
                .OnComplete(() =>
                {
                    if (closedMenuImage != null) closedMenuImage.gameObject.SetActive(false);
                    if (openMenuImage != null) openMenuImage.gameObject.SetActive(true);
                });
        }
        else
        {
            // ���j���[�����F�p�l�������̈ʒu�ɖ߂��A������ɉ摜�ؑ�
            menuPanel.DOAnchorPos(defaultPos, animationDuration)
                .OnComplete(() =>
                {
                    if (closedMenuImage != null) closedMenuImage.gameObject.SetActive(true);
                    if (openMenuImage != null) openMenuImage.gameObject.SetActive(false);
                });
        }
        isOpen = !isOpen;
    }
}
