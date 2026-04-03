using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Create/LevelsList", fileName = "LevelList")]
public class LevelsListSO : ScriptableObject
{
    public List<LevelSO> Levels;
}
