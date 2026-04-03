using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TapNPlay.Core.Progression
{
    public class UnlockablePanel : MonoBehaviour
    {
        [SerializeField] private GameObject renderImage;
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI unlockableNameText;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Animator animator;

        #region  PUBLIC

        public void Display(object args)
        {
            Unlockable unlockable = (Unlockable)args;
            if (unlockable != null)
            {
                switch (unlockable.DisplayType)
                {
                    case DisplayType.RENDER_IMAGE:
                        SetupRenderImage(unlockable);
                        break;
                    case DisplayType.IMAGE:
                        SetupIconImage(unlockable);
                        break;
                }
            }

            gameObject.SetActive(true);
        }

        //BUTTON EVENT
        public void Close()
        {
            animator.SetTrigger("Close");
        }

        //ANIMATION EVENT
        public void DisablePanel()
        {
            gameObject.SetActive(false);
        }

        #endregion

        #region PRIVATE

        private void SetupRenderImage(Unlockable unlockable)
        {
            renderImage.gameObject.SetActive(true);
            iconImage.gameObject.SetActive(false);
            unlockableNameText.text = unlockable.UnlockableName;
        }

        private void SetupIconImage(Unlockable unlockable)
        {
            renderImage.gameObject.SetActive(false);
            iconImage.sprite = unlockable.BgIcon;
            iconImage.gameObject.SetActive(true);
            unlockableNameText.text = unlockable.UnlockableName;
        }

        #endregion
    }
}
