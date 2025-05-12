using UnityEngine;

/// <summary>
/// シングルトンパターンを実装するクラス。 
/// 継承クラスは、Tとして自身の型を指定する。
/// 例: public class GameManager : Singleton<GameManager> { ... }
/// </summary>
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static object _lock = new object();
    private static bool _applicationIsQuitting = false;

    /// <summary>
    /// シングルトンインスタンスへのアクセスプロパティ。
    /// まだ存在しない場合はFindまたは作成して取得。
    /// アプリケーション終了時はnullを返す。
    /// </summary>
    public static T Instance
    {
        get
        {
            if (_applicationIsQuitting)
            {

                Debug.LogWarning($"[Singleton] インスタンス '{typeof(T)}' はアプリケーション終了時に既に破棄されています。null を返します。");
                return null;
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    // シーン内を検索
                    _instance = (T)FindObjectOfType(typeof(T));

                    // 見つからなければ新しいオブジェクトを作成
                    if (_instance == null)
                    {
                        var singletonObject = new GameObject(typeof(T).Name);
                        _instance = singletonObject.AddComponent<T>();
                        DontDestroyOnLoad(singletonObject);
                    }
                }

                return _instance;
            }
        }
    }

    /// <summary>
    /// 継承クラスがAwakeをOverrideする場合は
    /// base.Awake()を必ず呼び出してください。
    /// </summary>
    protected virtual void Awake()
    {
        // 既にインスタンスが存在し、自身がそれでない場合は削除
        if (_instance != null && _instance != this as T)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this as T;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    /// <summary>
    /// アプリケーション終了時にインスタンスをマーク
    /// </summary>
    protected virtual void OnApplicationQuit()
    {
        _applicationIsQuitting = true;
    }
}
