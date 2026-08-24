using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    public CustomCarUtilities util;

    private void Start()
    {
        util = FindFirstObjectByType<CustomCarUtilities>();
    }

    public void Resume()
    {
        util.PauseMenu();
    }
    public void QuitToMainMenuFromRace()
    {
        util.DestroyDataObj();
        SceneManager.LoadScene(0);
    }
    public void QuitToDesktopFromRace()
    {
        Debug.Log("Quitting to desktop");
        util.DestroyDataObj();
        Application.Quit();
    }
}
