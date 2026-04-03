using System;

namespace TapNPlay.Core.Data
{
    [Serializable]
    public class TutorialSaveData
    {
        public int TutorialIndex;

        public TutorialSaveData(int index)
        {
            TutorialIndex = index;
        }
    }
}