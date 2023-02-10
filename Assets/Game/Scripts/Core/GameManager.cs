using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
    using Game.Combat;

    public class GameManager : MonoBehaviour
    {
        [SerializeField]
        private Tree _mainTree;

        private static List<Damageable> _damageables;
        private static List<Attacker> _attackers;

        public static GameManager Instance { get; private set; }
        public static Camera MainCamera { get; private set; }
        public static Tree MainTree { get; private set; }

        public static Action<Damageable> DamageableAdded;
        public static Action<Damageable> DamageableRemoved;
        public static Action<Attacker> AttackerAdded;
        public static Action<Attacker> AttackerRemoved;

        public void Awake()
        {
            Instance = this;
            MainCamera = Camera.main;
            MainTree = _mainTree;

            DamageableAdded = delegate { };
            DamageableRemoved = delegate { };
            AttackerAdded = delegate { };
            AttackerRemoved = delegate { };

            _damageables = new List<Damageable>();
            _attackers = new List<Attacker>();
        }

        public static void AddDamageable(Damageable damageable)
        {
            _damageables.Add(damageable);
            DamageableAdded.Invoke(damageable);
        }

        public static void RemoveDamageable(Damageable damageable)
        {
            _damageables.Remove(damageable);
            DamageableRemoved.Invoke(damageable);
        }

        public static void AddAttacker(Attacker attacker)
        {
            _attackers.Add(attacker);
            AttackerAdded.Invoke(attacker);
        }

        public static void RemoveAttacker(Attacker attacker)
        {
            _attackers.Remove(attacker);
            AttackerRemoved.Invoke(attacker);
        }
    }
}