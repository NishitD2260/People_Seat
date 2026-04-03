using UnityEngine;

public class PersonView : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;

    public SeatColor Color { get; private set; }
    public bool IsSeated { get; private set; }

    public void Initialize(SeatColor color, Material mat)
    {
        Color = color;
        IsSeated = false;

        if (meshRenderer == null)
            meshRenderer = GetComponentInChildren<MeshRenderer>();

        if (meshRenderer != null && mat != null)
            meshRenderer.material = mat;
    }

    public void SetSeated(bool seated)
    {
        IsSeated = seated;
    }
}
