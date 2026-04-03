using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Create/ColorThemeConfig", fileName = "ColorThemeConfig")]
public class ColorThemeConfig : ScriptableObject
{
    [Serializable]
    public struct ColorMaterialPair
    {
        public SeatColor color;
        public Material material;
        public Material accessibleSeatMaterial;
        public Material inaccessibleSeatMaterial;
        public Material peopleMaterial;
    }

    public ColorMaterialPair[] mappings;

    public Material GetMaterial(SeatColor color)
    {
        for (int i = 0; i < mappings.Length; i++)
        {
            if (mappings[i].color == color)
                return mappings[i].material;
        }
        return null;
    }

    public Material GetPeopleMaterial(SeatColor color)
    {
        for (int i = 0; i < mappings.Length; i++)
        {
            if (mappings[i].color == color)
                return mappings[i].peopleMaterial;
        }
        return null;
    }

    public Material GetAccessibleSeatMaterial(SeatColor color)
    {
        for (int i = 0; i < mappings.Length; i++)
        {
            if (mappings[i].color == color)
                return mappings[i].accessibleSeatMaterial;
        }
        return null;
    }

    public Material GetInaccessibleSeatMaterial(SeatColor color)
    {
        for (int i = 0; i < mappings.Length; i++)
        {
            if (mappings[i].color == color)
                return mappings[i].inaccessibleSeatMaterial;
        }
        return null;
    }
}
