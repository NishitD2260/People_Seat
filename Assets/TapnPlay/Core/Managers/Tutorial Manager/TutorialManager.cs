using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TapNPlay.Core.Utilities;
using Coffee.UIExtensions;
using AYellowpaper.SerializedCollections;

namespace TapNPlay.Core.Managers
{
    public class TutorialManager : SingletonBase<TutorialManager>
    {
        [Header("Tutorial Data Reference")]
        [SerializeField] private List<TutorialDataSO> tutorialInfo;

        [SerializeField] SerializedDictionary<TutorialTextBoxSize, Vector2> textBoxSizeDictionary;

        [Header("Main Gameobject References")]
        public TutorialReferences tutorialReferences;

        [Header("Tween Settings")]
        [SerializeField] private TutorialTweenSO tutorialTweenSO;

        public TutorialDataSO CurrentTutorialData;
        public int stepIndex = 0;
        private int LevelNo;

        #region Unity

        private void OnEnable()
        {
            EventController.StartListening(GameEvent.EVENT_TUTORIAL_NEXT_STEP, updateStep);
            EventController.StartListening(GameEvent.EVENT_LEVEL_LOADED_NUMBER, OnStartTutorial);
        }

        private void OnDisable()
        {
            EventController.StopListening(GameEvent.EVENT_TUTORIAL_NEXT_STEP, updateStep);
            EventController.StopListening(GameEvent.EVENT_LEVEL_LOADED_NUMBER, OnStartTutorial);
        }

        private void Update()
        {
            if (CurrentTutorialData != null && stepIndex < CurrentTutorialData.Steps.Count)
            {
                CurrentTutorialData.OnStepUpdate(stepIndex);
            }
        }

        #endregion

        #region Public

        public void StartTutorial(int currentLevel)
        {
            tapPositions ??= new();
            tapPositions.Clear();
            LevelNo = currentLevel;
            foreach (TutorialDataSO tutorial in tutorialInfo)
            {
                if (tutorial.tutorial_Settings.level_To_Display == LevelNo)
                {
                    CurrentTutorialData = tutorial;
                    break;
                }
            }

            if (CurrentTutorialData != null && CurrentTutorialData.tutorial_Settings.level_To_Display == LevelNo)
            {
                ExecuteTutorial();
            }
            else
            {
                Debug.Log("Tutorial data not found for level: " + LevelNo);
            }
        }

        /// <summary>
        /// Changes the current tutorial step index and executes that step.
        /// </summary>
        public void ChangeStepAndExecute(int newStepIndex)
        {
            if (CurrentTutorialData == null)
            {
                Debug.LogWarning("Cannot change step: No active tutorial data!");
                return;
            }

            if (newStepIndex < 0 || newStepIndex >= CurrentTutorialData.Steps.Count)
            {
                Debug.LogWarning($"Cannot change to step {newStepIndex}: Step index out of range (0-{CurrentTutorialData.Steps.Count - 1})");
                return;
            }

            stepIndex = newStepIndex;
            ExecuteTutorial();
        }

        /// <summary>
        /// Re-executes the current tutorial step without changing the step index.
        /// </summary>
        public void ReExecuteCurrentStep()
        {
            if (CurrentTutorialData == null)
            {
                Debug.LogWarning("Cannot re-execute step: No active tutorial data!");
                return;
            }

            ExecuteTutorial();
        }

        #endregion

        #region Private

        private void TurnOffAll()
        {
            tutorialReferences.mainContainer.SetActive(false);
            tutorialReferences.handIcon.gameObject.SetActive(false);
            tutorialReferences.textBox.gameObject.SetActive(false);
            tutorialReferences.MaskedObjectsParent.SetActive(false);
        }

        private void ExecuteTutorial()
        {
            TurnOffAll();

            if (stepIndex < CurrentTutorialData.Steps.Count)
            {
                TutorialStep tStep = CurrentTutorialData.Steps[stepIndex];

                if (tStep.gif_Step_Settings.show_GIF)
                {
                    tutorialReferences.gifPanel.EnableGifPanel(tStep.gif_Step_Settings.gif_Clip_Index);
                    return;
                }

                tutorialReferences.mainContainer.SetActive(true);
                tutorialReferences.textBox.transform.localScale = Vector3.zero;

                StartCoroutine(CurrentTutorialData.ExecuteStep(
                    stepIndex,
                    tutorialReferences,
                    tutorialTweenSO,
                    tapPositions,
                    textBoxSizeDictionary,
                    this
                ));
            }
            else
            {
                tutorialReferences.skipTutorialStepButton.gameObject.SetActive(false);
                TNPLogger.Log("Tutorial completed successfully");
                stepIndex = 0;
                CurrentTutorialData = null;
                TurnOffAll();
            }
        }

        HashSet<Vector2> tapPositions = new();

        #endregion

        #region Callbacks

        private void updateStep(object Args)
        {
            if (CurrentTutorialData != null)
            {
                StartCoroutine(UpdateStepCoroutine());
            }
        }

        private IEnumerator UpdateStepCoroutine()
        {
            if (stepIndex < CurrentTutorialData.Steps.Count)
            {
                CurrentTutorialData.OnStepEnded(stepIndex, tutorialReferences, this);
            }

            yield return null;

            stepIndex++;
            ExecuteTutorial();
        }

        private void OnStartTutorial(object Args)
        {
            LevelNo = (int)Args;
            StartTutorial(LevelNo);
        }

        #endregion
    }

    [Serializable]
    public class TutorialReferences
    {
        public GameObject mainContainer;
        public RectTransform handIcon;
        public RectTransform textBox;
        public RectTransform textBoxBG;
        public TMP_Text textInfo;
        public GameObject MaskedObjectsParent;
        public Image MaskedBgImage;
        public Color fadeColor;
        public Color transparentColor;
        public Color BlackedColor;
        public UnmaskRaycastFilter unmaskRaycastFilter;
        public GifPanel gifPanel;
        public SkipTutorialStepButton skipTutorialStepButton;
    }
}

public enum TutorialTextBoxSize
{
    Small,
    Medium,
    Large,
}
