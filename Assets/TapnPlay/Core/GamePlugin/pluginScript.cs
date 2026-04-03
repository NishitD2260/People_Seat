using System.Collections.Generic;
using UnityEngine;

public class pluginScript : MonoBehaviour
{
    public static pluginScript Instance;

    // [Header("Economy")]
    // string currency = "Coins";
    // string itemType = "LevelEnd";

    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            DestroyImmediate(gameObject);

    }

    #region Main Event
    public void LevelStart(int levelNo)
    {

        // TinySauce.OnGameStarted(levelNo);
    }

    public void OnGameFinished(bool isUserCompleteLevel, float score, int levelNo)
    {
        //TinySauce.OnGameFinished(isUserCompleteLevel, score, levelNo);
    }
    #endregion



    #region Currency
    public void CoinsEarned(int currencyAmount, string itemID)
    {
        #region Example
        #endregion
        //TinySauce.OnCurrencyGiven(currency, currencyAmount, itemType, "level" + itemID);
    }

    public void CoinsSpent(int currencyAmount, string itemID)
    {
        // TinySauce.OnCurrencyTaken(currency, currencyAmount, itemType, "level" + itemID);
    }
    #endregion
}
