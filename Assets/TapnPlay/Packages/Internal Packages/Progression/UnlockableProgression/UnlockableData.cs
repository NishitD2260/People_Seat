using System;
using System.Collections.Generic;
using UnityEngine;

namespace TapNPlay.Core.Progression
{
    [CreateAssetMenu(fileName = "Unlockable Data", menuName = "TapnPlay/Data/Unlockable Data")]
    public class UnlockableData : ScriptableObject
    {
        public List<Unlockable> Unlockables;
    }

    [Serializable]
    public class Unlockable
    {
        public string UnlockableName;
        public DisplayType DisplayType;
        public Sprite BgIcon;
        public Sprite IconFiller;
        public Sprite UnlockableIcon;
        public Mesh Mesh;
        public int UnlockLevel;
    }

    public enum DisplayType
    {
        IMAGE = 0,
        RENDER_IMAGE = 1
    }
}