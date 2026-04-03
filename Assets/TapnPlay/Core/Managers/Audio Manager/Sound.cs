using UnityEngine;

[CreateAssetMenu(menuName = "Create/Sound")]
public class Sound : ScriptableObject
{
    public AudioClip Clip;
    public bool loop = false;
    public bool IsRandom;
    [Tooltip("Only if its random")]
    public AudioClip[] Clips;

    public AudioClip GetRandomClip()
    {
        return Clips[Random.Range(0, Clips.Length)];
    }

    [Range(0f, 1f)]
    public float volume = .75f;

    [Range(.1f, 3f)]
    public float pitch = 1f;




}