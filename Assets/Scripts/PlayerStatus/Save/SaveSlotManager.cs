using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveSlotManager : Singleton<SaveSlotManager>
{
    public bool isSaveMode; // （true: セーブ、false: ロード）
    public SaveSlotUI[] saveSlotUIs;

    // シーン名を取得するため
    private string currentSceneName;

    private void Start()
    {
        // 現在のシーン名を取得
        currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        RefreshSlots();
    }

    // スロット一覧を更新する
    public void RefreshSlots()
    {
        foreach(var slotUI in saveSlotUIs)
        {
            slotUI.UpdateSlotInfo();
        }
    }

    // スロットが選択された時の処理
    public void OnSlotSelected(string slotName)
    {
        if (isSaveMode)
        {
            // セーブモードの場合はそのままセーブ
            StatusManager.Instance.playerStatus.saveDate =
                System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            StatusManager.Instance.SaveStatus(slotName);

            // セーブスロットを更新
            RefreshSlots();

            // セーブパネルを閉じる
            if (SaveLoadUI.Instance != null)
            {
                SaveLoadUI.Instance.CloseAllPanels();
            }
        }
        else
        {
            // ロードモードの場合
            if (SceneManager.GetActiveScene().name == "TitleScene")
            {
                // タイトル画面でロードした場合
                // 選択されたスロット情報をPlayerPrefsに保存
                PlayerPrefs.SetString("SelectedSlotName", slotName);
                PlayerPrefs.SetInt("ShouldLoadOnStart", 1);
                PlayerPrefs.Save();

                // ロードパネルを閉じる
                if (SaveLoadUI.Instance != null)
                {
                    SaveLoadUI.Instance.CloseAllPanels();
                }

                // MainSceneに遷移
                SceneTransitionManager.Instance.GoToMainScene();
            }
            else
            {
                // MainScene内でのロード
                StatusManager.Instance.LoadStatus(slotName);
                RefreshSlots();

                // ロードパネルを閉じる（StatusManagerのLoadStatusでも閉じるが念のため）
                if (SaveLoadUI.Instance != null)
                {
                    SaveLoadUI.Instance.CloseAllPanels();
                }
            }
        }
    }
}
