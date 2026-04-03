using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

public class ThemeManager : MonoBehaviour
{
    [SerializeField] List<Material> ThemeAssociatedMat;
    [SerializeField] Material BaseMat;
    [SerializeField] int LevelInterval;

    private void OnEnable()
    {
        EventController.StartListening(GameEvent.EVENT_LEVEL_LOADED_NUMBER, LoadTheme);
    }

    void LoadTheme(object args)
    {
        int level = (int)args;
        BaseMat.CopyPropertiesFromMaterial(GetMaterialForLevel(level));
    }

    Material GetMaterialForLevel(int level)
    {
        if (ThemeAssociatedMat == null || ThemeAssociatedMat.Count == 0 || LevelInterval <= 0)
            return null;

        int themeIndex = ((level - 1) / LevelInterval) % ThemeAssociatedMat.Count;
        return ThemeAssociatedMat[themeIndex];
    }

}
