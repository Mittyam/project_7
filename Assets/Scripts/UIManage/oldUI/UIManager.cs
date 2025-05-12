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
    [Header("共通UI要素")]
    [SerializeField] private GameObject commonUI; // 共通UI要素
    [SerializeField] private GameObject subCommonUI; // イベントUI要素

    [Header("メインUI要素")]
    [SerializeField] private GameObject weekdayUI;
    [SerializeField] private GameObject holidayUI;
    [SerializeField] private GameObject eveningUI;
    [SerializeField] private GameObject nightUI;
    [SerializeField] private GameObject novelUI;

    [Header("サブUI要素")]
    [SerializeField] private GameObject autoTriggerUI;
    [SerializeField] private GameObject choiceTriggerUI;

    /// <summary>
    /// メインイベントに対応したUIの更新
    /// </summary>
    private void UpdateUI(UIState state)
    {
        // すべての共通UIを非表示にする
        commonUI.SetActive(false);
        subCommonUI.SetActive(false);

        // すべてのメインUIを非表示にする
        weekdayUI.SetActive(false);
        holidayUI.SetActive(false);
        eveningUI.SetActive(false);
        nightUI.SetActive(false);
        novelUI.SetActive(false);

        // すべてのサブUIを非表示にする
        autoTriggerUI.SetActive(false);
        choiceTriggerUI.SetActive(false);

        // 状態に合わせたUIを表示
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
