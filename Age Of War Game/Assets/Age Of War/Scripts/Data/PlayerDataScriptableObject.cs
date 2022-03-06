using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "AgeOfWar/Player/PlayerData")]
public class PlayerDataScriptableObject : ScriptableObject
{
    public PlayerData Data = new PlayerData();
}

[System.Serializable]
public class PlayerData
{
    public string UserName;
    
    // Rank Mode Data
    public List<RankingData> RankedPlayData;

    // Quick Play Mode Data
    public List<RankingData> QuickPlayData;

    // Custom Game Mode Data
    public List<RankingData> CustomGamePlayData;

    // Custom Game Mode Data
    public List<RankingData> VsComputerPlayData;

    public RankingData GetPlayData(PlayModes PlayMode, RaceTypes RaceWanted)
    {
        if (PlayMode == PlayModes.Ranked)
        {
            foreach(RankingData Data in RankedPlayData)
            {
                if (Data.RaceType == RaceWanted)
                {
                    return Data;
                }
            }
        }
        else if (PlayMode == PlayModes.Quickplay)
        {
            foreach (RankingData Data in QuickPlayData)
            {
                if (Data.RaceType == RaceWanted)
                {
                    return Data;
                }
            }
        }
        else if (PlayMode == PlayModes.CustomGame)
        {
            foreach (RankingData Data in CustomGamePlayData)
            {
                if (Data.RaceType == RaceWanted)
                {
                    return Data;
                }
            }
        }
        else if (PlayMode == PlayModes.VsComputer)
        {
            foreach (RankingData Data in VsComputerPlayData)
            {
                if (Data.RaceType == RaceWanted)
                {
                    return Data;
                }
            }
        }

        return new RankingData();
    }
}


[System.Serializable]
public class RankingData
{
    public RaceTypes RaceType = RaceTypes.Unknown;
    public int RankingScore = 2500; // Starting rank

    public int BattlesPlayed = 0;
    public int BattlesWon = 0;
    public int BattlesLost = 0;
    public int BattlesTied = 0;
    public int BattlesLeftEarly = 0;
}

public enum PlayModes
{
    None,
    VsComputer,
    Quickplay,
    Ranked,
    CustomGame,
}