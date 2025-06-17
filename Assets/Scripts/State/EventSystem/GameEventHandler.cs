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
        // ����{�^��
        TypedEventManager.Instance.Subscribe<GameEvents.BathButtonClicked>(OnBathButtonClicked);
        // TypedEventManager.Instance.Subscribe<GameEvents.TouchButtonClicked>(OnTouchButtonClicked);
        TypedEventManager.Instance.Subscribe<GameEvents.ItemButtonClicked>(OnItemButtonClicked);
        TypedEventManager.Instance.Subscribe<GameEvents.MemoryButtonClicked>(OnMemoryButtonClicked);

        // ����{�^��
        TypedEventManager.Instance.Subscribe<GameEvents.CloseButtonClicked>(OnCloseButtonClicked);
    }

    private void UnsubscribeFromEvents()
    {
        if (TypedEventManager.Instance == null) return;

        // ����{�^��
        TypedEventManager.Instance.Unsubscribe<GameEvents.BathButtonClicked>(OnBathButtonClicked);
        // TypedEventManager.Instance.Unsubscribe<GameEvents.TouchButtonClicked>(OnTouchButtonClicked);
        TypedEventManager.Instance.Unsubscribe<GameEvents.ItemButtonClicked>(OnItemButtonClicked);
        TypedEventManager.Instance.Unsubscribe<GameEvents.MemoryButtonClicked>(OnMemoryButtonClicked);

        // ����{�^��
        TypedEventManager.Instance.Unsubscribe<GameEvents.CloseButtonClicked>(OnCloseButtonClicked);
    }

    #region �C�x���g�n���h���[
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

    // ���G��{�^���������̏���
    private void OnTouchButtonClicked(GameEvents.TouchButtonClicked eventData)
    {
        // �G�ꍇ����TouchState�ɒ��ڑJ�ځi�A�N�V�����|�C���g����Ȃ��j
        GameLoop.Instance.PushMiniEvent(StateID.Touch);
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
    }

    #endregion
}
