using System.Collections.Generic;
using UnityEngine;

namespace PeopleSeat.Gameplay
{
    /// <summary>
    /// Runtime visuals for authored level data:
    /// - Spawns seat grid from level blueprint (or seat list fallback)
    /// - Spawns lane group people placeholders
    /// - Keeps seat occupancy + lane front visuals synced after taps
    /// </summary>
    public class SeatingRuntimeSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameFlowController flow;
        [SerializeField] private SeatingLevelSO levelOverride;
        [SerializeField] private GameObject seatPrefab;
        [SerializeField] private Material redSeatMaterial;
        [SerializeField] private Material blueSeatMaterial;
        [SerializeField] private Material yellowSeatMaterial;
        [SerializeField] private Material pinkSeatMaterial;
        [SerializeField] private Material greenSeatMaterial;
        [SerializeField] private Material orangeSeatMaterial;

        [Header("Containers")]
        [SerializeField] private Transform seatRoot;
        [SerializeField] private Transform laneRoot;

        [Header("Seat Layout")]
        [SerializeField] private Vector3 seatOrigin = new Vector3(0f, 0f, -2f);
        [SerializeField] private float aisleHalfGap = 1.15f;
        [SerializeField] private float seatDepthSpacing = 1.0f;
        [SerializeField] private float rowSpacing = 1.0f;
        [SerializeField] private float topBottomGap = 1.4f;

        [Header("Lane Layout")]
        [SerializeField] private Vector3 laneOrigin = new Vector3(0f, 0.15f, -8f);
        [SerializeField] private float laneSpacingX = 4.4f;
        [SerializeField] private float laneGroupSpacingZ = 1.2f;
        [SerializeField] private float personSpacing = 0.45f;
        [SerializeField] private float personScale = 0.32f;

        private readonly List<GameObject> _seatVisualsByIndex = new List<GameObject>();
        private readonly List<Queue<GameObject>> _laneFrontGroups = new List<Queue<GameObject>>();

        private void Awake()
        {
            if (flow == null)
                flow = GetComponent<GameFlowController>() ?? GetComponentInParent<GameFlowController>();
        }

        private void Start()
        {
            var level = levelOverride != null ? levelOverride : flow?.Level;
            if (flow == null || level == null)
            {
                Debug.LogWarning($"{nameof(SeatingRuntimeSpawner)} missing references. Flow: {flow != null}, Level: {level != null}");
                return;
            }

            if (flow.Grid == null)
                flow.InitializeFromLevel(level);

            BuildVisuals(level);
            ApplySeatOccupancyView();
            ApplyLaneFrontGateView();

            flow.OnPlacementResolved += OnPlacementResolved;
            flow.OnPlacementRejected += OnPlacementRejected;
        }

        private void OnDestroy()
        {
            if (flow == null) return;
            flow.OnPlacementResolved -= OnPlacementResolved;
            flow.OnPlacementRejected -= OnPlacementRejected;
        }

        private void BuildVisuals(SeatingLevelSO level)
        {
            EnsureRoots();
            ClearChildren(seatRoot);
            ClearChildren(laneRoot);
            _seatVisualsByIndex.Clear();
            _laneFrontGroups.Clear();

            if (level.GridBlueprint != null && level.GridBlueprint.RowsBottom + level.GridBlueprint.RowsTop > 0)
                SpawnSeatsFromBlueprint(level);
            else
                SpawnSeatsFallbackFromList(level.Seats);

            SpawnLaneGroups(level);
        }

        private void SpawnSeatsFromBlueprint(SeatingLevelSO level)
        {
            var bp = level.GridBlueprint;
            bp.EnsureListSizes();

            var idx = 0;
            for (var row = 0; row < bp.RowsBottom; row++)
            {
                idx = SpawnLeftRow(bp.BottomLeft, row, bp.ColsLeft, idx, bottomHalf: true, bp.RowsBottom);
                idx = SpawnRightRow(bp.BottomRight, row, bp.ColsRight, idx, bottomHalf: true, bp.RowsBottom);
            }

            for (var row = 0; row < bp.RowsTop; row++)
            {
                idx = SpawnLeftRow(bp.TopLeft, row, bp.ColsLeft, idx, bottomHalf: false, bp.RowsBottom);
                idx = SpawnRightRow(bp.TopRight, row, bp.ColsRight, idx, bottomHalf: false, bp.RowsBottom);
            }
        }

