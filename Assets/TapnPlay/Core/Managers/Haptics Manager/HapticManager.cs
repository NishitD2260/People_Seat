using TMPro;
using UnityEngine;

#if UNITY_IOS
using TapticPlugin;
#endif

public class HapticManager : SingletonBase<HapticManager>
{
    private bool canVibrate = false;

    #region UNITY_METHODS

    #endregion

    #region PUBLIC METHODS

    public void PlayHaptics(HapticsID id)
    {
        if (!canVibrate)
            return;

        switch (id)
        {
            case HapticsID.HAPTIC_LIGHT:
                VibrateLight();
                break;
            case HapticsID.HAPTIC_MEDIUM:
                VibrateMedium();
                break;
            case HapticsID.HAPTIC_HEAVY:
                VibrateHeavy();
                break;
            case HapticsID.HAPTIC_SUCCESS:
                VibrateSuccess();
                break;
            case HapticsID.HAPTIC_FAILURE:
                VibrateFailure();
                break;
        }
    }

    public void ToggleHaptic(bool isOn)
    {
        canVibrate = isOn;
    }

    public void PlayContinuousHaptics(HapticsID type, float duration, float interval = 0.05f)
    {
        StartCoroutine(ContinuousHapticsRoutine(duration, interval, type));
    }

    #endregion

    #region  PRIVATE METHODS

    private System.Collections.IEnumerator ContinuousHapticsRoutine(float duration, float interval, HapticsID type)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            PlayHaptics(type);
            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }
    }

    private void VibrateFailure()
    {
        if (canVibrate)
        {
#if UNITY_ANDROID
            Taptic.Failure();
#elif UNITY_IOS
            TapticManager.Impact(ImpactFeedback.Heavy);
#endif
        }
    }

    private void VibrateSuccess()
    {
        if (canVibrate)
        {
#if UNITY_ANDROID
            Taptic.Success();
#elif UNITY_IOS
            TapticManager.Impact(ImpactFeedback.Heavy);
#endif
        }
    }

    private void VibrateLight()
    {
        if (canVibrate)
        {
#if UNITY_ANDROID
            Taptic.Light();
#elif UNITY_IOS
            TapticManager.Impact(ImpactFeedback.Light);
#endif
        }
    }

    private void VibrateMedium()
    {
        if (canVibrate)
        {
#if UNITY_ANDROID
            Taptic.Medium();
#elif UNITY_IOS
            TapticManager.Impact(ImpactFeedback.Medium);
#endif
        }
    }

    private void VibrateHeavy()
    {
        if (canVibrate)
        {
#if UNITY_ANDROID
            Taptic.Heavy();
#elif UNITY_IOS
            TapticManager.Impact(ImpactFeedback.Heavy);
#endif
        }
    }

    #endregion

}
