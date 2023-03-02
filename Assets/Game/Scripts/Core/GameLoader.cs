using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Core
{
    using Game.Combat;

    public class GameLoader : MonoBehaviour
    {
        public const int GAME_SCENE_INDEX = 1;

        [SerializeField]
        private Tree _mainTree;

        [SerializeField]
        private int _level;

        public void Awake()
        {
            bool initialized = GameManager.Instance != null;

            if (initialized)
            {
                GameManager.SetMainTree(_mainTree);
                return;
            }

            SceneManager.LoadSceneAsync(GAME_SCENE_INDEX, LoadSceneMode.Additive)
                .completed += (async) => {
                    GameManager.SetMainTree(_mainTree);
                    GameManager.Scenes.SetCurrentLevel(_level);
                };
        }
    }
}