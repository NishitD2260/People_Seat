using System;
using System.Collections.Generic;

public enum GameEvent
{
    //GAME EVENTS
    EVENT_DATA_LOADED,

    // LEVEL EVENTS
    EVENT_LEVEL_LOADED_NUMBER,
    EVENT_LEVEL_LOADED_DATA,
    EVENT_LEVEL_ENDED,
    EVENT_LEVEL_FAIL_CONTINUE,
    EVENT_SET_LEVEL_DATA,

    // ECONOMY EVENTS
    EVENT_CURRENCY_CHANGED,

    // BOOSTER EVENTS
    EVENT_BOOSTER_PURCHASED,
    EVENT_BOOSTER_USED,
    EVENT_BOOSTER_ACTIVE,

    //BUTTON EVENTS
    EVENT_NEXT_BUTTON_CLICKED,
    EVENT_NEXT_SKIP_BUTTON,
    EVENT_RELOAD_BUTTON,

    //UNLOCKABLE EVENT
    EVENT_UNLOCKABLE_UPDATED,

    //SAVE EVENTS

    //TUTORIAL EVENTS
    EVENT_TUTORIAL_START,
    EVENT_TUTORIAL_NEXT_STEP,

    //TOAST MESSAGE
    EVENT_ON_POWERUP_ADDED,

    //PEOPLE SEAT EVENTS
    EVENT_PEOPLE_GROUP_TAPPED,
    EVENT_PEOPLE_SEATED,
    EVENT_PEOPLE_SENT_TO_WAITING,
    EVENT_WAITING_AREA_UPDATED,
    EVENT_LEVEL_WIN,
    EVENT_LEVEL_LOSE
}


//Unused
// public static class GameEvents
// {
//     public static Action<int> OnLevelStarted;
//     public static Action<Dictionary<Currency, int>> OnCurrencyLoaded;
//     public static Action<Currency, int> OnCurrencyUpdated;
//     public static Action<AudioID> OnPlaySfx;
//     public static Action OnLevelCompleted;
//     public static Action OnLevelFailed;
//     public static Action<int> OnSceneTransition;
// }
