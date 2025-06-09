using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class MessagePrinter : MonoBehaviour
{
    public TextMeshProUGUI messageText;       // ���b�Z�[�W�\���p TextMeshPro
    // public float characterDelay = 0.05f;   // �Œ�l�ł͂Ȃ�ConfigUIManager����擾
    public float messageDisplayTime = 2.0f;   // �\��������Ƀ��b�Z�[�W�������܂ł̎���
    public bool autoClearMessage = true;      // ��莞�Ԍ�Ƀ��b�Z�[�W���N���A���邩

    private Coroutine currentCoroutine = null;
    private Coroutine clearCoroutine = null;

    /// <summary>
    /// �e�L�X�g�̃t���\���i�Ō�̕����܂ŕ\���j�������ɌĂяo�����C�x���g
    /// NovelEventManager �̃I�[�g�i�s�����̃C�x���g�𗘗p���Ă���
    /// </summary>
    public event Action OnTextFullDisplayed;

    /// <summary>
    /// �V�������b�Z�[�W��\������
    /// </summary>
    /// <param name="message">�\������������</param>
    public void PrintMessage(string message)
    {
        // ���łɕ\�����̃R���[�`��������Β�~
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        // ���b�Z�[�W�N���A�҂��R���[�`������~
        if (clearCoroutine != null)
        {
            StopCoroutine(clearCoroutine);
            clearCoroutine = null;
        }

        // �V�����R���[�`���J�n
        currentCoroutine = StartCoroutine(DisplayMessageRoutine(message));
    }


    /// <summary>
    /// ���b�Z�[�W��\������i�R���[�`�����g�p���Ȃ��j
    /// </summary>
    /// <param name="message"></param>
    public void ShowMessage(string message)
    {
        // ���łɕ\�����̃R���[�`��������Β�~
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }
        // ���b�Z�[�W�N���A�҂��R���[�`������~
        if (clearCoroutine != null)
        {
            StopCoroutine(clearCoroutine);
            clearCoroutine = null;
        }
        // ���b�Z�[�W�𑦍��ɕ\��
        messageText.text = message;
        // �����Ń��b�Z�[�W�N���A����ݒ�Ȃ��莞�Ԍ�ɃN���A
        if (autoClearMessage)
        {
            clearCoroutine = StartCoroutine(ClearMessageAfterDelay());
        }
    }

    /// <summary>
    /// 1�������\�����Ă����R���[�`��
    /// </summary>
    private IEnumerator DisplayMessageRoutine(string message)
    {
        messageText.text = ""; // �\�������Z�b�g

        int seCounter = 0; // SE�J�E���^�[������

        // ConfigUIManager���當�����葬�x���擾
        float characterDelay = ConfigUIManager.Instance.TextSpeed;

        // 1�������ǉ����ĕ\��
        foreach (char c in message)
        {
            messageText.text += c;

            if (seCounter % 2 == 0)
            { // 2�������Ƃ�SE��炷
                SoundManager.Instance.PlaySE(11);
            }
            seCounter++;

            yield return new WaitForSeconds(characterDelay);
        }

        // �S�����\��������ʒm
        OnTextFullDisplayed?.Invoke();

        // �����Ń��b�Z�[�W�N���A����ݒ�Ȃ��莞�Ԍ�ɃN���A
        if (autoClearMessage)
        {
            clearCoroutine = StartCoroutine(ClearMessageAfterDelay());
        }

        currentCoroutine = null;
    }

    /// <summary>
    /// �w�莞�Ԍ�Ƀ��b�Z�[�W���N���A����
    /// </summary>
    private IEnumerator ClearMessageAfterDelay()
    {
        yield return new WaitForSeconds(messageDisplayTime);
        messageText.text = "";
        clearCoroutine = null;
    }

    /// <summary>
    /// �e�L�X�g���Ō�܂ŕ\������Ă��邩�ǂ�����Ԃ�
    /// �i�R���[�`��������Ă��Ȃ���� = �S�\������ or �����\�����Ă��Ȃ��j
    /// </summary>
    public bool IsTextFullyDisplayed()
    {
        // currentCoroutine �� null �ł���΁u�������肪�I����Ă���v���
        return (currentCoroutine == null);
    }

    /// <summary>
    /// ���b�Z�[�W���N���A����
    /// </summary>
    public void ClearMessage()
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
            clearCoroutine = null;
        }
        messageText.text = "";
    }
}
