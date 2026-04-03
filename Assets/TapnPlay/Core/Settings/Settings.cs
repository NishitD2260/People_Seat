using UnityEngine;
using TapNPlay.Core.SaveSystem;
using TapNPlay.Core.Data;
using Lean.Gui;

public class Settings : MonoBehaviour
{

    [SerializeField] private SettingsDataSystem.SettingsData settingsData;

    [SerializeField] LeanToggle soundToggle;
    [SerializeField] LeanToggle musicToggle;
    [SerializeField] LeanToggle hapticToggle;
    private bool sound;
    private bool music;
    private bool haptic;

    #region UNITY METHODS

    void Start()
    {
        LoadData();
    }

    #endregion

    #region  PUBLIC

    public void ToggleSound(bool isOn)
    {
        sound = isOn;
        if (AudioManager.Instance)
        {
            AudioManager.Instance.PlayAudio(AudioID.TAP);
        }

        if (HapticManager.Instance)
        {
            HapticManager.Instance.PlayHaptics(HapticsID.HAPTIC_HEAVY);
        }

        AudioManager.Instance.ToggleSound(sound);
        settingsData.Sound_On = sound;
        SaveData();
    }

    public void ToggleMusic(bool isOn)
    {
        music = isOn;
        AudioManager.Instance.ToggleMusic(music);
        settingsData.BGM_On = music;
        SaveData();
    }

    public void ToggleHaptic(bool isOn)
    {
        haptic = isOn;
        HapticManager.Instance.ToggleHaptic(haptic);
        // settingsPanel.HapticToggle(haptic);
        settingsData.Haptic_On = haptic;
        SaveData();
    }

    #endregion

    #region  PRIVATE

    public void SaveData()
    {
        SettingsDataSystem.Save(settingsData);
    }

    private void LoadData()
    {
        settingsData = SettingsDataSystem.Load();
        sound = settingsData.Sound_On;
        music = settingsData.BGM_On;
        haptic = settingsData.Haptic_On;
        soundToggle.On = sound;
        musicToggle.On = music;
        hapticToggle.On = haptic;
        AudioManager.Instance.ToggleMusic(music);
        AudioManager.Instance.ToggleSound(sound);
        HapticManager.Instance.ToggleHaptic(haptic);
    }

    #endregion
}
