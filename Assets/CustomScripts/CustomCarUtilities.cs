using UnityEngine;
using TMPro;

public class CustomCarUtilities : MonoBehaviour
{
    #region SPEED TEXT (UI)
    [Space(20)]
    [Header("UI")]
    [Space(10)]
    //The following variable lets you to set up a UI text to display the speed of your car.
    public bool useUI = false;
    [SerializeField] private GameObject _drivingUI;
    public TextMeshProUGUI carSpeedText; // Used to store the UI object that is going to show the speed of the car.
    private float carSpeed;

    private RayCastCarController carController;
    [SerializeField] private GameObject _pauseMenu;
    #endregion

    private void Start()
    {
        carController = GetComponent<RayCastCarController>();
    }
    // Update is called once per frame
    void Update()
    {
        carSpeed = Mathf.Abs(carController.carSpeed);
        CarSpeedUI();
    }
    // This method converts the car speed data from float to string, and then set the text of the UI carSpeedText with this value.
    public void CarSpeedUI()
    {
        if (useUI)
        {
                float absoluteCarSpeed = carSpeed;
                carSpeedText.text = Mathf.RoundToInt(absoluteCarSpeed).ToString();
        }

        if (carController.cancelKeyPressed)
        {
            if (!_pauseMenu.activeSelf)
            {
                _drivingUI.SetActive(false);
                _pauseMenu.SetActive(true);
            }
            else
            {
                _drivingUI.SetActive(true);
                _pauseMenu.SetActive(false);
            }
            //stuff
        }
    }
}
