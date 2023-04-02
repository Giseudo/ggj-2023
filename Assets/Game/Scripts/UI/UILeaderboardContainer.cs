using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Core;

namespace Game.UI
{
    public class UILeaderboardContainer : MonoBehaviour
    {
        [SerializeField]
        private LeaderboardData _data;

        [SerializeField]
        private List<UIRankRow> _rows = new List<UIRankRow>();
    }
}