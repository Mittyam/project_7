using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
    [Header("Audio Sources")]
    public AudioSource bgmSource; // BGM���Đ�����AudioSource
    public AudioSource voiceSource; // Voice���Đ�����AudioSource
    public List<AudioSource> seSources; // SE���Đ�����AudioSource�̃��X�g�i������SE�𓯎��Đ��\�j

    [Header("Sound Data Objects")]
    public BGMData bgmData; // BGM�̃f�[�^���i�[����I�u�W�F�N�g
    public SEData seData; // SE�̃f�[�^���i�[����I�u�W�F�N�g
    public VoiceData voiceData; // Voice�̃f�[�^���i�[����I�u�W�F�N�g
    public MainVoiceData mainVoiceData; // MainVoice�̃f�[�^���i�[����I�u�W�F�N�g

    // �Đ��I�����̃C�x���g�i��Ƃ���Voice�p�j
    public event Action<int> OnVoiceFinished; // �����Ƃ��čĐ�����Voice��index��Ԃ�


    // BGM���Đ�����
    public void PlayBGM(int index, bool loop = true)
    {
        AudioClip clip = bgmData.GetClip(index);

        if (clip != null)
        {
            bgmSource.clip = clip;
            bgmSource.loop = loop; // ���[�v�Đ��̐ݒ�
            bgmSource.Play();
        }
    }


    // BGM���~����
    public void StopBGM()
    {
        bgmSource.Stop();
    }


    // SE���Đ�����
    public void PlaySE(int index)
    {
        AudioClip clip = seData.GetClip(index);

        if (clip != null)
        {
            // �Đ��\��AudioSource��T��
            AudioSource availableSource = seSources.Find(source => !source.isPlaying);
            if (availableSource != null)
            {
                availableSource.clip = clip;
                availableSource.Play();
            }
            else
            {
                Debug.LogWarning("�g�p�\��SE��AudioSource������܂���B");
            }
        }
    }


    // �����SE���~����
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


    // �S�Ă�SE���~����
    public void StopAllSE()
    {
        foreach (var source in seSources)
        {
            source.Stop();
        }
    }


    // Voice���Đ�����
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
    /// Voice���Đ����A�Đ��I�����ɃR�[���o�b�N�܂��̓C�x���g�𔭉΂���B
    /// </summary>
    public void PlayVoiceWithCallback(int index, Action callback = null)
    {
        AudioClip clip = voiceData.GetClip(index);
        if (clip != null)
        {
            // �ʏ��Voice�Đ�����
            voiceSource.clip = clip;
            voiceSource.Play();

            // �Đ��I�������m����R���[�`�����J�n
            StartCoroutine(VoiceCompleteRoutine(index, clip, callback));
        }
    }


    /// <summary>
    /// Voice�Đ����I�������Ƃ��ɌĂяo�����R���[�`���B
    /// </summary>
    private IEnumerator VoiceCompleteRoutine(int index, AudioClip clip, Action callback)
    {
        // AudioSource�̍Đ����Ԃɍ��킹�đҋ@�i�V�[�N�o�[���œr���Œ�~���ꂽ�ꍇ�̍l���͕ʓr�����j
        yield return new WaitForSeconds(clip.length);

        // �C�x���g���o�^����Ă���Δ���
        OnVoiceFinished?.Invoke(index);

        // �R�[���o�b�N���w�肳��Ă���Ύ��s
        callback?.Invoke();
    }

    // MainVoice���Đ�����
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
    /// MainVoice���Đ����A�Đ��I�����ɃR�[���o�b�N�܂��̓C�x���g�𔭉΂���B
    /// </summary>
    public void PlayMainVoiceWithCallback(int index, Action callback = null)
    {
        AudioClip clip = mainVoiceData.GetClip(index);
        if (clip != null)
        {
            // �ʏ��Voice�Đ�����
            voiceSource.clip = clip;
            voiceSource.Play();

            // �Đ��I�������m����R���[�`�����J�n
            StartCoroutine(MainVoiceCompleteRoutine(index, clip, callback));
        }
    }


    /// <summary>
    /// MainVoice�Đ����I�������Ƃ��ɌĂяo�����R���[�`���B
    /// </summary>
    private IEnumerator MainVoiceCompleteRoutine(int index, AudioClip clip, Action callback)
    {
        // AudioSource�̍Đ����Ԃɍ��킹�đҋ@�i�V�[�N�o�[���œr���Œ�~���ꂽ�ꍇ�̍l���͕ʓr�����j
        yield return new WaitForSeconds(clip.length);

        // �C�x���g���o�^����Ă���Δ���
        OnVoiceFinished?.Invoke(index);

        // �R�[���o�b�N���w�肳��Ă���Ύ��s
        callback?.Invoke();
    }

    // BGM�p�Ƀt�F�[�h�C���E�t�F�[�h�A�E�g��g�ݍ��킹����
    public void PlayBGMWithFadeIn(int index, float fadeDuration, Action callback = null, bool loop = true)
    {
        AudioClip clip = bgmData.GetClip(index);
        if (clip != null)
        {
            bgmSource.clip = clip;
            // �t�F�[�h�C���J�n�i���̖ڕW���ʂ�ێ����Ă����K�v������̂ŁA�����ł͗�Ƃ���1.0f��ڕW�Ƃ���j
            StartCoroutine(FadeIn(bgmSource, fadeDuration, 1.0f, loop));
            // �K�v�ł���΍Đ�������̃R�[���o�b�N���R���[�`���ŊǗ����邱�Ƃ��\
            StartCoroutine(BGMCompleteRoutine(index, clip, callback));
        }
    }

    // BGM���t�F�[�h�A�E�g�����Ē�~����
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
        // �C�x���g�E�R�[���o�b�N�̔��Ώ����iBGM�ɂ��C�x���g���K�v�Ȃ�ʂ̃C�x���g��p�Ӂj
        callback?.Invoke();
    }


    // Voice���~����
    public void StopVoice()
    {
        voiceSource.Stop();
    }

    // MainVoice���~����
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


    // BGM�̒������擾
    public float GetBGMLength(int index)
    {
        AudioClip clip = bgmData.GetClip(index);
        if (clip != null)
        {
            Debug.Log($"BGM[{index}]�̒���: {clip.length}�b");
            return clip.length;
        }
        Debug.LogWarning("�w�肳�ꂽBGM��������܂���B");
        return 0f;
    }


    // SE�̒������擾
    public float GetSELength(int index)
    {
        AudioClip clip = seData.GetClip(index);
        if (clip != null)
        {
            Debug.Log($"SE[{index}]�̒���: {clip.length}�b");
            return clip.length;
        }
        Debug.LogWarning("�w�肳�ꂽSE��������܂���B");
        return 0f;
    }


    // Voice�̒������擾
    public float GetVoiceLength(int index)
    {
        AudioClip clip = voiceData.GetClip(index);
        if (clip != null)
        {
            Debug.Log($"Voice[{index}]�̒���: {clip.length}�b");
            return clip.length;
        }
        Debug.LogWarning("�w�肳�ꂽVoice��������܂���B");
        return 0f;
    }


    /// <summary>
    /// �w�肵��AudioSource�̉��ʂ��t�F�[�h�C�������Ȃ���Đ�����
    /// </summary>
    private IEnumerator FadeIn(AudioSource source, float fadeDuration, float targetVolume, bool loop)
    {
        source.volume = 0f;
        source.loop = loop; // ���[�v�Đ��̐ݒ�
        source.Play();
        float time = 0f;
        while (time < fadeDuration)
        {
            // ���`�⊮�ŉ��ʂ𑝉�
            source.volume = Mathf.Lerp(0f, targetVolume, time / fadeDuration);
            time += Time.deltaTime;
            yield return null;
        }
        source.volume = targetVolume; // �ŏI�I�ȉ��ʂ�ݒ�
    }


    /// <summary>
    /// �w�肵��AudioSource�̉��ʂ��t�F�[�h�A�E�g�����A�I����ɒ�~����B
    /// </summary>
    private IEnumerator FadeOut(AudioSource source, float fadeDuration)
    {
        float startVolume = source.volume;
        float time = 0f;
        while (time < fadeDuration)
        {
            // ���`��Ԃŉ��ʂ�����
            source.volume = Mathf.Lerp(startVolume, 0f, time / fadeDuration);
            time += Time.deltaTime;
            yield return null;
        }
        source.volume = 0f;
        source.Stop();
    }
}