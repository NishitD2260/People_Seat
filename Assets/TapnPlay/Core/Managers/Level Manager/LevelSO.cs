using System.Collections.Generic;
using TapNPlay.Core.Data;
using UnityEngine;

[CreateAssetMenu(menuName = "Create/Level", fileName = "Level")]
public class LevelSO : ScriptableObject
{
    public LevelType LevelType;
    public List<BoosterReward> BoosterRewardList;
    public PeopleSeatLevelConfig PeopleSeatConfig;
}
