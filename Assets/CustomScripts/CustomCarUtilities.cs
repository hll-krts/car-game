using CarControllerMain;
using System;
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
    public TextMeshProUGUI carSpeedText; // Used to store the UI object that is going to show the speed of the car.
    private float carSpeed;
    private CustomPrometeoCarController carController;
    #endregion

    private void Start()
    {
        carController = GetComponent<CustomPrometeoCarController>();
    }
    // Update is called once per frame
    void Update()
    {
        carSpeed = carController.carSpeed;
        CarSpeedUI();
    }
    // This method converts the car speed data from float to string, and then set the text of the UI carSpeedText with this value.
    public void CarSpeedUI()
    {
        if (useUI)
        {
            try
            {
                float absoluteCarSpeed = Mathf.Abs(carSpeed);
                carSpeedText.text = Mathf.RoundToInt(absoluteCarSpeed).ToString();
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex);
            }
        }

    }
}
