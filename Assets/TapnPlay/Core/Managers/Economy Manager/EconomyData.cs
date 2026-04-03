using System;
using System.Collections.Generic;
using UnityEngine;

namespace TapNPlay.Core.Data
{
    [CreateAssetMenu(fileName = "EconomyData", menuName = "TapnPlay/Data/Economy Data", order = 0)]
    public class EconomyData : ScriptableObject
    {
        public List<BoosterInfo> BoosterCostsData;
        public List<CurrencyValuePair> AvailableCurrencies;
        public int RevivalCost;
        public LevelRewards levelWinRewards;

        #region Helper Methods

        public int GetBoosterCost(Booster booster)
        {
            for (int i = 0; i < BoosterCostsData.Count; i++)
            {
                if (BoosterCostsData[i].Type == booster)
                {
                    return BoosterCostsData[i].Amount;
                }
            }
            return 0;
        }

        public Sprite GetBoosterIcon(Booster booster)
        {
            for (int i = 0; i < BoosterCostsData.Count; i++)
            {
                if (BoosterCostsData[i].Type == booster)
                {
                    return BoosterCostsData[i].Icon;
                }
            }

            return null;
        }

        public Sprite GetCurrencyIcon(Currency currency)
        {
            for (int i = 0; i < AvailableCurrencies.Count; i++)
            {
                if (AvailableCurrencies[i].Type == currency)
                {
                    return AvailableCurrencies[i].Icon;
                }
            }
            return null;
        }

        #endregion
    }

    [Serializable]
    public struct LevelRewards
    {
        public List<RewardTypeValueContainer> BasicReward;
        public List<RewardTypeValueContainer> BossReward;
        public List<RewardTypeValueContainer> BonusReward;
    }

    [Serializable]
    public struct RewardTypeValueContainer
    {
        public Currency currencyType;
        public int Amount;
    }

    [Serializable]
    public struct CurrencyValuePair
    {
        public Currency Type;
        public int Amount;
        public Sprite Icon;
    }


    [Serializable]
    public struct BoosterInfo
    {
        public Booster Type;
        public int Amount;
        public Sprite Icon;
    }
}