using System;
using UnityEngine;

namespace Game.Core
{
    [Serializable]
    public class LeaderboardPosition
    {
        public string name;
        public int score;
    }

    [Serializable]
    [CreateAssetMenu(fileName = "Leaderboard", menuName = "Game/Data/Leaderboard Data")]
    public class LeaderboardData : JSONSerializableScriptableObject
    {
        [SerializeField]
        private LeaderboardPosition _first;

        [SerializeField]
        private LeaderboardPosition _second;

        [SerializeField]
        private LeaderboardPosition _third;

        [SerializeField]
        private LeaderboardPosition _fourth;

        [SerializeField]
        private LeaderboardPosition _fifth;

        public LeaderboardPosition First => _first;
        public LeaderboardPosition Second => _second;
        public LeaderboardPosition Third => _third;
        public LeaderboardPosition Fourth => _fourth;
        public LeaderboardPosition Fifth => _fifth;
    }
}