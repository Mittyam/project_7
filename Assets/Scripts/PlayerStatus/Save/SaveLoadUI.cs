using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // シーン管理用の名前空間を追加

public class SaveLoadUI : Singleton<SaveLoadUI>
{
    [Header("スロットメニュー")]
    public GameObject slotPanel;

    [Header("閉じるボタン")]
    public Button closeButton;

    [Header("UI設定")]
    [SerializeField] private float planeDistance = 10f; // キャンバスのプレーン距離

    private Canvas slotPanelCanvas; // スロットパネルのキャンバス参照
    private Camera mainCamera; // メインカメラの参照

    protected override void Awake()
    {
        base.Awake(); // シングルトンの初期化処理

        // 初期状態では両方非表示
        if (slotPanel != null) slotPanel.SetActive(false);

        // キャンバスコンポーネントを取得
        if (slotPanel != null)
        {
            slotPanelCanvas = slotPanel.GetComponent<Canvas>();
            if (slotPanelCanvas == null)
            {
                Debug.LogWarning("SaveLoadUI: スロットパネルにCanvasコンポーネントが見つかりません");
            }
        }

        // カメラをセットアップする
        SetupCamera();
    }

    private void OnEnable()
    {
        // シーン読み込み時のイベントを登録
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // イベント登録解除
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // 閉じるボタンのクリックイベントにメソッドを登録
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseAllPanels);
        }
    }

    private void Update()
    {
        // エスケープキーを押したら、表示中のパネルを閉じる
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseAllPanels();
        }
    }

    // シーン読み込み時のイベントハンドラ
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"SaveLoadUI: シーン '{scene.name}' が読み込まれました。カメラを再設定します");
        // シーンが変わったらカメラを再設定
        SetupCamera();
    }

    // カメラをセットアップする
    private void SetupCamera()
    {
        // メインカメラを検索
        mainCamera = Camera.main;

        // スロットパネルのキャンバスが存在し、メインカメラが見つかった場合に設定
        if (slotPanelCanvas != null && mainCamera != null)
        {
            slotPanelCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            slotPanelCanvas.worldCamera = mainCamera;
            slotPanelCanvas.planeDistance = planeDistance;

            Debug.Log($"SaveLoadUI: Canvas {slotPanel.name} のレンダーモードをScreenSpaceCamera、カメラを {mainCamera.name} に設定しました");
        }
        else if (slotPanelCanvas != null && mainCamera == null)
        {
            // カメラが見つからない場合は警告を出し、ScreenSpaceOverlayモードを使用
            Debug.LogWarning("SaveLoadUI: MainCameraが見つかりません。代わりにScreenSpaceOverlayモードを使用します。");
            slotPanelCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }
    }

    // 直接セーブパネルを開く
    public void OpenSavePanel()
    {
        // パネルを開く前にカメラを確認（念のため）
        if (slotPanelCanvas != null && slotPanelCanvas.worldCamera == null)
        {
            SetupCamera();
        }

        SaveSlotManager.Instance.isSaveMode = true;
        if (slotPanel != null) slotPanel.SetActive(true);
        SaveSlotManager.Instance.RefreshSlots();
    }

    // 直接ロードパネルを開く
    public void OpenLoadPanel()
    {
        // パネルを開く前にカメラを確認（念のため）
        if (slotPanelCanvas != null && slotPanelCanvas.worldCamera == null)
        {
            SetupCamera();
        }

        SaveSlotManager.Instance.isSaveMode = false;
        if (slotPanel != null) slotPanel.SetActive(true);
        SaveSlotManager.Instance.RefreshSlots();
    }

    // すべてのパネルを閉じる
    public void CloseAllPanels()
    {
        if (slotPanel != null && slotPanel.activeSelf)
        {
            slotPanel.SetActive(false);
        }
    }
}