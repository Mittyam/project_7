using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Unity.VisualScripting;
using System;

public class AnimationTriggerState : MonoBehaviour, ISubState
{
    [Header("基本参照")]
    [SerializeField] private SubStateMachine subStateMachine;
    [SerializeField] private ProgressManager progressManager;

    [Header("アニメーション設定")]
    [SerializeField] private RectTransform animationCanvas;
    [SerializeField] private Image backgroundImage;    // BGイメージ
    [SerializeField] private Image animBaseImage;      // アニメーションのベースイメージ
    [SerializeField] private float animationDuration = 2.0f;
    [SerializeField] private float centerPauseDuration = 1.0f;
    [SerializeField] private float fadeInOutDuration = 0.5f;  // フェードイン/アウト時間
    [SerializeField] private float offScreenX = 1000f;  // 画面外の座標

    [Header("イベント別アニメーション")]
    [SerializeField]
    private List<EventAnimation> eventAnimations
        = new List<EventAnimation>();

    private int eventID;
    private bool isAnimationPlaying;
    private Image currentAnimationImage;
    private RectTransform currentAnimationRect;
    private DG.Tweening.Sequence currentSequence;

    public event Action OnAnimationFinished;

    public StatusChange LastStatusChange { get; private set; }

    bool ISubState.enabled
    {
        get => base.enabled;    // MonoBehaviourのenabled
        set => base.enabled = value;
    }

    [System.Serializable]
    public class EventAnimation
    {
        public int eventID;
        public string animationName;
        public Sprite animationSprite;
        public Vector2 spriteSize = new Vector2(200, 200);
        public StatusChange statusChange;
    }

    [System.Serializable]
    public class  StatusChange
    {
        public int affectionChange;
        public int lovelityChange;
        public int moneyChange;
    }

    public void SetEventID(int id)
    {
        eventID = id;
    }

    public void OnEnter()
    {
        Debug.Log($"イベントID:{eventID}のアニメーションサブステート開始");

        // アニメーションキャンバスをアクティブにする
        if (animationCanvas != null)
        {
            animationCanvas.gameObject.SetActive(true);

            // 初期状態では背景とベースは透明に
            if (backgroundImage != null)
                backgroundImage.color = new Color(backgroundImage.color.r, backgroundImage.color.g, backgroundImage.color.b, 0);

            if (animBaseImage != null)
                animBaseImage.color = new Color(animBaseImage.color.r, animBaseImage.color.g, animBaseImage.color.b, 0);
        }
        else
        {
            Debug.LogError("AnimationCanvas が設定されていません。");
            CompleteAnimation();
            return;
        }

        // 背景やベースイメージの参照確認
        if (backgroundImage == null || animBaseImage == null)
        {
            Debug.LogError("背景イメージまたはアニメーションベースイメージが設定されていません。");
            CompleteAnimation();
            return;
        }

        // 対応するイベントアニメーションを探す
        EventAnimation eventAnimation = eventAnimations.Find(ea => ea.eventID == eventID);

        if (eventAnimation != null)
        {
            if (eventAnimation.animationSprite == null)
            {
                Debug.LogError($"イベントID:{eventID}のアニメーションスプライトが設定されていません。");
                CompleteAnimation();
                return;
            }

            PlayAnimation(eventAnimation);
        }
        else
        {
            Debug.LogWarning($"イベントID:{eventID}に対応するアニメーションが見つかりません");
            // アニメーションがない場合は即終了
            CompleteAnimation();
        }
    }

    public void OnUpdate()
    {
        // アニメーションの完了を待つだけなので、特に何もしない
    }

    public void OnExit()
    {
        // シーケンスが実行中なら停止
        if (currentSequence != null && currentSequence.IsActive())
        {
            currentSequence.Kill();
        }

        // 生成した画像があれば削除
        if (currentAnimationImage != null)
        {
            Destroy(currentAnimationImage.gameObject);
            currentAnimationImage = null;
            currentAnimationRect = null;
        }

        // 背景とベースを透明に戻す
        if (backgroundImage != null)
            backgroundImage.color = new Color(backgroundImage.color.r, backgroundImage.color.g, backgroundImage.color.b, 0);

        if (animBaseImage != null)
            animBaseImage.color = new Color(animBaseImage.color.r, animBaseImage.color.g, animBaseImage.color.b, 0);

        // 念のため、アニメーションキャンバスを非アクティブにする
        if (animationCanvas != null)
        {
            animationCanvas.gameObject.SetActive(false);
        }
    }

