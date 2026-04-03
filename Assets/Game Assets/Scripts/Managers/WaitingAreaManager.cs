using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class WaitingAreaManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform waitingAreaParent;
    [SerializeField] private TextMeshProUGUI countText;

    [Header("Layout")]
    [SerializeField] private int maxPerRow = 5;
    [SerializeField] private float personSpacing = 0.8f;
    [SerializeField] private float rearrangeDuration = 0.2f;
    [Tooltip("Wait this long after the waiting list changes before sliding people into grid slots. Another add/remove restarts the timer.")]
    [SerializeField] private float rearrangeDelay = 0.15f;

    [Header("Waiting count UI")]
    [SerializeField] private float countPunchPeakScale = 1.12f;
    [SerializeField] private float countPunchUpDuration = 0.1f;
    [SerializeField] private float countPunchReturnDuration = 0.2f;

    private int capacity;
    private List<PersonView> waitingPeople = new List<PersonView>();
    private bool _suppressCountPunch;
    private Tween _rearrangeScheduleTween;
    private Vector3 _countTextBaseLocalScale = Vector3.one;
    private bool _countTextBaseCaptured;

    public int CurrentCount => waitingPeople.Count;

    private void Awake()
    {
        CaptureCountTextBaseScale();
    }

    private void OnDisable()
    {
        _rearrangeScheduleTween?.Kill(false);
        _rearrangeScheduleTween = null;

        if (countText != null)
        {
            countText.transform.DOKill();
            if (_countTextBaseCaptured)
                countText.transform.localScale = _countTextBaseLocalScale;
        }
    }

    public void Initialize(int capacity)
    {
        this.capacity = capacity;
        _rearrangeScheduleTween?.Kill(false);
        _rearrangeScheduleTween = null;
        waitingPeople.Clear();
        _suppressCountPunch = true;
        UpdateUI();
        _suppressCountPunch = false;
    }

    public void AddPerson(PersonView person)
    {
        waitingPeople.Add(person);
        if (waitingAreaParent != null)
            person.transform.SetParent(waitingAreaParent);

        person.transform.DOKill();
        person.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f, vibrato: 7, elasticity: 0.55f);

        UpdateUI();
        EventController.TriggerEvent(GameEvent.EVENT_WAITING_AREA_UPDATED, CurrentCount);
        ScheduleRearrangeWaitingPeople();
    }

    public PersonView RemovePerson(SeatColor color)
    {
        for (int i = 0; i < waitingPeople.Count; i++)
        {
            if (waitingPeople[i].Color == color)
            {
                PersonView p = waitingPeople[i];
                waitingPeople.RemoveAt(i);
                p.transform.SetParent(null);
                UpdateUI();
                ScheduleRearrangeWaitingPeople();
                return p;
            }
        }
        return null;
    }

    public int CountPeopleOfColor(SeatColor color)
    {
        int count = 0;
        for (int i = 0; i < waitingPeople.Count; i++)
        {
            if (waitingPeople[i].Color == color) count++;
        }
        return count;
    }

    public List<SeatColor> GetDistinctWaitingColors()
    {
        HashSet<SeatColor> colors = new HashSet<SeatColor>();
        foreach (var p in waitingPeople)
        {
            colors.Add(p.Color);
        }
        return new List<SeatColor>(colors);
    }

    public bool IsFull()
    {
        return waitingPeople.Count >= capacity;
    }

    public int RemainingCapacity()
    {
        return Mathf.Max(0, capacity - waitingPeople.Count);
    }

    public Vector3 GetNextWaitingPosition()
    {
        return GetWaitingPositionForIndex(waitingPeople.Count);
    }

    public Vector3 GetWaitingPositionForIndex(int index)
    {
        int col = index % maxPerRow;
        int row = index / maxPerRow;
        Vector3 basePos = waitingAreaParent != null ? waitingAreaParent.position : transform.position;
        float xOffset = (col - (maxPerRow - 1) / 2f) * personSpacing;
        float zOffset = -row * personSpacing;
        return basePos + new Vector3(xOffset, 0f, zOffset);
    }

    private void ScheduleRearrangeWaitingPeople()
    {
        _rearrangeScheduleTween?.Kill(false);
        _rearrangeScheduleTween = null;

        if (waitingPeople.Count == 0) return;

        _rearrangeScheduleTween = DOVirtual.DelayedCall(rearrangeDelay, ExecuteRearrangeWaitingPeople)
            .SetLink(gameObject);
    }

    private void ExecuteRearrangeWaitingPeople()
    {
        _rearrangeScheduleTween = null;
        if (waitingPeople.Count == 0) return;

        for (int i = 0; i < waitingPeople.Count; i++)
        {
            PersonView person = waitingPeople[i];
            if (person == null) continue;

            Vector3 targetPos = GetWaitingPositionForIndex(i);
            person.transform.DOKill();
            person.transform.DOMove(targetPos, rearrangeDuration).SetEase(Ease.OutQuad);
        }
    }

    private void CaptureCountTextBaseScale()
    {
        if (countText == null || _countTextBaseCaptured) return;
        _countTextBaseLocalScale = countText.transform.localScale;
        _countTextBaseCaptured = true;
    }

    private void UpdateUI()
    {
        if (countText != null)
        {
            CaptureCountTextBaseScale();
            countText.text = $"{waitingPeople.Count}/{capacity}";
            if (!_suppressCountPunch)
                PlayCountTextScalePunch();
        }
    }

    private void PlayCountTextScalePunch()
    {
        Transform t = countText.transform;
        t.DOKill();

        Vector3 b = _countTextBaseLocalScale;
        Sequence seq = DOTween.Sequence();
        seq.Append(t.DOScale(b * countPunchPeakScale, countPunchUpDuration).SetEase(Ease.OutQuad));
        seq.Append(t.DOScale(b, countPunchReturnDuration).SetEase(Ease.OutQuad));
        seq.SetLink(gameObject);
    }
}
