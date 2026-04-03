using DG.Tweening;
using UnityEngine;

public class ButtonTween : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private float pressScale = 0.9f;
    [SerializeField] private float pressYValue = 1f;
    [SerializeField] private float duration = 0.1f;           // Speed of the animation
    [SerializeField] private Ease tweenEase = Ease.OutQuad;

    private Tween currentTween;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            PressDown();
        }
    }

    public void PressScale()
    {
        currentTween?.Kill();

        currentTween = rectTransform.DOScale(pressScale, duration)
            .SetEase(tweenEase)
            .OnComplete(() =>
            {
                rectTransform.DOScale(1f, duration).SetEase(tweenEase);
            });
    }

    public void PressDown()
    {
        currentTween?.Kill();
        currentTween = rectTransform.DOAnchorPos(new Vector2(0, pressYValue), duration).SetEase(tweenEase).OnComplete(() =>
        {
            rectTransform.DOAnchorPos(Vector2.zero, duration).SetEase(tweenEase);
        });
    }
}
