using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// �V�[���ԂƃX�e�[�g�Ԃ̃g�����W�V�������ʂ��Ǘ�����O���[�o���}�l�[�W���[
/// </summary>
public class TransitionManager : Singleton<TransitionManager>
{
    [Header("�g�����W�V�����ݒ�")]
    [SerializeField] private float transitionDuration = 0.5f;
    [SerializeField] private Color fadeColor = Color.black;

    private CanvasGroup fadeCanvasGroup;
    private Canvas fadeCanvas;
    private Image fadeImage;
    private Coroutine currentTransition;
    private bool isTransitioning = false;

    protected override void Awake()
    {
        base.Awake(); // �d�v�FSingleton�̊�{�@�\���Ăяo��
        InitializeFadeCanvas();
    }

    /// <summary>
    /// �t�F�[�h�L�����o�X�̏�����
    /// </summary>
    private void InitializeFadeCanvas()
    {
        // �t�F�[�h�p�̃L�����o�X�ƃp�l�����쐬
        GameObject canvasObj = new GameObject("FadeCanvas");
        canvasObj.transform.SetParent(transform);

        fadeCanvas = canvasObj.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 9999; // �őO�ʕ\��

        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject panelObj = new GameObject("FadePanel");
        panelObj.transform.SetParent(fadeCanvas.transform, false);

        RectTransform rt = panelObj.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero; // ��ʑS�̂𕢂�

        fadeImage = panelObj.AddComponent<Image>();
        fadeImage.color = fadeColor;
        fadeImage.raycastTarget = false; // ���̓C�x���g���u���b�N���Ȃ�

        fadeCanvasGroup = panelObj.AddComponent<CanvasGroup>();
        fadeCanvasGroup.alpha = 0;
        fadeCanvasGroup.blocksRaycasts = false;

        Debug.Log("TransitionManager: �t�F�[�h�L�����o�X�����������܂���");
    }

    /// <summary>
    /// �t�F�[�h�C�������s�i���������j
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
    /// �t�F�[�h�A�E�g�����s�i���������j
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
    /// �N���X�t�F�[�h�����s�i���݂̏�ԁ����������j
    /// </summary>
    public void CrossFade(Action midPointAction = null, Action onComplete = null)
    {
        StartCoroutine(CrossFadeCoroutine(transitionDuration, midPointAction, onComplete));
    }

    /// <summary>
    /// �V�[���J�ڗp�̃t�F�[�h
    /// </summary>
    public void FadeAndLoadScene(string sceneName, Action beforeSceneLoad = null, Action afterSceneLoad = null)
    {
        StartCoroutine(LoadSceneCoroutine(sceneName, beforeSceneLoad, afterSceneLoad));
    }

    private IEnumerator FadeCoroutine(float startAlpha, float endAlpha, float duration, Action onComplete = null)
    {
        isTransitioning = true;
        fadeCanvasGroup.alpha = startAlpha;
        fadeCanvasGroup.blocksRaycasts = startAlpha > 0.5f; // �������ȏ�Ńu���b�N

        float elapsedTime = 0;

        // 0.1�b�ҋ@
        yield return new WaitForSeconds(0.1f);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);

            // ���C�L���X�g���u���b�N���邩�ǂ����𓮓I�ɕύX
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
        // �t�F�[�h�A�E�g
        yield return FadeCoroutine(0, 1, duration / 2, null);

        // ���ԃ|�C���g�Ŏ��s����A�N�V����
        midPointAction?.Invoke();

        // �t�F�[�h�C��
        yield return FadeCoroutine(1, 0, duration / 2, null);

        onComplete?.Invoke();
    }

    private IEnumerator LoadSceneCoroutine(string sceneName, Action beforeSceneLoad = null, Action afterSceneLoad = null)
    {
        // �t�F�[�h�A�E�g
        yield return FadeCoroutine(0, 1, transitionDuration, null);

        // �V�[���ǂݍ��ݑO�̏���
        beforeSceneLoad?.Invoke();

        // �V�[���񓯊��ǂݍ���
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // �V�[���ǂݍ��݌�̏���
        afterSceneLoad?.Invoke();

        // �t�F�[�h�C��
        yield return FadeCoroutine(1, 0, transitionDuration, null);
    }

    /// <summary>
    /// �t�F�[�h�F��ύX
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
    /// �g�����W�V�������Ԃ�ύX
    /// </summary>
    public void SetTransitionDuration(float duration)
    {
        transitionDuration = Mathf.Max(0.1f, duration);
    }

    /// <summary>
    /// �����Ƀt�F�[�h�p�l���̏�Ԃ�ݒ�i�A�j���[�V�����Ȃ��j
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
