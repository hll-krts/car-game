using UnityEngine;
using TMPro;
using System.Collections;

[HideInInspector]
public enum GearState
{
    Running,
    Neutral,
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
    public float peakEnginePowerRPM;
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
    public AnimationCurve rpmToTorque;

    [Space(20)]
    [Header("Other Script Assigned Specs")]
    public float _gasPressed;
    public float wheelRPM;
    public float localWheelRPM;
    public int revLimiterActive;
    public float clutchEngagement; // 0 = Disengaged, 1 = Engaged

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gear = 0;
        _currentGearRatio = gearRatios[gear];
    }

    // Update is called once per frame
    void Update()
    {
        currentTorque = CalculateTorque();

        if(localWheelRPM >= redLineRPM + Random.Range(-100, 100))
        {
            revLimiterActive = 0;
        }
        else
        {
            revLimiterActive = 1;
        }
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
        int torq = 0;

        localWheelRPM = Mathf.Abs(wheelRPM * _currentGearRatio * differantialRatio);

        if (clutchEngagement <= _clutchEffectiveValue || _gearState == GearState.Neutral)
        {
            RPM = Mathf.Lerp(RPM, Mathf.Max(idleRPM + Random.Range(-10, 10), (redLineRPM + Random.Range(-100, 100)) * _gasPressed), Time.deltaTime * rpmLerpSpeed);
            torq = 0;
        }
        else
        {
            RPM = Mathf.Lerp(RPM, localWheelRPM, Time.deltaTime * rpmLerpSpeed);

            float peakEngineTorque = peakEnginePower * 7127 / peakEnginePowerRPM;

            torq = revLimiterActive * Mathf.FloorToInt(rpmToTorque.Evaluate(RPM/maxRPM) * peakEngineTorque * _currentGearRatio * differantialRatio);
            //torq = rpmToTorque.Evaluate(RPM) * peakEngineTorque * clutchEngagement * _currentGearRatio * differentialRatio;
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
    public void NeutralGear()
    {
        StartCoroutine(NeutralGearRoutine());
    }
    public void RunningGear()
    {
        StartCoroutine(RunningGearRoutine());
    }

    IEnumerator IncreaseGearRoutine()
    {
        _gearState = GearState.CheckingChange;
        if (gear < gearRatios.Length - 1)
        {
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
    IEnumerator NeutralGearRoutine()
    {
        _gearState = GearState.CheckingChange;
        ClutchDisengaged();
        yield return new WaitForSeconds(changeGearTime);
        _gearState = GearState.Neutral;
        yield break;
    }
    IEnumerator RunningGearRoutine()
    {
        _gearState = GearState.CheckingChange;
        yield return new WaitForSeconds(changeGearTime);
        _currentGearRatio = gearRatios[gear];
        ClutchEngaged();
        yield break;
    }
}
