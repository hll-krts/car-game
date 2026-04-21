using UnityEngine;

[CreateAssetMenu(fileName = "CarStatistics_", menuName = "Scriptable Objects/CarStatistics")]
public class CarStatistics : ScriptableObject
{
    [Header("Variables of ScriptableObject")]

    [Space(10)]
    public int carID;
    [Space(10)]
    public GameObject carPrefab;

    [Space(10)]
    public float WheelRadius;
    public float WheelWidth;
    public float BrakeForce;
    public float handbrakepower;

    [Space(5)]
    public float frontGripPercentage;
    public float rearGripPercentage;
    public float FrontSpringStiffness;
    public float RearSpringStiffness;

    [Space(5)]
    [Tooltip("Keep it something small, like 0,01")]
    public float RollingResCoefficient;

    [Space(5)]
    public float RestLength;
    public float SpringTravel;
    public float DamperStiffness;

    [Space(5)]
    public float areaFrontal;
    public float carAerodynamicCoefficient;

    [Tooltip("Assign manually")]
    public AudioClip _engineSound;
    [Tooltip("Assign manually")]
    public AudioClip _tireScreech;

    public float wheelbase;
    public float reartrack;
    public float turnRadius;
    public float steeringSpeed;
}
