using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
    [Serializable]
    public class LeaderboardPosition
    {
        [SerializeField]
        private string _name;
        [SerializeField]
        private int _score;
        
        public int Score { get => _score; set => _score = value; }
        public string Name { get => _name; set => _name = value; }

        public LeaderboardPosition(int score = 0, string name = "")
        {
            Score = score;
            Name = name;
        }
    }

    [Serializable]
    [CreateAssetMenu(fileName = "Leaderboard", menuName = "Game/Data/Leaderboard Data")]
    public class LeaderboardData : JSONSerializableScriptableObject
    {
        [SerializeField]
        private List<LeaderboardPosition> _positions = new List<LeaderboardPosition>();

        public List<LeaderboardPosition> Positions => _positions;

        public LeaderboardPosition GetPosition(int index)
        {
            if (index >= _positions.Count)
                return AddScore(index);

            return _positions[index];
        }

        public LeaderboardPosition AddScore(int index, int score = 0, string name = "")
        {
            _positions.Insert(index, new LeaderboardPosition(score, name));

            return _positions[index];
        }

        public void SetScore(int index, int score = 0, string name = "")
        {
            if (index >= _positions.Count) return;

            _positions[index].Score = score;
            _positions[index].Name = name;
        }
    }
}