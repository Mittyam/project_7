using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Unity.VisualScripting;
using System;

public class AnimationTriggerState : MonoBehaviour, ISubState
{
    [Header("��{�Q��")]
    [SerializeField] private SubStateMachine subStateMachine;
    [SerializeField] private ProgressManager progressManager;

    [Header("�A�j���[�V�����ݒ�")]
    [SerializeField] private RectTransform animationCanvas;
    [SerializeField] private Image backgroundImage;    // BG�C���[�W
    [SerializeField] private Image animBaseImage;      // �A�j���[�V�����̃x�[�X�C���[�W
    [SerializeField] private float animationDuration = 2.0f;
    [SerializeField] private float centerPauseDuration = 1.0f;
    [SerializeField] private float fadeInOutDuration = 0.5f;  // �t�F�[�h�C��/�A�E�g����
    [SerializeField] private float offScreenX = 1000f;  // ��ʊO�̍��W

    [Header("�C�x���g�ʃA�j���[�V����")]
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
        get => base.enabled;    // MonoBehaviour��enabled
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
        Debug.Log($"�C�x���gID:{eventID}�̃A�j���[�V�����T�u�X�e�[�g�J�n");

        // �A�j���[�V�����L�����o�X���A�N�e�B�u�ɂ���
        if (animationCanvas != null)
        {
            animationCanvas.gameObject.SetActive(true);

            // ������Ԃł͔w�i�ƃx�[�X�͓�����
            if (backgroundImage != null)
                backgroundImage.color = new Color(backgroundImage.color.r, backgroundImage.color.g, backgroundImage.color.b, 0);

            if (animBaseImage != null)
                animBaseImage.color = new Color(animBaseImage.color.r, animBaseImage.color.g, animBaseImage.color.b, 0);
        }
        else
        {
            Debug.LogError("AnimationCanvas ���ݒ肳��Ă��܂���B");
            CompleteAnimation();
            return;
        }

        // �w�i��x�[�X�C���[�W�̎Q�Ɗm�F
        if (backgroundImage == null || animBaseImage == null)
        {
            Debug.LogError("�w�i�C���[�W�܂��̓A�j���[�V�����x�[�X�C���[�W���ݒ肳��Ă��܂���B");
            CompleteAnimation();
            return;
        }

        // �Ή�����C�x���g�A�j���[�V������T��
        EventAnimation eventAnimation = eventAnimations.Find(ea => ea.eventID == eventID);

