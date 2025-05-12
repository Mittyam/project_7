using UnityEngine;
using UnityEngine.UI;

public class ExclusiveToggleLink : MonoBehaviour
{
    public Toggle[] otherToggles;

    private void Awake()
    {
        GetComponent<Toggle>().onValueChanged.AddListener(OnChanged);
    }

    private void OnChanged(bool isOn)
    {
        if (isOn && otherToggles != null)
        {
            foreach (Toggle t in otherToggles)
            {
                t.isOn = false;
            }
        }
    }
}
