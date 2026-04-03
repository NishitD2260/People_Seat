using TapNPlay.Core.Managers;
using UnityEngine;

namespace TapNPlay.Core.Helpers
{
    public class CheatHandler : MonoBehaviour
    {
        [SerializeField] private GameObject mainContainer;

        #region Unity

        private void Start()
        {
            //mainContainer.SetActive(false);
            mainContainer.SetActive(true);
        }

        #endregion

        #region Public
        public void GrantCoins()
        {
            EconomyManager.Instance.AddCurrency(Currency.PRIMARY, 500);
        }

        public void ReduceCoins()
        {
            EconomyManager.Instance.ReduceCurrency(Currency.PRIMARY, 500);
        }

        public void WinLevel()
        {
            EventController.TriggerEvent(GameEvent.EVENT_LEVEL_ENDED, true);
        }

        public void LoseLevel()
        {
            EventController.TriggerEvent(GameEvent.EVENT_LEVEL_ENDED, false);
        }
        #endregion

    }
}
