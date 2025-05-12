using Live2D.Cubism.Core;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Live2DController : MonoBehaviour
{
    [SerializeField] private GameObject modelContainer;
    [SerializeField] private List<Live2DModelAsset> modelAssets;

    // パラメータ更新時間をインスペクターから設定可能にする
    [Header("パラメータ設定")]
    [SerializeField] private float parameterTransitionTime = 0.5f;

    private Dictionary<string, GameObject> activeModels = new Dictionary<string, GameObject>();

    // パラメータの現在値を保持するための辞書
    // キー：モデルID_パラメータID、値：現在の値と目標値のペア
    private Dictionary<string, ParameterState> activeParameters = new Dictionary<string, ParameterState>();

    private class ParameterState
    {
        public float CurrentValue;      // 現在地
        public float TargetValue;       // 目標値
        public float StartValue;        // 開始値
        public float TransitionTime;    // 遷移時間
        public float ElapsedTime;       // 経過時間
        public CubismParameter Parameter; // パラメータへの参照
        public bool IsTransitioning; // 遷移中フラグ
    }

    [System.Serializable]
    public class Live2DModelAsset
    {
        public string id;  // モデルID
        public GameObject prefab;  // モデルプレハブ
    }

    // Start処理
    private void Start()
    {
        // 必要に応じた初期化処理
    }

    // LateUpdateでパラメータを更新
    private void LateUpdate()
    {
        // パラメータ更新処理
        UpdateParameters();
    }

    // カンマ区切りの文字列からVector2を生成するユーティリティメソッド
    private Vector2 ParseVector2(string vectorString)
    {
        if (string.IsNullOrEmpty(vectorString))
            return Vector2.zero;

        try
        {
            // カンマで分割
            string[] components = vectorString.Split(',');
            if (components.Length >= 2)
            {
                float x = float.Parse(components[0]);
                float y = float.Parse(components[1]);
                return new Vector2(x, y);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Vector2のパースに失敗しました: {vectorString}, エラー: {e.Message}");
        }

        return Vector2.zero; // デフォルト値
    }

    // パラメータを更新するメソッド
    private void UpdateParameters()
    {
        List<string> keysToRemove = new List<string>();

        foreach (var pair in activeParameters)
        {
            var paramState = pair.Value;

            // 遷移中の場合
            if (paramState.IsTransitioning)
            {
                paramState.ElapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(paramState.ElapsedTime / paramState.TransitionTime);

                // 線形補間で値を更新
                paramState.CurrentValue = Mathf.Lerp(paramState.StartValue, paramState.TargetValue, t);

                // パラメータ値を設定
                if (paramState.Parameter != null)
                {
                    paramState.Parameter.Value = paramState.CurrentValue;
                }

                // 遷移完了チェック
                if (t >= 1.0f)
                {
                    paramState.IsTransitioning = false;
                    paramState.CurrentValue = paramState.TargetValue;
                }
            }
            else
            {
                // 遷移していない場合は現在の値を維持
                if (paramState.Parameter != null)
                {
                    paramState.Parameter.Value = paramState.CurrentValue;
                }
                else
                {
                    // パラメータが無効になった場合はリストから削除
                    keysToRemove.Add(pair.Key);
                }
            }
        }

        // 無効なパラメータをリストから削除
        foreach (var key in keysToRemove)
        {
            activeParameters.Remove(key);
        }
    }

    public void ShowModel(string modelID, float scale, string positionString)
    {
        if (string.IsNullOrEmpty(modelID)) return;

        // 特殊な値「deleteModel」の場合は全モデル削除
        if (modelID.ToLower() == "deletemodel")
        {
            DeleteAllModels();
            return;
        }

        // 文字列からVector2に変換
        Vector2 position = ParseVector2(positionString);

        // モデルプレハブを事前登録リストから探す
        Live2DModelAsset asset = modelAssets.Find(a => a.id == modelID);
        if (asset == null)
        {
            Debug.LogError($"Live2Dモデル '{modelID}' が登録されていません。ModelAssetsにモデルを追加してください。");
            return;
        }

        // すでに同じIDのモデルが表示されている場合は更新するため一旦削除
        if (activeModels.ContainsKey(modelID))
        {
            Destroy(activeModels[modelID]);
            activeModels.Remove(modelID);
        }

        // モデルを生成
        GameObject modelObj = Instantiate(asset.prefab, modelContainer.transform);

        // スケールと位置を設定
        Transform transform = modelObj.GetComponent<Transform>();
        if (transform != null)
        {
            transform.localScale = new Vector3(scale, scale, scale);
            transform.position = position;
        }

        // 管理用Dictionaryに保存
        activeModels[modelID] = modelObj;

        Debug.Log($"Live2Dモデル '{modelID}' を表示しました。スケール: {scale}, 位置: {position}");
    }

    /// <summary>
    /// Vector2の位置を指定してモデルを表示
    /// </summary>
    public void ShowModel(string modelID, float scale, Vector2 position)
    {
        // 位置情報を文字列に変換
        string positionStr = $"{position.x},{position.y}";
        ShowModel(modelID, scale, positionStr);
    }

    public void HideModel(string modelID)
    {
        if (string.IsNullOrEmpty(modelID)) return;

        if (activeModels.TryGetValue(modelID, out GameObject modelObj))
        {
            // モデル関連のパラメータをクリア
            List<string> keysToRemove = new List<string>();
            foreach (var key in activeParameters.Keys)
            {
                if (key.StartsWith(modelID + "_"))
                {
                    keysToRemove.Add(key);
                }
            }

            foreach (var key in keysToRemove)
            {
                activeParameters.Remove(key);
            }

            Destroy(modelObj);
            activeModels.Remove(modelID);
            Debug.Log($"Live2Dモデル '{modelID}' を削除しました。");
        }
    }

    public void PlayAnimation(string modelID, string animationTrigger)
    {
        if (string.IsNullOrEmpty(animationTrigger) || string.IsNullOrEmpty(modelID)) return;

        if (activeModels.TryGetValue(modelID, out GameObject modelObj))
        {
            // Live2D Cubismアニメーション再生コンポーネントへのアクセス
            var animator = modelObj.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetTrigger(animationTrigger);
                Debug.Log($"Live2Dモデル '{modelID}' のアニメーション '{animationTrigger}' を再生しました。");
            }
            else
            {
                Debug.LogWarning($"Live2Dモデル '{modelID}' にAnimatorコンポーネントが見つかりません。");
            }
        }
    }

    // カンマ区切りのパラメータIDと値を処理する新メソッド
    public void SetParameters(string modelID, string paramIDsStr, string paramValuesStr, float transitionTime = -1.0f)
    {
        if (string.IsNullOrEmpty(paramIDsStr) || string.IsNullOrEmpty(modelID)) return;

        // デフォルトの遷移時間を使用するか、指定された値を使用
        float actualTransitionTime = (transitionTime < 0) ? parameterTransitionTime : transitionTime;

        // カンマ区切りの文字列を配列に変換
        string[] paramIDs = paramIDsStr.Split(',');
        string[] paramValuesStrArray = paramValuesStr.Split(',');

        // モデルを取得
        if (!activeModels.TryGetValue(modelID, out GameObject modelObj))
        {
            Debug.LogWarning($"Live2Dモデル '{modelID}' が見つかりません。");
            return;
        }

        // CubismModelコンポーネントを取得
        var cubismModel = modelObj.GetComponent<CubismModel>();
        if (cubismModel == null)
        {
            Debug.LogWarning($"Live2Dモデル '{modelID}' にCubismModelコンポーネントが見つかりません。");
            return;
        }

        // パラメータの数が一致しない場合は警告を出す
        if (paramIDs.Length != paramValuesStrArray.Length)
        {
            Debug.LogWarning($"パラメータIDと値の数が一致しません。IDs: {paramIDs.Length}, Values: {paramValuesStrArray.Length}");
            return;
        }

        // 各パラメータを処理
        for (int i = 0; i < paramIDs.Length; i++)
        {
            string paramID = paramIDs[i].Trim();

            // パラメータ値を解析
            if (!float.TryParse(paramValuesStrArray[i].Trim(), out float paramValue))
            {
                Debug.LogWarning($"パラメータ値 '{paramValuesStrArray[i]}' を数値に変換できません。");
                continue;
            }

            // インデックスを解析（n1, n2, n3...形式）
            if (paramID.StartsWith("n") && int.TryParse(paramID.Substring(1), out int paramIndex))
            {
                // インデックスが範囲外でないか確認
                if (paramIndex >= 0 && paramIndex < cubismModel.Parameters.Length)
                {
                    CubismParameter parameter = cubismModel.Parameters[paramIndex];

                    // パラメータの辞書キー
                    string paramKey = $"{modelID}_{paramID}";

                    // 既存のパラメータ状態を取得または新規作成
                    if (!activeParameters.TryGetValue(paramKey, out ParameterState paramState))
                    {
                        paramState = new ParameterState
                        {
                            CurrentValue = parameter.Value,
                            Parameter = parameter
                        };
                        activeParameters[paramKey] = paramState;
                    }

                    // 遷移を開始
                    paramState.StartValue = paramState.CurrentValue;
                    paramState.TargetValue = paramValue;
                    paramState.TransitionTime = actualTransitionTime;
                    paramState.ElapsedTime = 0;
                    paramState.IsTransitioning = true;

                    Debug.Log($"Live2Dモデル '{modelID}' のパラメータ '{paramID}' を {paramState.CurrentValue} から {paramValue} に {actualTransitionTime}秒かけて変更します。");
                }
                else
                {
                    Debug.LogWarning($"パラメータインデックス {paramIndex} はモデル '{modelID}' の範囲外です。");
                }
            }
            else
            {
                Debug.LogWarning($"パラメータID '{paramID}' が無効な形式です。n1, n2 などの形式を使用してください。");
            }
        }
    }

    // 単一のパラメータを設定する既存のメソッド
    // (旧メソッドとの互換性のために残す)
    public void SetParameter(string modelID, string paramID, float value)
    {
        SetParameters(modelID, paramID, value.ToString(), parameterTransitionTime);
    }

    /// <summary>
    /// 現在アクティブなLive2DモデルのIDを取得
    /// </summary>
    public string GetActiveModelID()
    {
        // アクティブなモデルが存在するかチェック
        if (activeModels != null && activeModels.Count > 0)
        {
            // 通常は1つのモデルしか表示されないので、最初のキーを返す
            return activeModels.Keys.FirstOrDefault();
        }
        return "";
    }

    public void DeleteAllModels()
    {
        // 既存のコード + パラメータのクリア
        foreach (var modelEntry in activeModels)
        {
            Destroy(modelEntry.Value);
        }
        activeModels.Clear();
        activeParameters.Clear();
        Debug.Log("すべてのLive2Dモデルを削除しました。");
    }

    /// <summary>
    /// 指定したIDに対応するモデルのGameObjectを取得します
    /// </summary>
    public GameObject GetModelObject(string modelID)
    {
        if (string.IsNullOrEmpty(modelID) || activeModels == null)
            return null;

        // モデルIDが指定されていればそれに対応するモデルを取得
        if (activeModels.TryGetValue(modelID, out GameObject modelObj))
        {
            return modelObj;
        }

        // IDが見つからないが、アクティブなモデルが1つだけなら返す
        if (activeModels.Count == 1)
        {
            return activeModels.Values.FirstOrDefault();
        }

        return null;
    }
}
