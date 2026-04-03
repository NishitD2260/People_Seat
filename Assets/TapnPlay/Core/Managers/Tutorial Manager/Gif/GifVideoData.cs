using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu(fileName = "Gif Video Data", menuName = "TapnPlay/Gif video Data")]
public class GifVideoData : ScriptableObject
{
    public List<GifData> GifDataList;
}

[Serializable]
public class GifData
{
    public string Title;
    public string Description;
    public Sprite Icon;
    public VideoClip GifClip;
    public Vector2 gifVideoPosition;
}