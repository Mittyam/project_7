using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SEData", menuName = "Audio/SEData")]
public class SEData : ScriptableObject
{
    public List<AudioClip> seClips;

    public AudioClip GetClip(int index)
    {
        if (index >= 0 && index < seClips.Count)
        {
            return seClips[index];
        }
        Debug.LogWarning("SE index out of range.");
        return null;
    }
}