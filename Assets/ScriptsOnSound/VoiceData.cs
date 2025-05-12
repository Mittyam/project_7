using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "VoiceData", menuName = "Audio/VoiceData")]
public class VoiceData : ScriptableObject
{
    public List<AudioClip> voiceClips;

    public AudioClip GetClip(int index)
    {
        if (index >= 0 && index < voiceClips.Count)
        {
            return voiceClips[index];
        }
        Debug.LogWarning("Voice index out of range.");
        return null;
    }
}