using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// NovelEventData.conditions を評価して true / false を返す静的クラス
/// </summary>
public static class ConditionEvaluator
{
    // もし新しい ConditionType を追加したら、このディクショナリに
    // 1 行 delegate を登録するだけで済む設計にしてある
    private static readonly Dictionary<ConditionType, Func<Condition, bool>> _handlers
        = new()
    {
        // 好感度判定
        { ConditionType.CheckAffinity ,  c => {
            var value = StatusManager.Instance.GetStatus().affection;
            return c.isGreaterThanOrEqual ? value >= c.threshold : value <= c.threshold;
        }},

        // 愛情判定
        { ConditionType.CheckLove     ,  c => {
            var value = StatusManager.Instance.GetStatus().love;
            return c.isGreaterThanOrEqual ? value >= c.threshold : value <= c.threshold;
        }},

        // 日付判定
        { ConditionType.CheckDate     ,  c => {
            var value = StatusManager.Instance.GetStatus().day;
            return c.isGreaterThanOrEqual ? value >= c.threshold : value <= c.threshold;
        }},
    };

    /// <summary>
    /// 条件リストをすべて満たしているか？
    /// </summary>
    public static bool Evaluate(IEnumerable<Condition> conditions)
    {
        foreach (var cond in conditions)
        {
            if (!_handlers.TryGetValue(cond.conditionType, out var handler))
            {
                Debug.LogWarning($"ConditionEvaluator: 未対応の ConditionType {cond.conditionType}");
                return false;                       // 未対応条件があったら発火させない
            }

            bool ok = handler(cond);
#if UNITY_EDITOR
            Debug.Log($"[COND] {cond.conditionType} (thr={cond.threshold}, param={cond.paramName}) → {ok}"); 
#endif
            if (!ok) return false;                 // 1 つでも false なら全体で false
        }
        return true;
    }

    /// <summary>
    /// 実行時にハンドラを差し込む拡張ポイント
    /// 例) ConditionEvaluator.RegisterHandler(ConditionType.CheckMoney, c => …);
    /// </summary>
    public static void RegisterHandler(ConditionType type, Func<Condition, bool> handler)
        => _handlers[type] = handler;
}