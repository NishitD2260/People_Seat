using DG.Tweening;
using UnityEngine;

public enum GridSide { Left, Right }

public class Seat : MonoBehaviour
{
    [SerializeField] private MeshRenderer baseMesh;
    // [SerializeField] private MeshRenderer backrestMesh;

    [Header("Accessibility visuals")]
    [Tooltip("Local scale multiplier when seat is not playable yet (inaccessible). 1 = same as base.")]
    [SerializeField] [Range(0.3f, 1f)] private float inaccessibleScalePercent = 0.82f;

    [Tooltip("If off, inaccessible state uses uniform XYZ scale. If on, only local Z (forward) shrinks — pivot at forward face (unfold along depth).")]
    [SerializeField] private bool shrinkHeightAxisOnly = true;

    [Tooltip("Scale-up when seat becomes active (playable) — OutBack gives the punch.")]
    [SerializeField] private float scaleUpDuration = 0.38f;

    [Tooltip("Scale-down when seat becomes inactive.")]
    [SerializeField] private float scaleDownDuration = 0.26f;

    [SerializeField] private Ease scaleUpEase = Ease.OutBack;

    [SerializeField] private Ease scaleDownEase = Ease.InOutSine;

    private Material defaultMaterial;
    private Vector3 _baseLocalScale = Vector3.one;
    private bool _hasBaseScale;

    private Renderer _renderer;
    private Vector3 _lockedAnchorWorld;
    private bool _hasLockedAnchor;

    public SeatColor Color { get; private set; }
    public int Row { get; private set; }
    public int AisleDistance { get; private set; }
    public GridSide Side { get; private set; }
    public bool IsOccupied { get; private set; }
    public bool IsBlocked { get; private set; }

    /// <summary>Someone has an active walk anim targeting this seat (resolver treats like blocked).</summary>
    public bool HasWalkIncoming => _walkIncomingCount > 0;

    private int _walkIncomingCount;

    /// <summary>When true, <see cref="SetAccessibilityVisual"/> matches <see cref="_lastIsAccessible"/> and skips material/scale so one sit does not re-unlock the whole grid.</summary>
    private bool _hasAccessibilityVisualState;

    private bool _lastIsAccessible;

    public void AddWalkIncoming()
    {
        _walkIncomingCount++;
    }

    /// <param name="baseSeatMaterial">Theme base material (fallback for occupied / null refs).</param>
    /// <param name="initialVacantSurfaceMaterial">Usually inactive seat material — vacant seats start dim, not bright.</param>
    public void Initialize(SeatColor color, int row, int aisleDistance, GridSide side, Material baseSeatMaterial, Material initialVacantSurfaceMaterial, bool isBlocked = false)
    {
        Color = color;
        Row = row;
        AisleDistance = aisleDistance;
        Side = side;
        IsBlocked = isBlocked;
        IsOccupied = false;
        _walkIncomingCount = 0;
        CaptureBaseScale();

        defaultMaterial = baseSeatMaterial != null ? baseSeatMaterial : initialVacantSurfaceMaterial;
        Material surface = initialVacantSurfaceMaterial != null ? initialVacantSurfaceMaterial : baseSeatMaterial;
        if (surface != null)
            ApplyMaterial(surface);

        gameObject.SetActive(!isBlocked);

        // Do not set _hasAccessibilityVisualState here — Initialize used to apply bright mat + mark "inactive",
        // so SetAccessibilityVisual(false) skipped and never swapped to inaccessible material.
        // Scale + active/inactive materials come from the first RefreshSeatAccessibilityVisuals.
        if (!isBlocked)
            TweenAccessibilityScale(isAccessible: false, immediate: true);
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }

    private void EnsureRenderer()
    {
        if (_renderer == null)
            _renderer = baseMesh != null ? baseMesh : GetComponentInChildren<Renderer>();
    }

    private void CaptureBaseScale()
    {
        EnsureRenderer();
        _baseLocalScale = transform.localScale;
        if (_baseLocalScale.sqrMagnitude < 1e-6f)
            _baseLocalScale = Vector3.one;
        _hasBaseScale = true;
    }

