using TMPro;
using UnityEngine;

public class RayCastCarController : MonoBehaviour
{
    Rigidbody rb;
    EngineFunctions engineFunctions;
    public enum DriveType { FWD, RWD, AWD }
    public DriveType driveType = DriveType.AWD;

    [Space(10)]
    [Header("Stuff")]
    //I SHALL MAKE A GLOBAL CONTROLLER TO SET THE GRAVITY. I THINK.
    public float _gravitanionalAccel;
    public float WheelRadius;
    public float WheelWidth;
    [Tooltip("Keep it something small, like .01")]
    public float RollingResCoefficient;
    public float BrakeForce;
    [Space(5)]
    public float FrontSpringStiffness;
    public float RearSpringStiffness;
    [Space(5)]
    public float RestLength;
    public float SpringTravel;
    public float DamperStiffness;
    public bool WillRenderMesh;
    [Space(5)]
    public float airDensity;
    public float carAerodynamicCoefficient;
    public float areaFrontal;
    private Vector3 airDragForce;

    [SerializeField] GameObject centerOfMass;
    public LayerMask groundLayer;
    public Suspension[] wheels;

    [Tooltip("Keep it something small, like .1")]
    public float axleWidth;
    [SerializeField] private float wheelbase;
    [SerializeField] private float reartrack;
    [SerializeField] private float turnRadius;

    [Space(10)]
    [Header("Steering")]

    public AnimationCurve steeringCurve;
    [SerializeField] private float ackermanLeft;
    [SerializeField] private float ackermanRight;
    [SerializeField] private float steeringinput;
    [SerializeField] private float steeringSpeed;

    [Space(10)]
    [Header("Input")]

    private CustomDefaultActions inputActions;

    [Space(10)]
    [Header("REMOVE FROM EDITOR WHEN DONE")]
    [Space(10)]
    [Header("Moving the car")]
    public float carSpeed;
    [SerializeField] private float gasPressed;
    [SerializeField] private float brakePressed;
    [SerializeField] private bool handbrakePressed;
    [SerializeField] private float forwardButtonsPressed, brakingStrength;
    [Space(10)]
    [SerializeField] private float motorTorque;

    private float leftFrontVelocity = 0, rightFrontVelocity = 0, leftRearVelocity = 0, rightRearVelocity = 0;
    private float leftFrontRPM = 0, rightFrontRPM = 0, leftRearRPM = 0, rightRearRPM = 0;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        engineFunctions = GetComponent<EngineFunctions>();
        inputActions = new CustomDefaultActions();
        inputActions.CarPlaying.Enable();

        inputActions.CarPlaying.ShiftUp.performed += ctx => engineFunctions.IncreaseGear();
        inputActions.CarPlaying.ShiftDown.performed += ctx => engineFunctions.DecreaseGear();
    }
    private void Update()
    {
        Physics.gravity = new Vector3(0, -_gravitanionalAccel, 0);
        rb.centerOfMass = centerOfMass.transform.localPosition;

        steeringinput = inputActions.CarPlaying.Turn.ReadValue<float>();
        gasPressed = inputActions.CarPlaying.Gas.ReadValue<float>();
        brakePressed = inputActions.CarPlaying.Brake.ReadValue<float>();
        handbrakePressed = inputActions.CarPlaying.HandBrake.IsPressed();

        forwardButtonsPressed = gasPressed;     
        brakingStrength = brakePressed * BrakeForce;

        #region Assigning Engine Values
        engineFunctions._gasPressed = Mathf.Abs(forwardButtonsPressed);
        motorTorque = engineFunctions.currentTorque * forwardButtonsPressed;

        engineFunctions.ClutchEngaged();
        #endregion

        #region Ackerman Steering Calculation
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
        #endregion


        //Shit about wheels
        foreach (Suspension suspension in wheels)
        {
            #region Variables for all suspensions
            suspension.groundLayer = groundLayer;
            suspension.wheelRadius = WheelRadius;
            suspension.restLength = RestLength;
            suspension.springTravel = SpringTravel;
            suspension.damperStiffness = DamperStiffness;

            suspension._gravitanionalForce = _gravitanionalAccel;
            suspension.rollingResistanceCoefficient = RollingResCoefficient;
            suspension._axleWidth = axleWidth;

            suspension.willRenderMesh = WillRenderMesh;
            #endregion

            #region Drivetrain and stuff 
            Driving(suspension);
            #endregion

            #region Individual wheels 
            if (suspension.frontLeft) // sol ön
            {
                suspension.springStiffness = FrontSpringStiffness;
                suspension.steeringAngle = ackermanLeft * steeringCurve.Evaluate(carSpeed/100f);
                suspension.steeringSpeed = steeringSpeed;

                suspension.Braking(brakingStrength);

                leftFrontVelocity = suspension._wheelVelocityLocal.z;
                leftFrontRPM = suspension.wheelRPM;
            }
            else if (suspension.frontRight) // sað ön
            {
                suspension.springStiffness = FrontSpringStiffness;
                suspension.steeringAngle = ackermanRight * steeringCurve.Evaluate(carSpeed / 100f);
                suspension.steeringSpeed = steeringSpeed;

                suspension.Braking(brakingStrength);

                rightFrontVelocity = suspension._wheelVelocityLocal.z;
                rightFrontRPM = suspension.wheelRPM;
            }
            else if (suspension.rearLeft) // sol arka
            {
                suspension.springStiffness = RearSpringStiffness;

                if (handbrakePressed)
                {
                    suspension.Handbrake();
                }

                leftRearVelocity = suspension._wheelVelocityLocal.z;
                leftRearRPM = suspension.wheelRPM;
            }
            else if (suspension.rearRight) // sað arka
            {
                suspension.springStiffness = RearSpringStiffness;

                if (handbrakePressed)
                {
                    suspension.Handbrake();
                }

                rightRearVelocity = suspension._wheelVelocityLocal.z;
                rightRearRPM = suspension.wheelRPM;
            }
            #endregion
        }

        carSpeed = ((rightFrontVelocity + leftFrontVelocity) / 2f) * (36f / 10f);
        //Debug.Log($"Car Speed: {carSpeed}, left front: {leftFrontVelocity}, right front: {rightFrontVelocity}, left rear: {leftRearVelocity}, right reat: {rightRearVelocity}");
    }
    private void FixedUpdate()
    {
        #region Air drag
        airDragForce = transform.forward * -1 * (.5f * carAerodynamicCoefficient * airDensity * areaFrontal * (carSpeed * carSpeed));
        rb.AddForceAtPosition(airDragForce, transform.position);
        #endregion
    }
    #region Driving part that does the driving which is based on what the driver who's driving the car that's doing the driving is doing
    public void Driving(Suspension suspension)
    {
        switch (driveType)
        {
            case RayCastCarController.DriveType.FWD:
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
                engineFunctions.wheelRPM = ((leftFrontRPM + rightFrontRPM + leftRearRPM + rightRearRPM) / 4);

                suspension.willReceiveTorque = true;
                suspension.forwardInputTorque = motorTorque;
                break;
        }
    }
    #endregion
}
