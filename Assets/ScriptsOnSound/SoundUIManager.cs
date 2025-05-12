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

    // �������S�ɕ������Ȃ��Ȃ郌�x���i�~���[�g���j
    private const float MuteDbLevel = -80f;

    private void Start()
    {
        // �e�`�����l����������
        foreach (var channel in audioChannels)
        {
            InitializeChannel(channel);

            // �X���C�_�[�̃R�[���o�b�N��o�^
            channel.volumeSlider.onValueChanged.AddListener(value =>
            {
                SetVolume(channel.parameterName, value);
            });

            // �g�O���̃R�[���o�b�N��o�^
            channel.muteToggle.onValueChanged.AddListener(isMuted =>
            {
                SetMute(channel.parameterName, isMuted);
            });
        }
    }

    /// <summary>
    /// �`�����l���̏�����
    /// </summary>
    private void InitializeChannel(AudioChannel channel)
    {
        // �����l��dB�֕ϊ�����AudioMixer�ɓK�p
        float defaultDb = LinearToDecibel(channel.defaultVolume);
        audioMixer.SetFloat(channel.parameterName, defaultDb);

        // �X���C�_�[�̒l���������ʂɍ��킹��
        channel.volumeSlider.value = channel.defaultVolume;
        channel.muteToggle.isOn = false;  // �~���[�g�͏����l���I�t��
    }

    /// <summary>
    /// ���ʂ̕ύX�i0.0 ~ 1.0 �� dB�ɕϊ��j
    /// </summary>
    private void SetVolume(string parameterName, float sliderValue)
    {
        float dB = LinearToDecibel(sliderValue);
        audioMixer.SetFloat(parameterName, dB);
    }

    /// <summary>
    /// �~���[�g�̃I��/�I�t
    /// </summary>
    private void SetMute(string parameterName, bool isMuted)
    {
        if (isMuted)
        {
            // -80dB���x�ɂ���Ύ����~���[�g
            audioMixer.SetFloat(parameterName, MuteDbLevel);
        }
        else
        {
            // �~���[�g�����������^�C�~���O�Ō��݂̃X���C�_�[�l���擾���Đݒ�
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
    /// 0.0~1.0 �̐��`�{�����[�����f�V�x���ɕϊ�
    /// </summary>
    private float LinearToDecibel(float linear)
    {
        // �{�����[����0����Log10�ŃG���[�ɂȂ邽��0.0001f��Clamp
        return Mathf.Log10(Mathf.Clamp(linear, 0.0001f, 1f)) * 20f;
    }
}