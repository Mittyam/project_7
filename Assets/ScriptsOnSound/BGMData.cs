// SoundDATA.cs ‚ÌƒJƒeƒSƒŠ[•ªŠ„‘Î‰
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BGMData", menuName = "Audio/BGMData")]
public class BGMData : ScriptableObject
{
    public List<AudioClip> bgmClips;

    public AudioClip GetClip(int index)
    {
        if (index >= 0 && index < bgmClips.Count)
        {
            return bgmClips[index];
        }
        Debug.LogWarning("BGM index out of range.");
        return null;
    }
}