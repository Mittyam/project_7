using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameEvents;

public class GameEventHandler : MonoBehaviour
{
    private void OnEnable()
    {
        SubscribeToEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    private void SubscribeToEvents()
    {
        // �e�{�^���C�x���g�̃T�u�X�N���C�u
        TypedEventManager.Instance.Subscribe<GameEvents.TalkButtonClicked>(OnTalkButtonClicked);
        TypedEventManager.Instance.Subscribe<GameEvents.OutingButtonClicked>(OnOutingButtonClicked);
        TypedEventManager.Instance.Subscribe<GameEvents.GameButtonClicked>(OnGameButtonClicked);
        TypedEventManager.Instance.Subscribe<GameEvents.LibraryButtonClicked>(OnLibraryButtonClicked);
        TypedEventManager.Instance.Subscribe<GameEvents.CafeButtonClicked>(OnCafeButtonClicked);
        TypedEventManager.Instance.Subscribe<GameEvents.WorkButtonClicked>(OnWorkButtonClicked);
        TypedEventManager.Instance.Subscribe<GameEvents.WalkButtonClicked>(OnWalkButtonClicked);
        TypedEventManager.Instance.Subscribe<GameEvents.SleepButtonClicked>(OnSleepButtonClicked);

        // ����{�^��
        TypedEventManager.Instance.Subscribe<GameEvents.BathButtonClicked>(OnBathButtonClicked);
        TypedEventManager.Instance.Subscribe<GameEvents.TouchButtonClicked>(OnTouchButtonClicked);
        TypedEventManager.Instance.Subscribe<GameEvents.ItemButtonClicked>(OnItemButtonClicked);
        TypedEventManager.Instance.Subscribe<GameEvents.MemoryButtonClicked>(OnMemoryButtonClicked);

        // ����{�^��
        TypedEventManager.Instance.Subscribe<GameEvents.CloseButtonClicked>(OnCloseButtonClicked);
    }

    private void UnsubscribeFromEvents()
    {
        if (TypedEventManager.Instance == null) return;

        // �e�{�^���C�x���g�̃T�u�X�N���C�u����
        TypedEventManager.Instance.Unsubscribe<GameEvents.TalkButtonClicked>(OnTalkButtonClicked);
        TypedEventManager.Instance.Unsubscribe<GameEvents.OutingButtonClicked>(OnOutingButtonClicked);
        TypedEventManager.Instance.Unsubscribe<GameEvents.GameButtonClicked>(OnGameButtonClicked);
        TypedEventManager.Instance.Unsubscribe<GameEvents.LibraryButtonClicked>(OnLibraryButtonClicked);
        TypedEventManager.Instance.Unsubscribe<GameEvents.CafeButtonClicked>(OnCafeButtonClicked);
        TypedEventManager.Instance.Unsubscribe<GameEvents.WorkButtonClicked>(OnWorkButtonClicked);
        TypedEventManager.Instance.Unsubscribe<GameEvents.WalkButtonClicked>(OnWalkButtonClicked);
        TypedEventManager.Instance.Unsubscribe<GameEvents.SleepButtonClicked>(OnSleepButtonClicked);

        // ����{�^��
        TypedEventManager.Instance.Unsubscribe<GameEvents.BathButtonClicked>(OnBathButtonClicked);
        TypedEventManager.Instance.Unsubscribe<GameEvents.TouchButtonClicked>(OnTouchButtonClicked);
        TypedEventManager.Instance.Unsubscribe<GameEvents.ItemButtonClicked>(OnItemButtonClicked);
        TypedEventManager.Instance.Unsubscribe<GameEvents.MemoryButtonClicked>(OnMemoryButtonClicked);

        // ����{�^��
        TypedEventManager.Instance.Unsubscribe<GameEvents.CloseButtonClicked>(OnCloseButtonClicked);
    }

    #region �C�x���g�n���h���[

    // �A�N�V�����X�e�[�g�n

    // �}���ك{�^���������̏���
    private void OnLibraryButtonClicked(GameEvents.LibraryButtonClicked eventData)
    {
        // �A�N�V�����|�C���g����`�F�b�N
        if (StatusManager.Instance.ConsumeActionPoint(eventData.ActionPointCost))
        {
            // ActionState�ɑJ�ڂ��邽�߂̃p�����[�^��ݒ�
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "ActionType", "Library" }
            };

            // GameLoop��ʂ��ă~�j�C�x���g���v�b�V��
            GameLoop.Instance.PushMiniEvent(StateID.Library, parameters);

            // �i�s�x�X�V�ʒm
            TypedEventManager.Instance.Publish(new GameEvents.DayProgressUpdated
            {
                ProgressValue = 0.5f,
                SourceStateID = GameLoop.Instance.MainStateMachine.CurrentStateID
            });

            // �{�^����ԍX�V���N�G�X�g
            TypedEventManager.Instance.Publish(new GameEvents.ButtonStateUpdateRequested
            {
                CurrentStateID = GameLoop.Instance.MainStateMachine.CurrentStateID
            });
        }
    }

    // �J�t�F�{�^���������̏���
    private void OnCafeButtonClicked(GameEvents.CafeButtonClicked eventData)
    {
        if (StatusManager.Instance.ConsumeActionPoint(eventData.ActionPointCost))
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "ActionType", "Cafe" }
            };

            GameLoop.Instance.PushMiniEvent(StateID.Cafe, parameters);

            // �i�s�x�X�V�ʒm
            TypedEventManager.Instance.Publish(new GameEvents.DayProgressUpdated
            {
                ProgressValue = 0.5f,
                SourceStateID = GameLoop.Instance.MainStateMachine.CurrentStateID
            });

            // �{�^����ԍX�V���N�G�X�g
            TypedEventManager.Instance.Publish(new GameEvents.ButtonStateUpdateRequested
            {
                CurrentStateID = GameLoop.Instance.MainStateMachine.CurrentStateID
            });
        }
    }

    // �o�C�g�{�^���������̏���
    private void OnWorkButtonClicked(GameEvents.WorkButtonClicked eventData)
    {
        if (StatusManager.Instance.ConsumeActionPoint(eventData.ActionPointCost))
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "ActionType", "Work" }
            };

            GameLoop.Instance.PushMiniEvent(StateID.PartJob, parameters);

            // �i�s�x�X�V�ʒm
            TypedEventManager.Instance.Publish(new GameEvents.DayProgressUpdated
            {
                ProgressValue = 0.5f,
                SourceStateID = GameLoop.Instance.MainStateMachine.CurrentStateID
            });

            // �{�^����ԍX�V���N�G�X�g
            TypedEventManager.Instance.Publish(new GameEvents.ButtonStateUpdateRequested
            {
                CurrentStateID = GameLoop.Instance.MainStateMachine.CurrentStateID
            });
        }
    }

    // �U���{�^���������̏���
    private void OnWalkButtonClicked(GameEvents.WalkButtonClicked eventData)
    {
        if (StatusManager.Instance.ConsumeActionPoint(eventData.ActionPointCost))
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "ActionType", "Walk" }
            };

            GameLoop.Instance.PushMiniEvent(StateID.Walk, parameters);

            // �i�s�x�X�V�ʒm
            TypedEventManager.Instance.Publish(new GameEvents.DayProgressUpdated
            {
                ProgressValue = 0.5f,
                SourceStateID = GameLoop.Instance.MainStateMachine.CurrentStateID
            });

            // �{�^����ԍX�V���N�G�X�g
            TypedEventManager.Instance.Publish(new GameEvents.ButtonStateUpdateRequested
            {
                CurrentStateID = GameLoop.Instance.MainStateMachine.CurrentStateID
            });
        }
    }

    // �Q�[���{�^���������̏���
    private void OnGameButtonClicked(GameEvents.GameButtonClicked eventData)
    {
        if (StatusManager.Instance.ConsumeActionPoint(eventData.ActionPointCost))
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "ActionType", "Game" }
            };

            GameLoop.Instance.PushMiniEvent(StateID.Game, parameters);

            // �i�s�x�X�V�ʒm
            TypedEventManager.Instance.Publish(new GameEvents.DayProgressUpdated
            {
                ProgressValue = 0.5f,
                SourceStateID = GameLoop.Instance.MainStateMachine.CurrentStateID
            });

            // �{�^����ԍX�V���N�G�X�g
            TypedEventManager.Instance.Publish(new GameEvents.ButtonStateUpdateRequested
            {
                CurrentStateID = GameLoop.Instance.MainStateMachine.CurrentStateID
            });
        }
    }

    // ���b�{�^���������̏���
    private void OnTalkButtonClicked(GameEvents.TalkButtonClicked eventData)
    {
        if (StatusManager.Instance.ConsumeActionPoint(eventData.ActionPointCost))
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "ActionType", "Talk" }
            };

            GameLoop.Instance.PushMiniEvent(StateID.Talk, parameters);

            // �i�s�x�X�V�ʒm
            TypedEventManager.Instance.Publish(new GameEvents.DayProgressUpdated
            {
                ProgressValue = 0.5f,
                SourceStateID = GameLoop.Instance.MainStateMachine.CurrentStateID
            });

            // �{�^����ԍX�V���N�G�X�g
            TypedEventManager.Instance.Publish(new GameEvents.ButtonStateUpdateRequested
            {
                CurrentStateID = GameLoop.Instance.MainStateMachine.CurrentStateID
            });
        }
    }

    // ���o�����{�^���������̏���
    private void OnOutingButtonClicked(GameEvents.OutingButtonClicked eventData)
    {
        if (StatusManager.Instance.ConsumeActionPoint(eventData.ActionPointCost))
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "ActionType", "Outing" }
            };

            GameLoop.Instance.PushMiniEvent(StateID.Outing, parameters);

            // �i�s�x�X�V�ʒm�i���o�����͐i�s�x��1.0�ɂȂ遁����I���j
            TypedEventManager.Instance.Publish(new GameEvents.DayProgressUpdated
            {
                ProgressValue = 1.0f,
                SourceStateID = GameLoop.Instance.MainStateMachine.CurrentStateID
            });

            // �{�^����ԍX�V���N�G�X�g
            TypedEventManager.Instance.Publish(new GameEvents.ButtonStateUpdateRequested
            {
                CurrentStateID = GameLoop.Instance.MainStateMachine.CurrentStateID
            });
        }
    }

    // �����{�^���������̏���
    private void OnSleepButtonClicked(GameEvents.SleepButtonClicked eventData)
    {
        // �����͎��Ԍo�߂݂̂ŃA�N�V�����|�C���g����Ȃ�
        Dictionary<string, object> parameters = new Dictionary<string, object>
        {
            { "ActionType", "Sleep" }
        };

        GameLoop.Instance.PushMiniEvent(StateID.Sleep, parameters);

        // �i�s�x�X�V�ʒm�i�����͐i�s�x��1.0�ɂȂ遁����I���j
        StateID currentState = GameLoop.Instance.MainStateMachine.CurrentStateID;

        if (currentState == StateID.Day)
        {
            TypedEventManager.Instance.Publish(new GameEvents.DayProgressUpdated
            {
                ProgressValue = 1.0f,
                SourceStateID = currentState
            });
        }
        else if (currentState == StateID.Evening)
        {
            TypedEventManager.Instance.Publish(new GameEvents.EveningProgressUpdated
            {
                ProgressValue = 1.0f
            });
        }
        else if (currentState == StateID.Night)
        {
            TypedEventManager.Instance.Publish(new GameEvents.NightProgressUpdated
            {
                ProgressValue = 1.0f
            });
        }

        // �{�^����ԍX�V���N�G�X�g
        TypedEventManager.Instance.Publish(new GameEvents.ButtonStateUpdateRequested
        {
            CurrentStateID = currentState
        });
    }

    // ����X�e�[�g�n

    // �����C�{�^���������̏���
    private void OnBathButtonClicked(GameEvents.BathButtonClicked eventData)
    {
        // �����C��BathState�ɒ��ڑJ�ځi�A�N�V�����|�C���g����Ȃ��j
        GameLoop.Instance.PushMiniEvent(StateID.Bath);

        // �{�^����ԍX�V���N�G�X�g
        TypedEventManager.Instance.Publish(new GameEvents.ButtonStateUpdateRequested
        {
            CurrentStateID = GameLoop.Instance.MainStateMachine.CurrentStateID
        });
    }

    // �G�ꍇ���{�^���������̏���
    private void OnTouchButtonClicked(GameEvents.TouchButtonClicked eventData)
    {
        // �G�ꍇ����TouchState�ɒ��ڑJ�ځi�A�N�V�����|�C���g����Ȃ��j
        GameLoop.Instance.PushMiniEvent(StateID.Touch);

        // �{�^����ԍX�V���N�G�X�g
        TypedEventManager.Instance.Publish(new GameEvents.ButtonStateUpdateRequested
        {
            CurrentStateID = GameLoop.Instance.MainStateMachine.CurrentStateID
        });
    }

    // �A�C�e���{�^���������̏���
    private void OnItemButtonClicked(GameEvents.ItemButtonClicked eventData)
    {
        // �A�C�e����ItemState�ɒ��ڑJ�ځi�A�N�V�����|�C���g����Ȃ��j
        GameLoop.Instance.PushMiniEvent(StateID.item);

        // �{�^����ԍX�V���N�G�X�g
        TypedEventManager.Instance.Publish(new GameEvents.ButtonStateUpdateRequested
        {
            CurrentStateID = GameLoop.Instance.MainStateMachine.CurrentStateID
        });
    }

    // �v���o�{�^���������̏���
    private void OnMemoryButtonClicked(GameEvents.MemoryButtonClicked eventData)
    {
        // �v���o��MemoryState�ɒ��ڑJ�ځi�A�N�V�����|�C���g����Ȃ��j
        GameLoop.Instance.PushMiniEvent(StateID.Memory);

        // �{�^����ԍX�V���N�G�X�g
        TypedEventManager.Instance.Publish(new GameEvents.ButtonStateUpdateRequested
        {
            CurrentStateID = GameLoop.Instance.MainStateMachine.CurrentStateID
        });
    }

    // ����{�^���������̏���
    private void OnCloseButtonClicked(GameEvents.CloseButtonClicked eventData)
    {
        // ����{�^���̏����i�K�v�ɉ����Ď����j
        // Debug.Log($"����{�^����������܂���: {eventData.SourcePanelName}");
    }

    #endregion
}
