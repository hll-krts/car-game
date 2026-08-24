using UnityEngine;

public class RayCastCarController : MonoBehaviour
{
    [Space(5)]
    #region Physics Variables
    [Space(5)]
    public float _gravitanionalAccel;
    [Space(5)]
    public float airDensity;
    private Vector3 airDragForce;
    [Tooltip("Keep it something small")]
    public float stationaryMaxSpeed;
    #endregion

    [Space(5)]
    public CarStatistics carStatistics;
    private GameObject _carPrefab;
    Rigidbody rb;
    EngineFunctions engineFunctions;
    public enum DriveType { FWD, RWD, AWD }
    public DriveType driveType = DriveType.AWD;

    public bool isControllable = false;
    [SerializeField] private bool isTestingSomething = true;
    public float testWheelDiameterInInches = 12f;
   [SerializeField] private bool isRealisticDrag;


    #region Internal Control Variables
    [Space(20)]
    [Header("Internal Control Variables")]
    public LayerMask groundLayer;
    public Suspension[] wheels;

    #region Sounds
    [Space(5)]
    [Tooltip("Assign manually")]
    public AudioSource _engineSound;
    [Tooltip("Assign manually")]
    public AudioSource _tireScreech;

    public bool useMainSounds;

    public bool useEngineSounds;
    private float initialCarEngineSoundPitch;
    [SerializeField] private float subtractionValueToMatchIdleEnginePitch = .2f;

    public bool useTireSounds;
    #endregion

    #region Input Variables
    [Space(5)]
    public bool _manualTransmission = true;
    public CustomDefaultActions inputActions;
    private float steeringinput;
    private float gasPressed;
    private float brakePressed;
    private bool handbrakePressed;
    #endregion

    #region Car Movement Variables
    [Space(5)]
    private float forwardButtonsPressed, brakingStrength;
    private float ackermanLeft;
    private float ackermanRight;
    public float carSpeed;
    private float driveWheelVelocity;
    private float leftFrontVelocity = 0, rightFrontVelocity = 0, leftRearVelocity = 0, rightRearVelocity = 0;
    public float RPM;
    private float motorTorque;
    private float leftFrontRPM = 0, rightFrontRPM = 0, leftRearRPM = 0, rightRearRPM = 0;
    private Vector3 rightFrontForwardForce, rightRearForwardForce, leftFrontForwardForce, leftRearForwardForce;
    #endregion

    #region Tire Bools
    [SerializeField] private bool isGrounded;
    private bool _frG, _flG, _rrG, _rlG;
    [SerializeField] private bool isStationary;
    private bool _frS, _flS, _rrS, _rlS;
    [SerializeField] private bool isSlipping;
    #endregion

    #endregion

    #region Variables to pull from ScriptableObject
    [Header("Variables to pull from ScriptableObject")]
    [Space(10)]
    [SerializeField] private float wheelDiameterInMeters;
    private float WheelWidth; //I don't think I'll use this but I'll keep it in just in case
    private float BrakeForce;
    private float handbrakepower;
    [Space(5)]
    private float frontGripPercentage;
    private float rearGripPercentage;
    private float FrontSpringStiffness;
    private float RearSpringStiffness;
    [Space(5)]
    [Tooltip("Keep it something small, like 0,01")]
    [SerializeField] private float RollingResCoefficient;

    [Space(5)]
    private GameObject centerOfMass; //LoadCart() will assign it but MAKE SURE THE CoM HAS THE PROPER TAG
    private float RestLength;
    private float SpringTravel;
    private float DamperStiffness;
    [Space(5)]
    [SerializeField] private bool WillRenderMesh;
    [Space(5)]
    [SerializeField] private float areaFrontal;
    [SerializeField] private float carAerodynamicCoefficient;

    private float wheelbase;
    private float reartrack;
    private float turnRadius;
    private float steeringSpeed;
    #endregion


