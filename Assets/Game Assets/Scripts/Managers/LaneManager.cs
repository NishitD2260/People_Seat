using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class LaneManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform[] laneParents; // 3 transforms for lane positions
    [SerializeField] private GameObject peopleGroupPrefab;
    [SerializeField] private GameObject personPrefab;

    [Header("Layout")]
    [SerializeField] private float groupSpacing = 1.5f;

    private List<PeopleGroup>[] lanes;

    private const string LaneLocalMoveTweenId = "LaneLocalMove";

    public void BuildLanes(List<LaneData> laneDataList, ColorThemeConfig theme)
    {
        ClearLanes();

        int laneCount = Mathf.Min(laneDataList.Count, laneParents.Length);
        lanes = new List<PeopleGroup>[laneCount];

        for (int laneIdx = 0; laneIdx < laneCount; laneIdx++)
        {
            lanes[laneIdx] = new List<PeopleGroup>();
            LaneData ld = laneDataList[laneIdx];

            if (ld.groups == null) continue;

            for (int groupIdx = 0; groupIdx < ld.groups.Count; groupIdx++)
            {
                GameObject groupObj;
                if (peopleGroupPrefab != null)
                {
                    groupObj = Instantiate(peopleGroupPrefab, laneParents[laneIdx]);
                }
                else
                {
                    groupObj = new GameObject($"PeopleGroup_{laneIdx}_{groupIdx}");
                    groupObj.transform.SetParent(laneParents[laneIdx]);
                }

                // index 0 = topmost (first tappable), stack downward
                groupObj.transform.localPosition = new Vector3(0f, 0f, -groupIdx * groupSpacing);

                PeopleGroup pg = groupObj.GetComponent<PeopleGroup>();
                if (pg == null) pg = groupObj.AddComponent<PeopleGroup>();
                pg.Initialize(ld.groups[groupIdx], laneIdx, theme, personPrefab);
                pg.SetTappable(false);

                lanes[laneIdx].Add(pg);
            }

            // Only the first group (index 0) is tappable
            if (lanes[laneIdx].Count > 0)
            {
                lanes[laneIdx][0].SetTappable(true);
            }
        }
    }

    public void RemoveTopGroup(int laneIndex)
    {
        if (laneIndex < 0 || laneIndex >= lanes.Length) return;
        var lane = lanes[laneIndex];
        if (lane.Count == 0) return;

        PeopleGroup top = lane[0];
        lane.RemoveAt(0);
        Destroy(top.gameObject);

        // Enable next top group
        if (lane.Count > 0)
        {
            AnimateLaneShift(laneIndex);
            lane[0].SetTappable(true, animateScale: true);
        }

        EventController.TriggerEvent(GameEvent.EVENT_WAITING_AREA_UPDATED);
    }

    private void AnimateLaneShift(int laneIndex)
    {
        var lane = lanes[laneIndex];
        for (int i = 0; i < lane.Count; i++)
        {
            Transform gt = lane[i].transform;
            DOTween.Kill(gt, LaneLocalMoveTweenId, false);
            Vector3 targetPos = new Vector3(0f, 0f, -i * groupSpacing);
            gt.DOLocalMove(targetPos, 0.32f)
                .SetEase(Ease.OutBack)
                .SetId(LaneLocalMoveTweenId);
        }
    }

    public PeopleGroup GetTopGroup(int laneIndex)
    {
        if (laneIndex < 0 || laneIndex >= lanes.Length) return null;
        if (lanes[laneIndex].Count == 0) return null;
        return lanes[laneIndex][0];
    }

    public bool AllLanesEmpty()
    {
        if (lanes == null) return true;
        for (int i = 0; i < lanes.Length; i++)
        {
            if (lanes[i].Count > 0) return false;
        }
        return true;
    }

    public int LaneCount => lanes != null ? lanes.Length : 0;

    public int GetTotalRemainingPeople()
    {
        if (lanes == null) return 0;
        int total = 0;
        for (int i = 0; i < lanes.Length; i++)
        {
            for (int j = 0; j < lanes[i].Count; j++)
            {
                total += lanes[i][j].PeopleCount;
            }
        }
        return total;
    }

    public void ClearLanes()
    {
        if (lanes != null)
        {
            foreach (var lane in lanes)
            {
                if (lane != null)
                {
                    foreach (var group in lane)
                    {
                        if (group != null) Destroy(group.gameObject);
                    }
                }
            }
        }
        lanes = null;
    }
}
