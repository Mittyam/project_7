using UnityEngine;
using Live2D.Cubism.Framework.Raycasting;

/// <summary>
/// Live2Dモデルのタッチ検出用デバッグクラス
/// </summary>
public class CubismHitTest : MonoBehaviour
{
    private void Update()
    {
        // ユーザー操作がない場合は早期リターン
        if (!Input.GetMouseButtonDown(0))
        {
            return;
        }

        var raycaster = GetComponent<CubismRaycaster>();
        if (raycaster == null) return;

        // 当たり判定の結果を最大4つまで取得
        var results = new CubismRaycastHit[4];

        // マウス位置からレイを飛ばす
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var hitCount = raycaster.Raycast(ray, results);

        // 結果の表示
        var resultsText = hitCount.ToString();
        for (var i = 0; i < hitCount; i++)
        {
            resultsText += "\n" + results[i].Drawable.name;
        }
        Debug.Log(resultsText);
    }
}