    private void OnEnable()
    {
        LoadCar(); 
        StartFunc();
    }
    private void StartFunc()
    {  
        rb = GetComponent<Rigidbody>();

        if (isControllable)
        {
            engineFunctions = GetComponent<EngineFunctions>();
            inputActions = new CustomDefaultActions();
            inputActions.CarPlaying.Enable();
            SetCarStatistics();
        }

        wheels = GetComponentsInChildren<Suspension>();

    }
    private void LoadCar()
    {
        if (carStatistics != null)
        {
            _carPrefab = carStatistics.carPrefab;
            GameObject carModel = Instantiate(_carPrefab, transform.position, transform.rotation, transform);
            foreach (Transform transform in this.transform)
            {
                if (transform.CompareTag("CenterOfMass"))
                {
                    centerOfMass = transform.gameObject;
                    rb.centerOfMass = centerOfMass.transform.localPosition;
                    break;
                }
            }

            //I have to divide it by 2 as well because diameter=çap and radius=yarýçap
            wheelDiameterInMeters = (carStatistics.WheelDiameterInInches * 2.54f) / 100f; 
            BrakeForce = carStatistics.BrakeForce;
            handbrakepower = carStatistics.handbrakepower;
            frontGripPercentage = carStatistics.frontGripPercentage;
            rearGripPercentage = carStatistics.rearGripPercentage;
            FrontSpringStiffness = carStatistics.FrontSpringStiffness;
            RearSpringStiffness = carStatistics.RearSpringStiffness;
            //RollingResCoefficient = carStatistics.RollingResCoefficient;
            RestLength = carStatistics.RestLength;
            SpringTravel = carStatistics.SpringTravel;
            DamperStiffness = carStatistics.DamperStiffness;
            areaFrontal = carStatistics.areaFrontal;
            carAerodynamicCoefficient = carStatistics.carAerodynamicCoefficient;
            wheelbase = carStatistics.wheelbase;
            reartrack = carStatistics.reartrack;
            turnRadius = carStatistics.turnRadius;
            steeringSpeed = carStatistics.steeringSpeed;
        }

    }
    private void SetCarStatistics()
    {
        _engineSound.clip = carStatistics._engineSound;
        initialCarEngineSoundPitch = _engineSound.pitch - subtractionValueToMatchIdleEnginePitch;
        _tireScreech.clip = carStatistics._tireScreech;
    }

    private void Update()
    {
        if (isTestingSomething)
        {
            wheelDiameterInMeters = (testWheelDiameterInInches * 2.54f) / 100f;
        }

        Physics.gravity = new Vector3(0, -_gravitanionalAccel, 0);

        if (isControllable)
        {
            InputReading();
            AckermanCalculation();
            EngineStuff();
            SoundsFunctions();
            StationaryCalculation();
        }


        //Shit about wheels
        foreach (Suspension suspension in wheels)
        {
            suspension.wheelDiameterInMeter = wheelDiameterInMeters;
            GlobalWheelVariables(suspension);

            if (isControllable)
            {
                Drivetrain(suspension); 
            }

            #region Individual wheels 
            if (suspension.frontLeft) // sol ön
            {
                FrontLeftWheel(suspension);
            }
            else if (suspension.frontRight) // sað ön
            {
                FrontRightWheel(suspension);
            }
            else if (suspension.rearLeft) // sol arka
            {
                RearLeftWheel(suspension);
            }
            else if (suspension.rearRight) // sað arka
            {
                RearRightWheel(suspension);
            }
            #endregion
        }

        isSlipping = _flS || _frS || _rlS || _rrS;
        isGrounded = _flG || _frG || _rlG || _rrG;
        carSpeed = driveWheelVelocity * (36f / 10f);
    }

    private void SoundsFunctions()
    {
        _tireScreech.mute = !useMainSounds;
        _engineSound.mute = !useMainSounds;
        if (useMainSounds)
        {
            if (useEngineSounds)
            {
                EngineSound();
            }
            else
            {
                _engineSound.Stop();
            }

            if (useTireSounds)
            {
                TireScreechFunction(isSlipping);
            }
        }
    }
    
    private void InputReading()
    {
        steeringinput = inputActions.CarPlaying.Turn.ReadValue<float>();
        handbrakePressed = inputActions.CarPlaying.HandBrake.IsPressed();

        #region Transmission Type
        if (_manualTransmission)
        {
            gasPressed = inputActions.CarPlaying.Gas.ReadValue<float>();
            brakePressed = inputActions.CarPlaying.Brake.ReadValue<float>();
            inputActions.CarPlaying.ShiftUp.performed += ctx => engineFunctions.IncreaseGear();
            inputActions.CarPlaying.ShiftDown.performed += ctx => engineFunctions.DecreaseGear(); 
        }
        else
        {
            //automatic transmission function
        }
        #endregion

        forwardButtonsPressed = gasPressed;
        brakingStrength = brakePressed * BrakeForce;
    }

