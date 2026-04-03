using DG.Tweening;
using UnityEngine;

public class HandTween : MonoBehaviour
{
    private Tweener HandScleTweener;
    [SerializeField] RectTransform pivot;
    Vector3 pivotScale;
    [SerializeField] private TutorialTweenSO tutorialTweenSO;

    private void OnEnable()
    {
        pivotScale = pivot.localScale;
        HandScleTweener = transform.DOScale(pivotScale * tutorialTweenSO.handScaleTarget, tutorialTweenSO.handScaleDuration).SetLoops(-1, LoopType.Yoyo).SetEase(tutorialTweenSO.handScaleEase);
    }

    private void OnDisable()
    {
        HandScleTweener.Kill();
        transform.localScale = Vector3.one;
        pivot.localScale = pivotScale;
    }
}