using CarControllerMain;
using UnityEngine;

internal class CarStatisticsClass : MonoBehaviour
{
    CustomPrometeoCarController carControl;
    public DriveType _driveType;

    [Space(10)]
    public GameObject frontLeftMesh;
    public WheelCollider frontLeftCollider;
    [Space(10)]
    public GameObject frontRightMesh;
    public WheelCollider frontRightCollider;
    [Space(10)]
    public GameObject rearLeftMesh;
    public WheelCollider rearLeftCollider;
    [Space(10)]
    public GameObject rearRightMesh;
    public WheelCollider rearRightCollider;

    [Space(10)]
    public GameObject leftBrakeLight, rightBrakeLight;

    [Space(10)]
    // The following particle systems are used as tire smoke when the car drifts.
    public ParticleSystem RLWParticleSystem;
    public ParticleSystem RRWParticleSystem;

    [Space(10)]
    // The following trail renderers are used as tire skids when the car loses traction.
    public TrailRenderer RLWTireSkid;
    public TrailRenderer RRWTireSkid;

    [Space(10)]
    public AudioSource carEngineSound;
    public AudioSource tireScreechSound;

    [Space(10)]
    public CarStatisticsScriptableObject carStats;

    [Space(10)]
    public AnimationCurve steeringCurve;
    private void Awake()
    {
        carControl = this.GetComponentInParent<CustomPrometeoCarController>();
        //Debug.Log("CarStatisticsClass initialized for car: " + carControl.name);

        carControl._driveType = _driveType;

        carControl.maxSpeed = carStats.maxSpeed;
        carControl.maxReverseSpeed = carStats.maxReverseSpeed;
        carControl.motorTorque = carStats.motorTorque;
        carControl.maxSteeringAngle = carStats.maxSteeringAngle;
        carControl.steeringSpeed = carStats.steeringSpeed;
        carControl.brakeForce = carStats.brakeForce;
        carControl.decelerationMultiplier = carStats.decelerationMultiplier;
        carControl.handbrakeDriftMultiplier = carStats.handbrakeDriftMultiplier;

        carControl.frontLeftCollider = frontLeftCollider;
        carControl.frontRightCollider = frontRightCollider;
        carControl.rearLeftCollider = rearLeftCollider;
        carControl.rearRightCollider = rearRightCollider;

        carControl.frontLeftMesh = frontLeftMesh;
        carControl.frontRightMesh = frontRightMesh;
        carControl.rearLeftMesh = rearLeftMesh;
        carControl.rearRightMesh = rearRightMesh;

        carControl.carEngineSound = carEngineSound;
        carControl.tireScreechSound = tireScreechSound;

        carControl.RLWParticleSystem = RLWParticleSystem;
        carControl.RRWParticleSystem = RRWParticleSystem;
        carControl.RLWTireSkid = RLWTireSkid;
        carControl.RRWTireSkid = RRWTireSkid;

        carControl.leftBrakeLight = leftBrakeLight;
        carControl.rightBrakeLight = rightBrakeLight;

        carControl.steeringCurve = steeringCurve;
    }
    private void Start()
    {
        carControl.carRigidbody.mass = carStats.carWeight;
    }
}