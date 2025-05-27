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

    // WaitForAdvanceCommand����ڍs�����t�B�[���h
    [Header("UI Controls")]
    [SerializeField] private Toggle autoToggle;
    [SerializeField] private Toggle skipToggle;

    // �����̃t�B�[���h�ɒǉ�
    [Header("Image Transition Settings")]
    [SerializeField] private float fadeInDuration = 0.6f;  // �t�F�[�h�C���b��
    [SerializeField] private float fadeOutDuration = 0.6f; // �t�F�[�h�A�E�g�b��
    [SerializeField] private float crossFadeDuration = 1.0f; // �N���X�t�F�[�h�b��

    [Header("Live2D Settings")]
    [SerializeField] private float parameterTransitionTime = 0.5f; // �p�����[�^�J�ڎ���

    private NovelState.PlaybackMode playbackMode = NovelState.PlaybackMode.Click;
    private float autoAdvanceDelay = 3.0f;
    private string currentModelID = ""; // ���ݕ\������Live2D���f��ID

    private void Awake()
    {
        // �g�O���C�x���g�̐ݒ�
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
            // SE��������
            if (entity.seIndex >= 0)
            {
                SoundManager.Instance.PlaySE(entity.seIndex);
            }

            // �e�L�X�g����
            if (!string.IsNullOrEmpty(entity.text) && messagePrinter != null)
            {
                messagePrinter.PrintMessage(entity.text);
            }

            // �w�i�摜����
            if (!string.IsNullOrEmpty(entity.imagePath))
            {
                bool shouldFade = DetermineShouldFade(entity.isImageFade);
                yield return HandleImageChange(entity.imagePath, shouldFade);
            }

            // �{�C�X��������
            if (entity.voiceIndex >= 0)
            {
                SoundManager.Instance.StopVoice();
                SoundManager.Instance.PlayVoice(entity.voiceIndex);
            }

            // BGM��������
            if (entity.bgmIndex >= 0)
            {
                SoundManager.Instance.StopBGMWithFadeOut(2f);
                SoundManager.Instance.PlayBGMWithFadeIn(entity.bgmIndex, 2f);
            }

            // Live2D����
            yield return HandleLive2D(entity);

            // �i�s�ҋ@
            yield return WaitForAdvance();
        }
    }

    // ���b�Z�[�W�v�����^�[�Ɣw�i�摜�̏���������
    public IEnumerator Initialize()
    {
        // �����̉摜�̃X�v���C�g�����擾
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

        // ���݂̃X�v���C�g���擾  
        Sprite previousSprite = novelState.BackgroundImage.sprite;

        // "deleteImage" �̏ꍇ�A�摜���폜  
        if (imagePath.ToLower() == "deleteimage")
        {
            if (previousSprite != null)
            {
                if (shouldFade)
                {
                    // �t�F�[�h�A�E�g�ŉ摜���폜  
                    yield return FadeOutImage(novelState.BackgroundImage, fadeOutDuration);
                }
                else
                {
                    // �����ɉ摜���폜  
                    Color color = novelState.BackgroundImage.color;
                    color.a = 0f;
                    novelState.BackgroundImage.color = color;
                }
            }
            novelState.BackgroundImage.sprite = null;
            yield break;
        }

        // Resources����摜�����[�h  
        Sprite newSprite = Resources.Load<Sprite>($"Images/{imagePath}");

        if (newSprite == null)
        {
            Debug.LogWarning($"�摜��������܂���: Images/{imagePath}");
            yield break;
        }

        // ���݂̉摜���\������Ă��邩���`�F�b�N  
        bool hasVisibleImage = previousSprite != null && novelState.BackgroundImage.color.a > 0f;

        if (shouldFade)
        {
            // �t�F�[�h����̏ꍇ  
            if (hasVisibleImage && previousSprite != newSprite)
            {
                // ���݂̉摜���قȂ�ꍇ�̓N���X�t�F�[�h  
                yield return CrossFadeImages(novelState.BackgroundImage, previousSprite, newSprite, crossFadeDuration);
            }
            else
            {
                // �V�K�摜���t�F�[�h�C��  
                novelState.BackgroundImage.sprite = newSprite;
                yield return FadeInImage(novelState.BackgroundImage, fadeInDuration);
            }
        }
        else
        {
            // �t�F�[�h�Ȃ��̏ꍇ�i�����ɐ؂�ւ��j  
            novelState.BackgroundImage.sprite = newSprite;
            Color color = novelState.BackgroundImage.color;
            color.a = 1f;
            novelState.BackgroundImage.color = color;
        }
    }

    /// <summary>
    /// �摜���t�F�[�h�C��
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
    /// �摜���t�F�[�h�A�E�g
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
    /// 2�̉摜���N���X�t�F�[�h
    /// </summary>
    private IEnumerator CrossFadeImages(Image image, Sprite oldSprite, Sprite newSprite, float duration)
    {
        // �Â��摜�p�̈ꎞ�I��Image���쐬�iUI�̔w��ɔz�u�j
        GameObject tempImageObj = new GameObject("TempCrossFadeImage");

        // �e��ݒ肷��O�ɁA�܂��K�؂Ȉʒu�ɔz�u
        tempImageObj.transform.SetParent(image.transform.parent, false);

        // siblingIndex�𒲐����āA�Â��摜���V�����摜�̌��ɗ���悤�ɂ���
        tempImageObj.transform.SetSiblingIndex(image.transform.GetSiblingIndex());

        Image tempImage = tempImageObj.AddComponent<Image>();
        RectTransform tempRect = tempImage.GetComponent<RectTransform>();

        // �ʒu�ƃT�C�Y�����킹��
        RectTransform originalRect = image.GetComponent<RectTransform>();
        tempRect.anchorMin = originalRect.anchorMin;
        tempRect.anchorMax = originalRect.anchorMax;
        tempRect.pivot = originalRect.pivot;
        tempRect.anchoredPosition = originalRect.anchoredPosition;
        tempRect.sizeDelta = originalRect.sizeDelta;

        // �Â��摜���ꎞImage�ɐݒ�
        tempImage.sprite = oldSprite;
        tempImage.preserveAspect = image.preserveAspect;

        // ���݂�Alpha�l��ێ�
        Color tempImageColor = tempImage.color;
        tempImageColor.a = image.color.a;
        tempImage.color = tempImageColor;

        // �V�����摜������Image�ɐݒ肵�A�ŏ��͓�����
        image.sprite = newSprite;
        Color newColor = image.color;
        newColor.a = 0f;
        image.color = newColor;

        // �N���X�t�F�[�h���s
        float time = 0f;
        float startAlpha = tempImageColor.a;

        while (time < duration)
        {
            time += Time.deltaTime;
            float progress = time / duration;

            // �Â��摜�t�F�[�h�A�E�g
            Color oldColor = tempImage.color;
            oldColor.a = Mathf.Lerp(startAlpha, 0f, progress);
            tempImage.color = oldColor;

            // �V�����摜�t�F�[�h�C��
            newColor.a = progress;
            image.color = newColor;

            yield return null;
        }

        // �t�F�[�h����
        newColor.a = 1f;
        image.color = newColor;

        // �ꎞ�I�u�W�F�N�g���폜
        Destroy(tempImageObj);
    }

    private IEnumerator HandleLive2D(EventEntity entity)
    {
        if (live2DController == null) yield break;

        // ���f���폜����
        if (entity.live2DAnimTrigger != null && entity.live2DAnimTrigger.ToLower() == "deletemodel")
        {
            if (!string.IsNullOrEmpty(entity.live2DModelID))
            {
                // ����̃��f�����폜
                live2DController.HideModel(entity.live2DModelID);
                Debug.Log($"Live2D���f�� '{entity.live2DModelID}' ���폜���܂����B");
            }
            else
            {
                // �S���f���폜
                live2DController.DeleteAllModels();
                Debug.Log("���ׂĂ�Live2D���f�����폜���܂����B");
            }

            yield break;
        }

        if (!string.IsNullOrEmpty(entity.live2DModelID))
            currentModelID = entity.live2DModelID; // ���݂̃��f��ID���X�V

        // ���f���\��
        if (!string.IsNullOrEmpty(entity.live2DModelID))
        {
            // Live2DController�Ƀ��f��ID�A�X�P�[���A�ʒu����������̂܂ܓn��
            live2DController.ShowModel(
                entity.live2DModelID,
                entity.live2DScale,
                entity.live2DPosition
            );
        }

        // �A�j���[�V�����Đ��ideleteModel�ȊO�̏ꍇ�̂݁j
        if (!string.IsNullOrEmpty(entity.live2DAnimTrigger) && !string.IsNullOrEmpty(currentModelID))
        {
            live2DController.PlayAnimation(currentModelID, entity.live2DAnimTrigger);
        }

        // �p�����[�^�ݒ�
        if (!string.IsNullOrEmpty(entity.live2DParamIDs) && !string.IsNullOrEmpty(entity.live2DModelID))
        {
            // �p�����[�^ID�ƒl�̕������n��
            // �����̃p�����[�^�l���K�v�Ȃ���EventEntity�N���X���C��
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
    /// isImageFade�̕����񂩂�t�F�[�h�̗L���𔻒�
    /// </summary>
    private bool DetermineShouldFade(string isImageFade)
    {
        // �󕶎���̏ꍇ��true�Ƃ��Ĉ���
        if (string.IsNullOrEmpty(isImageFade))
        {
            return true;
        }

        // "false"�i�啶����������ʂȂ��j�̏ꍇ�̂�false
        if (isImageFade.ToLower() == "false")
        {
            return false;
        }

        // ����ȊO�̕�����͖�������true��Ԃ�
        return true;
    }
}
