using UnityEngine;
using Live2D.Cubism.Framework.Raycasting;

/// <summary>
/// Live2D���f���̃^�b�`���o�p�f�o�b�O�N���X
/// </summary>
public class CubismHitTest : MonoBehaviour
{
    private void Update()
    {
        // ���[�U�[���삪�Ȃ��ꍇ�͑������^�[��
        if (!Input.GetMouseButtonDown(0))
        {
            return;
        }

        var raycaster = GetComponent<CubismRaycaster>();
        if (raycaster == null) return;

        // �����蔻��̌��ʂ��ő�4�܂Ŏ擾
        var results = new CubismRaycastHit[4];

        // �}�E�X�ʒu���烌�C���΂�
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var hitCount = raycaster.Raycast(ray, results);

        // ���ʂ̕\��
        var resultsText = hitCount.ToString();
        for (var i = 0; i < hitCount; i++)
        {
            resultsText += "\n" + results[i].Drawable.name;
        }
        Debug.Log(resultsText);
    }
}