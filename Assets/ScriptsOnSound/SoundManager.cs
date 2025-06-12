using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ゲーム内の音声を管理するシングルトンクラス
/// BGM、SE、Voice、タッチ音声などを扱う
/// </summary>
public class SoundManager : Singleton<SoundManager>
{
    [Header("Audio Sources")]
    public AudioSource bgmSource;         // BGMを再生するAudioSource
    public AudioSource voiceSource;       // Voiceを再生するAudioSource（1つ目）
    public AudioSource voiceSource2;      // Voiceを再生するAudioSource（2つ目）- クロスフェード用
    public List<AudioSource> seSources;   // SEを再生するAudioSourceのリスト（複数のSEを同時再生可能）

    [Header("Sound Data Objects")]
    public BGMData bgmData;               // BGMのデータを格納するオブジェクト
    public SEData seData;                 // SEのデータを格納するオブジェクト
    public VoiceData voiceData;           // Voiceのデータを格納するオブジェクト
    public MainVoiceData mainVoiceData;   // MainVoiceのデータを格納するオブジェクト

    [Header("フェード設定")]
    [SerializeField] private float defaultFadeOutDuration = 0.3f;  // デフォルトのフェードアウト時間（秒）
    [SerializeField] private float defaultCrossfadeDuration = 0.2f; // デフォルトのクロスフェード時間（秒）

    // 再生終了時のイベント
    public event Action<int> OnVoiceFinished;  // 引数として再生したVoiceのindexを返す

    // 音声再生制御用
    private Coroutine currentVoiceCoroutine;  // 現在再生中の音声コルーチン
    private AudioClip currentVoiceClip;        // 現在再生中の音声クリップ
    private bool isVoicePlaying = false;       // 音声再生中フラグ

    // クロスフェード用
    private AudioSource activeVoiceSource;     // 現在アクティブなVoiceSource
    private AudioSource inactiveVoiceSource;   // 現在非アクティブなVoiceSource
    private Coroutine crossfadeCoroutine;      // クロスフェード処理用のコルーチン

    /// <summary>
    /// 初期化処理
    /// </summary>
    protected override void Awake()
    {
        base.Awake();

        // AudioSourceの初期設定をチェック
        CheckAndInitializeAudioSources();

        // 初期のアクティブ/非アクティブソースを設定
        activeVoiceSource = voiceSource;
        inactiveVoiceSource = voiceSource2;
    }

    /// <summary>
    /// AudioSourceの初期化チェック
    /// </summary>
    private void CheckAndInitializeAudioSources()
    {
        // BGM用AudioSourceのチェックと初期化
        if (bgmSource == null)
        {
            Debug.LogWarning("SoundManager: bgmSourceが設定されていません。新しく作成します。");
            GameObject bgmObj = new GameObject("BGM_AudioSource");
            bgmObj.transform.SetParent(transform);
            bgmSource = bgmObj.AddComponent<AudioSource>();
            bgmSource.playOnAwake = false;
            bgmSource.loop = true;
        }

        // Voice用AudioSource（1つ目）のチェックと初期化
        if (voiceSource == null)
        {
            Debug.LogWarning("SoundManager: voiceSourceが設定されていません。新しく作成します。");
            GameObject voiceObj = new GameObject("Voice_AudioSource");
            voiceObj.transform.SetParent(transform);
            voiceSource = voiceObj.AddComponent<AudioSource>();
            voiceSource.playOnAwake = false;
            voiceSource.loop = false;
        }

        // Voice用AudioSource（2つ目）のチェックと初期化
        if (voiceSource2 == null)
        {
            Debug.LogWarning("SoundManager: voiceSource2が設定されていません。新しく作成します。");
            GameObject voiceObj2 = new GameObject("Voice_AudioSource_2");
            voiceObj2.transform.SetParent(transform);
            voiceSource2 = voiceObj2.AddComponent<AudioSource>();
            voiceSource2.playOnAwake = false;
            voiceSource2.loop = false;
        }

        // SE用AudioSourceのチェックと初期化
        if (seSources == null || seSources.Count == 0)
        {
            Debug.LogWarning("SoundManager: seSourcesが設定されていません。新しく作成します。");
            seSources = new List<AudioSource>();

            // デフォルトで3つのSE用AudioSourceを作成
            for (int i = 0; i < 3; i++)
            {
                GameObject seObj = new GameObject($"SE_AudioSource_{i}");
                seObj.transform.SetParent(transform);
                AudioSource seSource = seObj.AddComponent<AudioSource>();
                seSource.playOnAwake = false;
                seSource.loop = false;
                seSources.Add(seSource);
            }
        }
    }

