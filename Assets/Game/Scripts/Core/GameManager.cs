using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
    using Game.Combat;

    public class GameManager : MonoBehaviour
    {
        [SerializeField]
        private GameScenes _scenes;

        private static List<Damageable> _damageables;
        private static List<Attacker> _attackers;

        public static GameManager Instance { get; private set; }
        public static Camera MainCamera { get; private set; }
        public static Tree MainTree { get; private set; }
        public static Light MainLight { get; private set; }
        public static GameScenes Scenes { get; private set; }

        public static Action<Damageable> DamageableAdded;
        public static Action<Damageable> DamageableRemoved;
        public static Action<Attacker> AttackerAdded;
        public static Action<Attacker> AttackerRemoved;
        public static Action<Tree> MainTreeChanged;
        public static Action<Camera> MainCameraChanged;
        public static Action<Light> MainLightChanged;

        public static List<Damageable> Damageables => _damageables;

        public void Awake()
        {
            Instance = this;
            MainCamera = Camera.main;
            Scenes = _scenes;

            DamageableAdded = delegate { };
            DamageableRemoved = delegate { };
            AttackerAdded = delegate { };
            AttackerRemoved = delegate { };
            MainTreeChanged = delegate { };
            MainCameraChanged = delegate { };
            MainLightChanged = delegate { };

            _damageables = new List<Damageable>();
            _attackers = new List<Attacker>();

            DataHandler.LoadGameData();
            DataHandler.SaveGameData();
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

        public static void SetMainTree(Tree tree)
        {
            MainTree = tree;
            MainTreeChanged.Invoke(tree);
        }

        public static void SetMainCamera(Camera camera)
        {
            MainCamera = camera;
            MainCameraChanged.Invoke(camera);
        }

        public static void SetMainLight(Light light)
        {
            MainLight = light;
            MainLightChanged.Invoke(light);
        }

        private void OnApplicationQuit()
        {
            DataHandler.SaveGameData();
        }
    }
}