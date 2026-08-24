using TMPro;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.InputSystem;

public class CustomCarUtilities : MonoBehaviour
{
    public CustomDefaultActions inputActions;
    public DataObject dataobj;

    #region SPEED TEXT (UI)
    [Space(20)]
    [Header("UI")]
    [Space(10)]
    //The following variable lets you to set up a UI text to display the speed of your car.
    public bool useUI = false;
    [SerializeField] private GameObject _drivingUI;
    public TextMeshProUGUI carSpeedText; // Used to store the UI object that is going to show the speed of the car.
    private float carSpeed;

    public RayCastCarController carController;
    [SerializeField] private GameObject _pauseMenu;
    #endregion

    private void Start()
    {
        carController = GetComponent<RayCastCarController>();
        inputActions = carController.inputActions;
        inputActions.CarPlaying.Cancel.Enable();
    }
    // Update is called once per frame
    void Update()
    {
        carSpeed = Mathf.Abs(carController.carSpeed);
        if (useUI)
        {
            CarSpeedUI();
        }

        inputActions.CarPlaying.Cancel.performed += ctx => PauseMenu();
    }

    public void PauseMenu()
    {
        if (!_pauseMenu.activeSelf)
        {
            Time.timeScale = 0f;
            _drivingUI.SetActive(false);
            _pauseMenu.SetActive(true);
        }
        else
        {
            Time.timeScale = Mathf.Lerp(0f, 1f, 1f);
            _drivingUI.SetActive(true);
            _pauseMenu.SetActive(false);
        }
    }

    // This method converts the car speed data from float to string, and then set the text of the UI carSpeedText with this value.
    public void CarSpeedUI()
    {
        float absoluteCarSpeed = carSpeed;
        carSpeedText.text = Mathf.RoundToInt(absoluteCarSpeed).ToString();

    }

    public void DestroyDataObj()
    {
        inputActions.CarPlaying.Disable();
        dataobj.DestroyObj();
    }
}