    /// <summary>Center of the renderer AABB face most aligned with <paramref name="forward"/> (seat scales around this world point).</summary>
    private static Vector3 ForwardFaceCenterWorld(Bounds b, Vector3 forward)
    {
        if (forward.sqrMagnitude < 1e-6f)
            forward = Vector3.forward;
        forward.Normalize();

        Vector3 c = b.center;
        Vector3 e = b.extents;
        float ax = Mathf.Abs(forward.x);
        float ay = Mathf.Abs(forward.y);
        float az = Mathf.Abs(forward.z);

        Vector3 face = c;
        if (ax >= ay && ax >= az)
            face.x = c.x + Mathf.Sign(forward.x) * e.x;
        else if (ay >= az)
            face.y = c.y + Mathf.Sign(forward.y) * e.y;
        else
            face.z = c.z + Mathf.Sign(forward.z) * e.z;
        return face;
    }

    private void CaptureLockedAnchorFromCurrentPose()
    {
        EnsureRenderer();
        if (_renderer == null)
        {
            _hasLockedAnchor = false;
            return;
        }

        _lockedAnchorWorld = ForwardFaceCenterWorld(_renderer.bounds, transform.forward);
        _hasLockedAnchor = true;
    }

    private Vector3 GetTargetLocalScale(bool isAccessible)
    {
        float m = isAccessible ? 1f : inaccessibleScalePercent;
        if (!shrinkHeightAxisOnly)
            return _baseLocalScale * m;

        // Local X = width, Y = up (unchanged), Z = forward — pivot on forward face keeps front stable while depth folds.
        return Vector3.Scale(_baseLocalScale, new Vector3(1f, 1f, m));
    }

    private void SnapTransformToLockedAnchor()
    {
        if (!_hasLockedAnchor || _renderer == null) return;

        Vector3 anchorNow = ForwardFaceCenterWorld(_renderer.bounds, transform.forward);
        transform.position += _lockedAnchorWorld - anchorNow;
    }

    private void TweenAccessibilityScale(bool isAccessible, bool immediate)
    {
        if (!_hasBaseScale) CaptureBaseScale();

        transform.DOKill();

        var target = GetTargetLocalScale(isAccessible);

        CaptureLockedAnchorFromCurrentPose();

        if (immediate)
        {
            transform.localScale = target;
            SnapTransformToLockedAnchor();
            return;
        }

        var duration = isAccessible ? scaleUpDuration : scaleDownDuration;
        var ease = isAccessible ? scaleUpEase : scaleDownEase;

        transform.DOScale(target, duration)
            .SetEase(ease)
            .OnUpdate(SnapTransformToLockedAnchor)
            .SetLink(gameObject);
    }

    public void MarkOccupied()
    {
        IsOccupied = true;
        _walkIncomingCount = 0;
    }

    public void MarkVacated()
    {
        IsOccupied = false;
        _walkIncomingCount = 0;
        _hasAccessibilityVisualState = false;
    }

    public void SetMeshRenderers(MeshRenderer baseMR, MeshRenderer backrestMR)
    {
        baseMesh = baseMR;
        _renderer = baseMR;
        //  backrestMesh = backrestMR;
    }

    public void SetAccessibilityVisual(bool isAccessible, Material accessibleMaterial, Material inaccessibleMaterial)
    {
        if (IsBlocked || IsOccupied) return;

        if (_hasAccessibilityVisualState && _lastIsAccessible == isAccessible)
            return;

        _hasAccessibilityVisualState = true;
        _lastIsAccessible = isAccessible;

        Material target = isAccessible ? accessibleMaterial : inaccessibleMaterial;
        if (target == null) target = defaultMaterial;
        ApplyMaterial(target);
        TweenAccessibilityScale(isAccessible, immediate: false);
    }

    /// <summary>Filled seat: full scale, surface material typically inactive/inaccessible from theme.</summary>
    public void RefreshFilledSeatVisual(Material seatSurfaceMaterial)
    {
        var mat = seatSurfaceMaterial != null ? seatSurfaceMaterial : defaultMaterial;
        ApplyMaterial(mat);
        if (!_hasBaseScale) CaptureBaseScale();

        transform.DOKill();

        CaptureLockedAnchorFromCurrentPose();

        transform.DOScale(_baseLocalScale, scaleUpDuration)
            .SetEase(scaleUpEase)
            .OnUpdate(SnapTransformToLockedAnchor)
            .SetLink(gameObject);
    }

    private void ApplyMaterial(Material mat)
    {
        if (mat == null) return;
        if (baseMesh != null) baseMesh.material = mat;
        //  if (backrestMesh != null) backrestMesh.material = mat;
    }
}