    #region BGM再生機能

    /// <summary>
    /// BGMを再生する
    /// </summary>
    /// <param name="index">BGMのインデックス</param>
    /// <param name="loop">ループ再生するかどうか</param>
    public void PlayBGM(int index, bool loop = true)
    {
        AudioClip clip = bgmData.GetClip(index);

        if (clip != null)
        {
            bgmSource.clip = clip;
            bgmSource.loop = loop;
            bgmSource.Play();
        }
        else
        {
            Debug.LogWarning($"SoundManager: インデックス {index} のBGMが見つかりません。");
        }
    }

    /// <summary>
    /// BGMをフェードイン付きで再生する
    /// </summary>
    public void PlayBGMWithFadeIn(int index, float fadeDuration, Action callback = null, bool loop = true)
    {
        AudioClip clip = bgmData.GetClip(index);
        if (clip != null)
        {
            bgmSource.clip = clip;
            // フェードイン開始
            StartCoroutine(FadeIn(bgmSource, fadeDuration, 1.0f, loop));
            // 必要であれば再生完了後のコールバックをコルーチンで管理
            if (callback != null)
            {
                StartCoroutine(DelayedCallback(clip.length, callback));
            }
        }
        else
        {
            Debug.LogWarning($"SoundManager: インデックス {index} のBGMが見つかりません。");
        }
    }

