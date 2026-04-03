using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

namespace TapNPlay.Core.Managers.UI
{
    public class HUDHandler : MonoBehaviour
    {
        [SerializeField] private TMP_Text levelIndicatorText;
        [SerializeField] private string PrefixText;
        public GameObject mainContainer;
        [SerializeField] private Image levelImage;
        [SerializeField] private Button retryButton;

        private LevelType levelType;

        #region Unity
        private void OnEnable()
        {
            EventController.StartListening(GameEvent.EVENT_LEVEL_LOADED_NUMBER, OnLevelLoaded);
            EventController.StartListening(GameEvent.EVENT_SET_LEVEL_DATA, SetLevelType);
            EventController.StartListening(GameEvent.EVENT_LEVEL_ENDED, OnLevelEnded);
        }

        private void OnDisable()
        {
            EventController.StopListening(GameEvent.EVENT_LEVEL_LOADED_NUMBER, OnLevelLoaded);
            EventController.StopListening(GameEvent.EVENT_SET_LEVEL_DATA, SetLevelType);
            EventController.StopListening(GameEvent.EVENT_LEVEL_ENDED, OnLevelEnded);
        }

        #endregion

        #region Public

        public void RetryButton()
        {
            retryButton.enabled = false;
            StartCoroutine(ReloadScene());
        }

        private IEnumerator ReloadScene()
        {
            yield return new WaitForSeconds(0.5f);
            yield return Resources.UnloadUnusedAssets();
            yield return new WaitForEndOfFrame();
            TransitionCanvas.Instance.PlayTransitionIN();
            yield return new WaitForSeconds(1f);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        #endregion

        #region Private
        #endregion

        #region Callbacks
        private void OnLevelLoaded(object Args)
        {
            mainContainer.SetActive(true);
            int level = (int)Args;
            levelIndicatorText.text = PrefixText + " " + level.ToString();
        }

        private void SetLevelType(object Args)
        {
            LevelSO levelData = (LevelSO)Args;
            levelType = levelData.LevelType;
        }

        private void OnLevelEnded(object Args)
        {
            mainContainer.SetActive(false);
        }
        #endregion
    }
}
