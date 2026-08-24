using UnityEngine;
using UnityEngine.SceneManagement;

public class DataObject : MonoBehaviour
{
    public CarStatisticsDatas carStatisticsDatas;
    public int carID;
    public CarStatistics selectedCarStats;

    public LevelsDataScriptableObject levelsData;
    public string selectedSceneName;

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public void SelectCar(int tempCarID)
    {
        carID = tempCarID;
        selectedCarStats = carStatisticsDatas._carStatistics[carID];
    }

    public void SelectLevel(string sceneName)
    {
        selectedSceneName = sceneName;
    }
    public void LoadScene()
    {
        SceneManager.LoadScene(selectedSceneName);
    }
    public void DestroyObj()
    {
        Destroy(this.gameObject);
    }
}