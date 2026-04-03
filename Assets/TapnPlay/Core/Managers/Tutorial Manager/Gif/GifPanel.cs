using System.Collections.Generic;
using DG.Tweening;
using MEC;
using TapNPlay.Core.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class GifPanel : MonoBehaviour
{
    [SerializeField] private GifVideoData gifVideoData;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RectTransform gifVideoRectTransform;
    [SerializeField] private Animator animator;
    [SerializeField] Button ContinueButton;
    [SerializeField] float CanContinueDelay = 0.25f;

    public void EnableGifPanel(int gifIndex)
    {
        ContinueButton.interactable = false;
        GifData gifData = gifVideoData.GifDataList[gifIndex];

        if (gifData != null && descriptionText != null && videoPlayer != null)
        {
            descriptionText.text = gifData.Description;

            try
            {
                videoPlayer.clip = gifData.GifClip; // This might be a point of failure
                gifVideoRectTransform.anchoredPosition = new Vector2(gifData.gifVideoPosition.x, gifData.gifVideoPosition.y);
                // Invoke(nameof(EnableGifInDelay), 0.25f);
                Timing.RunCoroutine(GifEnableCoroutine());
            }
            catch (UnityException E)
            {
                Debug.LogError($"Error playing GIF: {E.Message}");
            }
        }
    }

    private IEnumerator<float> GifEnableCoroutine(float t = 0.25f)
    {
        yield return Timing.WaitForSeconds(t);
        EnableGif();
    }

    private void EnableGif()
    {
        if (videoPlayer != null)
        {
            try
            {
                videoPlayer.Play();
                AudioManager.Instance.PlayAudio(AudioID.POP_UP);
                gameObject.SetActive(true);
                DOVirtual.DelayedCall(CanContinueDelay, () => ContinueButton.interactable = true);
            }
            catch (UnityException e)
            {
                Debug.LogError($"Error playing GIF: {e.Message}");
            }
        }
    }

    public void Close()
    {
        animator.SetTrigger("Close");
    }

    public void DisablePanel()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            videoPlayer.clip = null;
        }
        gameObject.SetActive(false);
    }

    public void ExecuteTutorial()
    {
        if (videoPlayer != null)
        {
            videoPlayer.clip = null;
            videoPlayer.Stop();
        }
        EventController.TriggerEvent(GameEvent.EVENT_TUTORIAL_NEXT_STEP);
        gameObject.SetActive(false);
    }
}