    private void FixedUpdate()
    {
        AirDrag();
    }
    private void AirDrag()
    {
        if (isRealisticDrag)
        {
            airDragForce = transform.forward * -1 * (.5f * carAerodynamicCoefficient * airDensity * areaFrontal
                * (driveWheelVelocity * driveWheelVelocity)); 
        }
        else
        {
            //something else
        }
        rb.AddForceAtPosition(airDragForce, transform.position);
    }

    #region Driving part that does the driving which is based on what the driver who's driving the car that's doing the driving is doing
    private void EngineStuff()
    {
        if (carSpeed > 2f || carSpeed < -2f)
        {
            if (engineFunctions.gear == 0)
            {
                inputActions.CarPlaying.ShiftDown.Disable(); 
            }
            else
            {
                inputActions.CarPlaying.ShiftDown.Enable();
            }

            if (engineFunctions.gear == -1)
            {
                inputActions.CarPlaying.ShiftUp.Disable();
            }
            else
            {
                inputActions.CarPlaying.ShiftUp.Enable();
            }
        }
        else
        {
            inputActions.CarPlaying.ShiftUp.Enable();
            inputActions.CarPlaying.ShiftDown.Enable();
        }

        RPM = engineFunctions.RPM;
        engineFunctions._gasPressed = Mathf.Abs(forwardButtonsPressed);
        motorTorque = engineFunctions.currentTorque * forwardButtonsPressed;

        if (engineFunctions._gearState == GearState.Running)
        {
            engineFunctions.ClutchEngaged();
        }
    }
    private void StationaryCalculation()
    {
        if (gasPressed == 0 && (Mathf.Abs(carSpeed) < stationaryMaxSpeed || RPM <= (engineFunctions.idleRPM*2)/5) && engineFunctions._gearState == GearState.Running)
        {
            isStationary = true;
            rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
            engineFunctions.NeutralGear();
        }
        else if (gasPressed != 0 && engineFunctions._gearState == GearState.Neutral)
        {
            rb.constraints = RigidbodyConstraints.None;
            isStationary = false;
            engineFunctions.RunningGear();
        }
    }
    public void Drivetrain(Suspension suspension)
    {
        switch (driveType)
        {
            case RayCastCarController.DriveType.FWD:
                driveWheelVelocity = (leftFrontVelocity + rightFrontVelocity) / 2;
                engineFunctions.wheelRPM = ((leftFrontRPM + rightFrontRPM) / 2);

                if (suspension.frontLeft || suspension.frontRight)
                {
                    suspension.willReceiveTorque = true;
                    suspension.forwardInputTorque = motorTorque;
                }
                else
                {
                    suspension.willReceiveTorque = false;
                    suspension.forwardInputTorque = 0;
                }
                break;
            case RayCastCarController.DriveType.RWD:
                driveWheelVelocity = (leftRearVelocity + rightRearVelocity) / 2;
                engineFunctions.wheelRPM = ((leftRearRPM + rightRearRPM) / 2);

                if (suspension.rearLeft || suspension.rearRight)
                {
                    suspension.willReceiveTorque = true;
                    suspension.forwardInputTorque = motorTorque;
                }
                else
                {
                    suspension.willReceiveTorque = false;
                    suspension.forwardInputTorque = 0;
                }
                break;
            case RayCastCarController.DriveType.AWD:
                driveWheelVelocity = (leftFrontVelocity + rightFrontVelocity + leftRearVelocity + rightRearVelocity) / 4;
                engineFunctions.wheelRPM = (leftFrontRPM + rightFrontRPM) / 2;

                suspension.willReceiveTorque = true;
                suspension.forwardInputTorque = motorTorque;
                break;
        }
    }
    #endregion

