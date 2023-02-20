using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Core
{
    [Serializable]
    public class GameScenes
    {
        [SerializeField]
        private List<int> _levelScenesIndex = new List<int>();

        private int _currentLevel;
        private int _currentLevelIndex = -1;

        public int CurrentLevel => _currentLevel;
        public List<int> LevelScenes => _levelScenesIndex;

        public Action<int> loadedLevel = delegate { };

        public void LoadMenuScene()
        {
            SceneManager.LoadScene(0);
        }

        public void LoadFirstLevel()
        {
            int firstLevelIndex = _levelScenesIndex[0];
            Scene scene = SceneManager.GetSceneByBuildIndex(firstLevelIndex);

            if (scene == null)
            {
                Debug.LogWarning($"Scene with index {firstLevelIndex} was not found.");
                return;
            }

            _currentLevel = 0;

            LoadScene(firstLevelIndex);
        }

        public void LoadNextLevel()
        {
            if (_currentLevel + 1 >= _levelScenesIndex.Count)
                return;

            _currentLevel++;

            int nextLevelIndex = _levelScenesIndex[_currentLevel];

            LoadScene(nextLevelIndex);
        }

        public void RestartLevel()
        {
            if (_currentLevelIndex < 0) return;

            LoadScene(_currentLevelIndex);
        }

        public void LoadScene(int index)
        {
            if (index < 0) return;

            void Load() => SceneManager.LoadSceneAsync(index, LoadSceneMode.Additive)
                .completed += (async) => {
                    _currentLevelIndex = _levelScenesIndex[_currentLevel];
                    loadedLevel.Invoke(_currentLevel);

                    Scene currentScene = SceneManager.GetSceneByBuildIndex(_currentLevelIndex);

                    SceneManager.SetActiveScene(currentScene);
                    GameManager.SetMainCamera(Camera.main);
                };

            if (_currentLevelIndex >= 0)
                SceneManager.UnloadSceneAsync(_currentLevelIndex)
                    .completed += (async) => Load();
            else
                Load();
        }

        public void SetCurrentLevel(int value)
        {
            int currentLevelIndex = _levelScenesIndex[value];

            _currentLevel = value;
            _currentLevelIndex = currentLevelIndex;
        }
    }
}