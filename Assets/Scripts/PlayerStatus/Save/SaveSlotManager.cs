using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveSlotManager : Singleton<SaveSlotManager>
{
    public bool isSaveMode; // �itrue: �Z�[�u�Afalse: ���[�h�j
    public SaveSlotUI[] saveSlotUIs;

    // �X���b�g�ꗗ���X�V����
    public void RefreshSlots()
    {
        foreach(var slotUI in saveSlotUIs)
        {
            slotUI.UpdateSlotInfo();
        }
    }

    // �X���b�g���I�����ꂽ���̏���
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
        // �����A�X���b�g�̕\�����X�V
        RefreshSlots();
    }
}
