using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System;

public class SoundUIManager : MonoBehaviour
{
    [Serializable]
    public class AudioChannel
    {
        [Header("Audio Mixer Parameter Name")]
        public string parameterName;

        [Header("UI References")]
        public Slider volumeSlider;
        public Toggle muteToggle;

        [Header("Default Volume (0.0 ~ 1.0)")]
        [Range(0f, 1f)] public float defaultVolume = 1f;
    }

    [Header("Audio Channels")]
    public AudioChannel[] audioChannels;

    [Header("Audio Mixer")]
    public AudioMixer audioMixer;

    // 音が完全に聞こえなくなるレベル（ミュート時）
    private const float MuteDbLevel = -80f;

    private void Start()
    {
        // 各チャンネルを初期化
        foreach (var channel in audioChannels)
        {
            InitializeChannel(channel);

            // スライダーのコールバックを登録
            channel.volumeSlider.onValueChanged.AddListener(value =>
            {
                SetVolume(channel.parameterName, value);
            });

            // トグルのコールバックを登録
            channel.muteToggle.onValueChanged.AddListener(isMuted =>
            {
                SetMute(channel.parameterName, isMuted);
            });
        }
    }

    /// <summary>
    /// チャンネルの初期化
    /// </summary>
    private void InitializeChannel(AudioChannel channel)
    {
        // 初期値をdBへ変換してAudioMixerに適用
        float defaultDb = LinearToDecibel(channel.defaultVolume);
        audioMixer.SetFloat(channel.parameterName, defaultDb);

        // スライダーの値も初期音量に合わせる
        channel.volumeSlider.value = channel.defaultVolume;
        channel.muteToggle.isOn = false;  // ミュートは初期値をオフに
    }

    /// <summary>
    /// 音量の変更（0.0 ~ 1.0 を dBに変換）
    /// </summary>
    private void SetVolume(string parameterName, float sliderValue)
    {
        float dB = LinearToDecibel(sliderValue);
        audioMixer.SetFloat(parameterName, dB);
    }

    /// <summary>
    /// ミュートのオン/オフ
    /// </summary>
    private void SetMute(string parameterName, bool isMuted)
    {
        if (isMuted)
        {
            // -80dB程度にすれば実質ミュート
            audioMixer.SetFloat(parameterName, MuteDbLevel);
        }
        else
        {
            // ミュートを解除したタイミングで現在のスライダー値を取得して設定
            foreach (var channel in audioChannels)
            {
                if (channel.parameterName == parameterName)
                {
                    float currentDb = LinearToDecibel(channel.volumeSlider.value);
                    audioMixer.SetFloat(parameterName, currentDb);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 0.0~1.0 の線形ボリュームをデシベルに変換
    /// </summary>
    private float LinearToDecibel(float linear)
    {
        // ボリュームが0だとLog10でエラーになるため0.0001fでClamp
        return Mathf.Log10(Mathf.Clamp(linear, 0.0001f, 1f)) * 20f;
    }
}