using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class HoliDayUIManager : MonoBehaviour
{
    [Header("UI�Q��")]
    [SerializeField] private DayState dayState;

    [Header("���b�Z�[�W�p�l���Q��")]
    [SerializeField] private AnimationTriggerState animationTriggerState;
    [SerializeField] private MessagePanelController messagePanel;
    [SerializeField] private MessagePrinter messagePrinter;

    [Header("�I�����p�l����\��")]
    [SerializeField] private ChoiceButtonGenerator choicePanel;

    [Header("�{�^���Q��")]
    [SerializeField] private Button gameButton;
    [SerializeField] private Button outingButton;
    [SerializeField] private Button workButton;
    [SerializeField] private Button talkButton;
    [SerializeField] private Button itemButton;
    [SerializeField] private Button memoryButton;

    [Header("InfoPanel�Q��")]
    [SerializeField] private InfoPanelController infoPanel;

    [Header("�T�u�X�e�[�g�}�V��")]
    [SerializeField] private SubStateMachine subStateMachine;

    [Header("�T�u�C�x���gID")]
    [SerializeField] private int gameEventID;
    [SerializeField] private int outingEventID;
    [SerializeField] private int workEventID;
    [SerializeField] private int talkEventID;

    private void Start()
    {
        //gameButton.onClick.AddListener(OnGameButtonClicked);
        //outingButton.onClick.AddListener(OnOutingButtonClicked);
        //workButton.onClick.AddListener(OnWorkButtonClicked);
        //talkButton.onClick.AddListener(OnTalkButtonClicked);
        //itemButton.onClick.AddListener(OnItemButtonClicked);
        //memoryButton.onClick.AddListener(OnMemoryButtonClicked);

        // AnimationTriggerState ����̊����ʒm���w�ǁiOnAnimationFinished �C�x���g�j
        if (animationTriggerState != null)
        {
            animationTriggerState.OnAnimationFinished += OnAnimationFinishedHandler;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // �I�������\���ɂ���
            choicePanel.ClosePanel();
            // ���b�Z�[�W�p�l�����\���ɂ���
            messagePanel.ShowChoicePanel(() =>
            {
                messagePrinter.ClearMessage();
            });
        }
    }

    private void OnGameButtonClicked()
    {
        // �A�C�e���ɃQ�[�������邩�m�F
        // �Q�[��������Ƃ��ƂȂ��Ƃ��ŕ���
        // �Q�[�����I�����ꂽ��A�C�e������Q�[����1����

        infoPanel.ResetMenuPanel();

        messagePanel.ShowMessagePanel(() =>
        {
            messagePrinter.PrintMessage("���̑O�������ቮ�Ŕ����Ă����Q�[���A��������邩�ȁB�U���Ă݂邩�B");
        });

        var choices = new List<ChoiceOption>();

        choices.Add(new ChoiceOption
        {
            choiceText = "�����ƈꏏ�ɃQ�[���ŗV��",
            onSelected = () =>
            {
                // �Q�[�����P����
                int itemId = 1; // game��itemID=1�Ɖ���
                StatusManager.Instance.DecreaseItem(itemId);
                // �T�u�C�x���g���J�n
                subStateMachine.
                    StartSubEvent(gameEventID, EventType.Animation);
                messagePanel.ShowChoicePanel(() =>
                {
                    messagePrinter.ClearMessage();
                });
            }
        });

        choices.Add(new ChoiceOption
        {
            choiceText = "����ς��߂Ă���",
            onSelected = () =>
            {
                // �T�u�C�x���g���J�n
                messagePanel.ShowChoicePanel(() =>
                {
                    messagePrinter.ClearMessage();
                });
            }
        });

        // �I������\��
        choicePanel.ShowChoices(choices);
    }

    private void OnOutingButtonClicked()
    {
        infoPanel.ResetMenuPanel();

        messagePanel.ShowMessagePanel(() =>
        {
            messagePrinter.PrintMessage("�������w�Z�ȊO�ŏo�����Ă鏊���݂Ȃ��ȁB�O�ɘA��Ă��Ă݂邩�B");
        });

        var choices = new List<ChoiceOption>();

        choices.Add(new ChoiceOption
        {
            choiceText = "�����ƈꏏ�ɏo������",
            onSelected = () =>
            {
                // �T�u�C�x���g���J�n
                subStateMachine.
                    StartSubEvent(outingEventID, EventType.Animation);
                messagePanel.ShowChoicePanel(() =>
                {
                    messagePrinter.ClearMessage();
                });
            }
        });

        choices.Add(new ChoiceOption
        {
            choiceText = "���͕ʂ̂��Ƃ����悤",
            onSelected = () =>
            {
                // �Q�[��������Ȃ�
                // �T�u�C�x���g���J�n
                messagePanel.ShowChoicePanel(() =>
                {
                    messagePrinter.ClearMessage();
                });
            }
        });

        // �I������\��
        choicePanel.ShowChoices(choices);
    }

    private void OnWorkButtonClicked()
    {
        infoPanel.ResetMenuPanel();

        messagePanel.ShowMessagePanel(() =>
        {
            messagePrinter.PrintMessage("�����̃o�C�g�̗\����m�F���Ă݂悤�B");
        });

        var choices = new List<ChoiceOption>();

        choices.Add(new ChoiceOption
        {
            choiceText = "�o�C�g�ɍs��",
            onSelected = () =>
            {
                // �T�u�C�x���g���J�n
                subStateMachine.
                    StartSubEvent(workEventID, EventType.Animation);
                messagePanel.ShowChoicePanel(() =>
                {
                    messagePrinter.ClearMessage();
                });
            }
        });

        choices.Add(new ChoiceOption
        {
            choiceText = "�����͓����ĂȂ��݂�����",
            onSelected = () =>
            {
                // �Q�[��������Ȃ�
                // �T�u�C�x���g���J�n
                messagePanel.ShowChoicePanel(() =>
                {
                    messagePrinter.ClearMessage();
                });
            }
        });

        // �I������\��
        choicePanel.ShowChoices(choices);
    }

    private void OnTalkButtonClicked()
    {
        infoPanel.ResetMenuPanel();

        messagePanel.ShowMessagePanel(() =>
        {
            messagePrinter.PrintMessage("�ŋ߂̊w�Z�̒��q�ɂ��ĕ����Ă݂邩�B");
        });

        var choices = new List<ChoiceOption>();

        choices.Add(new ChoiceOption
        {
            choiceText = "�����Ƃ��b����",
            onSelected = () =>
            {
                // �T�u�C�x���g���J�n
                subStateMachine.
                    StartSubEvent(talkEventID, EventType.Animation);
                messagePanel.ShowChoicePanel(() =>
                {
                    messagePrinter.ClearMessage();
                });
            }
        });

        choices.Add(new ChoiceOption
        {
            choiceText = "���͂����Ƃ��Ă�����",
            onSelected = () =>
            {
                // �Q�[��������Ȃ�
                // �T�u�C�x���g���J�n
                messagePanel.ShowChoicePanel(() =>
                {
                    messagePrinter.ClearMessage();
                });
            }
        });

        // �I������\��
        choicePanel.ShowChoices(choices);
    }

    private void OnItemButtonClicked()
    {

    }

    private void OnMemoryButtonClicked()
    {

    }

    private void PlayAnimation(string spriteName)
    {

    }

    private void ShowItemUI()
    {

    }

    private void ShowMemoryList()
    {

    }

    // �A�j���[�V�����I�����ɌĂ΂��R�[���o�b�N
    private void OnAnimationFinishedHandler()
    {
        // AnimationTriggerState ���Őݒ肵�����߂̃X�e�[�^�X�ύX���e���擾
        var statusChange = animationTriggerState.LastStatusChange;
        if (statusChange != null)
        {
            // �����ł͍D���x (lovelityChange) �� 1 �ȏ�̏㏸�̏ꍇ�̗�
            if (statusChange.lovelityChange > 0)
            {
                string message = $"�D���x��{statusChange.lovelityChange}�オ��܂����B";
                // ���b�Z�[�W�p�l����\�����A���b�Z�[�W����
                messagePanel.ShowMessagePanel(() =>
                {
                    messagePrinter.PrintMessage(message);
                });

                // 2�b���choicePanel�̕\���ɖ߂��������J�n
                StartCoroutine(ShowChoiceAfterDelay(2.5f));
            }
        }
    }

    // �w��b���ҋ@��� choicePanel ���ĕ\������R���[�`��
    private IEnumerator ShowChoiceAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        messagePanel.ShowChoicePanel(() =>
        {
            // �I�v�V����: ���b�Z�[�W�N���A�ȂǕK�v�Ȃ�΂����ōs��
            messagePrinter.ClearMessage();
        });
    }

    private void OnDestroy()
    {
        if (animationTriggerState != null)
        {
            animationTriggerState.OnAnimationFinished -= OnAnimationFinishedHandler;
        }
    }
}
