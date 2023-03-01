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
            _health.Show(1f);
            _time.Show(1f);
        }

        private void OnLevelComplete()
        {
            _health.Hide();
            _time.Hide();
        }
    }
}