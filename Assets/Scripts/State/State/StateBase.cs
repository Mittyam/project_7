// StateBase.cs - �V�[���z�u�Ή�
using UnityEngine;
using UnityEngine.UI;

public abstract class StateBase : MonoBehaviour, IState
{
    // GameLoop����̎Q�Ɨp�w���p�[���\�b�h
    protected GameLoop GameLoop => GameLoop.Instance;
    protected PushdownStateMachine PushdownStack => GameLoop.Instance.PushdownStack;
    protected MainStateMachine MainStateMachine => GameLoop.Instance.MainStateMachine;
    protected NovelEventScheduler NovelEventScheduler => GameLoop.Instance.NovelEventScheduler;
    protected StatesContainer StatesContainer => GameLoop.Instance.StatesContainer;

    [Header("UI Rendering")]
    [SerializeField] protected Camera uiRenderCamera; // UI�\���p�J����

    // IState���\�b�h�i���ۃ��\�b�h�Ƃ��Ē�`�j
    public abstract void OnEnter();
    public abstract void OnUpdate();
    public abstract void OnExit();

    // IState�v���p�e�B����
    public IState NextState { get; set; }

    bool IState.enabled
    {
        get => enabled;
        set => enabled = value;
    }

    /// <summary>
    /// UI�v�f�̏������Ɛݒ�
    /// </summary>
    protected virtual void SetupUI()
    {
        // ���N���X�ł͉������Ȃ��A�q�N���X�ŃI�[�o�[���C�h����
        Debug.Log($"{GetType().Name}: UI�ݒ�̊�{���\�b�h���R�[������܂����B");
    }

    /// <summary>
    /// ���ׂĂ�UI�v�f���\���ɂ���
    /// </summary>
    protected virtual void HideAllUI()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// �w�肳�ꂽ�R���|�[�l���g���q���猟�����Ď擾����
    /// </summary>
    protected T GetUIComponent<T>(string path) where T : Component
    {
        Transform target = transform.Find(path);
        if (target != null)
        {
            return target.GetComponent<T>();
        }
        return null;
    }

    /// <summary>
    /// UIPrefab���w��̐e�̉��ɐ�������
    /// </summary>
    protected GameObject InstantiateUI(GameObject prefab, Transform parent = null)
    {
        if (prefab == null) return null;

        // �e���w�肳��Ă��Ȃ���Ύ������g���g�p
        Transform targetParent = parent != null ? parent : transform;

        // �������ĕԂ�
        GameObject instance = Instantiate(prefab, targetParent);
        instance.SetActive(true);
        return instance;
    }

    /// <summary>
    /// UIPrefab��Canvas�𒲐����Ă��琶������
    /// </summary>
    protected GameObject InstantiateUIWithCamera(GameObject prefab, Transform parent = null)
    {
        if (prefab == null) return null;

        Transform targetParent = parent != null ? parent : transform;
        GameObject instance = Instantiate(prefab, targetParent);
        instance.SetActive(true);

        // Canvas�ݒ�
        Canvas canvas = instance.GetComponent<Canvas>();
        if (canvas != null && uiRenderCamera != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = uiRenderCamera;
            canvas.planeDistance = 10f;

            // Canvas Scaler�ݒ�
            CanvasScaler scaler = instance.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                // 16:9�ɌŒ肷��ݒ�
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080); // 16:9
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand; // �܂��� MatchWidthOrHeight

                // MatchWidthOrHeight���g���ꍇ�́A0�i���ɍ��킹��j��
                // 1�i�����ɍ��킹��j��ݒ肵�܂�
                // 0.5�͕��ƍ����̃o�����X���Ƃ�܂�
                scaler.matchWidthOrHeight = 0;

                Debug.Log($"Canvas {instance.name} �̃X�P�[���[��16:9�ɐݒ肵�܂���");
            }
        }
        else if (canvas != null && uiRenderCamera == null)
        {
            Debug.LogWarning($"UIRenderCamera���ݒ肳��Ă��Ȃ����߁A{instance.name}��Canvas�̓f�t�H���g�ݒ�̂܂܂ł�");
        }

        return instance;
    }

    protected void CleanupUI(Transform container)
    {
        if (container == null) return;

        // �R���e�i���̑S�Ă̎q�I�u�W�F�N�g��j��
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }
}

// MainStateBase.cs - ���C���X�e�[�g�p�x�[�X�N���X
public abstract class MainStateBase : StateBase, IPausableState
{
    [Header("State Data")]
    [SerializeField] protected MainStateData stateData;

    public StateID GetStateID()
    {
        return stateData != null ? stateData.stateID : StateID.None;
    }

    public StateID GetNextStateID()
    {
        return stateData != null ? stateData.nextStateID : StateID.None;
    }

    // IPausableState�̎���
    public virtual void OnPause()
    {
        Debug.Log($"MainState: {stateData?.displayName} ���ꎞ��~���܂�");

        // UI�v�f�̔�\������
        HideAllUI();
    }

    public virtual void OnResume()
    {
        Debug.Log($"MainState: {stateData?.displayName} ���ĊJ���܂�");

        // UI�v�f�̍ĕ\������
        SetupUI();

        // �q�N���X�ł͂��̌��ShowXXXUI()�Ȃǂ�UI�\�����\�b�h���ĂԂ��Ƃ�z��
    }

    // SetupUI�̃I�[�o�[���C�h�i�ʏ�͎q�N���X�ł���ɃI�[�o�[���C�h�����j
    protected override void SetupUI()
    {
        base.SetupUI();

        // �X�e�[�g�f�[�^����UI�v���n�u��ǂݍ��ޏꍇ�̏���
        if (stateData != null && stateData.uiPrefab != null)
        {
            foreach (var prefab in stateData.uiPrefab)
            {
                if (prefab != null)
                {
                    // �J�����ݒ�t���̃C���X�^���X�����\�b�h���g�p
                    InstantiateUIWithCamera(prefab, transform);
                }
            }
        }
    }
}

// MiniEventStateBase.cs - �~�j�C�x���g�p�x�[�X�N���X
public abstract class MiniEventStateBase : StateBase, IPausableState
{
    [Header("State Data")]
    [SerializeField] protected MiniEventStateData stateData;

    [Header("UI Container")]
    [SerializeField] protected GameObject contentContainer;

    public StateID GetStateID()
    {
        return stateData != null ? stateData.stateID : StateID.None;
    }

    public virtual void OnPause()
    {
        Debug.Log($"MiniEventState: {stateData.displayName} ���ꎞ��~���܂�");

        // UI���\���ɂ���Ȃǂ̏���
        if (contentContainer != null)
        {
            contentContainer.SetActive(false);
        }
    }

    public virtual void OnResume()
    {
        Debug.Log($"MiniEventState: {stateData.displayName} ���ĊJ���܂�");

        // UI���ĕ\������Ȃǂ̏���
        if (contentContainer != null)
        {
            contentContainer.SetActive(true);
        }
    }

    /// <summary>
    /// �C�x���g�I�������i���[�U�[�A�N�V��������Ă΂��j
    /// </summary>
    public void CompleteEvent()
    {
        // �������g��PushdownStack����pop
        PushdownStack.Pop();
    }
}
