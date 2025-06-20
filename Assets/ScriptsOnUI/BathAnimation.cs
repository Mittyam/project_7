using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Pointer イベント処理に必要
using DG.Tweening;

public class BathAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image panelImage;      // UIのImageコンポーネント（パネル）
    public Sprite[] frames;       // 切り替えるスプライト配列
    public float frameRate = 0.1f;  // 各フレームの表示継続時間

    // Inspector で設定可能な変数
    [Header("Slide Settings")]
    [Tooltip("カーソルが乗ったときの左方向のせり出す量（ピクセル、正の値で左に移動）")]
    public float slideOffset = 50f;
    [Tooltip("カーソルが乗ったとき・外れたときのアニメーション継続時間（秒）")]
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
        CreateSpriteAnimation();
    }

    // スプライトアニメーションを作成する関数
    private void CreateSpriteAnimation()
    {
        if (frames == null || frames.Length == 0) return;

        animationSequence = DOTween.Sequence();
        foreach (Sprite frame in frames)
        {
            // null チェックを含む安全なコールバック
            animationSequence.AppendCallback(() => {
                if (this != null && panelImage != null && panelImage.gameObject != null)
                {
                    panelImage.sprite = frame;
                }
            });
            animationSequence.AppendInterval(frameRate);
        }
        animationSequence.SetLoops(-1); // 無限ループ
    }

    // マウスカーソルがパネル上に入ったとき
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 元の位置から左へ slideOffset ピクセルだけ移動させる
        if (panelRect != null)
        {
            panelRect.DOAnchorPos(originalPos + new Vector2(-slideOffset, 0f), slideDuration);
        }
    }

    // マウスカーソルがパネル上から外れたとき
    public void OnPointerExit(PointerEventData eventData)
    {
        // 元の位置に戻す
        if (panelRect != null)
        {
            panelRect.DOAnchorPos(originalPos, slideDuration);
        }
    }

    // オブジェクトが破棄される際に DOTween を適切に停止
    void OnDestroy()
    {
        // このオブジェクトに関連するすべての DOTween を停止
        DOTween.Kill(this);

        // 念のため panelImage と panelRect に関連する DOTween も停止
        if (panelImage != null)
        {
            DOTween.Kill(panelImage);
        }
        if (panelRect != null)
        {
            DOTween.Kill(panelRect);
        }

        // Sequence を手動で停止
        if (animationSequence != null)
        {
            animationSequence.Kill();
            animationSequence = null;
        }
    }

    // オブジェクトが無効になったときも DOTween を停止
    void OnDisable()
    {
        if (animationSequence != null)
        {
            animationSequence.Pause();
        }
    }

    // オブジェクトが有効になったときに DOTween を再開
    void OnEnable()
    {
        if (animationSequence != null)
        {
            animationSequence.Play();
        }
    }
}