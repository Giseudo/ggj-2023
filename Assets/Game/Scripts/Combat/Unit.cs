using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Combat
{
    public class Unit : MonoBehaviour
    {
        [SerializeField]
        private UnitData _data;

        public UnitData Data => _data;
    }
}