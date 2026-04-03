using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using AYellowpaper.SerializedCollections;
using TapNPlay.Core.Data;

namespace TapNPlay.Core.SaveSystem
{
    #region  MANAGER
    public static class LevelDataSystem
    {
        public const string Key = "LevelData";
        [Serializable]
        public struct LevelData
        {
            public int CurrentLevelNumber;
        }
        public static void Save(LevelData data)
        {
            ES3.Save(Key, data);
        }
        public static LevelData Load()
        {
            if (ES3.KeyExists(Key))
                return ES3.Load<LevelData>(Key);
            var defaultData = new LevelData { CurrentLevelNumber = 1 };
            Save(defaultData);
            return defaultData;
        }
#if UNITY_EDITOR
        [MenuItem("TapnPlay/ClearData/CurrentLevel")]
        public static void Clear()
        {
            ES3.DeleteKey(Key);
            Debug.Log($"ClearCurrentLevelData: Deleted {Key} - Exists now: {ES3.KeyExists(Key)}");
        }

        [MenuItem("TapnPlay/SetLevel/Set Current Level...")]
        public static void OpenSetLevelWindow() => SetLevelWindow.Open();

        private class SetLevelWindow : EditorWindow
        {
            private int _targetLevel = 1;

            public static void Open()
            {
                var window = GetWindow<SetLevelWindow>(true, "Set Current Level", true);
                window.minSize = new Vector2(260, 90);
                window.maxSize = new Vector2(260, 90);
                window._targetLevel = Load().CurrentLevelNumber;
                window.ShowUtility();
            }

            private void OnGUI()
            {
                EditorGUILayout.Space(8);
                EditorGUILayout.LabelField("Current Level Number", EditorStyles.boldLabel);
                _targetLevel = EditorGUILayout.IntField("Level", _targetLevel);
                EditorGUILayout.Space(6);

                if (GUILayout.Button("Set"))
                {
                    var data = Load();
                    data.CurrentLevelNumber = _targetLevel;
                    Save(data);
                    Debug.Log($"SetCurrentLevel: CurrentLevelNumber set to {_targetLevel}");
                    Close();
                }
            }
        }
#endif
    }

    public static class SettingsDataSystem
    {
        public const string Key = "SettingsData";
        [Serializable]
        public struct SettingsData
        {
            public bool Sound_On;
            public bool BGM_On;
            public bool Haptic_On;
            public float SoundValue;
            public float BGMValue;
            public SettingsData(bool soundOn = true, bool bgmOn = true, bool hapticOn = true, float soundValue = 1, float bgmValue = 1)
            {
                Sound_On = soundOn;
                BGM_On = bgmOn;
                Haptic_On = hapticOn;
                SoundValue = soundValue;
                BGMValue = bgmValue;
            }
        }
        public static void Save(SettingsData data)
        {
            ES3.Save(Key, data);
        }
        public static SettingsData Load()
        {
            if (ES3.KeyExists(Key))
                return ES3.Load<SettingsData>(Key);
            var defaultData = new SettingsData();
            Save(defaultData);
            return defaultData;
        }
#if UNITY_EDITOR
        [MenuItem("TapnPlay/ClearData/Settings")]
        public static void Clear()
        {
            ES3.DeleteKey(Key);
            Debug.Log($"ClearSettingsData: Deleted {Key} - Exists now: {ES3.KeyExists(Key)}");
        }
#endif
    }

    public static class CurrencyDataSystem
    {
        public const string Key = "CurrencyData";
        [Serializable]
        public struct CurrencyData
        {
            public SerializedDictionary<Currency, int> CurrencyValues;
        }
        public static void Save(CurrencyData data)
        {
            ES3.Save(Key, data);
        }
        public static CurrencyData Load()
        {
            if (ES3.KeyExists(Key))
                return ES3.Load<CurrencyData>(Key);
            var defaultData = new CurrencyData { CurrencyValues = new SerializedDictionary<Currency, int>() };
            Save(defaultData);
            return defaultData;
        }
#if UNITY_EDITOR
        [MenuItem("TapnPlay/ClearData/Currency")]
        public static void Clear()
        {
            ES3.DeleteKey(Key);
            Debug.Log($"ClearCurrencyData: Deleted {Key} - Exists now: {ES3.KeyExists(Key)}");
        }
#endif
    }
    #endregion

    #region ALL DATA CLEARER
    public static class ClearDataManager
    {
#if UNITY_EDITOR
        [MenuItem("TapnPlay/ClearData/ClearAll")]
        public static void ClearAllData()
        {
            LevelDataSystem.Clear();
            SettingsDataSystem.Clear();
            CurrencyDataSystem.Clear();
            Debug.Log("Cleared all main save data keys.");
        }
#endif
    }
    #endregion
}