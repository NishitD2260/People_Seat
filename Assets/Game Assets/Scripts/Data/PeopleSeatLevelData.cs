using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SeatEntry
{
    public int row;
    public int col;
    public SeatColor color;
    public bool isBlocked;
}

[Serializable]
public class GridData
{
    public int totalRows;
    public int seatsPerSide;
    public List<int> dividerAfterRows = new List<int>();
    public List<SeatEntry> seats = new List<SeatEntry>();
}

[Serializable]
public class PeopleGroupData
{
    public SeatColor color;
    public int count;
}

[Serializable]
public class LaneData
{
    public List<PeopleGroupData> groups = new List<PeopleGroupData>();
}

[Serializable]
public class PeopleSeatLevelConfig
{
    public GridData gridData = new GridData();
    public List<LaneData> lanes = new List<LaneData>();
    public int waitingAreaCapacity = 10;
}
