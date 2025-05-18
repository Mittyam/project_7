using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �^���S�ȃC�x���g�}�l�[�W���[
/// �Q�[���S�̂Ŏg�p�ł��鋤�ʂ̃C�x���g�o�X��񋟂���
/// </summary>
public class TypedEventManager : Singleton<TypedEventManager>
{
    // �^���Ƃ̃C�x���g�n���h���[��ێ����鎫��
    private readonly Dictionary<Type, Delegate> eventHandlers = new Dictionary<Type, Delegate>();

    /// <summary>
    /// �C�x���g���w�ǂ��܂�
    /// </summary>
    /// <typeparam name="T">�C�x���g�f�[�^�̌^</typeparam>
    /// <param name="handler">�C�x���g�������ɌĂяo�����n���h��</param>
    public void Subscribe<T>(Action<T> handler) where T : struct
    {
        Type eventType = typeof(T);

        if (!eventHandlers.ContainsKey(eventType))
        {
            eventHandlers[eventType] = null;
        }

        eventHandlers[eventType] = Delegate.Combine(eventHandlers[eventType], handler);

        // Debug.Log($"�C�x���g{eventType.Name}�̍w�ǂ�o�^���܂���");
    }

    /// <summary>
    /// �C�x���g�w�ǂ��������܂�
    /// </summary>
    /// <typeparam name="T">�C�x���g�f�[�^�̌^</typeparam>
    /// <param name="handler">��������n���h��</param>
    public void Unsubscribe<T>(Action<T> handler) where T : struct
    {
        Type eventType = typeof(T);

        if (!eventHandlers.ContainsKey(eventType))
        {
            return;
        }

        eventHandlers[eventType] = Delegate.Remove(eventHandlers[eventType], handler);

        // �n���h���[���Ȃ��Ȃ����ꍇ�̓L�[���폜
        if (eventHandlers[eventType] == null)
        {
            eventHandlers.Remove(eventType);
        }

        // Debug.Log($"�C�x���g{eventType.Name}�̍w�ǂ��������܂���");
    }

    /// <summary>
    /// �C�x���g�𔭍s���܂�
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="eventData"></param>
    public void Publish<T>(T eventData) where T : struct
    {
        Type eventType = typeof(T);

        if (!eventHandlers.ContainsKey(eventType))
        {
            Debug.LogWarning($"�C�x���g{eventType.Name}�ɍw�ǎ҂����݂��܂���");
            return;
        }

        var handler = eventHandlers[eventType] as Action<T>;
        handler?.Invoke(eventData);

        Debug.Log($"�C�x���g{eventType.Name}�����s����܂���");
    }

    /// <summary>
    /// ���ׂẴC�x���g�w�ǂ��������܂�
    /// �V�[���J�ڎ��ȂǂɎg�p
    /// </summary>
    public void ClearAllSubscriptions()
    {
        eventHandlers.Clear();
        Debug.Log("���ׂẴC�x���g�w�ǂ���������܂���");
    }
}
