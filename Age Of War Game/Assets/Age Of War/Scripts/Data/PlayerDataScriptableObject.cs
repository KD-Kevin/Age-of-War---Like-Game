using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AgeOfWar.Data
{
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

        public PlayerData()
        {
            UserName = "No Name";
            RankedPlayData = new List<RankingData>();
            QuickPlayData = new List<RankingData>();
            CustomGamePlayData = new List<RankingData>();
            VsComputerPlayData = new List<RankingData>();
        }

        public string SerializeToJSON()
        {
            JsonSerializerSettings setting = new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            string json = JsonConvert.SerializeObject(this, setting);

            return json;
        }

        public byte[] SerializeToByteArray()
        {
            string jsonSerialization = SerializeToJSON();
            using (MemoryStream m = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(m))
                {
                    writer.Write(jsonSerialization);
                }
                return m.ToArray();
            }
        }

        public static PlayerData Deserialize(string json)
        {
            PlayerData PlayerData = JsonConvert.DeserializeObject<PlayerData>(json);
            return PlayerData;
        }

        public static PlayerData Deserialize(byte[] bytaArr)
        {
            string json;
            using (MemoryStream m = new MemoryStream(bytaArr))
            {
                using (BinaryReader reader = new BinaryReader(m))
                {
                    json = reader.ReadString();
                }
            }
            return Deserialize(json);
        }

        public void SaveLocal(string saveName)
        {
            string path = Path.Combine(Application.streamingAssetsPath + "/PlayerData/", $"PlayerData'{saveName}'.json");
            string json = SerializeToJSON();
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            File.WriteAllText(path, json);
        }

        public static PlayerData LoadLocal(string saveName)
        {
            string path = Path.Combine(Application.streamingAssetsPath + "/PlayerData/", $"PlayerData'{saveName}'.json");
            if (File.Exists(path))
            {
                string fileContent = File.ReadAllText(path);
                PlayerData PlayerData = Deserialize(fileContent);
                return PlayerData;
            }
            else
            {
                return null;
            }
        }

        public RankingData GetPlayData(PlayModes PlayMode, RaceTypes RaceWanted)
        {
            if (PlayMode == PlayModes.Ranked)
            {
                foreach (RankingData Data in RankedPlayData)
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
}