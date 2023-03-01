using UnityEngine;
using Game.Core;

namespace Game.UI
{
    public class UIHUDContainer : MonoBehaviour
    {
        [SerializeField]
        private UIHealth _health;

        [SerializeField]
        private UITime _time;

        [SerializeField]
        private UIEnergy _energy;

        void Start()
        {
            GameManager.Scenes.loadedLevel += OnLevelLoad;
            MatchManager.LevelCompleted += OnLevelComplete;
            MatchManager.GameOver += OnLevelComplete;
        }

        public void OnDestroy()
        {
            GameManager.Scenes.loadedLevel -= OnLevelLoad;
            MatchManager.LevelCompleted -= OnLevelComplete;
            MatchManager.GameOver -= OnLevelComplete;
        }

        private void OnLevelLoad(int level)
        {
            _health.Show();
            _time.Show();
        }

        private void OnLevelComplete()
        {
            _health.Hide();
            _time.Hide();
        }
    }
}