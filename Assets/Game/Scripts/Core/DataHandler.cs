using System;
using UnityEngine;

namespace Game.Core
{
    public static class DataHandler
    {
        public static void LoadGameData()
        {
            // load game parameters
            LeaderboardData[] gameParametersList = Resources.LoadAll<LeaderboardData>("ScriptableObjects/Parameters");
            foreach (LeaderboardData parameters in gameParametersList)
                parameters.LoadFromFile();
        }

        public static void SaveGameData()
        {
            // save game parameters
            LeaderboardData[] gameParametersList = Resources.LoadAll<LeaderboardData>("ScriptableObjects/Leaderboard");
            foreach (LeaderboardData parameters in gameParametersList)
                parameters.SaveToFile();
        }
    }
}