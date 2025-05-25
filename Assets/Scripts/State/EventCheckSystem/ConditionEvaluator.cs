using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// NovelEventData.conditions ��]������ true / false ��Ԃ��ÓI�N���X
/// </summary>
public static class ConditionEvaluator
{
    // �����V���� ConditionType ��ǉ�������A���̃f�B�N�V���i����
    // 1 �s delegate ��o�^���邾���ōςސ݌v�ɂ��Ă���
    private static readonly Dictionary<ConditionType, Func<Condition, bool>> _handlers
        = new()
    {
        // �D���x����
        { ConditionType.CheckAffinity ,  c => {
            var value = StatusManager.Instance.GetStatus().affection;
            return c.isGreaterThanOrEqual ? value >= c.threshold : value <= c.threshold;
        }},

        // �����
        { ConditionType.CheckLove     ,  c => {
            var value = StatusManager.Instance.GetStatus().love;
            return c.isGreaterThanOrEqual ? value >= c.threshold : value <= c.threshold;
        }},

        // ���t����
        { ConditionType.CheckDate     ,  c => {
            var value = StatusManager.Instance.GetStatus().day;
            return c.isGreaterThanOrEqual ? value >= c.threshold : value <= c.threshold;
        }},
    };

    /// <summary>
    /// �������X�g�����ׂĖ������Ă��邩�H
    /// </summary>
    public static bool Evaluate(IEnumerable<Condition> conditions)
    {
        foreach (var cond in conditions)
        {
            if (!_handlers.TryGetValue(cond.conditionType, out var handler))
            {
                Debug.LogWarning($"ConditionEvaluator: ���Ή��� ConditionType {cond.conditionType}");
                return false;                       // ���Ή��������������甭�΂����Ȃ�
            }

            bool ok = handler(cond);
#if UNITY_EDITOR
            Debug.Log($"[COND] {cond.conditionType} (thr={cond.threshold}, param={cond.paramName}) �� {ok}"); 
#endif
            if (!ok) return false;                 // 1 �ł� false �Ȃ�S�̂� false
        }
        return true;
    }

    /// <summary>
    /// ���s���Ƀn���h�����������ފg���|�C���g
    /// ��) ConditionEvaluator.RegisterHandler(ConditionType.CheckMoney, c => �c);
    /// </summary>
    public static void RegisterHandler(ConditionType type, Func<Condition, bool> handler)
        => _handlers[type] = handler;
}