using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using TapNPlay.Core.Data;
using TapNPlay.Core.SaveSystem;
using MEC;
using System.Collections.Generic;
using System.Linq;

namespace TapNPlay.Core.Progression
{
    public class UnlockableProgression : MonoBehaviour
    {
        [SerializeField] private UnlockableData unlockableData;
        public int LastProgressionLevelNumber =>
            unlockableData?.Unlockables != null && unlockableData.Unlockables.Count > 0
            ? unlockableData.Unlockables[^1].UnlockLevel
            : 1;

        [SerializeField] private GameObject fillerPanel;
        [SerializeField] private Image bgIconImage;
        [SerializeField] private Image iconFillerImage;
        [SerializeField] private TextMeshProUGUI featureText;
        [SerializeField] private TextMeshProUGUI percentageText;
        [SerializeField] private float enablingDelay = 0.5f;

        [Header("Tween settings")]
        [SerializeField] private float fillingDuration = 0.5f;
        [SerializeField] private Ease fillerEase = Ease.Linear;

        private Unlockable currentUnlockable;
        private float tweenTargetFill;
        private int tweenTargetPercent;
        private CoroutineHandle _fillerRoutineHandle;

        #region PUBLIC

        public void PlayVictoryStart()
        {
            AudioManager.Instance.PlayAudio(AudioID.Victory_Start);
        }
        public void PlayVictoryFillBar()
        {
            AudioManager.Instance.PlayAudio(AudioID.Victory_FillBar);
        }
        public void PlayVictoryCoin()
        {
            AudioManager.Instance.PlayAudio(AudioID.Victory_Coin);
        }

        #endregion

        #region PRIVATE

        public int SetupAndSave()
        {
            int currentLevel = LevelManager.Instance.currentLevelNo;

            // Safety: unlockable list must exist
            if (unlockableData == null || unlockableData.Unlockables == null || unlockableData.Unlockables.Count == 0)
                return 0;

            // Find next unlockable (the one we are currently progressing toward)
            currentUnlockable = unlockableData.Unlockables
                .Where(x => x.UnlockLevel > currentLevel)
                .OrderBy(x => x.UnlockLevel)
                .FirstOrDefault();

            // If nothing to unlock (we are past last), just return
            if (currentUnlockable == null)
                return 0;

            int nextUnlockLevel = currentUnlockable.UnlockLevel;

            // Find previous unlock level (highest unlock < nextUnlockLevel).
            // If none found (first segment), previousUnlockLevel = 0 -> startLevel will become 1.
            int previousUnlockLevel = unlockableData.Unlockables
                .Where(x => x.UnlockLevel < nextUnlockLevel)
                .Select(x => x.UnlockLevel)
                .DefaultIfEmpty(0)
                .Max();

            // Determine segment start and end per your rule:
            // - For first segment (previousUnlockLevel == 0) start at level 1
            // - For subsequent segments start at previousUnlockLevel (inclusive)
            int startLevel = previousUnlockLevel == 0 ? 1 : previousUnlockLevel;
            int endLevel = nextUnlockLevel - 1;

            Debug.Log("startLevel: " + startLevel + " endLevel: " + endLevel);

            int totalLevelsInSegment = endLevel - startLevel + 1; // should be > 0 normally

            // -------------------------------------------------------
            // SET UI SPRITES
            // -------------------------------------------------------
            bgIconImage.sprite = currentUnlockable.BgIcon;
            iconFillerImage.sprite = currentUnlockable.IconFiller;
            featureText.text = currentUnlockable.UnlockableName;

            // -------------------------------------------------------
            // CALCULATE TARGET FILL (0..1) BASED ON RULE
            // -------------------------------------------------------
            float targetFill = 0f;

            // If totalLevelsInSegment is invalid (defensive), treat as full
            if (totalLevelsInSegment <= 0)
            {
                targetFill = 1f;
            }
            else
            {
                // If we are at or past the unlock level, target is full (we'll animate to full and then show)
                if (currentLevel >= nextUnlockLevel)
                {
                    targetFill = 1f;
                }
                else
                {
                    // Normal in-segment calculation:
                    // progress = (currentLevel - startLevel + 1) / totalLevelsInSegment
                    targetFill = (float)(currentLevel - startLevel + 1) / (float)totalLevelsInSegment;
                    targetFill = Mathf.Clamp01(targetFill);
                }
            }

            // convert to percent 0..100
            int targetPercent = Mathf.RoundToInt(targetFill * 100f);

            // set immediate UI values (these are the baseline values the tween will animate from)
            // If you prefer the bar to appear from zero every time, set these to 0 instead.
            iconFillerImage.fillAmount = (currentLevel - startLevel) / (float)totalLevelsInSegment;
            iconFillerImage.fillAmount = Mathf.Clamp01(iconFillerImage.fillAmount); // keep current or 0
            percentageText.text = Mathf.RoundToInt(iconFillerImage.fillAmount * 100f) + "%";

            // Save tween target values used by FillerRoutine
            tweenTargetFill = targetFill;       // float 0..1
            tweenTargetPercent = targetPercent; // int 0..100

            return targetPercent;

            // Note: DO NOT call DisplayUnlockablePanel() here.
            // The panel will be shown after the fill tween completes in FillerRoutine().
        }

