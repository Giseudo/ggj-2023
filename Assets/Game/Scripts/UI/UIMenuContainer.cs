using UnityEngine;
using UnityEngine.SceneManagement;

public class UIMenuContainer : MonoBehaviour
{
    public void LoadFirstLevel()
    {
        SceneManager.LoadScene(2);
    }
}
