using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioListData", menuName = "TapnPlay/Create/AudioList")]
public class AudioListData : ScriptableObject
{
    public SerializedDictionary<AudioID, Sound> soundDictionary = new SerializedDictionary<AudioID, Sound>();
}
