using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEditor;

public class MainMenuController : MonoBehaviour
{
    public DataObject _dataObject;
    [SerializeField] private RayCastCarController _carController;

    public GameObject mainMenuCanvas;
    public Button playbutton;

    public GameObject selectionMenuCanvas;
    public Transform carShowroomSpawnTransform;

    private bool isCarSelected;
    public Transform _carSelectionScrollViewContent;
    public GameObject _carSelectionScrollViewContentPrefab;
    [SerializeField]private int carCount;
    private string _carName;

    private bool isLevelSelected;
    public Transform _levelSelectionScrollViewContent;
    public GameObject _levelSelectionScrollViewContentPrefab;
    [SerializeField] private int levelCount;
    private string _sceneName;

    public RawImage _levelImage;
    private void Start()
    {
        _carController = FindFirstObjectByType<RayCastCarController>(FindObjectsInactive.Include);
        _dataObject = FindFirstObjectByType<DataObject>();

        playbutton.onClick.AddListener(() =>
        {
            _dataObject.LoadScene();
        });

        NotPlayable();

        if (_dataObject.carStatisticsDatas._carStatistics.Length != 0)
        {
            CreateCarSelectionButtons();
        }
        if (_dataObject.levelsData._sceneImages.Length != 0)
        {
            CreateLevelSelectionButtons(); 
        }

        mainMenuCanvas.SetActive(true);
        selectionMenuCanvas.SetActive(false);
    }
    private void Update()
    {
        if(isCarSelected && isLevelSelected)
        {
            playbutton.interactable = true;
        }
        else
        {
            playbutton.interactable = false;
        }
    }
    public void OpenSelectionMenu()
    {
        mainMenuCanvas.SetActive(!mainMenuCanvas.activeSelf);
        selectionMenuCanvas.SetActive(!selectionMenuCanvas.activeSelf);
    }

    private void NotPlayable()
    {
        _carController.isControllable = false;
    }

    #region Car Selection
    void CreateCarSelectionButtons()
    {
        carCount = _dataObject.carStatisticsDatas.CarCount;
        for (int i = 0; i < carCount; i++)
        {
            var item_go = Instantiate(_carSelectionScrollViewContentPrefab);
            _carName = _dataObject.carStatisticsDatas._carStatistics[i].carPrefab.name;
            item_go.GetComponentInChildren<TextMeshProUGUI>().text = _carName;
            item_go.GetComponent<Image>().color = i % 2 == 0 ? Color.yellow : Color.cyan;
            item_go.transform.SetParent(_carSelectionScrollViewContent);
            item_go.transform.localScale = Vector2.one;

            item_go.GetComponent<Button>().onClick.RemoveAllListeners();
            item_go.GetComponent<Button>().onClick.AddListener(() =>
            {
                CarSelectorButtonFunction(item_go.transform.GetSiblingIndex());
            });
        }
    }
    void CarSelectorButtonFunction(int i)
    {
        isCarSelected = true;
        _dataObject.SelectCar(i);
        _carController.carStatistics = _dataObject.selectedCarStats;
        StartCoroutine(CarLoadWaiterForDebug());
    }
    IEnumerator CarLoadWaiterForDebug()
    {
        int childno = _carController.transform.childCount;
        for (int i = 0; i < childno; i++)
        {
            Destroy(_carController.transform.GetChild(i).gameObject);
        }
        _carController.enabled = false;
        _carController.gameObject.SetActive(false);
        _carController.gameObject.transform.position = carShowroomSpawnTransform.position;
        _carController.gameObject.transform.rotation = carShowroomSpawnTransform.rotation;
        yield return new WaitForSeconds(.3f);
        _carController.enabled = true;
        _carController.gameObject.SetActive(true);
    }
    #endregion

    #region Level Selection
    void CreateLevelSelectionButtons()
    {
        levelCount = _dataObject.levelsData.SceneCount;
        for (int i = 0; i < levelCount; i++)
        {
            
            var item_go = Instantiate(_levelSelectionScrollViewContentPrefab);
            _sceneName = _dataObject.levelsData._sceneImages[i].name;
            item_go.GetComponentInChildren<TextMeshProUGUI>().text = _sceneName;
            item_go.GetComponentInChildren<TextMeshProUGUI>().autoSizeTextContainer = true;
            item_go.GetComponentInChildren<TextMeshProUGUI>().fontSizeMin = .1f;
            item_go.GetComponent<Image>().color = i % 2 == 0 ? Color.pink : Color.lightBlue;
            item_go.transform.SetParent(_levelSelectionScrollViewContent);
            item_go.transform.localScale = Vector2.one;

            item_go.GetComponent<Button>().onClick.RemoveAllListeners();
            item_go.GetComponent<Button>().onClick.AddListener(() =>
            {
                CallSelectScene(item_go.transform.GetSiblingIndex());
            });
        }
    }
    private void CallSelectScene(int i)
    {
        _levelImage.texture = _dataObject.levelsData._sceneImages[i];
        _dataObject.SelectLevel(_sceneName);

        if (_dataObject.selectedSceneName == _sceneName)
        {
            isLevelSelected = true;
        }
        else
        {
            isLevelSelected = false;
        }
    }
    private void CallLoadScene()
    {
        _dataObject.LoadScene();
    }
    #endregion
}
