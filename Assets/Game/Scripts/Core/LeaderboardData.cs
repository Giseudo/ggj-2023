using System;
using UnityEngine;

namespace Game.Core
{
    [Serializable]
    public class LeaderboardRow
    {
        public string name;
        public int score;
    }

    [Serializable]
    [CreateAssetMenu(fileName = "Leaderboard", menuName = "Game/Data/Leaderboard Data")]
    public class LeaderboardData : JSONSerializableScriptableObject
    {
        [SerializeField]
        private LeaderboardRow _first;

        [SerializeField]
        private LeaderboardRow _second;

        [SerializeField]
        private LeaderboardRow _third;

        [SerializeField]
        private LeaderboardRow _fourth;

        [SerializeField]
        private LeaderboardRow _fifth;
    }
}