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
        private static List<Damageable> _damageables = new List<Damageable>();

        public static GameManager Instance { get; private set; }
        public static Camera MainCamera { get; private set; }
        public static Tree MainTree { get; private set; }

        public static Action<Damageable> DamageableAdded = delegate { };
        public static Action<Damageable> DamageableRemoved = delegate { };

        public void Awake()
        {
            Instance = this;
            MainCamera = Camera.main;
            MainTree = _mainTree;
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
    }
}