using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveSlotManager : Singleton<SaveSlotManager>
{
    public bool isSaveMode; // �itrue: �Z�[�u�Afalse: ���[�h�j
    public SaveSlotUI[] saveSlotUIs;

    // �X���b�gUI������������
    public void Start()
    {
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

    // SaveSlotManager.cs �� OnSlotSelected ���\�b�h���C��
    public void OnSlotSelected(string slotName)
    {
        // �p�l�������i�Z�[�u�E���[�h���ʁj
        if (SaveLoadUI.Instance != null)
        {
            SaveLoadUI.Instance.CloseAllPanels();
        }

        if (isSaveMode)
        {
            // �Z�[�u����
            StatusManager.Instance.playerStatus.saveDate =
                System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            StatusManager.Instance.SaveStatus(slotName);

            // �X���b�g�\�����X�V
            RefreshSlots();

            Debug.Log($"�Q�[���f�[�^���X���b�g '{slotName}' �ɕۑ����܂���");
        }
        else
        {
            // ���[�h���� - ��ɃV�[�������[�h���s��
            GameLoadManager.SetLoadRequest(slotName);

            // ���݂̃V�[�������擾
            string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            // �����V�[�����ēx�ǂݍ��ށi�����I�ɍď������j
            TransitionManager.Instance.FadeAndLoadScene(
                currentSceneName,
                () => {
                    Debug.Log($"���[�h�̂��� {currentSceneName} �������[�h���܂�...");
                },
                null
            );

            Debug.Log($"�X���b�g '{slotName}' ����̃��[�h�v����o�^���܂���");
        }
    }
}