    private void PlayAnimation(EventAnimation eventAnimation)
    {
        isAnimationPlaying = true;

        // シーケンスの作成
        currentSequence = DOTween.Sequence();

        // 背景とベースを初期化（透明に）
        backgroundImage.color = new Color(backgroundImage.color.r, backgroundImage.color.g, backgroundImage.color.b, 0);
        animBaseImage.color = new Color(animBaseImage.color.r, animBaseImage.color.g, animBaseImage.color.b, 0);

        // アニメーション用の画像を生成
        GameObject animObj = new GameObject($"Animation_{eventAnimation.animationName}");
        animObj.transform.SetParent(animBaseImage.transform, false);

        // RectTransformの設定
        currentAnimationRect = animObj.AddComponent<RectTransform>();
        currentAnimationRect.anchorMin = new Vector2(0.5f, 0.5f);
        currentAnimationRect.anchorMax = new Vector2(0.5f, 0.5f);
        currentAnimationRect.pivot = new Vector2(0.5f, 0.5f);

        // サイズを明示的に設定（これが0になる問題の解決）
        currentAnimationRect.sizeDelta = eventAnimation.spriteSize;

        // 開始位置（画面外左）
        currentAnimationRect.anchoredPosition = new Vector2(-offScreenX, 0);

        // イメージコンポーネント設定
        currentAnimationImage = animObj.AddComponent<Image>();
        currentAnimationImage.sprite = eventAnimation.animationSprite;
        currentAnimationImage.preserveAspect = true; // アスペクト比を保持

        // スプライト設定が正しいかログ出力（デバッグ用）
        Debug.Log($"アニメーション設定: ID={eventID}, Sprite={eventAnimation.animationSprite}, Size={eventAnimation.spriteSize}");

        // 1. 背景とベースのフェードイン
        currentSequence.Append(backgroundImage.DOFade(1, fadeInOutDuration));
        currentSequence.Join(animBaseImage.DOFade(1, fadeInOutDuration));

        // 2. 左から中央へ移動
        currentSequence.Append(currentAnimationRect.DOAnchorPosX(0, animationDuration / 2)
            .SetEase(Ease.OutQuad));

        // 3. 中央で一時停止
        currentSequence.AppendInterval(centerPauseDuration);

        // 4. 中央から右へ移動
        currentSequence.Append(currentAnimationRect.DOAnchorPosX(offScreenX, animationDuration / 2)
            .SetEase(Ease.InQuad));

        // 5. 背景とベースのフェードアウト
        currentSequence.Append(backgroundImage.DOFade(0, fadeInOutDuration));
        currentSequence.Join(animBaseImage.DOFade(0, fadeInOutDuration));

        // 6. アニメーション完了時の処理
        currentSequence.OnComplete(() => {
            // ステータス変更を適用
            ApplyStatusChange(eventAnimation.statusChange);

            // アニメーション完了処理
            CompleteAnimation();
        });
    }

    private void ApplyStatusChange(StatusChange statusChange)
    {
        if (statusChange != null)
        {
            // 直近の変更内容を保持（UI 側が参照可能）
            LastStatusChange = statusChange;

            // StatusManagerを通じてステータスを変更
            StatusManager.Instance.UpdateStatus(
                0,
                statusChange.affectionChange,
                statusChange.lovelityChange,
                statusChange.moneyChange
                );

            Debug.Log($"ステータス変更: Affection={statusChange.affectionChange}, Love={statusChange.lovelityChange}, Money={statusChange.moneyChange}");
        }
    }

    // アニメーション完了時の処理
    private void CompleteAnimation()
    {
        isAnimationPlaying = false;

        // 背景とベースを透明に戻してからキャンバスを非表示にする
        // 注意: DOTweenシーケンスでフェードアウト後に呼ばれるケースと、
        // エラー時に即時呼ばれるケースの両方に対応
        if (backgroundImage != null)
            backgroundImage.color = new Color(backgroundImage.color.r, backgroundImage.color.g, backgroundImage.color.b, 0);

        if (animBaseImage != null)
            animBaseImage.color = new Color(animBaseImage.color.r, animBaseImage.color.g, animBaseImage.color.b, 0);

        // アニメーションキャンバスを非アクティブにする
        if (animationCanvas != null)
            animationCanvas.gameObject.SetActive(false);

        // ここで通知を発行（UI側はこれを受け取ってメッセージパネルを操作する）
        OnAnimationFinished?.Invoke();

        // 結果状態に移行
        subStateMachine.EndSubEvent();
    }

    // 外部からアニメーションをスキップするためのメソッド
    public void SkipAnimation()
    {
        if (isAnimationPlaying && currentSequence != null)
        {
            currentSequence.Complete();
        }
    }
}
