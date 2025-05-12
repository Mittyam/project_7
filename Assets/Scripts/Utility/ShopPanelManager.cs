using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ShopPanelManager : MonoBehaviour
{
    [Header("パネル参照")]
    [SerializeField] private GameObject storeSelectPanel;
    [SerializeField] private GameObject pharmacyPanel;
    [SerializeField] private GameObject toyStorePanel;
    [SerializeField] private GameObject bookStorePanel;

    [Header("アニメーション設定")]
    [SerializeField] private float animationDuration = 0.35f;       // より素早いアニメーション
    [SerializeField] private Ease inEasing = Ease.OutExpo;          // 入ってくるとき：初速が速く、徐々に減速
    [SerializeField] private Ease outEasing = Ease.OutQuart;        // 出ていくとき：初速が速く、自然に減速
    [SerializeField] private float moveDistance = 80f;              // 移動距離（ピクセル）
    [SerializeField] private float scaleChange = 0.08f;             // スケール変化量（控えめ）

    // 現在アクティブなパネル
    private GameObject currentActivePanel;
    // アニメーション中かどうか
    private bool isAnimating = false;
    // 各パネルの元の位置を保存
    private Dictionary<GameObject, Vector3> originalPositions = new Dictionary<GameObject, Vector3>();

    private void Start()
    {
        // 初期状態でショップ選択パネルのみ表示
        storeSelectPanel.SetActive(true);
        // 他のパネルは非表示に
        pharmacyPanel.SetActive(false);
        toyStorePanel.SetActive(false);
        bookStorePanel.SetActive(false);

        // 各パネルの元の位置を保存
        SaveOriginalPosition(storeSelectPanel);
        SaveOriginalPosition(pharmacyPanel);
        SaveOriginalPosition(toyStorePanel);
        SaveOriginalPosition(bookStorePanel);

        currentActivePanel = storeSelectPanel;
    }

    // パネルの元の位置を保存
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

    // ショップタイプに応じたパネルを開く
    public void OpenShopPanel(ShopItemDisplay.ShopType shopType)
    {
        // アニメーション中は操作を受け付けない
        if (isAnimating) return;

        isAnimating = true;

        // 開くパネルを決定
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
            // パネル切り替えアニメーション実行
            SwitchPanelWithAnimation(currentActivePanel, panelToOpen);
        }
    }

    // パネル切り替えアニメーション
    private void SwitchPanelWithAnimation(GameObject currentPanel, GameObject nextPanel)
    {
        // 現在のパネルのRectTransform
        RectTransform currentRect = currentPanel.GetComponent<RectTransform>();
        CanvasGroup currentCanvasGroup = currentPanel.GetComponent<CanvasGroup>();

        // 素早く開始して自然に終わるよう、OutEasingを使用

        // 現在のパネルが上に移動しながら縮小し、フェードアウト
        // すぐに動き始めるためのイージング設定
        currentRect.DOAnchorPos(
            originalPositions[currentPanel] + new Vector3(0, moveDistance, 0),
            animationDuration * 0.8f // 出ていくのは少し速く
        ).SetEase(outEasing);

        currentRect.DOScale(
            currentRect.localScale * (1 - scaleChange),
            animationDuration * 0.8f
        ).SetEase(outEasing);

        // 透明度変化は少し遅らせて素早く
        currentCanvasGroup.DOFade(
            0,
            animationDuration * 0.7f
        ).SetEase(Ease.OutQuad)
        .OnComplete(() => {
            currentPanel.SetActive(false);

            // 次のパネルを表示準備
            nextPanel.SetActive(true);
            RectTransform nextRect = nextPanel.GetComponent<RectTransform>();
            CanvasGroup nextCanvasGroup = nextPanel.GetComponent<CanvasGroup>();

            // 開始位置設定：下から登場
            nextRect.anchoredPosition = originalPositions[nextPanel] - new Vector3(0, moveDistance, 0);
            nextRect.localScale = nextRect.localScale * (1 - scaleChange);
            nextCanvasGroup.alpha = 0;

            // 次のパネル登場アニメーション
            // 素早く登場して、徐々に目的地に到達するような動きに
            nextRect.DOAnchorPos(
                originalPositions[nextPanel],
                animationDuration
            ).SetEase(inEasing);

            nextRect.DOScale(
                Vector3.one,
                animationDuration
            ).SetEase(inEasing);

            // 透明度変化は少し先行して素早く
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

    // ショップ選択画面に戻る
    public void ReturnToStoreSelection()
    {
        // アニメーション中は操作を受け付けない
        if (isAnimating) return;

        // 現在のパネルがショップ選択画面でなければアニメーション
        if (currentActivePanel != storeSelectPanel)
        {
            isAnimating = true;
            SwitchPanelWithAnimation(currentActivePanel, storeSelectPanel);
        }
    }

    // 必要に応じてパネルにCanvasGroupコンポーネントを追加する処理
    private void OnValidate()
    {
        EnsureCanvasGroup(storeSelectPanel);
        EnsureCanvasGroup(pharmacyPanel);
        EnsureCanvasGroup(toyStorePanel);
        EnsureCanvasGroup(bookStorePanel);
    }

    // CanvasGroupコンポーネントが必要
    private void EnsureCanvasGroup(GameObject panel)
    {
        if (panel != null && panel.GetComponent<CanvasGroup>() == null)
        {
            panel.AddComponent<CanvasGroup>();
        }
    }

    // アプリケーション終了時にTweenをすべてキル
    private void OnDestroy()
    {
        DOTween.KillAll();
    }
}