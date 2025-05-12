using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class InfoPanelController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("メニューパネルのRectTransform")]
    [SerializeField] private RectTransform menuPanel;

    [Header("メニューパネル画像 (直接Imageとして読み込む)")]
    [SerializeField] private Image closedMenuImage; // 閉じた状態 (「>」の画像)
    [SerializeField] private Image openMenuImage;   // 開いた状態 (「<」の画像)

    [Header("ホバー時に少し動かす量 (px)")]
    [SerializeField] private float hoverOffset = 10f;

    [Header("クリック時のメニュー開閉量 (px)")]
    [SerializeField] private float openOffset = 200f;

    [Header("アニメーション時間 (秒)")]
    [SerializeField] private float animationDuration = 0.2f;

    // 各状態の位置情報
    private Vector2 defaultPos;   // 閉じた状態の位置
    private Vector2 hoveredPos;   // ホバー時の位置
    private Vector2 openPos;      // 開いた状態の位置

    // メニューの開閉状態フラグ
    private bool isOpen = false;

    private void Start()
    {
        // 初期位置を記録
        defaultPos = menuPanel.anchoredPosition;
        hoveredPos = defaultPos + new Vector2(hoverOffset, 0);
        openPos = defaultPos + new Vector2(openOffset, 0);

        // 初期状態は閉じているので、閉じた状態の画像を表示
        if (closedMenuImage != null) closedMenuImage.gameObject.SetActive(true);
        if (openMenuImage != null) openMenuImage.gameObject.SetActive(false);
    }

    /// <summary>
    /// 外部から呼び出し、メニューパネルを元の位置にリセットする
    /// </summary>
    public void ResetMenuPanel()
    {
        // Tween を停止して、正しい位置に戻す
        menuPanel.DOKill();
        menuPanel.DOAnchorPos(defaultPos, animationDuration);
        if (closedMenuImage != null) closedMenuImage.gameObject.SetActive(true);
        if (openMenuImage != null) openMenuImage.gameObject.SetActive(false);
        isOpen = false;
    }

    /// <summary>
    /// ホバー時にメニューパネルが少し右に動く
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isOpen)
        {
            menuPanel.DOAnchorPos(hoveredPos, animationDuration);
        }
    }

    /// <summary>
    /// ホバーが外れたときにメニューパネルが元の位置に戻る
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isOpen)
        {
            menuPanel.DOAnchorPos(defaultPos, animationDuration);
        }
    }

    /// <summary>
    /// クリックでメニューの開閉を切り替え、画像を変更する
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isOpen)
        {
            // メニューを開く：パネルを右に移動し、完了後に画像切替
            menuPanel.DOAnchorPos(openPos, animationDuration)
                .OnComplete(() =>
                {
                    if (closedMenuImage != null) closedMenuImage.gameObject.SetActive(false);
                    if (openMenuImage != null) openMenuImage.gameObject.SetActive(true);
                });
        }
        else
        {
            // メニューを閉じる：パネルを元の位置に戻し、完了後に画像切替
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
