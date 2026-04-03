using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sitDuration = 0.3f;
    [SerializeField] private float staggerDelay = 0.1f;

    private GridManager gridManager;

    /// <summary>Delay between each person in a group starting their walk (seconds).</summary>
    public float StaggerDelay => staggerDelay;

    public void Initialize(GridManager gridManager)
    {
        this.gridManager = gridManager;
    }

    public void AnimateGroupToSeats(List<PersonView> people, List<Seat> targetSeats, Vector3 startPosition, Action onAllComplete)
    {
        int completed = 0;
        int total = people.Count;

        for (int i = 0; i < people.Count; i++)
        {
            PersonView person = people[i];
            Seat seat = targetSeats[i];
            float delay = i * staggerDelay;

            AnimateWalkToSeat(person, seat, startPosition, delay, () =>
            {
                completed++;
                if (completed >= total)
                    onAllComplete?.Invoke();
            });
        }
    }

    public void AnimateGroupToWaiting(List<PersonView> people, WaitingAreaManager waitingArea, Vector3 startPosition, Action onAllComplete)
    {
        int completed = 0;
        int total = people.Count;
        int startIndex = waitingArea.CurrentCount;

        for (int i = 0; i < people.Count; i++)
        {
            PersonView person = people[i];
            float delay = i * staggerDelay;
            Vector3 targetPos = waitingArea.GetWaitingPositionForIndex(startIndex + i);

            AnimateWalkToWaiting(person, waitingArea, targetPos, delay, () =>
            {
                completed++;
                if (completed >= total)
                    onAllComplete?.Invoke();
            });
        }
    }

    private void AnimateWalkToSeat(PersonView person, Seat targetSeat, Vector3 startPos, float delay, Action onComplete)
    {
        targetSeat.AddWalkIncoming();

        List<Vector3> waypoints = BuildPathToSeat(startPos, targetSeat);
        float totalDist = CalculatePathLength(person.transform.position, waypoints);
        float duration = Mathf.Max(totalDist / walkSpeed, 0.3f);

        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(delay);
        Vector3 hopBase = person.transform.localScale;
        seq.Append(person.transform.DOScale(hopBase * 1.12f, 0.06f).SetEase(Ease.OutQuad));
        seq.Append(person.transform.DOScale(hopBase, 0.08f).SetEase(Ease.InQuad));
        seq.Append(person.transform.DOPath(waypoints.ToArray(), duration, PathType.Linear)
            .SetEase(Ease.Linear));
        seq.Append(person.transform.DOScale(Vector3.one * 0.8f, sitDuration)
            .SetEase(Ease.InBack));
        seq.AppendCallback(() =>
        {
            person.SetSeated(true);
            targetSeat.MarkOccupied();
            EventController.TriggerEvent(GameEvent.EVENT_PEOPLE_SEATED, targetSeat);
            onComplete?.Invoke();
        });
    }

    private void AnimateWalkToWaiting(PersonView person, WaitingAreaManager waitingArea, Vector3 targetPos, float delay, Action onComplete)
    {
        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(delay);
        Vector3 hopBaseW = person.transform.localScale;
        seq.Append(person.transform.DOScale(hopBaseW * 1.1f, 0.05f).SetEase(Ease.OutQuad));
        seq.Append(person.transform.DOScale(hopBaseW, 0.07f).SetEase(Ease.InQuad));

        // Simple path: move to waiting area
        float duration = Mathf.Max(Vector3.Distance(person.transform.position, targetPos) / walkSpeed, 0.3f);
        seq.Append(person.transform.DOMove(targetPos, duration).SetEase(Ease.Linear));
        seq.AppendCallback(() =>
        {
            waitingArea.AddPerson(person);
            onComplete?.Invoke();
        });
    }

    public void AnimateWaitingToSeat(PersonView person, Seat targetSeat, Action onComplete)
    {
        targetSeat.AddWalkIncoming();

        Vector3 startPos = person.transform.position;
        List<Vector3> waypoints = BuildPathToSeat(startPos, targetSeat);
        float totalDist = CalculatePathLength(startPos, waypoints);
        float duration = Mathf.Max(totalDist / walkSpeed, 0.3f);

        Sequence seq = DOTween.Sequence();
        Vector3 hopBaseWs = person.transform.localScale;
        seq.Append(person.transform.DOScale(hopBaseWs * 1.12f, 0.06f).SetEase(Ease.OutQuad));
        seq.Append(person.transform.DOScale(hopBaseWs, 0.08f).SetEase(Ease.InQuad));
        seq.Append(person.transform.DOPath(waypoints.ToArray(), duration, PathType.Linear)
            .SetEase(Ease.Linear));
        seq.Append(person.transform.DOScale(Vector3.one * 0.8f, sitDuration)
            .SetEase(Ease.InBack));
        seq.AppendCallback(() =>
        {
            person.SetSeated(true);
            targetSeat.MarkOccupied();
            EventController.TriggerEvent(GameEvent.EVENT_PEOPLE_SEATED, targetSeat);
            onComplete?.Invoke();
        });
    }

    private List<Vector3> BuildPathToSeat(Vector3 startPos, Seat targetSeat)
    {
        List<Vector3> waypoints = new List<Vector3>();
        Vector3 seatPos = targetSeat.transform.position;

        // 1. Move to aisle entry (bottom of aisle)
        Vector3 aisleEntry = gridManager.GetAisleEntryBottom();
        waypoints.Add(new Vector3(aisleEntry.x, startPos.y, aisleEntry.z));

        // 2. Walk up the aisle to the target row
        Vector3 aisleAtRow = gridManager.GetAisleWorldPosition(targetSeat.Row);
        waypoints.Add(new Vector3(aisleAtRow.x, startPos.y, aisleAtRow.z));

        // 3. Turn into the row toward the seat
        waypoints.Add(new Vector3(seatPos.x, startPos.y, aisleAtRow.z));

        // 4. Move to seat position
        waypoints.Add(new Vector3(seatPos.x, seatPos.y, seatPos.z));

        // Clean waypoints that are very close to start
        CleanWaypoints(waypoints, startPos);

        return waypoints;
    }

    private void CleanWaypoints(List<Vector3> waypoints, Vector3 startPos)
    {
        // Remove waypoints that are too close to the starting position
        while (waypoints.Count > 1 && Vector3.Distance(startPos, waypoints[0]) < 0.1f)
        {
            waypoints.RemoveAt(0);
        }
    }

    private float CalculatePathLength(Vector3 start, List<Vector3> waypoints)
    {
        float dist = 0;
        Vector3 prev = start;
        foreach (var wp in waypoints)
        {
            dist += Vector3.Distance(prev, wp);
            prev = wp;
        }
        return dist;
    }
}
