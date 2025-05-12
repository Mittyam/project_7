using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// シーン間とステート間のトランジション効果を管理するグローバルマネージャー
/// </summary>
public class TransitionManager : Singleton<TransitionManager>
{
    [Header("トランジション設定")]
    [SerializeField] private float transitionDuration = 0.5f;
    [SerializeField] private Color fadeColor = Color.black;

    private CanvasGroup fadeCanvasGroup;
    private Canvas fadeCanvas;
    private Image fadeImage;
    private Coroutine currentTransition;
    private bool isTransitioning = false;

    protected override void Awake()
    {
        base.Awake(); // 重要：Singletonの基本機能を呼び出す
        InitializeFadeCanvas();
    }

    /// <summary>
    /// フェードキャンバスの初期化
    /// </summary>
    private void InitializeFadeCanvas()
    {
        // フェード用のキャンバスとパネルを作成
        GameObject canvasObj = new GameObject("FadeCanvas");
        canvasObj.transform.SetParent(transform);

        fadeCanvas = canvasObj.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 9999; // 最前面表示

        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject panelObj = new GameObject("FadePanel");
        panelObj.transform.SetParent(fadeCanvas.transform, false);

        RectTransform rt = panelObj.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero; // 画面全体を覆う

        fadeImage = panelObj.AddComponent<Image>();
        fadeImage.color = fadeColor;
        fadeImage.raycastTarget = false; // 入力イベントをブロックしない

        fadeCanvasGroup = panelObj.AddComponent<CanvasGroup>();
        fadeCanvasGroup.alpha = 0;
        fadeCanvasGroup.blocksRaycasts = false;

        Debug.Log("TransitionManager: フェードキャンバスを初期化しました");
    }

    /// <summary>
    /// フェードインを実行（黒→透明）
    /// </summary>
    public void FadeIn(Action onComplete = null)
    {
        if (isTransitioning && currentTransition != null)
        {
            StopCoroutine(currentTransition);
        }

        currentTransition = StartCoroutine(FadeCoroutine(1, 0, transitionDuration, onComplete));
    }

    /// <summary>
    /// フェードアウトを実行（透明→黒）
    /// </summary>
    public void FadeOut(Action onComplete = null)
    {
        if (isTransitioning && currentTransition != null)
        {
            StopCoroutine(currentTransition);
        }

        currentTransition = StartCoroutine(FadeCoroutine(0, 1, transitionDuration, onComplete));
    }

    /// <summary>
    /// クロスフェードを実行（現在の状態→黒→透明）
    /// </summary>
    public void CrossFade(Action midPointAction = null, Action onComplete = null)
    {
        StartCoroutine(CrossFadeCoroutine(transitionDuration, midPointAction, onComplete));
    }

    /// <summary>
    /// シーン遷移用のフェード
    /// </summary>
    public void FadeAndLoadScene(string sceneName, Action beforeSceneLoad = null, Action afterSceneLoad = null)
    {
        StartCoroutine(LoadSceneCoroutine(sceneName, beforeSceneLoad, afterSceneLoad));
    }

    private IEnumerator FadeCoroutine(float startAlpha, float endAlpha, float duration, Action onComplete = null)
    {
        isTransitioning = true;
        fadeCanvasGroup.alpha = startAlpha;
        fadeCanvasGroup.blocksRaycasts = startAlpha > 0.5f; // 半透明以上でブロック

        float elapsedTime = 0;

        // 0.1秒待機
        yield return new WaitForSeconds(0.1f);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);

            // レイキャストをブロックするかどうかを動的に変更
            fadeCanvasGroup.blocksRaycasts = fadeCanvasGroup.alpha > 0.5f;

            yield return null;
        }

        fadeCanvasGroup.alpha = endAlpha;
        fadeCanvasGroup.blocksRaycasts = endAlpha > 0.5f;

        isTransitioning = false;
        onComplete?.Invoke();
    }

    private IEnumerator CrossFadeCoroutine(float duration, Action midPointAction = null, Action onComplete = null)
    {
        // フェードアウト
        yield return FadeCoroutine(0, 1, duration / 2, null);

        // 中間ポイントで実行するアクション
        midPointAction?.Invoke();

        // フェードイン
        yield return FadeCoroutine(1, 0, duration / 2, null);

        onComplete?.Invoke();
    }

    private IEnumerator LoadSceneCoroutine(string sceneName, Action beforeSceneLoad = null, Action afterSceneLoad = null)
    {
        // フェードアウト
        yield return FadeCoroutine(0, 1, transitionDuration, null);

        // シーン読み込み前の処理
        beforeSceneLoad?.Invoke();

        // シーン非同期読み込み
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // シーン読み込み後の処理
        afterSceneLoad?.Invoke();

        // フェードイン
        yield return FadeCoroutine(1, 0, transitionDuration, null);
    }

    /// <summary>
    /// フェード色を変更
    /// </summary>
    public void SetFadeColor(Color color)
    {
        fadeColor = color;
        if (fadeImage != null)
        {
            fadeImage.color = color;
        }
    }

    /// <summary>
    /// トランジション時間を変更
    /// </summary>
    public void SetTransitionDuration(float duration)
    {
        transitionDuration = Mathf.Max(0.1f, duration);
    }

    /// <summary>
    /// 即座にフェードパネルの状態を設定（アニメーションなし）
    /// </summary>
    public void SetFadePanelAlpha(float alpha)
    {
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = alpha;
            fadeCanvasGroup.blocksRaycasts = alpha > 0.5f;
        }
    }
}
