using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TapNPlay.Core.Managers;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class SkipTutorialStepButton : MonoBehaviour
{
    [SerializeField] GameObject fillParent;
    [SerializeField] Image fillImage;
    [SerializeField] RectTransform TapToContinueText;
    float timeToSkip = 0;

    Coroutine skipTutorialStepCoroutine;
    [SerializeField] Button SkipButton;

    [Header("TapToContinueText animation")]
    [SerializeField] float tapTextScaleUpDuration = 0.15f;
    [SerializeField] float tapTextMinScaleMultiplier = 0.85f;
    [SerializeField] float tapTextPulseDuration = 0.4f;

    Vector3 tapTextMaxScale = Vector3.one;
    Tween tapTextTween;
    bool lockSkipTutorialStep;

    public void StartSkipTutorialStepCoroutine(float timeToSkip)
    {
        if (skipTutorialStepCoroutine != null)
            StopCoroutine(skipTutorialStepCoroutine);

        tapTextTween?.Kill();
        tapTextTween = null;

        this.timeToSkip = timeToSkip;
        SkipButton.interactable = false;
        lockSkipTutorialStep = true;

        if (TapToContinueText != null)
        {
            tapTextMaxScale = TapToContinueText.localScale;
            TapToContinueText.gameObject.SetActive(true);
            TapToContinueText.localScale = Vector3.zero; // countdown phase: do not animate
        }

        skipTutorialStepCoroutine = StartCoroutine(SkipTutorialStepCoroutine());
    }

    IEnumerator SkipTutorialStepCoroutine()
    {
        if (fillParent != null)
            fillParent.SetActive(true);

        if (fillImage != null)
            fillImage.fillAmount = 1f;

        // Edge case: immediate skip when time is 0/negative.
        if (timeToSkip <= 0f)
        {
            if (fillImage != null)
                fillImage.fillAmount = 0f;
            if (fillParent != null)
                fillParent.SetActive(false);

            SkipButton.interactable = true;
            lockSkipTutorialStep = false;
            if (TapToContinueText != null)
            {
                TapToContinueText.gameObject.SetActive(false);
                TapToContinueText.localScale = tapTextMaxScale;
            }
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < timeToSkip)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / timeToSkip);

            if (fillImage != null)
                fillImage.fillAmount = Mathf.Lerp(1f, 0f, t);

            yield return null;
        }

        if (fillImage != null)
            fillImage.fillAmount = 0f;
        if (fillParent != null)
            fillParent.SetActive(false);

        SkipButton.interactable = true;
        lockSkipTutorialStep = false;

        // After countdown: play the animation, then disable the text.
        if (TapToContinueText != null)
        {
            TapToContinueText.gameObject.SetActive(true);
            TapToContinueText.localScale = Vector3.zero;

            Vector3 minScale = tapTextMaxScale * tapTextMinScaleMultiplier;
            tapTextTween = TapToContinueText.DOScale(tapTextMaxScale, tapTextScaleUpDuration)
                .SetEase(Ease.OutSine)
                .OnComplete(() =>
                {
                    // After scaling up once, keep looping between max (current) and min.
                    tapTextTween = TapToContinueText.DOScale(minScale, tapTextPulseDuration * 0.5f)
                        .SetEase(Ease.InOutSine)
                        .SetLoops(-1, LoopType.Yoyo);
                });
        }
    }

    public void SkipTutorialStep()
    {
        if (lockSkipTutorialStep)
            return;

        tapTextTween?.Kill();
        tapTextTween = null;
        if (TapToContinueText != null)
        {
            TapToContinueText.DOKill();
            TapToContinueText.gameObject.SetActive(false);
        }

        if (skipTutorialStepCoroutine != null)
            StopCoroutine(skipTutorialStepCoroutine);

        if (TutorialManager.Instance.CurrentTutorialData != null && TutorialManager.Instance.CurrentTutorialData.Steps[TutorialManager.Instance.stepIndex].tutorial_Step_Settings.step_Type == Tutorial_Step_Type.waitTap)
        {
            EventController.TriggerEvent(GameEvent.EVENT_TUTORIAL_NEXT_STEP);
        }
    }
}
