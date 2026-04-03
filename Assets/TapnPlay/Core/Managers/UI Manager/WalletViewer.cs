using UnityEngine;
using MEC;
using TMPro;
using System.Collections.Generic;
using TapNPlay.Core.Data;
using UnityEngine.UI;
using AYellowpaper.SerializedCollections;

namespace TapNPlay.Core.Managers.UI
{
    public class WalletViewer : MonoBehaviour
    {
        [SerializeField] private TMP_Text ValueText;
        [SerializeField] private Image Icon;
        [SerializeField] private Currency currencyType;
        [SerializeField] private EconomyData Economydata;
        private int Value = 0;
        private string Tag;
        bool isSetupDone;

        #region Unity
        private void Awake()
        {
            Tag = currencyType.ToString() + "_Tag";
            SetIcon();
            EventController.StartListening(GameEvent.EVENT_CURRENCY_CHANGED, OnValueChanged);
        }

        private void OnDestroy()
        {
            EventController.StopListening(GameEvent.EVENT_CURRENCY_CHANGED, OnValueChanged);
        }

        #endregion

        #region Public
        #endregion

        #region Private
        private void SetIcon()
        {
            isSetupDone = false;
            Icon.sprite = Economydata.GetCurrencyIcon(currencyType);
        }

        private void UpdateValue(int newvalue, float duration = 0.5f, float delay = 0f)
        {
            if (isSetupDone && gameObject.activeInHierarchy)
            {
                Timing.RunCoroutine(UpdateValueRoutine(newvalue, duration, delay), Tag);
            }
            else
            {
                ValueText.text = NumberFormatter.FormatNumber(newvalue);
                Value = newvalue;
                isSetupDone = true;
            }
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
                    ValueText.text = Value.ToString();

                    yield return Timing.WaitForOneFrame;
                }
            }

            Value = newVal;
            ValueText.text = NumberFormatter.FormatNumber(Value);
        }
        #endregion

        #region Callbacks
        private void OnValueChanged(object Args)
        {
            SerializedDictionary<Currency, int> data = (SerializedDictionary<Currency, int>)Args;
            int Value = data[currencyType];

            Timing.KillCoroutines(Tag);
            UpdateValue(Value);
        }
        #endregion
    }
}