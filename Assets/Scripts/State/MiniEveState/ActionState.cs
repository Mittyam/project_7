using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// �A�j���[�V�����̃X�e�[�g�𓝊����Ĉ���
/// �}���فA�J�t�F�A�o�C�g�A�U���A�Q�[���A���o�����A���b�A����
/// </summary>
public class ActionState : MiniEventState
{
    [Header("Talk UI References")]
    [SerializeField] private List<Button> talkOptionButtons = new List<Button>();
    [SerializeField] private Button closeButton;

    private int selectedTopicID = -1;

    // �A�N�V�����^�C�v
    protected string actionType;

    public override void OnEnter()
    {
        // �e�N���X��OnEnter���Ăяo��
        base.OnEnter();

        Debug.Log("ActionState: ����܂��B");

        // stateData��null�̏ꍇ�͑������^�[���i�e�N���X�Ń`�F�b�N�ς݁j
        if (stateData == null) return;

        Debug.Log("ActionState: ��b�I�v�V������\�����܂��B");

        // �p�����[�^����A�N�V�����^�C�v���擾
        actionType = GetParameter<string>("ActionType", "");

        // �A�N�V�����^�C�v�Ɋ�Â�������
        switch (actionType)
        {
            case "Library":
                // �}���كA�N�V��������
                Debug.Log("�}���قł̃A�N�V�����J�n");
                break;

            case "Cafe":
                // �J�t�F�A�N�V��������
                Debug.Log("�J�t�F�ł̃A�N�V�����J�n");
                break;

            case "Work":
                // �o�C�g�A�N�V��������
                Debug.Log("�o�C�g�ł̃A�N�V�����J�n");
                break;

            case "Walk":
                // �U���A�N�V��������
                Debug.Log("�U���ł̃A�N�V�����J�n");
                break;

            case "Game":
                // �Q�[���A�N�V��������
                Debug.Log("�Q�[���ł̃A�N�V�����J�n");
                break;

            case "Talk":
                // ���b�A�N�V��������
                Debug.Log("���b�A�N�V�����J�n");
                break;

            case "Outing":
                // ���o�����A�N�V��������
                Debug.Log("���o�����A�N�V�����J�n");
                break;

            case "Sleep":
                // �����A�N�V��������
                Debug.Log("�����A�N�V�����J�n");
                break;

            default:
                Debug.LogWarning($"�s���ȃA�N�V�����^�C�v: {actionType}");
                break;
        }

        // �{�^���C�x���g�̓o�^
        SetupButtons();
    }

    public override void OnExit()
    {
        // �C�x���g���X�i�[�̉���
        //if (closeButton != null)
        //{
        //    closeButton.onClick.RemoveListener(CompleteEvent);
        //}

        //if (talkOptionButtons != null)
        //{
        //    for (int i = 0; i < talkOptionButtons.Length; i++)
        //    {
        //        talkOptionButtons[i].onClick.RemoveAllListeners();
        //    }
        //}

        base.OnExit();
    }

    /// <summary>
    /// ��b�g�s�b�N�̑I��
    /// </summary>
    private void SelectTopic(int topicID)
    {
        selectedTopicID = topicID;
        Debug.Log($"��b�g�s�b�N {topicID} ���I������܂���");

        // �I�������g�s�b�N�ɉ���������
        // ��F�@�����_���ȍD���x�㏸�Ȃ�
        int affectionChange = Random.Range(1, 4);
        StatusManager.Instance.UpdateStatus(0, affectionChange, 0, 0);

        // TODO: �I�������g�s�b�N�ɉ�������b�e�L�X�g�̕\������

        // �I����UI�̕\���i��������΁j
        ShowEndUI();

        // �I����͂����ɕ��邩�A������x�I���\�ɂ��邩�Ȃ�
        // �����ł͒P�����̂��߁A�I��������C�x���g���I������
        StartCoroutine(CompleteEventAfterDelay(2.0f));
    }

    /// <summary>
    /// �x����ɃC�x���g������������
    /// </summary>
    private IEnumerator CompleteEventAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        CompleteEvent();
    }

    /// <summary>
    /// �{�^���̃Z�b�g�A�b�v
    /// </summary>
    private void SetupButtons()
    {
        // closeButton�̃`�F�b�N
        if (closeButton == null)
        {
            // ContentContainer������T��
            //closeButton = contentContainer?.transform.Find("CloseButton")?.GetComponent<Button>();

            //if (closeButton == null)
            //{
            //    Debug.LogWarning("TalkState: CloseButton��������܂���");
            //}
            //else
            //{
            //    closeButton.onClick.AddListener(CompleteEvent);
            //}
        }
        else
        {
            //closeButton.onClick.AddListener(CompleteEvent);
        }

        // talkOptionButtons�̃`�F�b�N
        if (talkOptionButtons == null || talkOptionButtons.Count == 0)
        {
            // ContentContainer������{�^����T��
            //Button[] foundButtons = contentContainer?.GetComponentsInChildren<Button>();
            //if (foundButtons != null && foundButtons.Length > 0)
            //{
            //    // CloseButton�ȊO�̃{�^����talkOptionButtons�Ƃ��Ďg�p
            //    List<Button> options = new List<Button>();
            //    foreach (var button in foundButtons)
            //    {
            //        if (button != closeButton)
            //        {
            //            options.Add(button);
            //        }
            //    }
            //    talkOptionButtons = options.ToArray();
            //}
        }

        // ��b�I�v�V�����{�^���̃Z�b�g�A�b�v
        if (talkOptionButtons != null && talkOptionButtons.Count > 0)
        {
            //for (int i = 0; i < talkOptionButtons.Length; i++)
            //{
            //    if (talkOptionButtons[i] != null)
            //    {
            //        int topicID = i; // ���[�J���ϐ��ɃR�s�[
            //        talkOptionButtons[i].onClick.RemoveAllListeners();
            //        talkOptionButtons[i].onClick.AddListener(() => SelectTopic(topicID));
            //    }
            //}
        }
        else
        {
            Debug.LogWarning("TalkState: ��b�I�v�V�����{�^�����ݒ肳��Ă��܂���");
        }
    }
}