    /// <summary>
    /// BGMを停止する
    /// </summary>
    public void StopBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
        }
    }

    /// <summary>
    /// BGMをフェードアウトさせて停止する
    /// </summary>
    public void StopBGMWithFadeOut(float fadeDuration, Action callback = null)
    {
        if (bgmSource != null && bgmSource.isPlaying)
        {
            StartCoroutine(FadeOutAndCallback(bgmSource, fadeDuration, callback));
        }
        else if (callback != null)
        {
            callback.Invoke();
        }
    }

    #endregion

    #region SE再生機能

    /// <summary>
    /// SEを再生する
    /// </summary>
    /// <param name="index">SEのインデックス</param>
    public void PlaySE(int index)
    {
        AudioClip clip = seData.GetClip(index);

        if (clip != null)
        {
            // 再生可能なAudioSourceを探す
            AudioSource availableSource = seSources.Find(source => !source.isPlaying);
            if (availableSource != null)
            {
                availableSource.clip = clip;
                availableSource.Play();
            }
            else
            {
                Debug.LogWarning("SoundManager: 使用可能なSEのAudioSourceがありません。");
            }
        }
        else
        {
            Debug.LogWarning($"SoundManager: インデックス {index} のSEが見つかりません。");
        }
    }

    /// <summary>
    /// 特定のSEを停止する
    /// </summary>
    public void StopSE(int index)
    {
        AudioClip clip = seData.GetClip(index);
        if (clip == null) return;

        foreach (var source in seSources)
        {
            if (source.clip == clip && source.isPlaying)
            {
                source.Stop();
                break;
            }
        }
    }

    /// <summary>
    /// 全てのSEを停止する
    /// </summary>
    public void StopAllSE()
    {
        foreach (var source in seSources)
        {
            if (source.isPlaying)
            {
                source.Stop();
            }
        }
    }

    #endregion

    #region Voice再生機能（クロスフェード対応）

    /// <summary>
    /// AudioSourceを切り替える
    /// </summary>
    private void SwapAudioSources()
    {
        AudioSource temp = activeVoiceSource;
        activeVoiceSource = inactiveVoiceSource;
        inactiveVoiceSource = temp;
    }

    /// <summary>
    /// Voiceを再生する（クロスフェード対応版）
    /// </summary>
    /// <param name="index">Voiceのインデックス</param>
    public void PlayVoice(int index)
    {
        AudioClip clip = voiceData.GetClip(index);
        if (clip != null)
        {
            // クロスフェード処理を開始
            if (crossfadeCoroutine != null)
            {
                StopCoroutine(crossfadeCoroutine);
            }
            crossfadeCoroutine = StartCoroutine(CrossfadeToClip(clip, defaultCrossfadeDuration));

            currentVoiceClip = clip;
            isVoicePlaying = true;

            Debug.Log($"SoundManager: Voice再生（クロスフェード） - インデックス:{index}, クリップ:{clip.name}");
        }
        else
        {
            Debug.LogWarning($"SoundManager: インデックス {index} のVoiceが見つかりません。");
        }
    }

    /// <summary>
    /// Voiceを連続再生する（クロスフェード対応版）
    /// </summary>
    /// <param name="indices">再生するVoiceのインデックス配列</param>
    /// <param name="randomize">ランダム再生するかどうか</param>
    public void PlayVoiceSequence(int[] indices, bool randomize = false)
    {
        if (indices == null || indices.Length == 0)
        {
            Debug.LogWarning("SoundManager: 再生するVoiceのインデックスが指定されていません。");
            return;
        }

        // 既存の音声を停止
        StopVoice();

        // 音声シーケンスの再生開始
        currentVoiceCoroutine = StartCoroutine(PlayVoiceSequenceRoutineWithCrossfade(indices, randomize));
    }

    /// <summary>
    /// 音声シーケンスを再生するコルーチン（クロスフェード対応版）
    /// </summary>
    private IEnumerator PlayVoiceSequenceRoutineWithCrossfade(int[] indices, bool randomize)
    {
        isVoicePlaying = true;
        int currentIndex = 0;
        bool isFirstClip = true;

        // インデックスの初期化（ランダムの場合）
        if (randomize && indices.Length > 1)
        {
            currentIndex = UnityEngine.Random.Range(0, indices.Length);
        }

        while (isVoicePlaying)
        {
            // 現在のインデックスから音声クリップを取得
            AudioClip clip = voiceData.GetClip(indices[currentIndex]);

            if (clip != null)
            {
                Debug.Log($"SoundManager: 連続Voice再生 - インデックス:{indices[currentIndex]}, クリップ:{clip.name}");

                if (isFirstClip)
                {
                    // 最初のクリップは通常再生（クロスフェードなし）
                    activeVoiceSource.clip = clip;
                    activeVoiceSource.volume = 1f;
                    activeVoiceSource.Play();
                    currentVoiceClip = clip;
                    isFirstClip = false;
                }
                else
                {
                    // 2つ目以降はクロスフェード
                    yield return StartCoroutine(CrossfadeToClip(clip, defaultCrossfadeDuration));
                }

                // 音声の長さからクロスフェード時間を引いた時間だけ待機
                float waitTime = Mathf.Max(0.1f, clip.length - defaultCrossfadeDuration);
                yield return new WaitForSeconds(waitTime);

                // 次のインデックスを設定
                if (randomize)
                {
                    // ランダム選択（ただし同じものは避ける）
                    if (indices.Length > 1)
                    {
                        int prevIndex = currentIndex;
                        do
                        {
                            currentIndex = UnityEngine.Random.Range(0, indices.Length);
                        } while (currentIndex == prevIndex);
                    }
                }
                else
                {
                    // 順番に次のインデックス
                    currentIndex = (currentIndex + 1) % indices.Length;
                }
            }
            else
            {
                Debug.LogWarning($"SoundManager: インデックス {indices[currentIndex]} のVoiceが見つかりません。");
                // 無効なクリップの場合は次へ
                currentIndex = (currentIndex + 1) % indices.Length;
                yield return null;
            }

            // 再生中フラグが解除されていたら終了
            if (!isVoicePlaying)
                break;
        }
    }

    /// <summary>
    /// クロスフェード処理のコルーチン
    /// </summary>
    private IEnumerator CrossfadeToClip(AudioClip newClip, float duration)
    {
        // AudioSourceを切り替え
        SwapAudioSources();

        // 新しいクリップを非アクティブソースに設定
        activeVoiceSource.clip = newClip;
        activeVoiceSource.volume = 0f;
        activeVoiceSource.Play();

        // クロスフェード処理
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // アクティブソースをフェードイン
            activeVoiceSource.volume = Mathf.Lerp(0f, 1f, t);
            // 非アクティブソースをフェードアウト
            inactiveVoiceSource.volume = Mathf.Lerp(1f, 0f, t);

            yield return null;
        }

        // 最終値を設定
        activeVoiceSource.volume = 1f;
        inactiveVoiceSource.volume = 0f;

        // 非アクティブソースを停止
        inactiveVoiceSource.Stop();
        inactiveVoiceSource.clip = null;

        currentVoiceClip = newClip;
    }

    /// <summary>
    /// Voiceを再生し、コールバックを呼び出す（クロスフェード対応版）
    /// </summary>
    public void PlayVoiceWithCallback(int index, Action callback = null)
    {
        AudioClip clip = voiceData.GetClip(index);
        if (clip != null)
        {
            // クロスフェード処理を開始
            if (crossfadeCoroutine != null)
            {
                StopCoroutine(crossfadeCoroutine);
            }
            crossfadeCoroutine = StartCoroutine(CrossfadeToClip(clip, defaultCrossfadeDuration));

            currentVoiceClip = clip;
            isVoicePlaying = true;

            // コールバック用コルーチン開始
            StartCoroutine(VoiceCompleteRoutine(index, clip, callback));
        }
        else
        {
            Debug.LogWarning($"SoundManager: インデックス {index} のVoiceが見つかりません。");
            // クリップが見つからない場合でもコールバックは呼び出す
            if (callback != null)
            {
                callback.Invoke();
            }
        }
    }

    /// <summary>
    /// Voice再生完了時のコールバック処理
    /// </summary>
    private IEnumerator VoiceCompleteRoutine(int index, AudioClip clip, Action callback)
    {
        // 音声の長さだけ待機
        yield return new WaitForSeconds(clip.length);

        // イベント発火
        OnVoiceFinished?.Invoke(index);

        // コールバック実行
        callback?.Invoke();
    }

    /// <summary>
    /// Voiceを停止する（改善版）
    /// </summary>
    public void StopVoice()
    {
        // 連続再生中なら停止
        if (currentVoiceCoroutine != null)
        {
            StopCoroutine(currentVoiceCoroutine);
            currentVoiceCoroutine = null;
        }

        // クロスフェード中なら停止
        if (crossfadeCoroutine != null)
        {
            StopCoroutine(crossfadeCoroutine);
            crossfadeCoroutine = null;
        }

        // 両方のVoiceSourceを即座に停止
        if (voiceSource != null && voiceSource.isPlaying)
        {
            voiceSource.Stop();
            voiceSource.clip = null;
            voiceSource.volume = 1f;
        }

        if (voiceSource2 != null && voiceSource2.isPlaying)
        {
            voiceSource2.Stop();
            voiceSource2.clip = null;
            voiceSource2.volume = 1f;
        }

        // 再生中フラグを解除
        isVoicePlaying = false;
        currentVoiceClip = null;
    }

    /// <summary>
    /// Voiceをフェードアウトさせて停止する（改善版）
    /// </summary>
    public void StopVoiceWithFadeOut(float fadeDuration = 0.3f)
    {
        // 連続再生中なら停止
        if (currentVoiceCoroutine != null)
        {
            StopCoroutine(currentVoiceCoroutine);
            currentVoiceCoroutine = null;
        }

        // クロスフェード中なら停止
        if (crossfadeCoroutine != null)
        {
            StopCoroutine(crossfadeCoroutine);
            crossfadeCoroutine = null;
        }

        // 両方のVoiceSourceをフェードアウト
        if (voiceSource != null && voiceSource.isPlaying)
        {
            StartCoroutine(FadeOut(voiceSource, fadeDuration));
        }

        if (voiceSource2 != null && voiceSource2.isPlaying)
        {
            StartCoroutine(FadeOut(voiceSource2, fadeDuration));
        }

        // 再生中フラグを解除
        isVoicePlaying = false;
    }

    #endregion

    #region MainVoice再生機能（クロスフェード対応）

    /// <summary>
    /// MainVoiceを再生する（クロスフェード対応版）
    /// </summary>
    public void PlayMainVoice(int index)
    {
        AudioClip clip = mainVoiceData.GetClip(index);
        if (clip != null)
        {
            // クロスフェード処理を開始
            if (crossfadeCoroutine != null)
            {
                StopCoroutine(crossfadeCoroutine);
            }
            crossfadeCoroutine = StartCoroutine(CrossfadeToClip(clip, defaultCrossfadeDuration));

            currentVoiceClip = clip;
            isVoicePlaying = true;
        }
        else
        {
            Debug.LogWarning($"SoundManager: インデックス {index} のMainVoiceが見つかりません。");
        }
    }

    /// <summary>
    /// MainVoiceを再生し、コールバックを呼び出す（クロスフェード対応版）
    /// </summary>
    public void PlayMainVoiceWithCallback(int index, Action callback = null)
    {
        AudioClip clip = mainVoiceData.GetClip(index);
        if (clip != null)
        {
            // クロスフェード処理を開始
            if (crossfadeCoroutine != null)
            {
                StopCoroutine(crossfadeCoroutine);
            }
            crossfadeCoroutine = StartCoroutine(CrossfadeToClip(clip, defaultCrossfadeDuration));

            currentVoiceClip = clip;
            isVoicePlaying = true;

            // コールバック用コルーチン開始
            StartCoroutine(MainVoiceCompleteRoutine(index, clip, callback));
        }
        else
        {
            Debug.LogWarning($"SoundManager: インデックス {index} のMainVoiceが見つかりません。");
            // クリップが見つからない場合でもコールバックは呼び出す
            if (callback != null)
            {
                callback.Invoke();
            }
        }
    }

    /// <summary>
    /// MainVoice再生完了時のコールバック処理
    /// </summary>
    private IEnumerator MainVoiceCompleteRoutine(int index, AudioClip clip, Action callback)
    {
        // 音声の長さだけ待機
        yield return new WaitForSeconds(clip.length);

        // イベント発火
        OnVoiceFinished?.Invoke(index);

        // コールバック実行
        callback?.Invoke();
    }

    /// <summary>
    /// MainVoiceを停止する（通常のStopVoiceと同じ）
    /// </summary>
    public void StopMainVoice()
    {
        StopVoice();
    }

    #endregion

    #region ユーティリティ

    /// <summary>
    /// 全ての音声を停止する
    /// </summary>
    public void StopAllSounds()
    {
        StopBGM();
        StopAllSE();
        StopVoice();
    }

    /// <summary>
    /// BGMの長さを取得
    /// </summary>
    public float GetBGMLength(int index)
    {
        AudioClip clip = bgmData.GetClip(index);
        if (clip != null)
        {
            return clip.length;
        }
        Debug.LogWarning($"SoundManager: インデックス {index} のBGMが見つかりません。");
        return 0f;
    }

    /// <summary>
    /// SEの長さを取得
    /// </summary>
    public float GetSELength(int index)
    {
        AudioClip clip = seData.GetClip(index);
        if (clip != null)
        {
            return clip.length;
        }
        Debug.LogWarning($"SoundManager: インデックス {index} のSEが見つかりません。");
        return 0f;
    }

    /// <summary>
    /// Voiceの長さを取得
    /// </summary>
    public float GetVoiceLength(int index)
    {
        AudioClip clip = voiceData.GetClip(index);
        if (clip != null)
        {
            return clip.length;
        }
        Debug.LogWarning($"SoundManager: インデックス {index} のVoiceが見つかりません。");
        return 0f;
    }

    /// <summary>
    /// 指定時間後にコールバックを実行するコルーチン
    /// </summary>
    private IEnumerator DelayedCallback(float delay, Action callback)
    {
        yield return new WaitForSeconds(delay);
        callback?.Invoke();
    }

    /// <summary>
    /// AudioSourceの音量をフェードインさせながら再生する
    /// </summary>
    private IEnumerator FadeIn(AudioSource source, float fadeDuration, float targetVolume, bool loop)
    {
        if (source == null) yield break;

        // 初期設定
        source.volume = 0f;
        source.loop = loop;
        source.Play();

        // フェードイン処理
        float time = 0f;
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, targetVolume, time / fadeDuration);
            yield return null;
        }

        // 最終設定
        source.volume = targetVolume;
    }

    /// <summary>
    /// AudioSourceの音量をフェードアウトさせ、終了後に停止する
    /// </summary>
    private IEnumerator FadeOut(AudioSource source, float fadeDuration)
    {
        if (source == null || !source.isPlaying) yield break;

        // 初期設定
        float startVolume = source.volume;

        // フェードアウト処理
        float time = 0f;
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, time / fadeDuration);
            yield return null;
        }

        // 最終処理
        source.volume = 0f;
        source.Stop();

        // 音量を元に戻す
        source.volume = startVolume;
    }

    /// <summary>
    /// フェードアウト後にコールバックを実行する
    /// </summary>
    private IEnumerator FadeOutAndCallback(AudioSource source, float fadeDuration, Action callback)
    {
        yield return StartCoroutine(FadeOut(source, fadeDuration));
        callback?.Invoke();
    }

    #endregion
}