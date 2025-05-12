using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Pointer イベントを扱うのに必要
using DG.Tweening;

public class BathAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image panelImage;      // UIのImageコンポーネント（パネル）
    public Sprite[] frames;       // 切り替えるスプライト群
    public float frameRate = 0.1f;  // 各フレームの表示時間

    // Inspector で設定可能な変数
    [Header("Slide Settings")]
    [Tooltip("カーソルが乗ったときの左方向のせり出す量（ピクセル、正の値で左に移動）")]
    public float slideOffset = 50f;
    [Tooltip("カーソルが乗ったとき・外れたときのアニメーション時間（秒）")]
    public float slideDuration = 0.3f;

    private Sequence animationSequence;
    private RectTransform panelRect;   // パネルのRectTransform
    private Vector2 originalPos;       // パネルの元の位置

    void Start()
    {
        // パネルが非アクティブならアクティブにする
        if (!panelImage.gameObject.activeSelf)
        {
            panelImage.gameObject.SetActive(true);
        }

        // RectTransform への参照を取得
        panelRect = panelImage.GetComponent<RectTransform>();
        // 元の座標を覚えておく
        originalPos = panelRect.anchoredPosition;

        // スプライトをループ再生するアニメーションを作成
        animationSequence = DOTween.Sequence();
        foreach (Sprite frame in frames)
        {
            animationSequence.AppendCallback(() => panelImage.sprite = frame);
            animationSequence.AppendInterval(frameRate);
        }
        animationSequence.SetLoops(-1); // 無限ループ
    }

    // マウスカーソルがパネル上に入ったとき
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 元の位置から左へ slideOffset ピクセルだけ移動させる
        panelRect.DOAnchorPos(originalPos + new Vector2(-slideOffset, 0f), slideDuration);
    }

    // マウスカーソルがパネル上から外れたとき
    public void OnPointerExit(PointerEventData eventData)
    {
        // 元の位置に戻す
        panelRect.DOAnchorPos(originalPos, slideDuration);
    }
}
