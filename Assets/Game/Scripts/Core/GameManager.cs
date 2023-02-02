using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public static Camera MainCamera { get; private set; }

        public void Awake()
        {
            Instance = this;
            MainCamera = Camera.main;
        }
    }
}