using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveSlotManager : Singleton<SaveSlotManager>
{
    public bool isSaveMode; // （true: セーブ、false: ロード）
    public SaveSlotUI[] saveSlotUIs;

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
            StatusManager.Instance.playerStatus.saveDate =
                System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            StatusManager.Instance.SaveStatus(slotName);
        }
        else
        {
            StatusManager.Instance.LoadStatus(slotName);
        }
        // 操作後、スロットの表示を更新
        RefreshSlots();
    }
}
