using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 型安全なイベントマネージャー
/// ゲーム全体で使用できる共通のイベントバスを提供する
/// </summary>
public class TypedEventManager : Singleton<TypedEventManager>
{
    // 型ごとのイベントハンドラーを保持する辞書
    private readonly Dictionary<Type, Delegate> eventHandlers = new Dictionary<Type, Delegate>();

    /// <summary>
    /// イベントを購読します
    /// </summary>
    /// <typeparam name="T">イベントデータの型</typeparam>
    /// <param name="handler">イベント発生時に呼び出されるハンドラ</param>
    public void Subscribe<T>(Action<T> handler) where T : struct
    {
        Type eventType = typeof(T);

        if (!eventHandlers.ContainsKey(eventType))
        {
            eventHandlers[eventType] = null;
        }

        eventHandlers[eventType] = Delegate.Combine(eventHandlers[eventType], handler);

        // Debug.Log($"イベント{eventType.Name}の購読を登録しました");
    }

    /// <summary>
    /// イベント購読を解除します
    /// </summary>
    /// <typeparam name="T">イベントデータの型</typeparam>
    /// <param name="handler">解除するハンドラ</param>
    public void Unsubscribe<T>(Action<T> handler) where T : struct
    {
        Type eventType = typeof(T);

        if (!eventHandlers.ContainsKey(eventType))
        {
            return;
        }

        eventHandlers[eventType] = Delegate.Remove(eventHandlers[eventType], handler);

        // ハンドラーがなくなった場合はキーを削除
        if (eventHandlers[eventType] == null)
        {
            eventHandlers.Remove(eventType);
        }

        // Debug.Log($"イベント{eventType.Name}の購読を解除しました");
    }

    /// <summary>
    /// イベントを発行します
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="eventData"></param>
    public void Publish<T>(T eventData) where T : struct
    {
        Type eventType = typeof(T);

        if (!eventHandlers.ContainsKey(eventType))
        {
            Debug.LogWarning($"イベント{eventType.Name}に購読者が存在しません");
            return;
        }

        var handler = eventHandlers[eventType] as Action<T>;
        handler?.Invoke(eventData);

        Debug.Log($"イベント{eventType.Name}が発行されました");
    }

    /// <summary>
    /// すべてのイベント購読を解除します
    /// シーン遷移時などに使用
    /// </summary>
    public void ClearAllSubscriptions()
    {
        eventHandlers.Clear();
        Debug.Log("すべてのイベント購読が解除されました");
    }
}
