using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ShopPanelManager : MonoBehaviour
{
    [Header("�p�l���Q��")]
    [SerializeField] private GameObject storeSelectPanel;
    [SerializeField] private GameObject pharmacyPanel;
    [SerializeField] private GameObject toyStorePanel;
    [SerializeField] private GameObject bookStorePanel;

    [Header("�A�j���[�V�����ݒ�")]
    [SerializeField] private float animationDuration = 0.35f;       // ���f�����A�j���[�V����
    [SerializeField] private Ease inEasing = Ease.OutExpo;          // �����Ă���Ƃ��F�����������A���X�Ɍ���
    [SerializeField] private Ease outEasing = Ease.OutQuart;        // �o�Ă����Ƃ��F�����������A���R�Ɍ���
    [SerializeField] private float moveDistance = 80f;              // �ړ������i�s�N�Z���j
    [SerializeField] private float scaleChange = 0.08f;             // �X�P�[���ω��ʁi�T���߁j

    // ���݃A�N�e�B�u�ȃp�l��
    private GameObject currentActivePanel;
    // �A�j���[�V���������ǂ���
    private bool isAnimating = false;
    // �e�p�l���̌��̈ʒu��ۑ�
    private Dictionary<GameObject, Vector3> originalPositions = new Dictionary<GameObject, Vector3>();

    private void Start()
    {
        // ������ԂŃV���b�v�I���p�l���̂ݕ\��
        storeSelectPanel.SetActive(true);
        // ���̃p�l���͔�\����
        pharmacyPanel.SetActive(false);
        toyStorePanel.SetActive(false);
        bookStorePanel.SetActive(false);

        // �e�p�l���̌��̈ʒu��ۑ�
        SaveOriginalPosition(storeSelectPanel);
        SaveOriginalPosition(pharmacyPanel);
        SaveOriginalPosition(toyStorePanel);
        SaveOriginalPosition(bookStorePanel);

        currentActivePanel = storeSelectPanel;
    }

    // �p�l���̌��̈ʒu��ۑ�
    private void SaveOriginalPosition(GameObject panel)
    {
        if (panel != null)
        {
            RectTransform rect = panel.GetComponent<RectTransform>();
            if (rect != null)
            {
                originalPositions[panel] = rect.anchoredPosition;
            }
        }
    }

    // �V���b�v�^�C�v�ɉ������p�l�����J��
    public void OpenShopPanel(ShopItemDisplay.ShopType shopType)
    {
        // �A�j���[�V�������͑�����󂯕t���Ȃ�
        if (isAnimating) return;

        isAnimating = true;

        // �J���p�l��������
        GameObject panelToOpen = null;
        switch (shopType)
        {
            case ShopItemDisplay.ShopType.Pharmacy:
                panelToOpen = pharmacyPanel;
                break;
            case ShopItemDisplay.ShopType.ToyStore:
                panelToOpen = toyStorePanel;
                break;
            case ShopItemDisplay.ShopType.BookStore:
                panelToOpen = bookStorePanel;
                break;
        }

        if (panelToOpen != null)
        {
            // �p�l���؂�ւ��A�j���[�V�������s
            SwitchPanelWithAnimation(currentActivePanel, panelToOpen);
        }
    }

    // �p�l���؂�ւ��A�j���[�V����
    private void SwitchPanelWithAnimation(GameObject currentPanel, GameObject nextPanel)
    {
        // ���݂̃p�l����RectTransform
        RectTransform currentRect = currentPanel.GetComponent<RectTransform>();
        CanvasGroup currentCanvasGroup = currentPanel.GetComponent<CanvasGroup>();

        // �f�����J�n���Ď��R�ɏI���悤�AOutEasing���g�p

        // ���݂̃p�l������Ɉړ����Ȃ���k�����A�t�F�[�h�A�E�g
        // �����ɓ����n�߂邽�߂̃C�[�W���O�ݒ�
        currentRect.DOAnchorPos(
            originalPositions[currentPanel] + new Vector3(0, moveDistance, 0),
            animationDuration * 0.8f // �o�Ă����̂͏�������
        ).SetEase(outEasing);

        currentRect.DOScale(
            currentRect.localScale * (1 - scaleChange),
            animationDuration * 0.8f
        ).SetEase(outEasing);

        // �����x�ω��͏����x�点�đf����
        currentCanvasGroup.DOFade(
            0,
            animationDuration * 0.7f
        ).SetEase(Ease.OutQuad)
        .OnComplete(() => {
            currentPanel.SetActive(false);

            // ���̃p�l����\������
            nextPanel.SetActive(true);
            RectTransform nextRect = nextPanel.GetComponent<RectTransform>();
            CanvasGroup nextCanvasGroup = nextPanel.GetComponent<CanvasGroup>();

            // �J�n�ʒu�ݒ�F������o��
            nextRect.anchoredPosition = originalPositions[nextPanel] - new Vector3(0, moveDistance, 0);
            nextRect.localScale = nextRect.localScale * (1 - scaleChange);
            nextCanvasGroup.alpha = 0;

            // ���̃p�l���o��A�j���[�V����
            // �f�����o�ꂵ�āA���X�ɖړI�n�ɓ��B����悤�ȓ�����
            nextRect.DOAnchorPos(
                originalPositions[nextPanel],
                animationDuration
            ).SetEase(inEasing);

            nextRect.DOScale(
                Vector3.one,
                animationDuration
            ).SetEase(inEasing);

            // �����x�ω��͏�����s���đf����
            nextCanvasGroup.DOFade(
                1,
                animationDuration * 0.6f
            ).SetEase(Ease.OutQuad)
            .OnComplete(() => {
                currentActivePanel = nextPanel;
                isAnimating = false;
            });
        });
    }

    // �V���b�v�I����ʂɖ߂�
    public void ReturnToStoreSelection()
    {
        // �A�j���[�V�������͑�����󂯕t���Ȃ�
        if (isAnimating) return;

        // ���݂̃p�l�����V���b�v�I����ʂłȂ���΃A�j���[�V����
        if (currentActivePanel != storeSelectPanel)
        {
            isAnimating = true;
            SwitchPanelWithAnimation(currentActivePanel, storeSelectPanel);
        }
    }

    // �K�v�ɉ����ăp�l����CanvasGroup�R���|�[�l���g��ǉ����鏈��
    private void OnValidate()
    {
        EnsureCanvasGroup(storeSelectPanel);
        EnsureCanvasGroup(pharmacyPanel);
        EnsureCanvasGroup(toyStorePanel);
        EnsureCanvasGroup(bookStorePanel);
    }

    // CanvasGroup�R���|�[�l���g���K�v
    private void EnsureCanvasGroup(GameObject panel)
    {
        if (panel != null && panel.GetComponent<CanvasGroup>() == null)
        {
            panel.AddComponent<CanvasGroup>();
        }
    }

    // �A�v���P�[�V�����I������Tween�����ׂăL��
    private void OnDestroy()
    {
        DOTween.KillAll();
    }
}