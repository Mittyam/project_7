using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using static UnityEngine.EventSystems.EventTrigger;

public class NovelCommandExecutor : MonoBehaviour
{
    [SerializeField] private NovelState novelState;
    [SerializeField] private Live2DController live2DController;
    [SerializeField] private MessagePrinter messagePrinter;

    // WaitForAdvanceCommandから移行したフィールド
    [Header("UI Controls")]
    [SerializeField] private Toggle autoToggle;
    [SerializeField] private Toggle skipToggle;

    // 既存のフィールドに追加
    [Header("Image Transition Settings")]
    [SerializeField] private float fadeInDuration = 0.6f;  // フェードイン秒数
    [SerializeField] private float fadeOutDuration = 0.6f; // フェードアウト秒数
    [SerializeField] private float crossFadeDuration = 1.0f; // クロスフェード秒数

    [Header("Live2D Settings")]
    [SerializeField] private float parameterTransitionTime = 0.5f; // パラメータ遷移時間

    private NovelState.PlaybackMode playbackMode = NovelState.PlaybackMode.Click;
    private float autoAdvanceDelay = 3.0f;
    private string currentModelID = ""; // 現在表示中のLive2DモデルID

    private void Awake()
    {
        // トグルイベントの設定
        if (autoToggle) autoToggle.onValueChanged.AddListener(OnAutoToggleChanged);
        if (skipToggle) skipToggle.onValueChanged.AddListener(OnSkipToggleChanged);
    }

    private void OnDestroy()
    {
        if (autoToggle) autoToggle.onValueChanged.RemoveListener(OnAutoToggleChanged);
        if (skipToggle) skipToggle.onValueChanged.RemoveListener(OnSkipToggleChanged);
    }

    private void OnAutoToggleChanged(bool isOn)
    {
        playbackMode = isOn ? NovelState.PlaybackMode.Auto :
                     (playbackMode == NovelState.PlaybackMode.Auto ?
                      NovelState.PlaybackMode.Click : playbackMode);
    }

    private void OnSkipToggleChanged(bool isOn)
    {
        playbackMode = isOn ? NovelState.PlaybackMode.Skip :
                     (playbackMode == NovelState.PlaybackMode.Skip ?
                      NovelState.PlaybackMode.Click : playbackMode);
    }

    public void SetMode(NovelState.PlaybackMode mode)
    {
        playbackMode = mode;
        if (autoToggle) autoToggle.isOn = (mode == NovelState.PlaybackMode.Auto);
        if (skipToggle) skipToggle.isOn = (mode == NovelState.PlaybackMode.Skip);
    }

    public IEnumerator ExecuteCommands(List<EventEntity> entities, NovelState.PlaybackMode mode)
    {
        SetMode(mode);

        foreach (var entity in entities)
        {
            // SE音声処理
            if (entity.seIndex >= 0)
            {
                SoundManager.Instance.PlaySE(entity.seIndex);
            }

            // テキスト処理
            if (!string.IsNullOrEmpty(entity.text) && messagePrinter != null)
            {
                messagePrinter.PrintMessage(entity.text);
            }

            // 背景画像処理
            if (!string.IsNullOrEmpty(entity.imagePath))
            {
                bool shouldFade = DetermineShouldFade(entity.isImageFade);
                yield return HandleImageChange(entity.imagePath, shouldFade);
            }

            // ボイス音声処理
            if (entity.voiceIndex >= 0)
            {
                SoundManager.Instance.StopVoice();
                SoundManager.Instance.PlayVoice(entity.voiceIndex);
            }

            // BGM音声処理
            if (entity.bgmIndex >= 0)
            {
                SoundManager.Instance.StopBGMWithFadeOut(2f);
                SoundManager.Instance.PlayBGMWithFadeIn(entity.bgmIndex, 2f);
            }

            // Live2D処理
            yield return HandleLive2D(entity);

            // 進行待機
            yield return WaitForAdvance();
        }
    }

    // メッセージプリンターと背景画像の初期化処理
    public IEnumerator Initialize()
    {
        // 既存の画像のスプライト情報を取得
        SoundManager.Instance.StopBGMWithFadeOut(fadeOutDuration - 0.1f);
        yield return FadeOutImage(novelState.BackgroundImage, fadeOutDuration);
        Sprite previousSprite = novelState.BackgroundImage.sprite;
        novelState.BackgroundImage.sprite = null;
        messagePrinter.ShowMessage("");
        SoundManager.Instance.StopAllSounds();
    }

