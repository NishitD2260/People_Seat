using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using MEC;
using UnityEngine;

public class AudioManager : SingletonBase<AudioManager>
{
    public AudioListData AudioListData;
    //[SerializeField] private SerializedDictionary<AudioID, Sound> soundDictionary = new SerializedDictionary<AudioID, Sound>();
    [SerializeField] private AudioSource audioSourcePrefab;
    [SerializeField] private int poolSize = 10;

    private Queue<AudioSource> audioSourcePool = new Queue<AudioSource>();
    public AudioSource Bg;
    private bool isSfxEnabled = true;

    protected override void Awake()
    {
        base.Awake();

        for (int i = 0; i < poolSize; i++)
        {
            AudioSource source = Instantiate(audioSourcePrefab, transform);
            source.playOnAwake = true;
            source.gameObject.SetActive(false);
            audioSourcePool.Enqueue(source);
        }
    }

    private AudioSource GetPooledAudioSource()
    {
        if (audioSourcePool.Count > 0)
        {
            AudioSource source = audioSourcePool.Dequeue();
            source.gameObject.SetActive(true);
            return source;
        }
        else
        {
            Debug.LogWarning("AudioSource Pool exhausted! Consider increasing pool size.");
            return Instantiate(audioSourcePrefab, transform);
        }
    }

    private void ReturnAudioSourceToPool(AudioSource source)
    {
        source.Stop();
        source.clip = null;
        source.gameObject.SetActive(false);
        audioSourcePool.Enqueue(source);
    }

    public void PlayAudio(AudioID id)
    {
        if (!isSfxEnabled)
            return;

        if (!AudioListData.soundDictionary.ContainsKey(id))
        {
            Debug.LogWarning("Sound ID: " + id + " not found!");
            return;
        }

        Sound sound = AudioListData.soundDictionary[id];
        AudioSource source = GetPooledAudioSource();
        source.gameObject.name = id.ToString();

        source.clip = sound.IsRandom ? sound.GetRandomClip() : sound.Clip;
        source.volume = sound.volume;
        source.pitch = sound.pitch;

        source.Play();
        Timing.RunCoroutine(ReturnAfterPlaying(source));
    }

    private IEnumerator<float> ReturnAfterPlaying(AudioSource source)
    {
        yield return Timing.WaitForSeconds(source.clip.length);
        ReturnAudioSourceToPool(source);
    }

    public void ToggleSound(bool isOn)
    {
        isSfxEnabled = isOn;
    }

    public void ToggleMusic(bool isOn)
    {
        if (Bg != null)
        {
            Bg.mute = !isOn;
        }
    }

    public void SetSFXVolume(float value)
    {
        foreach (var source in audioSourcePool)
        {
            source.volume = value;
        }
    }

    public void SetMusicVolume(float value)
    {
        if (Bg != null)
        {
            Bg.volume = value;
        }
    }
}
