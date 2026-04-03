using DG.Tweening;
using TMPro;
using UnityEngine;

public class ToastManager : SingletonBase<ToastManager>
{
    [SerializeField] private RectTransform ToastBox;
    [SerializeField] private TMP_Text ToastText;
    [SerializeField] private Vector2 DestPosition;

    [Header("Tween Data")]
    [SerializeField] private float duration = 1f;
    [SerializeField] private float delay = 0.5f;
    [SerializeField] private Ease easeType = Ease.OutQuad;

    private bool isToastVisible = false;
    private Vector2 orgPosition;

    #region Unity
    private void Start()
    {
        orgPosition = ToastBox.anchoredPosition;
        ToastBox.gameObject.SetActive(false);
    }
    #endregion

    #region Public
    public void ShowToast(string message = "Insufficient space")
    {
        if (ToastBox != null && !isToastVisible)
        {
            ToastText.text = message;
            ToastBox.localScale = Vector3.zero;
            ToastBox.gameObject.SetActive(true);
            isToastVisible = true;

            TweenToast();
        }
    }
    #endregion

    #region Private
    private void TweenToast()
    {
        ToastBox.DOScale(Vector3.one, duration).SetDelay(delay).SetEase(easeType).
        OnComplete(
            () =>
            {
                ToastBox.DOAnchorPos(DestPosition, duration * 2.5f).OnComplete(
                    () =>
                    {
                        ToastBox.DOScale(Vector3.zero, duration / 2f).SetEase(easeType).OnComplete(
                            () =>
                            {
                                ToastBox.gameObject.SetActive(false);
                                ToastBox.anchoredPosition = orgPosition;
                                isToastVisible = false;
                            }
                        );
                    }
                );
            }
        );
    }
    #endregion

    #region Callbacks
    #endregion
}