    private IEnumerator WaitForAdvance()
    {
        float autoTimer = 0f;

        while (true)
        {
            switch (playbackMode)
            {
                case NovelState.PlaybackMode.Skip:
                    yield return new WaitForSeconds(0.3f);
                    yield break;

                case NovelState.PlaybackMode.Auto:
                    if (SoundManager.Instance.voiceSource.isPlaying)
                    {
                        autoTimer = 0f;
                    }
                    else if (messagePrinter != null && !messagePrinter.IsTextFullyDisplayed())
                    {
                        autoTimer = 0f;
                    }

                    autoTimer += Time.deltaTime;
                    if (autoTimer >= autoAdvanceDelay)
                        yield break;
                    break;

                case NovelState.PlaybackMode.Click:
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (IsClickOnTargetButton())
                        {
                            break;
                        }

                        yield return new WaitForSeconds(0.3f);

                        yield break;
                    }
                    break;
            }

            yield return null;
        }
    }

    private bool IsClickOnTargetButton()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject == autoToggle?.gameObject ||
                result.gameObject == skipToggle?.gameObject)
            {
                return true;
            }

            if (result.gameObject.GetComponentInParent<Toggle>() != null)
            {
                Toggle toggle = result.gameObject.GetComponentInParent<Toggle>();
                if (toggle == autoToggle || toggle == skipToggle)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private IEnumerator HandleImageChange(string imagePath, bool shouldFade = true)
    {
        if (novelState?.BackgroundImage == null) yield break;

        // 現在のスプライトを取得  
        Sprite previousSprite = novelState.BackgroundImage.sprite;

        // "deleteImage" の場合、画像を削除  
        if (imagePath.ToLower() == "deleteimage")
        {
            if (previousSprite != null)
            {
                if (shouldFade)
                {
                    // フェードアウトで画像を削除  
                    yield return FadeOutImage(novelState.BackgroundImage, fadeOutDuration);
                }
                else
                {
                    // 即座に画像を削除  
                    Color color = novelState.BackgroundImage.color;
                    color.a = 0f;
                    novelState.BackgroundImage.color = color;
                }
            }
            novelState.BackgroundImage.sprite = null;
            yield break;
        }

        // Resourcesから画像をロード  
        Sprite newSprite = Resources.Load<Sprite>($"Images/{imagePath}");

        if (newSprite == null)
        {
            Debug.LogWarning($"画像が見つかりません: Images/{imagePath}");
            yield break;
        }

        // 現在の画像が表示されているかをチェック  
        bool hasVisibleImage = previousSprite != null && novelState.BackgroundImage.color.a > 0f;

        if (shouldFade)
        {
            // フェードありの場合  
            if (hasVisibleImage && previousSprite != newSprite)
            {
                // 現在の画像が異なる場合はクロスフェード  
                yield return CrossFadeImages(novelState.BackgroundImage, previousSprite, newSprite, crossFadeDuration);
            }
            else
            {
                // 新規画像をフェードイン  
                novelState.BackgroundImage.sprite = newSprite;
                yield return FadeInImage(novelState.BackgroundImage, fadeInDuration);
            }
        }
        else
        {
            // フェードなしの場合（即座に切り替え）  
            novelState.BackgroundImage.sprite = newSprite;
            Color color = novelState.BackgroundImage.color;
            color.a = 1f;
            novelState.BackgroundImage.color = color;
        }
    }

    /// <summary>
    /// 画像をフェードイン
    /// </summary>
    private IEnumerator FadeInImage(Image image, float duration)
    {
        Color color = image.color;
        color.a = 0f;
        image.color = color;

        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Clamp01(time / duration);
            color.a = alpha;
            image.color = color;
            yield return null;
        }

        color.a = 1f;
        image.color = color;
    }

    /// <summary>
    /// 画像をフェードアウト
    /// </summary>
    private IEnumerator FadeOutImage(Image image, float duration)
    {
        Color color = image.color;
        float startAlpha = color.a;

        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, 0f, time / duration);
            color.a = alpha;
            image.color = color;
            yield return null;
        }

        color.a = 0f;
        image.color = color;
    }

    /// <summary>
    /// 2つの画像をクロスフェード
    /// </summary>
    private IEnumerator CrossFadeImages(Image image, Sprite oldSprite, Sprite newSprite, float duration)
    {
        // 古い画像用の一時的なImageを作成（UIの背後に配置）
        GameObject tempImageObj = new GameObject("TempCrossFadeImage");

        // 親を設定する前に、まず適切な位置に配置
        tempImageObj.transform.SetParent(image.transform.parent, false);

        // siblingIndexを調整して、古い画像が新しい画像の後ろに来るようにする
        tempImageObj.transform.SetSiblingIndex(image.transform.GetSiblingIndex());

        Image tempImage = tempImageObj.AddComponent<Image>();
        RectTransform tempRect = tempImage.GetComponent<RectTransform>();

        // 位置とサイズを合わせる
        RectTransform originalRect = image.GetComponent<RectTransform>();
        tempRect.anchorMin = originalRect.anchorMin;
        tempRect.anchorMax = originalRect.anchorMax;
        tempRect.pivot = originalRect.pivot;
        tempRect.anchoredPosition = originalRect.anchoredPosition;
        tempRect.sizeDelta = originalRect.sizeDelta;

        // 古い画像を一時Imageに設定
        tempImage.sprite = oldSprite;
        tempImage.preserveAspect = image.preserveAspect;

        // 現在のAlpha値を保持
        Color tempImageColor = tempImage.color;
        tempImageColor.a = image.color.a;
        tempImage.color = tempImageColor;

        // 新しい画像を元のImageに設定し、最初は透明に
        image.sprite = newSprite;
        Color newColor = image.color;
        newColor.a = 0f;
        image.color = newColor;

        // クロスフェード実行
        float time = 0f;
        float startAlpha = tempImageColor.a;

        while (time < duration)
        {
            time += Time.deltaTime;
            float progress = time / duration;

            // 古い画像フェードアウト
            Color oldColor = tempImage.color;
            oldColor.a = Mathf.Lerp(startAlpha, 0f, progress);
            tempImage.color = oldColor;

            // 新しい画像フェードイン
            newColor.a = progress;
            image.color = newColor;

            yield return null;
        }

        // フェード完了
        newColor.a = 1f;
        image.color = newColor;

        // 一時オブジェクトを削除
        Destroy(tempImageObj);
    }

    private IEnumerator HandleLive2D(EventEntity entity)
    {
        if (live2DController == null) yield break;

        // モデル削除処理
        if (entity.live2DAnimTrigger != null && entity.live2DAnimTrigger.ToLower() == "deletemodel")
        {
            if (!string.IsNullOrEmpty(entity.live2DModelID))
            {
                // 特定のモデルを削除
                live2DController.HideModel(entity.live2DModelID);
                Debug.Log($"Live2Dモデル '{entity.live2DModelID}' を削除しました。");
            }
            else
            {
                // 全モデル削除
                live2DController.DeleteAllModels();
                Debug.Log("すべてのLive2Dモデルを削除しました。");
            }

            yield break;
        }

        if (!string.IsNullOrEmpty(entity.live2DModelID))
            currentModelID = entity.live2DModelID; // 現在のモデルIDを更新

        // モデル表示
        if (!string.IsNullOrEmpty(entity.live2DModelID))
        {
            // Live2DControllerにモデルID、スケール、位置文字列をそのまま渡す
            live2DController.ShowModel(
                entity.live2DModelID,
                entity.live2DScale,
                entity.live2DPosition
            );
        }

        // アニメーション再生（deleteModel以外の場合のみ）
        if (!string.IsNullOrEmpty(entity.live2DAnimTrigger) && !string.IsNullOrEmpty(currentModelID))
        {
            live2DController.PlayAnimation(currentModelID, entity.live2DAnimTrigger);
        }

        // パラメータ設定
        if (!string.IsNullOrEmpty(entity.live2DParamIDs) && !string.IsNullOrEmpty(entity.live2DModelID))
        {
            // パラメータIDと値の文字列を渡す
            // 複数のパラメータ値が必要なためEventEntityクラスを修正
            live2DController.SetParameters(
                entity.live2DModelID,
                entity.live2DParamIDs,
                entity.live2DParamValues,
                parameterTransitionTime
            );
        }

        yield return null;
    }

    private IEnumerator WaitForAdvance(NovelState.PlaybackMode mode)
    {
        float autoTimer = 0f;

        while (true)
        {
            switch (mode)
            {
                case NovelState.PlaybackMode.Skip:
                    yield return new WaitForSeconds(0.3f);
                    yield break;

                case NovelState.PlaybackMode.Auto:
                    if (SoundManager.Instance.voiceSource.isPlaying)
                    {
                        autoTimer = 0f;
                    }
                    else if (!messagePrinter.IsTextFullyDisplayed())
                    {
                        autoTimer = 0f;
                    }

                    autoTimer += Time.deltaTime;
                    if (autoTimer >= 2.0f)
                        yield break;
                    break;

                case NovelState.PlaybackMode.Click:
                    if (Input.GetMouseButtonDown(0))
                    {
                        yield break;
                    }
                    break;
            }

            yield return null;
        }
    }

    /// <summary>
    /// isImageFadeの文字列からフェードの有無を判定
    /// </summary>
    private bool DetermineShouldFade(string isImageFade)
    {
        // 空文字列の場合はtrueとして扱う
        if (string.IsNullOrEmpty(isImageFade))
        {
            return true;
        }

        // "false"（大文字小文字区別なし）の場合のみfalse
        if (isImageFade.ToLower() == "false")
        {
            return false;
        }

        // それ以外の文字列は無視してtrueを返す
        return true;
    }
}
