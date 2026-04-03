using System;
using UnityEngine;
using System.Collections.Generic;

namespace TapNPlay.Core.Data
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "TapnPlay/Data/Level Data", order = 0)]
    public class LevelData : ScriptableObject
    {
        public List<Level> levels;
        public LoopType LoopType = LoopType.REGULAR;

        [Header("Regular Level Loop Data")]
        public int LoopLevelIndex;

        [Header("Batched Level Loop Data")]
        public List<BatchData> levelBatch;
        public int batchIndex = 0;
        public int batchElementIndex = 0;

        [Header("Pattern Levels Loop Data")]
        public List<Pattern> LevelPattern;
        public int patternIndex = 0;
        private List<int> patternLevels = new List<int>();
        private int MaxLevelIndex;

        private void OnEnable()
        {
            MaxLevelIndex = (levels != null && levels.Count > 0) ? levels.Count - 1 : 0;

            if (LoopType == LoopType.PATTERN && levels != null)
            {
                CreateFilterLevels();
            }
        }

        #region Level Loop Index
        public Level GetLevel(int levelNumber)
        {
            if (levels == null || levels.Count == 0)
            {
                Debug.LogWarning("Levels list is empty or null.");
                return null;
            }

            Level level = levels[0]; // Default to the first level
            switch (LoopType)
            {
                case LoopType.REGULAR:
                    level = GetValidLevelIndex(levelNumber);
                    break;
                case LoopType.BATCH:
                    level = GetBatchedLevel(levelNumber);
                    break;
                case LoopType.PATTERN:
                    level = GetPatternLevel(levelNumber);
                    break;
            }

            return level;
        }

        private Level GetValidLevelIndex(int levelNumber)
        {
            if (levels == null || levels.Count == 0)
            {
                Debug.LogWarning("Levels list is empty or null.");
                return null;
            }

            LoopLevelIndex = Mathf.Clamp(LoopLevelIndex, 0, MaxLevelIndex);

            if (levelNumber <= MaxLevelIndex + 1)
            {
                return levels[Mathf.Clamp(levelNumber - 1, 0, MaxLevelIndex)];
            }

            int rangeStart = LoopLevelIndex;
            int rangeEnd = MaxLevelIndex;
            int rangeCount = rangeEnd - rangeStart + 1;

            int index = (levelNumber - MaxLevelIndex - 1) % rangeCount + rangeStart;

            return levels[index];
        }


        private Level GetBatchedLevel(int levelNumber)
        {
            if (levelBatch == null || levelBatch.Count == 0) return GetValidLevelIndex(levelNumber);

            if (batchIndex >= levelBatch.Count) batchIndex = 0;
            BatchData currentBatch = levelBatch[batchIndex];

            if (currentBatch?.Batch == null || currentBatch.Batch.Count == 0)
            {
                Debug.LogWarning($"Batch at index {batchIndex} is null or empty.");
                return GetValidLevelIndex(levelNumber);
            }

            if (batchElementIndex >= currentBatch.Batch.Count)
            {
                batchIndex++;
                batchElementIndex = 0;
                if (batchIndex >= levelBatch.Count) batchIndex = 0;
            }

            int value = currentBatch.Batch[batchElementIndex];
            batchElementIndex++;
            Debug.Log("Value is {0}" + value.ToString());

            return (value >= 0 && value < levels.Count) ? levels[value] : GetValidLevelIndex(levelNumber);
        }

        private Level GetPatternLevel(int levelNumber)
        {
            if (patternLevels == null || patternLevels.Count == 0)
            {
                Debug.LogWarning("Pattern levels list is null or empty. Falling back to regular level logic.");
                return GetValidLevelIndex(levelNumber);
            }

            if (levelNumber <= MaxLevelIndex + 1)
            {
                return GetValidLevelIndex(levelNumber);
            }

            if (patternIndex >= patternLevels.Count)
            {
                patternIndex = 0;
            }

            int patternLevelIndex = patternLevels[patternIndex];
            patternIndex++;

            if (patternLevelIndex >= 0 && patternLevelIndex < levels.Count)
            {
                return levels[patternLevelIndex];
            }

            Debug.LogWarning($"Pattern level index {patternLevelIndex} is out of range. Falling back to regular level logic.");
            return GetValidLevelIndex(levelNumber);
        }

        #endregion

        public void CreateFilterLevels()
        {
            patternLevels.Clear();
            if (levels == null || LevelPattern == null)
            {
                Debug.LogWarning("Levels or LevelPattern is null.");
                return;
            }

            List<Level> duplicateLevels = new List<Level>(levels);

            foreach (Pattern pat in LevelPattern)
            {
                for (int i = 0; i < pat.Count; i++)
                {
                    Level level = duplicateLevels.Find(x => x.Type == pat.Type);
                    if (level != null)
                    {
                        patternLevels.Add(levels.IndexOf(level));
                        duplicateLevels.Remove(level);
                    }
                }
            }
        }
    }

    #region Necessary Capsules
    [Serializable]
    public class Level
    {
        public LevelType Type;
        public string textData;
        public List<CurrencyReward> currencyRewards;
        public List<BoosterReward> boosterRewards;
    }

    [Serializable]
    public class CurrencyReward
    {
        public Currency currency;
        public int amount;
    }

    [Serializable]
    public class BoosterReward
    {
        public Booster booster;
        public int amount;
    }

    [Serializable]
    public class BatchData
    {
        public List<int> Batch;
    }

    public enum LoopType
    {
        REGULAR = 1,
        BATCH = 2,
        PATTERN = 3
    }

    public enum LevelType
    {
        SIMPLE = 0,
        BASIC = 1,
        BONUS = 2,
        CHALLENGE = 3,
    }

    [Serializable]
    public class Pattern
    {
        public LevelType Type;
        public int Count;
    }
    #endregion
}
