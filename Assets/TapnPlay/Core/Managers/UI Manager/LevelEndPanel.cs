using System.Collections.Generic;
using AssetKits.ParticleImage;
using MEC;
using TapNPlay.Core.Data;
using TapNPlay.Core.Managers;
using TapNPlay.Core.Managers.UI;
using TapNPlay.Core.Progression;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelEndPanel : MonoBehaviour
{
    EconomyManager ECO_Manager => EconomyManager.Instance;
    [SerializeField] private GameObject mainContainer;
    private HUDHandler HUDManager;

    [Header("Level Won Panel")]
    [SerializeField] private GameObject LevelWonPanel;
    [SerializeField] private Animator levelWinAnimator;
    [SerializeField] private UnlockableProgression unlockableProgression;
    [SerializeField] private TextMeshProUGUI cashText;
    // [SerializeField] private Transform rewardParent;
    [SerializeField] private RewardViewer rewardPrefab;
    [SerializeField] private EconomyData economyData;
    [SerializeField] private ParticleImage coinParticle;
    [SerializeField] private Button nextButton;

    [Header("Level Lose Panel")]
    public bool IsTimerLoose = false;
    [SerializeField] private GameObject TimerLoosePanel;
    [SerializeField] private GameObject SpaceLoosePanel;
    [SerializeField] private TextMeshProUGUI SpaceRevivalCostText;
    [SerializeField] private TextMeshProUGUI TimerRevivalCostText;
    [SerializeField] private Button SpaceRetryButton;
    [SerializeField] private Button TimerRetryButton;
    [SerializeField] private Button SpaceExtraStorageButton;
    [SerializeField] private Button TimerExtraStorageButton;
    [SerializeField] private Image[] extraButtonImages;
    [SerializeField] private TextMeshProUGUI[] extraButtonTexts;
    [SerializeField] private Color lockedColor;
    [SerializeField] private Color unlockedColor;

    [Header("Skip Level Button")]
    [SerializeField] private GameObject SkipButton;
    [SerializeField] private int SkipThreshold = 3;

    private int TryCount;

    [SerializeField] private List<BoosterReward> boosterRewardList = new List<BoosterReward>();

    #region Unity

    private void Awake()
    {
        HUDManager = FindObjectOfType<HUDHandler>();
    }

    private void OnEnable()
    {
        EventController.StartListening(GameEvent.EVENT_LEVEL_LOADED_NUMBER, OnLevelLoaded);
        EventController.StartListening(GameEvent.EVENT_LEVEL_ENDED, OnLevelEnded);
        EventController.StartListening(GameEvent.EVENT_SET_LEVEL_DATA, LoadLevelRewardData);

        nextButton.onClick.AddListener(NextButton);
        SpaceRetryButton.onClick.AddListener(RetryButton);
        TimerRetryButton.onClick.AddListener(RetryButton);
    }

    private void OnDisable()
    {
        EventController.StopListening(GameEvent.EVENT_LEVEL_LOADED_NUMBER, OnLevelLoaded);
        EventController.StopListening(GameEvent.EVENT_LEVEL_ENDED, OnLevelEnded);
        EventController.StopListening(GameEvent.EVENT_SET_LEVEL_DATA, LoadLevelRewardData);

        nextButton.onClick.RemoveListener(NextButton);
        SpaceRetryButton.onClick.RemoveListener(RetryButton);
        TimerRetryButton.onClick.RemoveListener(RetryButton);
    }

    #endregion

    #region Public

    public void NextButton()
    {
        HapticManager.Instance.PlayHaptics(HapticsID.HAPTIC_LIGHT);
        PlayTransition();
        nextButton.enabled = false;
        EventController.TriggerEvent(GameEvent.EVENT_NEXT_BUTTON_CLICKED);
    }

    private void PlayTransition()
    {
        TransitionCanvas.Instance.PlayTransitionIN();
        Invoke(nameof(ReloadLevel), 1f);
    }

    public void RetryButton()
    {
        SpaceRetryButton.enabled = false;
        TimerRetryButton.enabled = false;
        HUDManager.RetryButton();
    }

    public void OnFirstParticleFinished()
    {
        int value = ECO_Manager.GetBalance(Currency.PRIMARY);
        UpdateValue(value);
    }

    #region AnimatateOnWinReward
    private int Value = 0;
    private void UpdateValue(int newvalue, float duration = 0.5f, float delay = 0f)
    {
        Timing.RunCoroutine(UpdateValueRoutine(newvalue, duration, delay));
    }
    private IEnumerator<float> UpdateValueRoutine(int newVal, float duration = 0.5f, float delay = 0f)
    {
        yield return Timing.WaitForSeconds(delay);

        int startValue = Value;
        float elapsedTime = 0f;

        if (startValue < 1500)
        {
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / duration);

                Value = Mathf.RoundToInt(Mathf.Lerp(startValue, newVal, t));
                cashText.text = Value.ToString();

                yield return Timing.WaitForOneFrame;
            }
        }

        Value = newVal;
        cashText.text = NumberFormatter.FormatNumber(Value);
    }
    #endregion
    public void OnLastParticleFinished()
    {
        Invoke(nameof(ReloadLevel), 1f);
    }

    private void ReloadLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        PlayTransition();
    }

    public void OnRetryButtonClicked()
    {
        TryCount++;
        EventController.TriggerEvent(GameEvent.EVENT_RELOAD_BUTTON);
    }

    public void OnSkipButtonClicked()
    {
        TryCount = 0;
        EventController.TriggerEvent(GameEvent.EVENT_NEXT_SKIP_BUTTON);
    }

    //Function for Calling Restart on GameFail it can be either adding 
    // extra space or extra time on Fail
    public void OnReviveButton()
    {
        if (ECO_Manager.CutRevivalCost())
        {
            HUDManager.mainContainer.SetActive(true);
            LevelWonPanel.SetActive(false);
            SpaceLoosePanel.SetActive(false);
            TimerLoosePanel.SetActive(false);

            if (pluginScript.Instance)
            {
                pluginScript.Instance.CoinsSpent(ECO_Manager.economyData.RevivalCost, LevelManager.Instance.currentLevelNo.ToString());
            }

            if (HapticManager.Instance)
            {
                HapticManager.Instance.PlayHaptics(HapticsID.HAPTIC_LIGHT);
            }
            PerformRevivalFunction();
        }
    }

    #endregion

    #region Private
    private void DisableAll()
    {
        mainContainer.SetActive(false);
        LevelWonPanel.SetActive(false);
        SpaceLoosePanel.SetActive(false && !IsTimerLoose);
        TimerLoosePanel.SetActive(false && IsTimerLoose);
    }

    private void ToggleLevelEndPanel(bool levelWon)
    {
        Value = ECO_Manager.GetBalance(Currency.PRIMARY);
        cashText.text = NumberFormatter.FormatNumber(Value);
        mainContainer.SetActive(true);
        LevelWonPanel.SetActive(levelWon);
        SpaceLoosePanel.SetActive(!levelWon && !IsTimerLoose);
        TimerLoosePanel.SetActive(!levelWon && IsTimerLoose);

        if (levelWon)
        {
            RewardSetup();
            if (LevelManager.Instance.currentLevelNo < unlockableProgression.LastProgressionLevelNumber)
            {
                if (unlockableProgression.SetupAndSave() == 100)
                {
                    levelWinAnimator.Play("NewFeatureUnlock_Panel");
                }
                else
                {
                    levelWinAnimator.Play("WinPanel_Anim");
                }
            }
            else
            {
                AudioManager.Instance.PlayAudio(AudioID.NoFeatureWin);
                levelWinAnimator.Play("NoFeatureWinAnimation");
            }
        }

        CheckSkipButton(levelWon);
    }

    private void RewardSetup()
    {
        var rewards = ECO_Manager.GrantLevelEndCurrency(LevelManager.Instance.CurrentLevelData.LevelType);

        if ((rewards != null && rewards.Count > 0) || boosterRewardList.Count > 1)
        {
            // ClearOldRewardInstance();

            if (rewards != null && rewards.Count != 0)
            {
                // rewardPrefab.gameObject.SetActive(false);
                // foreach (var reward in rewards)
                // {
                //     RewardViewer rewardViewer = Instantiate(rewardPrefab, rewardParent);
                //     rewardViewer.SetupInfo(economyData.GetCurrencyIcon(reward.currencyType),
                //                  reward.Amount.ToString());
                // }

                rewardPrefab.SetupInfo(economyData.GetCurrencyIcon(rewards[0].currencyType),
                                 rewards[0].Amount.ToString());
            }

            if (boosterRewardList.Count != 0)
            {
                // foreach (BoosterReward reward in boosterRewardList)
                // {
                //     RewardViewer rewardViewer = Instantiate(rewardPrefab, rewardParent);
                //     rewardViewer.SetupInfo(economyData.GetBoosterIcon(reward.booster),
                //                  reward.amount.ToString());
                // }
            }
        }
    }

    // private void ClearOldRewardInstance()
    // {
    //     int childCount = rewardParent.childCount;

    //     for (int i = 1; i < childCount; i++)
    //     {
    //         Destroy(rewardParent.GetChild(i).gameObject);
    //     }
    // }

    private void CheckSkipButton(bool isWon)
    {
        if (isWon)
        {
            if (TryCount >= SkipThreshold)
            {
                SkipButton.SetActive(true);
            }
        }
    }
    private void CheckExtraStorageButton()
    {
        SpaceRevivalCostText.text = NumberFormatter.FormatNumber(ECO_Manager.economyData.RevivalCost);
        TimerRevivalCostText.text = NumberFormatter.FormatNumber(ECO_Manager.economyData.RevivalCost);
        if (ECO_Manager.CanAffordRevive())
        {
            SpaceExtraStorageButton.interactable = true;
            TimerExtraStorageButton.interactable = true;
            foreach (Image img in extraButtonImages)
            {
                img.color = unlockedColor;
            }

            foreach (TextMeshProUGUI text in extraButtonTexts)
            {
                text.color = unlockedColor;
            }
        }
        else
        {
            SpaceExtraStorageButton.interactable = false;
            TimerExtraStorageButton.interactable = false;

            foreach (Image img in extraButtonImages)
            {
                img.color = lockedColor;
            }

            foreach (TextMeshProUGUI text in extraButtonTexts)
            {
                text.color = lockedColor;
            }
        }
    }

    #endregion

    #region Callbacks

    public void PerformRevivalFunction()
    {
        Debug.Log("PerformRevivalFunction");
        // Correct Revival Function Will be Called Here
    }

    private void LoadLevelRewardData(object args)
    {
        LevelSO levelData = (LevelSO)args;
        if (levelData != null)
        {
            boosterRewardList = new List<BoosterReward>(levelData.BoosterRewardList);
        }
    }

    private void OnLevelLoaded(object Args)
    {
        DisableAll();
    }

    private void OnLevelEnded(object Args)
    {
        bool levelWon = (bool)Args;
        if (levelWon && TutorialManager.Instance.CurrentTutorialData != null)
        {
            EventController.TriggerEvent(GameEvent.EVENT_TUTORIAL_NEXT_STEP);
        }
        if (levelWon)
        {
            if (HapticManager.Instance)
            {
                HapticManager.Instance.PlayHaptics(HapticsID.HAPTIC_HEAVY);
            }
        }
        else
        {
            CheckExtraStorageButton();

            AudioManager.Instance.PlayAudio(AudioID.LEVEL_FAIL);
            if (HapticManager.Instance)
            {
                HapticManager.Instance.PlayHaptics(HapticsID.HAPTIC_MEDIUM);
            }
        }
        ToggleLevelEndPanel(levelWon);

        if (!levelWon)
        {
            if (pluginScript.Instance)
            {
                pluginScript.Instance.OnGameFinished(false, ECO_Manager.GetBalance(Currency.PRIMARY), LevelManager.Instance.currentLevelNo);
            }
        }
    }

    #endregion
}