        if (eventAnimation != null)
        {
            if (eventAnimation.animationSprite == null)
            {
                Debug.LogError($"�C�x���gID:{eventID}�̃A�j���[�V�����X�v���C�g���ݒ肳��Ă��܂���B");
                CompleteAnimation();
                return;
            }

            PlayAnimation(eventAnimation);
        }
        else
        {
            Debug.LogWarning($"�C�x���gID:{eventID}�ɑΉ�����A�j���[�V������������܂���");
            // �A�j���[�V�������Ȃ��ꍇ�͑��I��
            CompleteAnimation();
        }
    }

    public void OnUpdate()
    {
        // �A�j���[�V�����̊�����҂����Ȃ̂ŁA���ɉ������Ȃ�
    }

    public void OnExit()
    {
        // �V�[�P���X�����s���Ȃ��~
        if (currentSequence != null && currentSequence.IsActive())
        {
            currentSequence.Kill();
        }

        // ���������摜������΍폜
        if (currentAnimationImage != null)
        {
            Destroy(currentAnimationImage.gameObject);
            currentAnimationImage = null;
            currentAnimationRect = null;
        }

        // �w�i�ƃx�[�X�𓧖��ɖ߂�
        if (backgroundImage != null)
            backgroundImage.color = new Color(backgroundImage.color.r, backgroundImage.color.g, backgroundImage.color.b, 0);

        if (animBaseImage != null)
            animBaseImage.color = new Color(animBaseImage.color.r, animBaseImage.color.g, animBaseImage.color.b, 0);

        // �O�̂��߁A�A�j���[�V�����L�����o�X���A�N�e�B�u�ɂ���
        if (animationCanvas != null)
        {
            animationCanvas.gameObject.SetActive(false);
        }
    }

    private void PlayAnimation(EventAnimation eventAnimation)
    {
        isAnimationPlaying = true;

        // �V�[�P���X�̍쐬
        currentSequence = DOTween.Sequence();

        // �w�i�ƃx�[�X���������i�����Ɂj
        backgroundImage.color = new Color(backgroundImage.color.r, backgroundImage.color.g, backgroundImage.color.b, 0);
        animBaseImage.color = new Color(animBaseImage.color.r, animBaseImage.color.g, animBaseImage.color.b, 0);

        // �A�j���[�V�����p�̉摜�𐶐�
        GameObject animObj = new GameObject($"Animation_{eventAnimation.animationName}");
        animObj.transform.SetParent(animBaseImage.transform, false);

        // RectTransform�̐ݒ�
        currentAnimationRect = animObj.AddComponent<RectTransform>();
        currentAnimationRect.anchorMin = new Vector2(0.5f, 0.5f);
        currentAnimationRect.anchorMax = new Vector2(0.5f, 0.5f);
        currentAnimationRect.pivot = new Vector2(0.5f, 0.5f);

        // �T�C�Y�𖾎��I�ɐݒ�i���ꂪ0�ɂȂ���̉����j
        currentAnimationRect.sizeDelta = eventAnimation.spriteSize;

        // �J�n�ʒu�i��ʊO���j
        currentAnimationRect.anchoredPosition = new Vector2(-offScreenX, 0);

        // �C���[�W�R���|�[�l���g�ݒ�
        currentAnimationImage = animObj.AddComponent<Image>();
        currentAnimationImage.sprite = eventAnimation.animationSprite;
        currentAnimationImage.preserveAspect = true; // �A�X�y�N�g���ێ�

        // �X�v���C�g�ݒ肪�����������O�o�́i�f�o�b�O�p�j
        Debug.Log($"�A�j���[�V�����ݒ�: ID={eventID}, Sprite={eventAnimation.animationSprite}, Size={eventAnimation.spriteSize}");

        // 1. �w�i�ƃx�[�X�̃t�F�[�h�C��
        currentSequence.Append(backgroundImage.DOFade(1, fadeInOutDuration));
        currentSequence.Join(animBaseImage.DOFade(1, fadeInOutDuration));

        // 2. �����璆���ֈړ�
        currentSequence.Append(currentAnimationRect.DOAnchorPosX(0, animationDuration / 2)
            .SetEase(Ease.OutQuad));

        // 3. �����ňꎞ��~
        currentSequence.AppendInterval(centerPauseDuration);

        // 4. ��������E�ֈړ�
        currentSequence.Append(currentAnimationRect.DOAnchorPosX(offScreenX, animationDuration / 2)
            .SetEase(Ease.InQuad));

        // 5. �w�i�ƃx�[�X�̃t�F�[�h�A�E�g
        currentSequence.Append(backgroundImage.DOFade(0, fadeInOutDuration));
        currentSequence.Join(animBaseImage.DOFade(0, fadeInOutDuration));

        // 6. �A�j���[�V�����������̏���
        currentSequence.OnComplete(() => {
            // �X�e�[�^�X�ύX��K�p
            ApplyStatusChange(eventAnimation.statusChange);

            // �A�j���[�V������������
            CompleteAnimation();
        });
    }

    private void ApplyStatusChange(StatusChange statusChange)
    {
        if (statusChange != null)
        {
            // ���߂̕ύX���e��ێ��iUI �����Q�Ɖ\�j
            LastStatusChange = statusChange;

            // StatusManager��ʂ��ăX�e�[�^�X��ύX
            StatusManager.Instance.UpdateStatus(
                0,
                statusChange.affectionChange,
                statusChange.lovelityChange,
                statusChange.moneyChange
                );

            Debug.Log($"�X�e�[�^�X�ύX: Affection={statusChange.affectionChange}, Love={statusChange.lovelityChange}, Money={statusChange.moneyChange}");
        }
    }

    // �A�j���[�V�����������̏���
    private void CompleteAnimation()
    {
        isAnimationPlaying = false;

        // �w�i�ƃx�[�X�𓧖��ɖ߂��Ă���L�����o�X���\���ɂ���
        // ����: DOTween�V�[�P���X�Ńt�F�[�h�A�E�g��ɌĂ΂��P�[�X�ƁA
        // �G���[���ɑ����Ă΂��P�[�X�̗����ɑΉ�
        if (backgroundImage != null)
            backgroundImage.color = new Color(backgroundImage.color.r, backgroundImage.color.g, backgroundImage.color.b, 0);

        if (animBaseImage != null)
            animBaseImage.color = new Color(animBaseImage.color.r, animBaseImage.color.g, animBaseImage.color.b, 0);

        // �A�j���[�V�����L�����o�X���A�N�e�B�u�ɂ���
        if (animationCanvas != null)
            animationCanvas.gameObject.SetActive(false);

        // �����Œʒm�𔭍s�iUI���͂�����󂯎���ă��b�Z�[�W�p�l���𑀍삷��j
        OnAnimationFinished?.Invoke();

        // ���ʏ�ԂɈڍs
        subStateMachine.EndSubEvent();
    }

    // �O������A�j���[�V�������X�L�b�v���邽�߂̃��\�b�h
    public void SkipAnimation()
    {
        if (isAnimationPlaying && currentSequence != null)
        {
            currentSequence.Complete();
        }
    }
}
