using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(fileName = "Tutorial Tween SO", menuName = "TapnPlay/Tutorial Tween SO")]
public class TutorialTweenSO : ScriptableObject
{
    [Header("Text Box Tween Settings")]
    public float textBoxTweenDuration = 1f;
    public Ease textBoxEase = Ease.InOutBack;

    [Header("Mask Tween Settings")]
    public float maskTweenDuration = 1f;
    public float maskTweendelay = 0.25f;
    public Ease maskEase = Ease.InOutBack;

    [Header("Hand Move Tween Settings")]
    public float handMoveDuration = 0.6f;
    public Ease handMoveEase = Ease.InOutQuad;

    [Header("Hand Scale Tween Settings")]
    public float handScaleTarget = 1.2f;
    public float handScaleDuration = 0.6f;
    public Ease handScaleEase = Ease.Linear;
}
