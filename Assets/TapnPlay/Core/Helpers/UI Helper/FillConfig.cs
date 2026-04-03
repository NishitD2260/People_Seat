using UnityEngine;

[CreateAssetMenu(menuName = "Progress/FillSpeedConfig")]
public class FillConfig : ScriptableObject
{
    public float fillSpeed = 1f; // units per second
    public Gradient colorGradient;
}
