using System;
using UnityEngine;

namespace Game.Core
{
    public static class DataHandler
    {
        public static void LoadGameData()
        {
            LeaderboardData[] leaderboards = Resources.LoadAll<LeaderboardData>("ScriptableObjects");

            foreach (LeaderboardData parameters in leaderboards)
                parameters.LoadFromFile();
        }

        public static void SaveGameData()
        {
            LeaderboardData[] leaderboards = Resources.LoadAll<LeaderboardData>("ScriptableObjects");

            foreach (LeaderboardData parameters in leaderboards)
                parameters.SaveToFile();
        }
    }
}