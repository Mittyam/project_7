using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveSlotManager : Singleton<SaveSlotManager>
{
    public bool isSaveMode; // （true: セーブ、false: ロード）
    public SaveSlotUI[] saveSlotUIs;

    // スロットUIを初期化する
    public void Start()
    {
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

    // SaveSlotManager.cs の OnSlotSelected メソッドを修正
    public void OnSlotSelected(string slotName)
    {
        // パネルを閉じる（セーブ・ロード共通）
        if (SaveLoadUI.Instance != null)
        {
            SaveLoadUI.Instance.CloseAllPanels();
        }

        if (isSaveMode)
        {
            // セーブ処理
            StatusManager.Instance.playerStatus.saveDate =
                System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            StatusManager.Instance.SaveStatus(slotName);

            // スロット表示を更新
            RefreshSlots();

            Debug.Log($"ゲームデータをスロット '{slotName}' に保存しました");
        }
        else
        {
            // ロード処理 - 常にシーンリロードを行う
            GameLoadManager.SetLoadRequest(slotName);

            // 現在のシーン名を取得
            string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            // 同じシーンを再度読み込む（強制的に再初期化）
            TransitionManager.Instance.FadeAndLoadScene(
                currentSceneName,
                () => {
                    Debug.Log($"ロードのため {currentSceneName} をリロードします...");
                },
                null
            );

            Debug.Log($"スロット '{slotName}' からのロード要求を登録しました");
        }
    }
}