        private void StartFilling()
        {
            // Recompute targets on each start and ensure only one active fill routine/tween set.
            SetupAndSave();
            Timing.KillCoroutines(_fillerRoutineHandle);
            if (iconFillerImage != null) iconFillerImage.DOKill();
            if (percentageText != null) percentageText.DOKill();
            _fillerRoutineHandle = Timing.RunCoroutine(FillerRoutine(enablingDelay));
        }

        // FIELDS: add these to your class if not present already
        // private float tweenTargetFill;      // target fill amount 0..1
        // private int tweenTargetPercent;     // target percent 0..100

        private IEnumerator<float> FillerRoutine(float delay)
        {
            yield return Timing.WaitForSeconds(delay);
            _fillerRoutineHandle = default;

            // Determine current displayed percent to tween FROM
            float currentFill = iconFillerImage != null ? iconFillerImage.fillAmount : 0f;
            float currentPercent = currentFill * 100f;

            // Safeguard: ensure tween target values are valid
            float targetPercentF = Mathf.Clamp(tweenTargetPercent, 0, 100);
            float targetFillF = Mathf.Clamp01(tweenTargetFill);

            Debug.Log("currentPercent: " + currentPercent + " targetPercentF: " + targetPercentF);

            // ----------------------------------------------
            // TWEEN PERCENT DISPLAY: from currentPercent -> targetPercentF
            // ----------------------------------------------
            DOTween.To(() => currentPercent, x =>
            {
                currentPercent = x;
                percentageText.text = Mathf.RoundToInt(currentPercent) + "%";
            },
            targetPercentF,
            fillingDuration)
            .OnUpdate(() =>
            {
                // final rounding fix so 98+ becomes 100 for UI readability
                if (currentPercent >= 98f)
                    percentageText.text = "100%";
            })
            .OnComplete(() =>
            {
                if (currentPercent == 100f)
                {
                    DOVirtual.DelayedCall(0.25f, () =>
                    {
                        percentageText.DOFade(0f, 0.25f).SetEase(Ease.Linear).OnComplete(() =>
                        {
                        });
                    });
                }
            });

            // ----------------------------------------------
            // TWEEN FILL AMOUNT: from currentFill -> targetFillF
            // ----------------------------------------------
            iconFillerImage
                .DOFillAmount(targetFillF, fillingDuration)
                .SetEase(fillerEase)
                .OnUpdate(() =>
                {
                    Debug.Log("iconFillerImage.fillAmount: " + iconFillerImage.fillAmount);
                })
                .OnComplete(() =>
                {
                    // Only after the tween completes do we show the unlock panel IF we've reached full
                    if (Mathf.Approximately(iconFillerImage.fillAmount, 1f))
                    {
                        AudioManager.Instance.PlayAudio(AudioID.Feature_Unlock);
                        DisplayUnlockablePanel();
                        percentageText.transform
                            .DOScale(0f, 0.25f)
                            .SetEase(Ease.Linear);
                    }
                });
        }

        private void DisplayUnlockablePanel()
        {
            EventController.TriggerEvent(GameEvent.EVENT_UNLOCKABLE_UPDATED, currentUnlockable);
            fillerPanel.SetActive(false);
        }

        #endregion
    }
}