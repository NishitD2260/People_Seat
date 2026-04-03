using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    [SerializeField] private List<RectTransform> buttonRectTransformList;

    [Header("Music Settings")]
    [SerializeField] private Image musicImage;
    [SerializeField] private Sprite musicOnSprite;
    [SerializeField] private Sprite musicOffSprite;

    [Header("Sound Settings")]
    [SerializeField] private Image soundImage;
    [SerializeField] private Sprite soundOnSprite;
    [SerializeField] private Sprite soundOffSprite;

    [Header("Haptics Settings")]
    [SerializeField] private Image hapticsImage;
    [SerializeField] private Sprite hapticsOnSprite;
    [SerializeField] private Sprite hapticsOffSprite;

    [Header("Button tween Settings")]
    [SerializeField] private float xOffsetValue = 41.7f;
    [SerializeField] private float buttonTweenDuration;
    [SerializeField] private float buttonDelay;
    [SerializeField] private Ease buttonTweenEase;

    private bool settingButtonToggle = false;
    private bool isButtonTransitioning = false;
    private Tween buttonTween;

    public void ToggleSettings()
    {
        if (isButtonTransitioning)
            return;

        // if (AudioManager.Instance)
        // {
        //     AudioManager.Instance.PlayAudio(AudioID.Settings_Toggle_Icons);
        // }

        settingButtonToggle = !settingButtonToggle;
        ToggleButtons(settingButtonToggle);
    }

    public void ToggleButtons(bool isEnabled)
    {
        StartCoroutine(ToggleButtonRoutine(isEnabled));
    }

    private IEnumerator ToggleButtonRoutine(bool isEnabled)
    {
        isButtonTransitioning = true;

        float offset = isEnabled ? -xOffsetValue : xOffsetValue;

        for (int i = 0; i < buttonRectTransformList.Count; i++)
        {
            RectTransform rect = buttonRectTransformList[i];

            if (isEnabled)
            {
                rect.gameObject.SetActive(true);
            }

            buttonTween = rect.DOAnchorPosX(offset, buttonTweenDuration)
                .SetEase(buttonTweenEase)
                .SetDelay(i * buttonDelay).OnComplete(() =>
                {
                    if (!isEnabled)
                    {
                        rect.gameObject.SetActive(false);
                    }
                });
        }

        yield return buttonTween.WaitForCompletion();
        isButtonTransitioning = false;
    }

    public void SoundToggle(bool isOn)
    {
        // if (AudioManager.Instance)
        // {
        //     AudioManager.Instance.PlayAudio(AudioID.Settings_Toggle_Icons);
        // }
        soundImage.sprite = isOn ? soundOnSprite : soundOffSprite;
    }

    public void MusicToggle(bool isOn)
    {
        // if (AudioManager.Instance)
        // {
        //     AudioManager.Instance.PlayAudio(AudioID.Settings_Toggle_Icons);
        // }
        // musicImage.sprite = isOn ? musicOnSprite : musicOffSprite;
    }

    public void HapticToggle(bool isOn)
    {
        // if (AudioManager.Instance)
        // {
        //     AudioManager.Instance.PlayAudio(AudioID.Settings_Toggle_Icons);
        // }
        hapticsImage.sprite = isOn ? hapticsOnSprite : hapticsOffSprite;
    }

    public void LoadSettingsData(bool sound, bool music, bool haptic)
    {
        musicImage.sprite = music ? musicOnSprite : musicOffSprite;
        soundImage.sprite = sound ? soundOnSprite : soundOffSprite;
        hapticsImage.sprite = haptic ? hapticsOnSprite : hapticsOffSprite;
    }

}
