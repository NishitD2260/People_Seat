using System;
using System.Collections.Generic;
using TapNPlay.Core.Managers;
using TapNPlay.Core.SaveSystem;
using UnityEngine;

public class LevelManager : SingletonBase<LevelManager>
{
    [SerializeField] LevelsListSO StartLevels;
    [SerializeField] LevelsListSO loopLevelList;

    private LevelDataSystem.LevelData levelData;
    public int currentLevelNo
    {
        get => levelData.CurrentLevelNumber;
        set { levelData.CurrentLevelNumber = value; }
    }
    [SerializeField] private LevelSO currentLevelData;

    #region  UNITY METHODS

    private void Start()
    {
        LoadLevelData();
    }

    private void OnEnable()
    {
        EventController.StartListening(GameEvent.EVENT_NEXT_BUTTON_CLICKED, NextLevel);
    }

    private void OnDisable()
    {
        EventController.StopListening(GameEvent.EVENT_NEXT_BUTTON_CLICKED, NextLevel);
    }

    #endregion

    #region PUBLIC METHODS

    public void NextLevel(object args)
    {
        currentLevelNo++;
        LevelDataSystem.Save(levelData);
        if (pluginScript.Instance)
            pluginScript.Instance.OnGameFinished(true, EconomyManager.Instance.GetBalance(Currency.PRIMARY), currentLevelNo - 1);
    }

    public LevelSO CurrentLevelData => currentLevelData;

    #endregion

    #region  CALLBACK FUNCTIONS

    private void LoadLevelData()
    {
        levelData = LevelDataSystem.Load();

        if (StartLevels == null || StartLevels.Levels == null || StartLevels.Levels.Count == 0)
        {
            Debug.LogError("FirstLevels.Levels is not set or empty. Returning null (no current level can be set).");
            currentLevelData = null;
        }
        else if (levelData.CurrentLevelNumber <= StartLevels.Levels.Count)
        {
            currentLevelData = StartLevels.Levels[Mathf.Clamp(levelData.CurrentLevelNumber - 1, 0, StartLevels.Levels.Count - 1)];
        }
        else if (loopLevelList == null || loopLevelList.Levels == null || loopLevelList.Levels.Count == 0)
        {
            Debug.LogError("loopLevelList.Levels is not set or empty. Returning first FirstLevel as fallback.");
            currentLevelData = StartLevels.Levels[0];
        }
        else
        {
            int loopLevelIndex = levelData.CurrentLevelNumber - StartLevels.Levels.Count - 1;
            int safeLoopIdx = ((loopLevelIndex % loopLevelList.Levels.Count) + loopLevelList.Levels.Count) % loopLevelList.Levels.Count; // safe modulo
            currentLevelData = loopLevelList.Levels[safeLoopIdx];
        }
        EventController.TriggerEvent(GameEvent.EVENT_SET_LEVEL_DATA, currentLevelData);
        EventController.TriggerEvent(GameEvent.EVENT_LEVEL_LOADED_NUMBER, levelData.CurrentLevelNumber);
        if (pluginScript.Instance)
            pluginScript.Instance.LevelStart(levelData.CurrentLevelNumber);
    }

    #endregion
}