        private int SpawnLeftRow(IReadOnlyList<PersonColor> cells, int row, int cols, int seatIndexStart, bool bottomHalf, int rowsBottom)
        {
            var seatIndex = seatIndexStart;
            for (var col = cols - 1; col >= 0; col--)
            {
                var depth = cols - 1 - col;
                var color = cells[row * cols + col];
                var pos = ComputeSeatPosition(leftSide: true, depth, row, bottomHalf, rowsBottom);
                SpawnSeatVisual(seatIndex++, color, pos);
            }

            return seatIndex;
        }

        private int SpawnRightRow(IReadOnlyList<PersonColor> cells, int row, int cols, int seatIndexStart, bool bottomHalf, int rowsBottom)
        {
            var seatIndex = seatIndexStart;
            for (var col = 0; col < cols; col++)
            {
                var depth = col;
                var color = cells[row * cols + col];
                var pos = ComputeSeatPosition(leftSide: false, depth, row, bottomHalf, rowsBottom);
                SpawnSeatVisual(seatIndex++, color, pos);
            }

            return seatIndex;
        }

        private void SpawnSeatsFallbackFromList(IReadOnlyList<SeatAuthoring> seats)
        {
            for (var i = 0; i < seats.Count; i++)
            {
                var s = seats[i];
                var x = s.DepthFromAisle == 0 ? 0f : (s.DepthFromAisle * seatDepthSpacing + aisleHalfGap);
                var side = (s.AisleProgress % 2 == 0) ? -1f : 1f;
                var pos = seatOrigin + new Vector3(side * x, 0f, s.AisleProgress * 0.25f);
                SpawnSeatVisual(i, s.Color, pos);
            }
        }

        private Vector3 ComputeSeatPosition(bool leftSide, int depth, int row, bool bottomHalf, int rowsBottom)
        {
            var x = leftSide
                ? -(aisleHalfGap + depth * seatDepthSpacing)
                : +(aisleHalfGap + depth * seatDepthSpacing);

            var z = bottomHalf
                ? row * rowSpacing
                : rowsBottom * rowSpacing + topBottomGap + row * rowSpacing;

            return seatOrigin + new Vector3(x, 0f, z);
        }

        private void SpawnSeatVisual(int seatIndex, PersonColor color, Vector3 worldPos)
        {
            var go = seatPrefab != null
                ? Instantiate(seatPrefab, worldPos, Quaternion.identity, seatRoot)
                : GameObject.CreatePrimitive(PrimitiveType.Cube);

            if (seatPrefab == null)
            {
                go.transform.SetParent(seatRoot, true);
                go.transform.position = worldPos;
                go.transform.localScale = new Vector3(0.9f, 0.25f, 0.9f);
                go.name = $"Seat_{seatIndex}";
            }
            else
            {
                go.name = $"Seat_{seatIndex}";
            }

            ApplySeatVisualColor(go, color);

            while (_seatVisualsByIndex.Count <= seatIndex)
                _seatVisualsByIndex.Add(null);
            _seatVisualsByIndex[seatIndex] = go;
        }

        private void SpawnLaneGroups(SeatingLevelSO level)
        {
            if (level.Lanes == null) return;

            var laneCount = level.Lanes.Count;
            var startX = laneOrigin.x - ((laneCount - 1) * laneSpacingX * 0.5f);

            for (var laneIdx = 0; laneIdx < laneCount; laneIdx++)
            {
                var laneQueue = new Queue<GameObject>();
                _laneFrontGroups.Add(laneQueue);

                var laneDef = level.Lanes[laneIdx];
                if (laneDef?.GroupsFrontToBack == null) continue;

                var laneX = startX + laneIdx * laneSpacingX;
                for (var groupIdx = 0; groupIdx < laneDef.GroupsFrontToBack.Count; groupIdx++)
                {
                    var g = laneDef.GroupsFrontToBack[groupIdx];
                    var groupRoot = new GameObject($"Lane_{laneIdx}_Group_{groupIdx}_{g.Color}_{g.Count}");
                    groupRoot.transform.SetParent(laneRoot, false);
                    groupRoot.transform.position = new Vector3(laneX, laneOrigin.y, laneOrigin.z - groupIdx * laneGroupSpacingZ);

                    SpawnPeopleCluster(groupRoot.transform, g.Color, g.Count);
                    laneQueue.Enqueue(groupRoot);
                }
            }
        }

        private void SpawnPeopleCluster(Transform parent, PersonColor color, int count)
        {
            for (var i = 0; i < count; i++)
            {
                var p = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                p.name = $"{color}_Person_{i}";
                p.transform.SetParent(parent, false);
                var row = i / 3;
                var col = i % 3;
                p.transform.localPosition = new Vector3((col - 1) * personSpacing, 0.16f, -row * personSpacing);
                p.transform.localScale = Vector3.one * personScale;
                ApplyTintColor(p, color);
            }
        }

