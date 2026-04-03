using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PeopleGroupTapData
{
    public SeatColor Color;
    public int Count;
    public PeopleGroup Source;
    public int LaneIndex;
}

public class PeopleGroup : MonoBehaviour, IInteractable
{
    [SerializeField] private BoxCollider tapCollider;
    [Tooltip("Uniform scale multiplier while this group is behind the tappable (front) group in its lane.")]
    [SerializeField] private float nonTappableScale = 0.7f;
    [SerializeField] private float tappableScaleTweenDuration = 0.28f;

    public SeatColor Color { get; private set; }
    public int LaneIndex { get; private set; }
    public int PeopleCount => people.Count;

    private const string StackScaleTweenId = "PeopleGroupStackScale";

    private List<PersonView> people = new List<PersonView>();
    private bool isTappable;
    private Vector3 visualBaseLocalScale = Vector3.one;

    public void Initialize(PeopleGroupData data, int laneIndex, ColorThemeConfig theme, GameObject personPrefab)
    {
        Color = data.color;
        LaneIndex = laneIndex;
        isTappable = false;

        Material mat = theme != null ? theme.GetPeopleMaterial(data.color) : null;

        // Spawn person views as children in a cluster
        for (int i = 0; i < data.count; i++)
        {
            GameObject personObj = Instantiate(personPrefab, transform);
            Vector3 localPos = GetClusterPosition(i, data.count);
            personObj.transform.localPosition = localPos;

            PersonView pv = personObj.GetComponent<PersonView>();
            if (pv == null) pv = personObj.AddComponent<PersonView>();
            pv.Initialize(data.color, mat);
            people.Add(pv);
        }

        // Setup collider to encompass the group
        if (tapCollider == null)
            tapCollider = GetComponent<BoxCollider>();
        if (tapCollider == null)
            tapCollider = gameObject.AddComponent<BoxCollider>();

        UpdateColliderBounds();

        visualBaseLocalScale = transform.localScale;
    }

    private Vector3 GetClusterPosition(int index, int total)
    {
        // Arrange in 2 rows: back row and front row
        int cols = Mathf.CeilToInt(total / 2f);
        int row = index / cols;
        int col = index % cols;
        float xOffset = (col - (cols - 1) / 2f) * 0.5f;
        float zOffset = row * -0.5f;
        return new Vector3(xOffset, 0f, zOffset);
    }

    private void UpdateColliderBounds()
    {
        if (tapCollider == null) return;
        tapCollider.center = Vector3.zero;
        float width = Mathf.Max(1f, Mathf.CeilToInt(people.Count / 2f) * 0.5f);
        tapCollider.size = new Vector3(width, 1f, 1f);
    }

    public void SetTappable(bool value, bool animateScale = false)
    {
        isTappable = value;
        if (tapCollider != null)
            tapCollider.enabled = value;
        ApplyStackDepthScale(animateScale);
    }

    private void ApplyStackDepthScale(bool animate)
    {
        Vector3 target = isTappable
            ? visualBaseLocalScale
            : visualBaseLocalScale * nonTappableScale;

        DOTween.Kill(transform, StackScaleTweenId, false);
        if (animate && tappableScaleTweenDuration > 0f)
        {
            transform.DOScale(target, tappableScaleTweenDuration)
                .SetEase(Ease.OutBack)
                .SetId(StackScaleTweenId);
        }
        else
            transform.localScale = target;
    }

    public PersonView ReleaseTopPerson()
    {
        if (people.Count == 0) return null;
        int lastIndex = people.Count - 1;
        PersonView p = people[lastIndex];
        people.RemoveAt(lastIndex);
        p.transform.SetParent(null);
        return p;
    }

    public List<PersonView> ReleaseAllPeople()
    {
        List<PersonView> released = new List<PersonView>(people);
        foreach (var p in released)
        {
            p.transform.SetParent(null);
        }
        people.Clear();
        return released;
    }

    // IInteractable implementation
    public void OnTap()
    {
        if (!isTappable) return;

        transform.DOKill();
        transform.DOPunchScale(Vector3.one * 0.22f, 0.24f, vibrato: 10, elasticity: 0.72f);

        Debug.Log("PeopleGroup tapped: " + Color + " " + people.Count + " " + LaneIndex);
        EventController.TriggerEvent(GameEvent.EVENT_PEOPLE_GROUP_TAPPED,
            new PeopleGroupTapData
            {
                Color = Color,
                Count = people.Count,
                Source = this,
                LaneIndex = LaneIndex
            });
    }

    public void OnHoldStart() { }
    public void OnHolding() { }
    public void OnHoldEnd() { }
    public void OnBeginDrag() { }
    public void OnDrag(Vector3 pos) { }
    public void OnEndDrag() { }
}
