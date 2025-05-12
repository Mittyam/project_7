using UnityEngine;

public enum UIState
{
    WeekDay,
    HoliDay,
    Evening,
    Night,
    Novel,
    AutoTrigger,
    ChoiceTrigger,
    AnimationTrigger,
}

public class UIManager : MonoBehaviour
{
    [Header("����UI�v�f")]
    [SerializeField] private GameObject commonUI; // ����UI�v�f
    [SerializeField] private GameObject subCommonUI; // �C�x���gUI�v�f

    [Header("���C��UI�v�f")]
    [SerializeField] private GameObject weekdayUI;
    [SerializeField] private GameObject holidayUI;
    [SerializeField] private GameObject eveningUI;
    [SerializeField] private GameObject nightUI;
    [SerializeField] private GameObject novelUI;

    [Header("�T�uUI�v�f")]
    [SerializeField] private GameObject autoTriggerUI;
    [SerializeField] private GameObject choiceTriggerUI;

    /// <summary>
    /// ���C���C�x���g�ɑΉ�����UI�̍X�V
    /// </summary>
    private void UpdateUI(UIState state)
    {
        // ���ׂĂ̋���UI���\���ɂ���
        commonUI.SetActive(false);
        subCommonUI.SetActive(false);

        // ���ׂẴ��C��UI���\���ɂ���
        weekdayUI.SetActive(false);
        holidayUI.SetActive(false);
        eveningUI.SetActive(false);
        nightUI.SetActive(false);
        novelUI.SetActive(false);

        // ���ׂẴT�uUI���\���ɂ���
        autoTriggerUI.SetActive(false);
        choiceTriggerUI.SetActive(false);

        // ��Ԃɍ��킹��UI��\��
        switch (state)
        {
            case UIState.WeekDay:
                commonUI.SetActive(true);
                subCommonUI.SetActive(true);
                weekdayUI.SetActive(true);
                break;
            case UIState.HoliDay:
                commonUI.SetActive(true);
                subCommonUI.SetActive(true);
                holidayUI.SetActive(true);
                break;
            case UIState.Evening:
                commonUI.SetActive(true);
                eveningUI.SetActive(true);
                break;
            case UIState.Night:
                commonUI.SetActive(true);
                subCommonUI.SetActive(true);
                nightUI.SetActive(true);
                break;
            case UIState.Novel:
                novelUI.SetActive(true);
                break;
            case UIState.AutoTrigger:
                commonUI.SetActive(true);
                autoTriggerUI.SetActive(true);
                break;
            case UIState.ChoiceTrigger:
                commonUI.SetActive(true);
                choiceTriggerUI.SetActive(true);
                break;
        }
    }

    public void UpdateWeekDayUI()
    {
        UpdateUI(UIState.WeekDay);
    }

    public void UpdateHoliDayUI()
    {
        UpdateUI(UIState.HoliDay);
    }

    public void UpdateEveningUI()
    {
        UpdateUI(UIState.Evening);
    }

    public void UpdateNightUI()
    {
        UpdateUI(UIState.Night);
    }

    public void UpdateNovelUI()
    {
        UpdateUI(UIState.Novel);
    }

    public void UpdateAutoTriggerUI()
    {
        UpdateUI(UIState.AutoTrigger);
    }

    public void UpdateChoiceTriggerUI()
    {
        UpdateUI(UIState.ChoiceTrigger);
    }
}
