using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject seatPrefab;
    [SerializeField] private Transform gridParent;
    [SerializeField] private GameObject dividerPrefab;

    [Header("Layout Settings")]
    [SerializeField] private float seatSpacing = 1.2f;
    [SerializeField] private float aisleWidth = 2.0f;
    [SerializeField] private float dividerHeight = 0.1f;
    [SerializeField] private float rowGapForDivider = 0.6f;

    private Seat[,] leftSeats;   // [row, aisleDistance]
    private Seat[,] rightSeats;  // [row, aisleDistance]
    private Vector3[] aislePositions;
    private float[] rowZPositions;

    public int RowCount { get; private set; }
    public int SeatsPerSide { get; private set; }

    private List<int> dividerRows = new List<int>();
    private List<GameObject> spawnedObjects = new List<GameObject>();

    public void BuildGrid(GridData data, ColorThemeConfig theme)
    {
        ClearGrid();

        RowCount = data.totalRows;
        SeatsPerSide = data.seatsPerSide;

        leftSeats = new Seat[RowCount, SeatsPerSide];
        rightSeats = new Seat[RowCount, SeatsPerSide];
        aislePositions = new Vector3[RowCount];
        rowZPositions = new float[RowCount];
        dividerRows = data.dividerAfterRows ?? new List<int>();

        // Build a lookup for seat config
        Dictionary<(int, int), SeatEntry> seatLookup = new Dictionary<(int, int), SeatEntry>();
        if (data.seats != null)
        {
            foreach (var entry in data.seats)
            {
                seatLookup[(entry.row, entry.col)] = entry;
            }
        }

        // Calculate starting Z so grid is centered
        float totalGridHeight = CalculateTotalGridHeight();
        float startZ = totalGridHeight / 2f;

        float currentZ = startZ + gridParent.position.z;

        for (int row = 0; row < RowCount; row++)
        {
            rowZPositions[row] = currentZ;
            aislePositions[row] = new Vector3(0f, 0f, currentZ);

            // Left seats: columns 0..(seatsPerSide-1) where col 0 = far left, col (seatsPerSide-1) = near aisle
            for (int d = 0; d < SeatsPerSide; d++)
            {
                int col = d; // col in data: 0 = leftmost (far edge), seatsPerSide-1 = near aisle
                float x = -(aisleWidth / 2f + (SeatsPerSide - 1 - d) * seatSpacing + seatSpacing / 2f);

                SeatEntry entry = seatLookup.ContainsKey((row, col)) ? seatLookup[(row, col)] : null;
                if (entry != null)
                {
                    Seat seat = SpawnSeat(new Vector3(x, 0f, currentZ), row, d, GridSide.Left, entry, theme);
                    leftSeats[row, d] = seat;
                }
            }

            // Right seats: columns seatsPerSide..(seatsPerSide*2-1) where seatsPerSide = near aisle, last = far right
            for (int d = 0; d < SeatsPerSide; d++)
            {
                int col = SeatsPerSide + d; // col in data
                float x = aisleWidth / 2f + (SeatsPerSide - 1 - d) * seatSpacing + seatSpacing / 2f;

                SeatEntry entry = seatLookup.ContainsKey((row, col)) ? seatLookup[(row, col)] : null;
                if (entry != null)
                {
                    Seat seat = SpawnSeat(new Vector3(x, 0f, currentZ), row, d, GridSide.Right, entry, theme);
                    rightSeats[row, d] = seat;
                }
            }

            currentZ -= seatSpacing;

            // Add divider gap after this row if needed
            if (dividerRows.Contains(row))
            {
                SpawnDivider(currentZ + seatSpacing / 2f);
                currentZ -= rowGapForDivider;
            }
        }
    }

    private Seat SpawnSeat(Vector3 position, int row, int aisleDistance, GridSide side, SeatEntry entry, ColorThemeConfig theme)
    {
        Transform parent = gridParent != null ? gridParent : transform;
        GameObject obj = Instantiate(seatPrefab, position, Quaternion.identity, parent);
        spawnedObjects.Add(obj);

        // Rotate to face the aisle
        // float yRotation = side == GridSide.Left ? 90f : -90f;
        // obj.transform.rotation = Quaternion.Euler(0f, yRotation, 0f);

        Seat seat = obj.GetComponent<Seat>();
        if (seat == null) seat = obj.AddComponent<Seat>();

        // Find mesh renderers on children
        MeshRenderer[] renderers = obj.GetComponentsInChildren<MeshRenderer>();
        if (renderers.Length >= 2)
        {
            seat.SetMeshRenderers(renderers[0], renderers[1]);
        }
        else if (renderers.Length == 1)
        {
            seat.SetMeshRenderers(renderers[0], null);
        }

        Material baseMat = theme != null ? theme.GetMaterial(entry.color) : null;
        Material inactiveMat = theme != null ? theme.GetInaccessibleSeatMaterial(entry.color) : null;
        seat.Initialize(entry.color, row, aisleDistance, side, baseMat, inactiveMat, entry.isBlocked);

        obj.name = $"Seat_{side}_{row}_{aisleDistance}";
        return seat;
    }

    private void SpawnDivider(float zPosition)
    {
        if (dividerPrefab != null)
        {
            Transform parent = gridParent != null ? gridParent : transform;
            GameObject divider = Instantiate(dividerPrefab, new Vector3(0f, dividerHeight, zPosition), Quaternion.identity, parent);
            spawnedObjects.Add(divider);
        }
    }

    private float CalculateTotalGridHeight()
    {
        float height = (RowCount - 1) * seatSpacing;
        if (dividerRows != null)
            height += dividerRows.Count * rowGapForDivider;
        return height;
    }

    public void ClearGrid()
    {
        foreach (var obj in spawnedObjects)
        {
            if (obj != null) Destroy(obj);
        }
        spawnedObjects.Clear();
    }

    // --- Public accessors ---

    public Seat GetSeat(int row, GridSide side, int aisleDistance)
    {
        if (row < 0 || row >= RowCount || aisleDistance < 0 || aisleDistance >= SeatsPerSide)
            return null;

        return side == GridSide.Left ? leftSeats[row, aisleDistance] : rightSeats[row, aisleDistance];
    }

    public Vector3 GetAisleWorldPosition(int row)
    {
        if (row >= 0 && row < RowCount)
            return aislePositions[row];
        return Vector3.zero;
    }

    public Vector3 GetAisleEntryBottom()
    {
        if (RowCount == 0) return Vector3.zero;
        float lowestRowZ = rowZPositions[RowCount - 1];
        return new Vector3(0f, 0f, lowestRowZ - seatSpacing);
    }

    public Vector3 GetSeatWorldPosition(Seat seat)
    {
        return seat.transform.position;
    }

    public float SeatSpacing => seatSpacing;
    public float AisleWidth => aisleWidth;
}
