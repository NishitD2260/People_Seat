using UnityEngine;
using AYellowpaper.SerializedCollections;
using TapNPlay.Core.Data;
using System.Collections.Generic;
using TapNPlay.Core.SaveSystem;


namespace TapNPlay.Core.Managers
{
    public class EconomyManager : SingletonBase<EconomyManager>
    {
        public EconomyData economyData; // SO for Data
        [SerializeField] private CurrencyDataSystem.CurrencyData currencyData;

        #region Unity
        private void OnEnable()
        {
            //InitializeCurrencies();
            EventController.StartListening(GameEvent.EVENT_LEVEL_LOADED_NUMBER, OnLevelLoaded);
            EventController.StartListening(GameEvent.EVENT_BOOSTER_PURCHASED, OnBoosterPurchased);
        }

        private void OnDisable()
        {
            EventController.StopListening(GameEvent.EVENT_LEVEL_LOADED_NUMBER, OnLevelLoaded);
            EventController.StopListening(GameEvent.EVENT_BOOSTER_PURCHASED, OnBoosterPurchased);
        }
        #endregion

        #region Public

        public void AddCurrency(Currency type, int amount) // Adds currency to the player's wallet.
        {
            if (currencyData.CurrencyValues.ContainsKey(type))
            {
                currencyData.CurrencyValues[type] += amount;
                EventController.TriggerEvent(GameEvent.EVENT_CURRENCY_CHANGED, currencyData.CurrencyValues);
            }
            else
            {
                Debug.LogWarning($"Currency type {type} not found.");
            }

            CurrencyDataSystem.Save(currencyData);
        }

        public void AddCurrency(List<RewardTypeValueContainer> rewards)
        {
            if (rewards != null && rewards.Count > 0)
            {
                foreach (var rewardPair in rewards)
                {
                    AddCurrency(rewardPair.currencyType, rewardPair.Amount);
                }
            }
        }

        public void ReduceCurrency(Currency type, int amount) // Reduces currency from the player's wallet if sufficient balance exists.
        {
            if (currencyData.CurrencyValues.ContainsKey(type) && currencyData.CurrencyValues[type] >= amount)
            {
                currencyData.CurrencyValues[type] -= amount;
                EventController.TriggerEvent(GameEvent.EVENT_CURRENCY_CHANGED, currencyData.CurrencyValues);
            }
            else
            {
                Debug.LogWarning($"Insufficient balance for currency type {type}.");
            }

            CurrencyDataSystem.Save(currencyData);
        }

        public bool CanAffordRevive()
        {
            return GetBalance(Currency.PRIMARY) > economyData.RevivalCost;
        }

        public bool CutRevivalCost()
        {
            if (CanAffordRevive())
            {
                ReduceCurrency(Currency.PRIMARY, economyData.RevivalCost);
                return true;
            }
            return false;
        }

        public List<RewardTypeValueContainer> GrantLevelEndCurrency(LevelType levelType)
        {
            switch (levelType)
            {
                case LevelType.BASIC:
                    AddCurrency(economyData.levelWinRewards.BasicReward);
                    return economyData.levelWinRewards.BasicReward;
                case LevelType.BONUS:
                    AddCurrency(economyData.levelWinRewards.BonusReward);
                    return economyData.levelWinRewards.BonusReward;
                case LevelType.BOSS:
                    AddCurrency(economyData.levelWinRewards.BossReward);
                    return economyData.levelWinRewards.BossReward;
                default:
                    return null;
            }
        }

        public bool CanAffordBooster(Booster type) // Checks if the player can afford a specified booster.
        {
            return currencyData.CurrencyValues.ContainsKey(Currency.PRIMARY) &&
                   currencyData.CurrencyValues[Currency.PRIMARY] >= economyData.GetBoosterCost(type);
        }

        public int GetBalance(Currency type) // Returns the current balance of a specified currency.
        {
            if (currencyData.CurrencyValues.ContainsKey(type))
            {
                return currencyData.CurrencyValues[type];
            }
            else
            {
                Debug.LogWarning($"Currency type {type} not found.");
                return 0;
            }
        }

        #endregion

        #region Private
        private void InitializeCurrencies()
        {
            foreach (CurrencyValuePair CVP in economyData.AvailableCurrencies)
            {
                if (!currencyData.CurrencyValues.ContainsKey(CVP.Type))
                {
                    currencyData.CurrencyValues[CVP.Type] = CVP.Amount;
                }
            }
        }

        #endregion

        #region Callbacks
        private void OnLevelLoaded(object arg)
        {
            InitializeCurrencies();

            if (ES3.KeyExists(CurrencyDataSystem.Key))
            {
                var LoadedCurrencyData = CurrencyDataSystem.Load();
                foreach (var C in LoadedCurrencyData.CurrencyValues)
                {
                    if (currencyData.CurrencyValues.ContainsKey(C.Key))
                    {
                        currencyData.CurrencyValues[C.Key] = C.Value;
                    }
                }
            }
            else
            {
                CurrencyDataSystem.Save(currencyData);
            }
            EventController.TriggerEvent(GameEvent.EVENT_CURRENCY_CHANGED, currencyData.CurrencyValues);
        }

        private void OnBoosterPurchased(object arg)
        {
            Debug.Log("Booster purchased event handled.");
        }
        #endregion
    }
}