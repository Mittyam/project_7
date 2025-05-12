using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReturnButton : MonoBehaviour
{
    [SerializeField] private ShopPanelManager shopPanelManager;
    private Button button;

    private void Start()
    {
        // �{�^���R���|�[�l���g���擾
        button = GetComponent<Button>();

        // ShopPanelManager���ݒ肳��Ă��Ȃ��ꍇ�͎����I�ɒT��
        if (shopPanelManager == null)
        {
            shopPanelManager = FindObjectOfType<ShopPanelManager>();
        }

        // �N���b�N�C�x���g��o�^
        if (button != null && shopPanelManager != null)
        {
            button.onClick.AddListener(shopPanelManager.ReturnToStoreSelection);
        }
    }
}