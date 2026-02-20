using UnityEngine;
using TMPro;
using System.Collections;

[HideInInspector]
public enum GearState
{
    Running,
    CheckingChange,
    Changing
}

public class EngineFunctions : MonoBehaviour
{
    [Header("Internal Script Assigned Specs")]
    public float RPM;
    public float currentTorque;
    public int gear;
    public float currentEnginePower;
    [Space(5)]
    public GearState _gearState;

    [Space(20)]
    [Header("User Assigned Specs")]
    [Tooltip("in HP")]
    public float peakEnginePower; // in hp
    [Space(10)]
    public float rpmLerpSpeed;
    public float maxRPM;
    public float redLineRPM;
    public float idleRPM;
    public float increaseGearRPM;
    public float decreaseGearRPM;
    [Space(10)]
    public float changeGearTime;
    public float _clutchEffectiveValue; // something below 1
    public float _clutchEngagementTime; // value to engage it faster or slower. 0 means clutch's fucked
    public float[] gearRatios;
    public float _currentGearRatio;
    public float reverseGearRatio;
    public float differantialRatio;
    [Space(10)]
    public AnimationCurve rpmToHP;
    public AnimationCurve rpmToTorque;

    [Space(20)]
    [Header("Other Script Assigned Specs")]
    public float _gasPressed;
    public float wheelRPM;
    public float localWheelRPM;
    public float clutchEngagement; // 0 = Disengaged, 1 = Engaged

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _currentGearRatio = gearRatios[gear];
        _gearState = GearState.Changing;
    }

    // Update is called once per frame
    void Update()
    {
        currentTorque = CalculateTorque();
    }

    public void ClutchEngaged()
    {
        clutchEngagement = Mathf.Lerp(clutchEngagement, 1, Time.deltaTime * _clutchEngagementTime);
        _gearState = GearState.Running;
    }
    public void ClutchDisengaged()
    {
        clutchEngagement = 0;
        _gearState = GearState.Changing;
    }

    float CalculateTorque()
    {
        float torq = 0;

        if (clutchEngagement <= _clutchEffectiveValue)
        {
            RPM = Mathf.Lerp(RPM, Mathf.Max(idleRPM + Random.Range(-10, 10), (redLineRPM + Random.Range(-100, 100)) * _gasPressed), Time.deltaTime * rpmLerpSpeed);
            torq = 0;
        }
        else
        {
            localWheelRPM = wheelRPM * _currentGearRatio * differantialRatio;

            // When it's not "Mathf.Min(localWheelRPM, redLineRPM + Random.Range(-100, 100))" it literally breaks but when it's like that 
            //it limits the rpm to wheel rpm which is lower than red line rpm and I've got no idea why
            //THAT WAS BECAUSE OF SUSPENSION SCRIPT GO CHECK THAT SHIT OUT

            //NEED TO CHANGE THIS INTO SOMETHING MORE REALISTIC!! THE CAR DOESN'T USE IDLERPM ALL THE TIME WHEN WHEEL RPM IS LOWER!!!
            RPM = Mathf.Lerp(RPM, Mathf.Max(idleRPM, Mathf.Min(localWheelRPM, redLineRPM + Random.Range(-100, 100))), Time.deltaTime * rpmLerpSpeed);

            currentEnginePower = rpmToHP.Evaluate(RPM / maxRPM) * peakEnginePower;

            torq = rpmToTorque.Evaluate(RPM / maxRPM) * (currentEnginePower / RPM) 
                * 721.4f * clutchEngagement * _currentGearRatio * differantialRatio;
        }
        return torq;
    }

    public void IncreaseGear()
    {
        StartCoroutine(IncreaseGearRoutine()); 
    }
    public void DecreaseGear()
    {
        StartCoroutine(DecreaseGearRoutine());
    }

    IEnumerator IncreaseGearRoutine()
    {
        _gearState = GearState.CheckingChange;
        if (gear < gearRatios.Length - 1)
        {
            //increase the gear
            //yield return new WaitForSeconds(0.7f);
            ClutchDisengaged();
            gear++;
            yield return new WaitForSeconds(changeGearTime);
            _currentGearRatio = gearRatios[gear];
            ClutchEngaged();
        }
        else
        {
            yield break;
        }
    }
    IEnumerator DecreaseGearRoutine()
    {
        _gearState = GearState.CheckingChange;
        if (gear > 0)
        {
            //increase the gear
            //yield return new WaitForSeconds(0.7f);
            ClutchDisengaged();
            gear--;
            yield return new WaitForSeconds(changeGearTime);
            _currentGearRatio = gearRatios[gear];
            ClutchEngaged();
        }
        else if (gear == 0)
        {
            ClutchDisengaged();
            gear = -1;
            yield return new WaitForSeconds(changeGearTime);
            _currentGearRatio = reverseGearRatio;
            //change gear text to R
            ClutchEngaged();
        }
        else
        {
            yield break;
        }
    }
}
