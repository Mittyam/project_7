using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveSlotManager : Singleton<SaveSlotManager>
{
    public bool isSaveMode; // �itrue: �Z�[�u�Afalse: ���[�h�j
    public SaveSlotUI[] saveSlotUIs;

    // �V�[�������擾���邽��
    private string currentSceneName;

    private void Start()
    {
        // ���݂̃V�[�������擾
        currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        RefreshSlots();
    }

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
            // �Z�[�u���[�h�̏ꍇ�͂��̂܂܃Z�[�u
            StatusManager.Instance.playerStatus.saveDate =
                System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            StatusManager.Instance.SaveStatus(slotName);

            // �Z�[�u�X���b�g���X�V
            RefreshSlots();

            // �Z�[�u�p�l�������
            if (SaveLoadUI.Instance != null)
            {
                SaveLoadUI.Instance.CloseAllPanels();
            }
        }
        else
        {
            // ���[�h���[�h�̏ꍇ
            if (SceneManager.GetActiveScene().name == "TitleScene")
            {
                // �^�C�g����ʂŃ��[�h�����ꍇ
                // �I�����ꂽ�X���b�g����PlayerPrefs�ɕۑ�
                PlayerPrefs.SetString("SelectedSlotName", slotName);
                PlayerPrefs.SetInt("ShouldLoadOnStart", 1);
                PlayerPrefs.Save();

                // ���[�h�p�l�������
                if (SaveLoadUI.Instance != null)
                {
                    SaveLoadUI.Instance.CloseAllPanels();
                }

                // MainScene�ɑJ��
                SceneTransitionManager.Instance.GoToMainScene();
            }
            else
            {
                // MainScene���ł̃��[�h
                StatusManager.Instance.LoadStatus(slotName);
                RefreshSlots();

                // ���[�h�p�l�������iStatusManager��LoadStatus�ł����邪�O�̂��߁j
                if (SaveLoadUI.Instance != null)
                {
                    SaveLoadUI.Instance.CloseAllPanels();
                }
            }
        }
    }
}
