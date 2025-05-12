using Live2D.Cubism.Core;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Live2DController : MonoBehaviour
{
    [SerializeField] private GameObject modelContainer;
    [SerializeField] private List<Live2DModelAsset> modelAssets;

    // �p�����[�^�X�V���Ԃ��C���X�y�N�^�[����ݒ�\�ɂ���
    [Header("�p�����[�^�ݒ�")]
    [SerializeField] private float parameterTransitionTime = 0.5f;

    private Dictionary<string, GameObject> activeModels = new Dictionary<string, GameObject>();

    // �p�����[�^�̌��ݒl��ێ����邽�߂̎���
    // �L�[�F���f��ID_�p�����[�^ID�A�l�F���݂̒l�ƖڕW�l�̃y�A
    private Dictionary<string, ParameterState> activeParameters = new Dictionary<string, ParameterState>();

    private class ParameterState
    {
        public float CurrentValue;      // ���ݒn
        public float TargetValue;       // �ڕW�l
        public float StartValue;        // �J�n�l
        public float TransitionTime;    // �J�ڎ���
        public float ElapsedTime;       // �o�ߎ���
        public CubismParameter Parameter; // �p�����[�^�ւ̎Q��
        public bool IsTransitioning; // �J�ڒ��t���O
    }

    [System.Serializable]
    public class Live2DModelAsset
    {
        public string id;  // ���f��ID
        public GameObject prefab;  // ���f���v���n�u
    }

    // Start����
    private void Start()
    {
        // �K�v�ɉ���������������
    }

    // LateUpdate�Ńp�����[�^���X�V
    private void LateUpdate()
    {
        // �p�����[�^�X�V����
        UpdateParameters();
    }

    // �J���}��؂�̕����񂩂�Vector2�𐶐����郆�[�e�B���e�B���\�b�h
    private Vector2 ParseVector2(string vectorString)
    {
        if (string.IsNullOrEmpty(vectorString))
            return Vector2.zero;

        try
        {
            // �J���}�ŕ���
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
            Debug.LogError($"Vector2�̃p�[�X�Ɏ��s���܂���: {vectorString}, �G���[: {e.Message}");
        }

        return Vector2.zero; // �f�t�H���g�l
    }

    // �p�����[�^���X�V���郁�\�b�h
    private void UpdateParameters()
    {
        List<string> keysToRemove = new List<string>();

        foreach (var pair in activeParameters)
        {
            var paramState = pair.Value;

            // �J�ڒ��̏ꍇ
            if (paramState.IsTransitioning)
            {
                paramState.ElapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(paramState.ElapsedTime / paramState.TransitionTime);

                // ���`��ԂŒl���X�V
                paramState.CurrentValue = Mathf.Lerp(paramState.StartValue, paramState.TargetValue, t);

                // �p�����[�^�l��ݒ�
                if (paramState.Parameter != null)
                {
                    paramState.Parameter.Value = paramState.CurrentValue;
                }

                // �J�ڊ����`�F�b�N
                if (t >= 1.0f)
                {
                    paramState.IsTransitioning = false;
                    paramState.CurrentValue = paramState.TargetValue;
                }
            }
            else
            {
                // �J�ڂ��Ă��Ȃ��ꍇ�͌��݂̒l���ێ�
                if (paramState.Parameter != null)
                {
                    paramState.Parameter.Value = paramState.CurrentValue;
                }
                else
                {
                    // �p�����[�^�������ɂȂ����ꍇ�̓��X�g����폜
                    keysToRemove.Add(pair.Key);
                }
            }
        }

        // �����ȃp�����[�^�����X�g����폜
        foreach (var key in keysToRemove)
        {
            activeParameters.Remove(key);
        }
    }

    public void ShowModel(string modelID, float scale, string positionString)
    {
        if (string.IsNullOrEmpty(modelID)) return;

        // ����Ȓl�udeleteModel�v�̏ꍇ�͑S���f���폜
        if (modelID.ToLower() == "deletemodel")
        {
            DeleteAllModels();
            return;
        }

        // �����񂩂�Vector2�ɕϊ�
        Vector2 position = ParseVector2(positionString);

        // ���f���v���n�u�����O�o�^���X�g����T��
        Live2DModelAsset asset = modelAssets.Find(a => a.id == modelID);
        if (asset == null)
        {
            Debug.LogError($"Live2D���f�� '{modelID}' ���o�^����Ă��܂���BModelAssets�Ƀ��f����ǉ����Ă��������B");
            return;
        }

        // ���łɓ���ID�̃��f�����\������Ă���ꍇ�͍X�V���邽�߈�U�폜
        if (activeModels.ContainsKey(modelID))
        {
            Destroy(activeModels[modelID]);
            activeModels.Remove(modelID);
        }

        // ���f���𐶐�
        GameObject modelObj = Instantiate(asset.prefab, modelContainer.transform);

        // �X�P�[���ƈʒu��ݒ�
        Transform transform = modelObj.GetComponent<Transform>();
        if (transform != null)
        {
            transform.localScale = new Vector3(scale, scale, scale);
            transform.position = position;
        }

        // �Ǘ��pDictionary�ɕۑ�
        activeModels[modelID] = modelObj;

        Debug.Log($"Live2D���f�� '{modelID}' ��\�����܂����B�X�P�[��: {scale}, �ʒu: {position}");
    }

    /// <summary>
    /// Vector2�̈ʒu���w�肵�ă��f����\��
    /// </summary>
    public void ShowModel(string modelID, float scale, Vector2 position)
    {
        // �ʒu���𕶎���ɕϊ�
        string positionStr = $"{position.x},{position.y}";
        ShowModel(modelID, scale, positionStr);
    }

    public void HideModel(string modelID)
    {
        if (string.IsNullOrEmpty(modelID)) return;

        if (activeModels.TryGetValue(modelID, out GameObject modelObj))
        {
            // ���f���֘A�̃p�����[�^���N���A
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
            Debug.Log($"Live2D���f�� '{modelID}' ���폜���܂����B");
        }
    }

    public void PlayAnimation(string modelID, string animationTrigger)
    {
        if (string.IsNullOrEmpty(animationTrigger) || string.IsNullOrEmpty(modelID)) return;

        if (activeModels.TryGetValue(modelID, out GameObject modelObj))
        {
            // Live2D Cubism�A�j���[�V�����Đ��R���|�[�l���g�ւ̃A�N�Z�X
            var animator = modelObj.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetTrigger(animationTrigger);
                Debug.Log($"Live2D���f�� '{modelID}' �̃A�j���[�V���� '{animationTrigger}' ���Đ����܂����B");
            }
            else
            {
                Debug.LogWarning($"Live2D���f�� '{modelID}' ��Animator�R���|�[�l���g��������܂���B");
            }
        }
    }

    // �J���}��؂�̃p�����[�^ID�ƒl����������V���\�b�h
    public void SetParameters(string modelID, string paramIDsStr, string paramValuesStr, float transitionTime = -1.0f)
    {
        if (string.IsNullOrEmpty(paramIDsStr) || string.IsNullOrEmpty(modelID)) return;

        // �f�t�H���g�̑J�ڎ��Ԃ��g�p���邩�A�w�肳�ꂽ�l���g�p
        float actualTransitionTime = (transitionTime < 0) ? parameterTransitionTime : transitionTime;

        // �J���}��؂�̕������z��ɕϊ�
        string[] paramIDs = paramIDsStr.Split(',');
        string[] paramValuesStrArray = paramValuesStr.Split(',');

        // ���f�����擾
        if (!activeModels.TryGetValue(modelID, out GameObject modelObj))
        {
            Debug.LogWarning($"Live2D���f�� '{modelID}' ��������܂���B");
            return;
        }

        // CubismModel�R���|�[�l���g���擾
        var cubismModel = modelObj.GetComponent<CubismModel>();
        if (cubismModel == null)
        {
            Debug.LogWarning($"Live2D���f�� '{modelID}' ��CubismModel�R���|�[�l���g��������܂���B");
            return;
        }

        // �p�����[�^�̐�����v���Ȃ��ꍇ�͌x�����o��
        if (paramIDs.Length != paramValuesStrArray.Length)
        {
            Debug.LogWarning($"�p�����[�^ID�ƒl�̐�����v���܂���BIDs: {paramIDs.Length}, Values: {paramValuesStrArray.Length}");
            return;
        }

        // �e�p�����[�^������
        for (int i = 0; i < paramIDs.Length; i++)
        {
            string paramID = paramIDs[i].Trim();

            // �p�����[�^�l�����
            if (!float.TryParse(paramValuesStrArray[i].Trim(), out float paramValue))
            {
                Debug.LogWarning($"�p�����[�^�l '{paramValuesStrArray[i]}' �𐔒l�ɕϊ��ł��܂���B");
                continue;
            }

            // �C���f�b�N�X����́in1, n2, n3...�`���j
            if (paramID.StartsWith("n") && int.TryParse(paramID.Substring(1), out int paramIndex))
            {
                // �C���f�b�N�X���͈͊O�łȂ����m�F
                if (paramIndex >= 0 && paramIndex < cubismModel.Parameters.Length)
                {
                    CubismParameter parameter = cubismModel.Parameters[paramIndex];

                    // �p�����[�^�̎����L�[
                    string paramKey = $"{modelID}_{paramID}";

                    // �����̃p�����[�^��Ԃ��擾�܂��͐V�K�쐬
                    if (!activeParameters.TryGetValue(paramKey, out ParameterState paramState))
                    {
                        paramState = new ParameterState
                        {
                            CurrentValue = parameter.Value,
                            Parameter = parameter
                        };
                        activeParameters[paramKey] = paramState;
                    }

                    // �J�ڂ��J�n
                    paramState.StartValue = paramState.CurrentValue;
                    paramState.TargetValue = paramValue;
                    paramState.TransitionTime = actualTransitionTime;
                    paramState.ElapsedTime = 0;
                    paramState.IsTransitioning = true;

                    Debug.Log($"Live2D���f�� '{modelID}' �̃p�����[�^ '{paramID}' �� {paramState.CurrentValue} ���� {paramValue} �� {actualTransitionTime}�b�����ĕύX���܂��B");
                }
                else
                {
                    Debug.LogWarning($"�p�����[�^�C���f�b�N�X {paramIndex} �̓��f�� '{modelID}' �͈̔͊O�ł��B");
                }
            }
            else
            {
                Debug.LogWarning($"�p�����[�^ID '{paramID}' �������Ȍ`���ł��Bn1, n2 �Ȃǂ̌`�����g�p���Ă��������B");
            }
        }
    }

    // �P��̃p�����[�^��ݒ肷������̃��\�b�h
    // (�����\�b�h�Ƃ̌݊����̂��߂Ɏc��)
    public void SetParameter(string modelID, string paramID, float value)
    {
        SetParameters(modelID, paramID, value.ToString(), parameterTransitionTime);
    }

    /// <summary>
    /// ���݃A�N�e�B�u��Live2D���f����ID���擾
    /// </summary>
    public string GetActiveModelID()
    {
        // �A�N�e�B�u�ȃ��f�������݂��邩�`�F�b�N
        if (activeModels != null && activeModels.Count > 0)
        {
            // �ʏ��1�̃��f�������\������Ȃ��̂ŁA�ŏ��̃L�[��Ԃ�
            return activeModels.Keys.FirstOrDefault();
        }
        return "";
    }

    public void DeleteAllModels()
    {
        // �����̃R�[�h + �p�����[�^�̃N���A
        foreach (var modelEntry in activeModels)
        {
            Destroy(modelEntry.Value);
        }
        activeModels.Clear();
        activeParameters.Clear();
        Debug.Log("���ׂĂ�Live2D���f�����폜���܂����B");
    }

    /// <summary>
    /// �w�肵��ID�ɑΉ����郂�f����GameObject���擾���܂�
    /// </summary>
    public GameObject GetModelObject(string modelID)
    {
        if (string.IsNullOrEmpty(modelID) || activeModels == null)
            return null;

        // ���f��ID���w�肳��Ă���΂���ɑΉ����郂�f�����擾
        if (activeModels.TryGetValue(modelID, out GameObject modelObj))
        {
            return modelObj;
        }

        // ID��������Ȃ����A�A�N�e�B�u�ȃ��f����1�����Ȃ�Ԃ�
        if (activeModels.Count == 1)
        {
            return activeModels.Values.FirstOrDefault();
        }

        return null;
    }
}
