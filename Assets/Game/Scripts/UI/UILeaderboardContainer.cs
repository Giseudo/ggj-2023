using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Core;
using System;

namespace Game.UI
{
    public class UILeaderboardContainer : MonoBehaviour
    {
        [SerializeField]
        private LeaderboardData _data;

        [SerializeField]
        private List<UIRankRow> _rows = new List<UIRankRow>(4);

        public UIRankRow First => _rows[0];
        public UIRankRow Second => _rows[1];
        public UIRankRow Third => _rows[2];
        public UIRankRow Fourth => _rows[3];
        public UIRankRow Fifth => _rows[4];

        public void Start()
        {
            First?.SetData(_data.First);
            Second?.SetData(_data.Second);
            Third?.SetData(_data.Third);
            Fourth?.SetData(_data.Fourth);
            Fifth?.SetData(_data.Fifth);
        }
    }
}