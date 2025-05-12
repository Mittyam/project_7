// StatesContainer.cs - 修正版
using System.Collections.Generic;
using UnityEngine;

public class StatesContainer : MonoBehaviour
{
    [Header("メインステート")]
    [SerializeField] private DayState dayState;
    [SerializeField] private EveningState eveningState;
    [SerializeField] private NightState nightState;

    [Header("ミニイベントステート")]
    [SerializeField] private ActionState actionState;
    [SerializeField] private BathState bathState;
    [SerializeField] private TouchState touchState;
    [SerializeField] private ItemState itemState;
    [SerializeField] private MemoryState memoryState;

    [Header("ノベルステート")]
    [SerializeField] private NovelState novelState;

    // ステートIDとステートのマッピング
    private Dictionary<StateID, IState> mainStateMap = new Dictionary<StateID, IState>();
    private Dictionary<StateID, IState> miniEventMap = new Dictionary<StateID, IState>();

    private void Awake()
    {
        // メインステートの初期化とマッピング
        InitializeState(dayState, StateID.Day, mainStateMap);
        InitializeState(eveningState, StateID.Evening, mainStateMap);
        InitializeState(nightState, StateID.Night, mainStateMap);

        // ミニイベントステートの初期化とマッピング
        // ActionStateを各種アクションのStateIDにマッピング
        InitializeState(actionState, StateID.Library, miniEventMap);
        InitializeState(actionState, StateID.Cafe, miniEventMap);
        InitializeState(actionState, StateID.PartJob, miniEventMap);
        InitializeState(actionState, StateID.Walk, miniEventMap);
        InitializeState(actionState, StateID.Game, miniEventMap);
        InitializeState(actionState, StateID.Talk, miniEventMap);
        InitializeState(actionState, StateID.Outing, miniEventMap);
        InitializeState(actionState, StateID.Sleep, miniEventMap);

        // その他の専用ミニイベントステート
        InitializeState(bathState, StateID.Bath, miniEventMap);
        InitializeState(touchState, StateID.Touch, miniEventMap);
        InitializeState(itemState, StateID.item, miniEventMap);
        InitializeState(memoryState, StateID.Memory, miniEventMap);

        // ノベルステートの初期化
        if (novelState != null)
        {
            novelState.gameObject.SetActive(false);
        }
    }

    // 各ステートの初期化を行うヘルパーメソッド
    private void InitializeState(MonoBehaviour state, StateID stateID, Dictionary<StateID, IState> stateMap)
    {
        if (state != null)
        {
            state.gameObject.SetActive(false);
            stateMap[stateID] = state as IState;
        }
    }

    // メインステートの取得
    public IState GetMainState(StateID stateID)
    {
        if (mainStateMap.TryGetValue(stateID, out var state))
        {
            return state;
        }
        return null;
    }

    // ミニイベントステートの取得
    public IState GetMiniEventState(StateID stateID)
    {
        if (miniEventMap.TryGetValue(stateID, out var state))
        {
            return state;
        }
        return null;
    }

    // ノベルステートの取得
    public NovelState GetNovelState()
    {
        return novelState;
    }
}