    #region Wheel Assignments
    private void AckermanCalculation()
    {
        if (steeringinput > 0) //sað
        {
            ackermanLeft = Mathf.Rad2Deg * Mathf.Atan(wheelbase / (turnRadius + (reartrack / 2))) * steeringinput;
            ackermanRight = Mathf.Rad2Deg * Mathf.Atan(wheelbase / (turnRadius - (reartrack / 2))) * steeringinput;
        }
        else if (steeringinput < 0) //sol
        {
            ackermanLeft = Mathf.Rad2Deg * Mathf.Atan(wheelbase / (turnRadius - (reartrack / 2))) * steeringinput;
            ackermanRight = Mathf.Rad2Deg * Mathf.Atan(wheelbase / (turnRadius + (reartrack / 2))) * steeringinput;
        }
        else
        {
            ackermanLeft = 0;
            ackermanRight = 0;
        }
    }
    private void GlobalWheelVariables(Suspension suspension)
    {
        suspension.groundLayer = groundLayer;
        suspension.wheelDiameterInMeter = wheelDiameterInMeters;
        suspension.restLength = RestLength;
        suspension.springTravel = SpringTravel;
        suspension.damperStiffness = DamperStiffness;

        suspension._gravitanionalForce = _gravitanionalAccel;
        suspension.rollingResistanceCoefficient = RollingResCoefficient;

        suspension.willRenderMesh = WillRenderMesh;
    }
    private void FrontLeftWheel(Suspension suspension)
    {
        _flG = suspension.isGrounded;
        _flS = suspension.isSlipping;

        suspension.gripPercentage = frontGripPercentage;
        suspension.springStiffness = FrontSpringStiffness;
        suspension.steeringAngle = ackermanLeft;
        suspension.steeringSpeed = steeringSpeed;

        suspension.Braking(brakingStrength);

        leftFrontVelocity = suspension._wheelVelocityLocal.z;
        leftFrontRPM = suspension.wheelRPM;

        leftFrontForwardForce = suspension.forceForward;
    }
    private void FrontRightWheel(Suspension suspension)
    {
        _frG = suspension.isGrounded;
        _frS = suspension.isSlipping;

        suspension.gripPercentage = frontGripPercentage;
        suspension.springStiffness = FrontSpringStiffness;
        suspension.steeringAngle = ackermanRight;
        suspension.steeringSpeed = steeringSpeed;

        suspension.Braking(brakingStrength);

        rightFrontVelocity = suspension._wheelVelocityLocal.z;
        rightFrontRPM = suspension.wheelRPM;

        rightFrontForwardForce = suspension.forceForward;
    }
    private void RearLeftWheel(Suspension suspension)
    {
        _rlG = suspension.isGrounded;
        _rlS = suspension.isSlipping;

        suspension.springStiffness = RearSpringStiffness;

        if (handbrakePressed)
        {
            suspension.gripPercentage = 0;
            suspension.Handbrake(handbrakepower);
        }
        else
        {
            suspension.gripPercentage = rearGripPercentage;
        }

            leftRearVelocity = suspension._wheelVelocityLocal.z;
        leftRearRPM = suspension.wheelRPM;

        leftRearForwardForce = suspension.forceForward;
    }
    private void RearRightWheel(Suspension suspension)
    {
        _rrG = suspension.isGrounded;
        _rrS = suspension.isSlipping;

        suspension.springStiffness = RearSpringStiffness;

        if (handbrakePressed)
        {
            suspension.gripPercentage = 0;
            suspension.Handbrake(handbrakepower);
        }
        else
        {
            suspension.gripPercentage = rearGripPercentage;
        }

        rightRearVelocity = suspension._wheelVelocityLocal.z;
        rightRearRPM = suspension.wheelRPM;

        rightRearForwardForce = suspension.forceForward;
    }
    #endregion

    #region VFX and Sounds
    public void TireScreechFunction(bool toggle)
    {
        if (isGrounded)
        {
            if (toggle)
            {
                _tireScreech.mute = !toggle;
            }
            else
            {
                _tireScreech.mute = !toggle;
            }
        }
        else
        {
            _tireScreech.mute = true;
        }
    }
    public void EngineSound()
    {
        if (_engineSound != null)
        {
            if (!_engineSound.isPlaying)
            {
                _engineSound.Play();
            }
            float engineSoundPitch = initialCarEngineSoundPitch + engineFunctions.rpmToMaxRpmRatio;
            _engineSound.pitch = engineSoundPitch;
        }
        else
        {
            _engineSound.Stop();
        }
    }
    #endregion
}
