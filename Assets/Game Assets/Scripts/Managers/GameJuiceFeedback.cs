using DG.Tweening;
using UnityEngine;

/// <summary>
/// Optional feel layer: camera punch + seat mesh punch on events. Add to a bootstrap object and assign the gameplay camera (or leave empty for Camera.main).
/// </summary>
public class GameJuiceFeedback : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;

    [Header("Camera punch")]
    [SerializeField] private float tapShakeStrength = 0.035f;
    [SerializeField] private float tapShakeDuration = 0.22f;

    [SerializeField] private float sitShakeStrength = 0.025f;
    [SerializeField] private float sitShakeDuration = 0.14f;

    [SerializeField] private float winShakeStrength = 0.085f;
    [SerializeField] private float winShakeDuration = 0.45f;

    [SerializeField] private float loseShakeStrength = 0.05f;
    [SerializeField] private float loseShakeDuration = 0.35f;

    [Header("Seat land (visual on mesh child, not root)")]
    [SerializeField] private float seatPunchScale = 0.14f;
    [SerializeField] private float seatPunchDuration = 0.18f;

    private Vector3 _cameraLocalPos;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera != null)
            _cameraLocalPos = targetCamera.transform.localPosition;
    }

    private void OnEnable()
    {
        EventController.StartListening(GameEvent.EVENT_PEOPLE_GROUP_TAPPED, OnGroupTapped);
        EventController.StartListening(GameEvent.EVENT_PEOPLE_SEATED, OnPeopleSeated);
        EventController.StartListening(GameEvent.EVENT_LEVEL_WIN, OnWin);
        EventController.StartListening(GameEvent.EVENT_LEVEL_LOSE, OnLose);
    }

    private void OnDisable()
    {
        EventController.StopListening(GameEvent.EVENT_PEOPLE_GROUP_TAPPED, OnGroupTapped);
        EventController.StopListening(GameEvent.EVENT_PEOPLE_SEATED, OnPeopleSeated);
        EventController.StopListening(GameEvent.EVENT_LEVEL_WIN, OnWin);
        EventController.StopListening(GameEvent.EVENT_LEVEL_LOSE, OnLose);

        if (targetCamera != null)
            targetCamera.transform.DOKill();
    }

    private void OnGroupTapped(object _)
    {
        ShakeCamera(tapShakeStrength, tapShakeDuration);
    }

    private void OnPeopleSeated(object arg)
    {
        Seat seat = arg as Seat;
        ShakeCamera(sitShakeStrength, sitShakeDuration);
        PunchSeatVisual(seat);
    }

    private void OnWin(object _)
    {
        ShakeCamera(winShakeStrength, winShakeDuration);
    }

    private void OnLose(object _)
    {
        ShakeCamera(loseShakeStrength, loseShakeDuration);
    }

    private void ShakeCamera(float strength, float duration)
    {
        if (targetCamera == null) return;

        Transform ct = targetCamera.transform;
        ct.DOKill();
        ct.localPosition = _cameraLocalPos;
        Vector3 s = new Vector3(strength, strength * 0.45f, strength);
        ct.DOShakePosition(duration, s, vibrato: 10, randomness: 72f, snapping: false, fadeOut: true)
            .OnComplete(() => ct.localPosition = _cameraLocalPos);
    }

    private void PunchSeatVisual(Seat seat)
    {
        if (seat == null) return;

        var renderer = seat.GetComponentInChildren<MeshRenderer>();
        if (renderer == null) return;

        Transform vt = renderer.transform;
        vt.DOKill();
        vt.DOPunchScale(Vector3.one * seatPunchScale, seatPunchDuration, vibrato: 8, elasticity: 0.55f)
            .SetEase(Ease.OutQuad);
    }
}
