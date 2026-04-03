using System.Collections.Generic;
using System.Linq;
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

    private int capacity;
    private List<PersonView> waitingPeople = new List<PersonView>();
    private bool _suppressCountPunch;

    public int CurrentCount => waitingPeople.Count;

    public void Initialize(int capacity)
    {
        this.capacity = capacity;
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
                RearrangeWaitingPeople();
                UpdateUI();
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

    private void RearrangeWaitingPeople()
    {
        for (int i = 0; i < waitingPeople.Count; i++)
        {
            PersonView person = waitingPeople[i];
            if (person == null) continue;

            Vector3 targetPos = GetWaitingPositionForIndex(i);
            person.transform.DOKill();
            person.transform.DOMove(targetPos, rearrangeDuration).SetEase(Ease.OutQuad);
        }
    }

    private void UpdateUI()
    {
        if (countText != null)
        {
            countText.text = $"{waitingPeople.Count}/{capacity}";
            if (!_suppressCountPunch)
            {
                countText.transform.DOKill();
                countText.transform.DOPunchScale(Vector3.one * 0.08f, 0.25f, vibrato: 6, elasticity: 0.58f);
            }
        }
    }
}
