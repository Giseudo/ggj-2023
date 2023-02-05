using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Combat
{
    public class Creep : MonoBehaviour
    {
        [SerializeField]
        private CreepData _data;

        public CreepData Data => _data;

        public float MaxSpeed => _maxSpeed;

        [SerializeField]
        private float _maxSpeed;
    }
}