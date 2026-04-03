using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Timers;

public class ProgressBar : MonoBehaviour
{
    [Header("Fill Settings")]
    [SerializeField] TextMeshProUGUI progress_Text;
    [SerializeField] Image progress_Fill_Bar;
    [SerializeField] FillConfig fillConfig;

    [Header("Mode Settings")]
    [SerializeField] Bar_Mode barType;
    [SerializeField] bool HasBar = true;

    [Header("Timer Settings")]
    [SerializeField] TimerSettings timerSettings;
    private float currentTime;

    // -------------------------------------------------------------
    // LIMIT BAR VARIABLES — external update only
    // -------------------------------------------------------------
    [Header("Limit Bar Settings (if isTimer = false)")]
    [SerializeField] FillBarSettings fillBarSettings;
    float currentValue = 0f;

    Coroutine routine;
    bool isPaused = false;

    float logicalFill = 0f;
    float visualFill = 0f;


    private void Start()
    {
        StartProgress();
    }

    // ---------------------------------------------------------------------
    public void SetTimer(float Time)
    {
        barType = Bar_Mode.Timer;
        timerSettings.timer = Time;
    }

    public void SetFillBar(float MaxCap)
    {
        barType = Bar_Mode.Capacity;
        fillBarSettings.maxValue = MaxCap;
    }

    public void ResetProgress()
    {
        isPaused = false;

        if (barType == Bar_Mode.Timer)
        {
            currentTime = timerSettings.timer;
            logicalFill = HasBar ? 1f : logicalFill;
        }
        else
        {
            logicalFill = Mathf.Clamp01(currentValue / fillBarSettings.maxValue);
        }

        visualFill = logicalFill;

        // Apply only if enabled
        if (barType == Bar_Mode.Timer && HasBar)
            progress_Fill_Bar.fillAmount = visualFill;
        else if (barType == Bar_Mode.Capacity && HasBar)
            progress_Fill_Bar.fillAmount = visualFill;

        UpdateText();
        ApplyColor(visualFill);
    }

    public void StartProgress()
    {
        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(barType == Bar_Mode.Timer ? TimerMode() : LimitBarMode());
    }

    public void PauseProgress() => isPaused = true;
    public void ResumeProgress() => isPaused = false;

    // =====================================================================
    #region TIMER MODE
    // =====================================================================

    private IEnumerator TimerMode()
    {
        float speed = fillConfig != null ? fillConfig.fillSpeed : 1f;
        currentTime = timerSettings.timer;

        while (true)
        {
            if (isPaused)
            {
                yield return null;
                continue;
            }

            currentTime -= speed * Time.deltaTime;

            if (HasBar)
                logicalFill = Mathf.Clamp01(currentTime / timerSettings.timer);

            UpdateVisuals();

            if (currentTime <= 0f)
                break;

            yield return null;
        }

        if (HasBar)
        {
            logicalFill = 0;
            UpdateVisuals();
        }

        OnCompleted();
    }

    #endregion
    // =====================================================================
    #region LIMIT BAR MODE
    // =====================================================================

    private void Update()
    {
        if (barType == Bar_Mode.Timer) return; // only for normal bar testing

        // Check number keys 0–9
        for (int i = 0; i <= 9; i++)
        {
            if (Input.GetKeyDown(i.ToString()))
            {
                UpdateCurrentValue(i);
                Debug.Log($"Set currentValue = {i}");
            }
        }
    }


    private IEnumerator LimitBarMode()
    {
        while (true)
        {
            if (isPaused)
            {
                yield return null;
                continue;
            }

            logicalFill = Mathf.Clamp01(currentValue / fillBarSettings.maxValue);

            UpdateVisuals();

            yield return null;
        }
    }

    #endregion
    // =====================================================================

    // ⭐ Handles smoothing, gradient, text all in single function
    private void UpdateVisuals()
    {
        if (progress_Fill_Bar == null) return;
        // Visual fill only if allowed
        if ((barType == Bar_Mode.Timer && HasBar) || (barType == Bar_Mode.Capacity && HasBar))
        {
            visualFill = Mathf.Lerp(visualFill, logicalFill, 8f * Time.deltaTime);
            progress_Fill_Bar.fillAmount = visualFill;
            ApplyColor(visualFill);
        }

        UpdateText();
    }

    // ---------------------------------------------------------------------
    public void UpdateCurrentValue(float newValue)
    {
        currentValue = Mathf.Clamp(newValue, 0, fillBarSettings.maxValue);
    }

    public void SetMaxAmount(float MaxValue)
    {
        fillBarSettings.maxValue = MaxValue;
        UpdateCurrentValue(currentValue);
    }

    // ---------------------------------------------------------------------
    private void UpdateText()
    {
        if (progress_Text == null) return;

        if (barType == Bar_Mode.Timer)
        {
            float remaining = Mathf.Max(0f, currentTime);
            int mins = Mathf.FloorToInt(remaining / 60f);
            int secs = Mathf.FloorToInt(remaining % 60f);
            progress_Text.text = $"{mins:00}:{secs:00}";
        }
        else
        {
            progress_Text.text = $"{Mathf.FloorToInt(currentValue)} / {fillBarSettings}";
        }
    }

    private void ApplyColor(float normalized)
    {
        if (fillConfig.colorGradient != null)
            progress_Fill_Bar.color = fillConfig.colorGradient.Evaluate(normalized);
    }

    private void OnCompleted()
    {
        Debug.Log("Progress Completed");
    }
}

public enum Bar_Mode
{
    Timer,
    Capacity
}

[Serializable]
public struct TimerSettings
{
    public float timer;

    public TimerSettings(float timer = 5f)
    {
        this.timer = timer;
    }
}

[Serializable]
public struct FillBarSettings
{
    public float maxValue;

    public FillBarSettings(float maxValue = 5f)
    {
        this.maxValue = maxValue;
    }
}