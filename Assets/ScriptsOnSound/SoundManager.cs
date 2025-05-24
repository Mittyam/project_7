using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
    [Header("Audio Sources")]
    public AudioSource bgmSource; // BGMを再生するAudioSource
    public AudioSource voiceSource; // Voiceを再生するAudioSource
    public List<AudioSource> seSources; // SEを再生するAudioSourceのリスト（複数のSEを同時再生可能）

    [Header("Sound Data Objects")]
    public BGMData bgmData; // BGMのデータを格納するオブジェクト
    public SEData seData; // SEのデータを格納するオブジェクト
    public VoiceData voiceData; // Voiceのデータを格納するオブジェクト
    public MainVoiceData mainVoiceData; // MainVoiceのデータを格納するオブジェクト

    // 再生終了時のイベント（例としてVoice用）
    public event Action<int> OnVoiceFinished; // 引数として再生したVoiceのindexを返す


    // BGMを再生する
    public void PlayBGM(int index, bool loop = true)
    {
        AudioClip clip = bgmData.GetClip(index);

        if (clip != null)
        {
            bgmSource.clip = clip;
            bgmSource.loop = loop; // ループ再生の設定
            bgmSource.Play();
        }
    }


    // BGMを停止する
    public void StopBGM()
    {
        bgmSource.Stop();
    }


    // SEを再生する
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
                Debug.LogWarning("使用可能なSEのAudioSourceがありません。");
            }
        }
    }


    // 特定のSEを停止する
    public void StopSE(int index)
    {
        foreach (var source in seSources)
        {
            if (source.clip == seData.GetClip(index) && source.isPlaying)
            {
                source.Stop();
                break;
            }
        }
    }


    // 全てのSEを停止する
    public void StopAllSE()
    {
        foreach (var source in seSources)
        {
            source.Stop();
        }
    }


    // Voiceを再生する
    public void PlayVoice(int index)
    {
        AudioClip clip = voiceData.GetClip(index);
        if (clip != null)
        {
            voiceSource.clip = clip;
            voiceSource.Play();
        }
    }


    /// <summary>
    /// Voiceを再生し、再生終了時にコールバックまたはイベントを発火する。
    /// </summary>
    public void PlayVoiceWithCallback(int index, Action callback = null)
    {
        AudioClip clip = voiceData.GetClip(index);
        if (clip != null)
        {
            // 通常のVoice再生処理
            voiceSource.clip = clip;
            voiceSource.Play();

            // 再生終了を検知するコルーチンを開始
            StartCoroutine(VoiceCompleteRoutine(index, clip, callback));
        }
    }


    /// <summary>
    /// Voice再生が終了したときに呼び出されるコルーチン。
    /// </summary>
    private IEnumerator VoiceCompleteRoutine(int index, AudioClip clip, Action callback)
    {
        // AudioSourceの再生時間に合わせて待機（シークバー等で途中で停止された場合の考慮は別途検討）
        yield return new WaitForSeconds(clip.length);

        // イベントが登録されていれば発火
        OnVoiceFinished?.Invoke(index);

        // コールバックが指定されていれば実行
        callback?.Invoke();
    }

    // MainVoiceを再生する
    public void PlayMainVoice(int index)
    {
        AudioClip clip = mainVoiceData.GetClip(index);
        if (clip != null)
        {
            voiceSource.clip = clip;
            voiceSource.Play();
        }
    }


    /// <summary>
    /// MainVoiceを再生し、再生終了時にコールバックまたはイベントを発火する。
    /// </summary>
    public void PlayMainVoiceWithCallback(int index, Action callback = null)
    {
        AudioClip clip = mainVoiceData.GetClip(index);
        if (clip != null)
        {
            // 通常のVoice再生処理
            voiceSource.clip = clip;
            voiceSource.Play();

            // 再生終了を検知するコルーチンを開始
            StartCoroutine(MainVoiceCompleteRoutine(index, clip, callback));
        }
    }


    /// <summary>
    /// MainVoice再生が終了したときに呼び出されるコルーチン。
    /// </summary>
    private IEnumerator MainVoiceCompleteRoutine(int index, AudioClip clip, Action callback)
    {
        // AudioSourceの再生時間に合わせて待機（シークバー等で途中で停止された場合の考慮は別途検討）
        yield return new WaitForSeconds(clip.length);

        // イベントが登録されていれば発火
        OnVoiceFinished?.Invoke(index);

        // コールバックが指定されていれば実行
        callback?.Invoke();
    }

    // BGM用にフェードイン・フェードアウトを組み合わせた例
    public void PlayBGMWithFadeIn(int index, float fadeDuration, Action callback = null, bool loop = true)
    {
        AudioClip clip = bgmData.GetClip(index);
        if (clip != null)
        {
            bgmSource.clip = clip;
            // フェードイン開始（元の目標音量を保持しておく必要があるので、ここでは例として1.0fを目標とする）
            StartCoroutine(FadeIn(bgmSource, fadeDuration, 1.0f, loop));
            // 必要であれば再生完了後のコールバックをコルーチンで管理することも可能
            StartCoroutine(BGMCompleteRoutine(index, clip, callback));
        }
    }

    // BGMをフェードアウトさせて停止する
    public void StopBGMWithFadeOut(float fadeDuration, Action callback = null)
    {
        if (bgmSource.isPlaying)
        {
            StartCoroutine(FadeOutAndCallback(bgmSource, fadeDuration, callback));
        }
    }


    private IEnumerator FadeOutAndCallback(AudioSource source, float fadeDuration, Action callback)
    {
        yield return FadeOut(source, fadeDuration);
        callback?.Invoke();
    }


    private IEnumerator BGMCompleteRoutine(int index, AudioClip clip, Action callback)
    {
        yield return new WaitForSeconds(clip.length);
        // イベント・コールバックの発火処理（BGMにもイベントが必要なら別のイベントを用意）
        callback?.Invoke();
    }


    // Voiceを停止する
    public void StopVoice()
    {
        voiceSource.Stop();
    }

    // MainVoiceを停止する
    public void StopMainVoice()
    {
        voiceSource.Stop();
    }


    public void StopAllSounds()
    {
        StopBGM();
        StopAllSE();
        StopVoice();
    }


    // BGMの長さを取得
    public float GetBGMLength(int index)
    {
        AudioClip clip = bgmData.GetClip(index);
        if (clip != null)
        {
            Debug.Log($"BGM[{index}]の長さ: {clip.length}秒");
            return clip.length;
        }
        Debug.LogWarning("指定されたBGMが見つかりません。");
        return 0f;
    }


    // SEの長さを取得
    public float GetSELength(int index)
    {
        AudioClip clip = seData.GetClip(index);
        if (clip != null)
        {
            Debug.Log($"SE[{index}]の長さ: {clip.length}秒");
            return clip.length;
        }
        Debug.LogWarning("指定されたSEが見つかりません。");
        return 0f;
    }


    // Voiceの長さを取得
    public float GetVoiceLength(int index)
    {
        AudioClip clip = voiceData.GetClip(index);
        if (clip != null)
        {
            Debug.Log($"Voice[{index}]の長さ: {clip.length}秒");
            return clip.length;
        }
        Debug.LogWarning("指定されたVoiceが見つかりません。");
        return 0f;
    }


    /// <summary>
    /// 指定したAudioSourceの音量をフェードインさせながら再生する
    /// </summary>
    private IEnumerator FadeIn(AudioSource source, float fadeDuration, float targetVolume, bool loop)
    {
        source.volume = 0f;
        source.loop = loop; // ループ再生の設定
        source.Play();
        float time = 0f;
        while (time < fadeDuration)
        {
            // 線形補完で音量を増加
            source.volume = Mathf.Lerp(0f, targetVolume, time / fadeDuration);
            time += Time.deltaTime;
            yield return null;
        }
        source.volume = targetVolume; // 最終的な音量を設定
    }


    /// <summary>
    /// 指定したAudioSourceの音量をフェードアウトさせ、終了後に停止する。
    /// </summary>
    private IEnumerator FadeOut(AudioSource source, float fadeDuration)
    {
        float startVolume = source.volume;
        float time = 0f;
        while (time < fadeDuration)
        {
            // 線形補間で音量を減少
            source.volume = Mathf.Lerp(startVolume, 0f, time / fadeDuration);
            time += Time.deltaTime;
            yield return null;
        }
        source.volume = 0f;
        source.Stop();
    }
}