        private void OnPlacementResolved(int laneIndex, PlacementOutcome outcome, int movedToWaiting)
        {
            if (laneIndex >= 0 && laneIndex < _laneFrontGroups.Count && _laneFrontGroups[laneIndex].Count > 0)
            {
                var root = _laneFrontGroups[laneIndex].Dequeue();
                if (root != null) Destroy(root);
            }

            ApplySeatOccupancyView();
            ApplyLaneFrontGateView();
        }

        private void OnPlacementRejected(int laneIndex, PlacementOutcome outcome, int movedToWaiting)
        {
            ApplyLaneFrontGateView();
        }

        private void ApplySeatOccupancyView()
        {
            if (flow?.Grid == null) return;
            var seats = flow.Grid.Seats;
            for (var i = 0; i < seats.Count && i < _seatVisualsByIndex.Count; i++)
            {
                var go = _seatVisualsByIndex[i];
                if (go == null) continue;

                var occupied = seats[i].Occupied;
                var scale = go.transform.localScale;
                scale.y = occupied ? Mathf.Max(0.34f, scale.y) : Mathf.Min(0.25f, scale.y);
                go.transform.localScale = scale;
            }
        }

        private void ApplyLaneFrontGateView()
        {
            for (var lane = 0; lane < _laneFrontGroups.Count; lane++)
            {
                var arr = _laneFrontGroups[lane].ToArray();
                for (var i = 0; i < arr.Length; i++)
                {
                    var isFront = i == 0;
                    var go = arr[i];
                    if (go == null) continue;
                    SetChildrenAlpha(go.transform, isFront ? 1f : 0.45f);
                }
            }
        }

        private static void SetChildrenAlpha(Transform root, float alpha)
        {
            for (var i = 0; i < root.childCount; i++)
            {
                var r = root.GetChild(i).GetComponent<Renderer>();
                if (r == null) continue;
                var c = r.material.color;
                c.a = alpha;
                r.material.color = c;
            }
        }

        private void ApplySeatVisualColor(GameObject go, PersonColor color)
        {
            var mat = MaterialFor(color);
            var renderers = go.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0) return;

            for (var i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null) continue;
                if (mat != null)
                    renderers[i].material = mat;
                else
                    renderers[i].material.color = ToUnityColor(color);
            }
        }

        private static void ApplyTintColor(GameObject go, PersonColor color)
        {
            var r = go.GetComponent<Renderer>();
            if (r == null) return;
            r.material.color = ToUnityColor(color);
        }

        private Material MaterialFor(PersonColor color)
        {
            switch (color)
            {
                case PersonColor.Red: return redSeatMaterial;
                case PersonColor.Blue: return blueSeatMaterial;
                case PersonColor.Yellow: return yellowSeatMaterial;
                case PersonColor.Pink: return pinkSeatMaterial;
                case PersonColor.Green: return greenSeatMaterial;
                case PersonColor.Orange: return orangeSeatMaterial;
                default: return null;
            }
        }

        private static Color ToUnityColor(PersonColor c)
        {
            switch (c)
            {
                case PersonColor.Red: return new Color(0.93f, 0.26f, 0.24f);
                case PersonColor.Blue: return new Color(0.24f, 0.47f, 0.93f);
                case PersonColor.Yellow: return new Color(0.95f, 0.88f, 0.19f);
                case PersonColor.Pink: return new Color(0.91f, 0.28f, 0.82f);
                case PersonColor.Green: return new Color(0.24f, 0.85f, 0.31f);
                case PersonColor.Orange: return new Color(0.95f, 0.58f, 0.18f);
                default: return Color.white;
            }
        }

        private void EnsureRoots()
        {
            if (seatRoot == null)
            {
                var go = new GameObject("SeatVisuals");
                go.transform.SetParent(transform, false);
                seatRoot = go.transform;
            }

            if (laneRoot == null)
            {
                var go = new GameObject("LaneVisuals");
                go.transform.SetParent(transform, false);
                laneRoot = go.transform;
            }
        }

        private static void ClearChildren(Transform t)
        {
            for (var i = t.childCount - 1; i >= 0; i--)
            {
                var c = t.GetChild(i).gameObject;
#if UNITY_EDITOR
                if (!Application.isPlaying) Object.DestroyImmediate(c);
                else Object.Destroy(c);
#else
                Object.Destroy(c);
#endif
            }
        }
    }
}
