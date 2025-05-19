// StateBase.cs - シーン配置対応
using UnityEngine;
using UnityEngine.UI;

public abstract class StateBase : MonoBehaviour, IState
{
    // GameLoopからの参照用ヘルパーメソッド
    protected GameLoop GameLoop => GameLoop.Instance;
    protected PushdownStateMachine PushdownStack => GameLoop.Instance.PushdownStack;
    protected MainStateMachine MainStateMachine => GameLoop.Instance.MainStateMachine;
    protected NovelEventScheduler NovelEventScheduler => GameLoop.Instance.NovelEventScheduler;
    protected StatesContainer StatesContainer => GameLoop.Instance.StatesContainer;

    [Header("UI Rendering")]
    [SerializeField] protected Camera uiRenderCamera; // UI表示用カメラ

    // IStateメソッド（抽象メソッドとして定義）
    public abstract void OnEnter();
    public abstract void OnUpdate();
    public abstract void OnExit();

    // IStateプロパティ実装
    public IState NextState { get; set; }

    bool IState.enabled
    {
        get => enabled;
        set => enabled = value;
    }

    /// <summary>
    /// UI要素の初期化と設定
    /// </summary>
    protected virtual void SetupUI()
    {
        // 基底クラスでは何もしない、子クラスでオーバーライドする
        Debug.Log($"{GetType().Name}: UI設定の基本メソッドがコールされました。");
    }

    /// <summary>
    /// すべてのUI要素を非表示にする
    /// </summary>
    protected virtual void HideAllUI()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 指定されたコンポーネントを子から検索して取得する
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
    /// UIPrefabを指定の親の下に生成する
    /// </summary>
    protected GameObject InstantiateUI(GameObject prefab, Transform parent = null)
    {
        if (prefab == null) return null;

        // 親が指定されていなければ自分自身を使用
        Transform targetParent = parent != null ? parent : transform;

        // 生成して返す
        GameObject instance = Instantiate(prefab, targetParent);
        instance.SetActive(true);
        return instance;
    }

    /// <summary>
    /// UIPrefabをCanvasを調整してから生成する
    /// </summary>
    protected GameObject InstantiateUIWithCamera(GameObject prefab, Transform parent = null)
    {
        if (prefab == null) return null;

        Transform targetParent = parent != null ? parent : transform;
        GameObject instance = Instantiate(prefab, targetParent);
        instance.SetActive(true);

        // Canvas設定
        Canvas canvas = instance.GetComponent<Canvas>();
        if (canvas != null && uiRenderCamera != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = uiRenderCamera;
            canvas.planeDistance = 10f;

            // Canvas Scaler設定
            CanvasScaler scaler = instance.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                // 16:9に固定する設定
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080); // 16:9
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand; // または MatchWidthOrHeight

                // MatchWidthOrHeightを使う場合は、0（幅に合わせる）か
                // 1（高さに合わせる）を設定します
                // 0.5は幅と高さのバランスをとります
                scaler.matchWidthOrHeight = 0;

                Debug.Log($"Canvas {instance.name} のスケーラーを16:9に設定しました");
            }
        }
        else if (canvas != null && uiRenderCamera == null)
        {
            Debug.LogWarning($"UIRenderCameraが設定されていないため、{instance.name}のCanvasはデフォルト設定のままです");
        }

        return instance;
    }

    protected void CleanupUI(Transform container)
    {
        if (container == null) return;

        // コンテナ内の全ての子オブジェクトを破棄
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }
}

// MainStateBase.cs - メインステート用ベースクラス
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

    // IPausableStateの実装
    public virtual void OnPause()
    {
        Debug.Log($"MainState: {stateData?.displayName} を一時停止します");

        // UI要素の非表示処理
        HideAllUI();
    }

    public virtual void OnResume()
    {
        Debug.Log($"MainState: {stateData?.displayName} を再開します");

        // UI要素の再表示処理
        SetupUI();

        // 子クラスではこの後にShowXXXUI()などのUI表示メソッドを呼ぶことを想定
    }

    // SetupUIのオーバーライド（通常は子クラスでさらにオーバーライドされる）
    protected override void SetupUI()
    {
        base.SetupUI();

        // ステートデータからUIプレハブを読み込む場合の処理
        if (stateData != null && stateData.uiPrefab != null)
        {
            foreach (var prefab in stateData.uiPrefab)
            {
                if (prefab != null)
                {
                    // カメラ設定付きのインスタンス化メソッドを使用
                    InstantiateUIWithCamera(prefab, transform);
                }
            }
        }
    }
}

// MiniEventStateBase.cs - ミニイベント用ベースクラス
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
        Debug.Log($"MiniEventState: {stateData.displayName} を一時停止します");

        // UIを非表示にするなどの処理
        if (contentContainer != null)
        {
            contentContainer.SetActive(false);
        }
    }

    public virtual void OnResume()
    {
        Debug.Log($"MiniEventState: {stateData.displayName} を再開します");

        // UIを再表示するなどの処理
        if (contentContainer != null)
        {
            contentContainer.SetActive(true);
        }
    }

    /// <summary>
    /// イベント終了処理（ユーザーアクションから呼ばれる）
    /// </summary>
    public void CompleteEvent()
    {
        // 自分自身をPushdownStackからpop
        PushdownStack.Pop();
    }
}
