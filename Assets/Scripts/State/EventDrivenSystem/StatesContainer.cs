// StatesContainer.cs - �C����
using System.Collections.Generic;
using UnityEngine;

public class StatesContainer : MonoBehaviour
{
    [Header("���C���X�e�[�g")]
    [SerializeField] private DayState dayState;
    [SerializeField] private EveningState eveningState;
    [SerializeField] private NightState nightState;

    [Header("�~�j�C�x���g�X�e�[�g")]
    [SerializeField] private ActionState actionState;
    [SerializeField] private BathState bathState;
    [SerializeField] private TouchState touchState;
    [SerializeField] private ItemState itemState;
    [SerializeField] private MemoryState memoryState;

    [Header("�m�x���X�e�[�g")]
    [SerializeField] private NovelState novelState;

    // �X�e�[�gID�ƃX�e�[�g�̃}�b�s���O
    private Dictionary<StateID, IState> mainStateMap = new Dictionary<StateID, IState>();
    private Dictionary<StateID, IState> miniEventMap = new Dictionary<StateID, IState>();

    private void Awake()
    {
        // ���C���X�e�[�g�̏������ƃ}�b�s���O
        InitializeState(dayState, StateID.Day, mainStateMap);
        InitializeState(eveningState, StateID.Evening, mainStateMap);
        InitializeState(nightState, StateID.Night, mainStateMap);

        // �~�j�C�x���g�X�e�[�g�̏������ƃ}�b�s���O
        // ActionState���e��A�N�V������StateID�Ƀ}�b�s���O
        InitializeState(actionState, StateID.Library, miniEventMap);
        InitializeState(actionState, StateID.Cafe, miniEventMap);
        InitializeState(actionState, StateID.PartJob, miniEventMap);
        InitializeState(actionState, StateID.Walk, miniEventMap);
        InitializeState(actionState, StateID.Game, miniEventMap);
        InitializeState(actionState, StateID.Talk, miniEventMap);
        InitializeState(actionState, StateID.Outing, miniEventMap);
        InitializeState(actionState, StateID.Sleep, miniEventMap);

        // ���̑��̐�p�~�j�C�x���g�X�e�[�g
        InitializeState(bathState, StateID.Bath, miniEventMap);
        InitializeState(touchState, StateID.Touch, miniEventMap);
        InitializeState(itemState, StateID.item, miniEventMap);
        InitializeState(memoryState, StateID.Memory, miniEventMap);

        // �m�x���X�e�[�g�̏�����
        if (novelState != null)
        {
            novelState.gameObject.SetActive(false);
        }
    }

    // �e�X�e�[�g�̏��������s���w���p�[���\�b�h
    private void InitializeState(MonoBehaviour state, StateID stateID, Dictionary<StateID, IState> stateMap)
    {
        if (state != null)
        {
            state.gameObject.SetActive(false);
            stateMap[stateID] = state as IState;
        }
    }

    // ���C���X�e�[�g�̎擾
    public IState GetMainState(StateID stateID)
    {
        if (mainStateMap.TryGetValue(stateID, out var state))
        {
            return state;
        }
        return null;
    }

    // �~�j�C�x���g�X�e�[�g�̎擾
    public IState GetMiniEventState(StateID stateID)
    {
        if (miniEventMap.TryGetValue(stateID, out var state))
        {
            return state;
        }
        return null;
    }

    // �m�x���X�e�[�g�̎擾
    public NovelState GetNovelState()
    {
        return novelState;
    }
}