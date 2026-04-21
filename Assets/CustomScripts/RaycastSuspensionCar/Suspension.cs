using System;
using UnityEngine;

public class Suspension : MonoBehaviour
{
    [Tooltip("Layer(s) that represent the ground the wheel will interact with.")]
    public LayerMask groundLayer;
    private Rigidbody rb;
    public GameObject _wheel;
    private MeshRenderer _wheelMeshRenderer;
    //SphereCollider _wheelCollider;
    [Space(10)]
    [Header("Wheel")]
    public bool frontLeft;
    public bool frontRight;
    public bool rearLeft;
    public bool rearRight;
    public bool willReceiveTorque;
    public float gripPercentage;

    #region Suspension stuff
    [Space(10)]
    [Header("Suspension Settings")]
    public float springStiffness = 1f;
    public float restLength = .6f;
    public float springTravel = .3f;
    public float wheelRadius = .33f; // DON'T FORGET TO SET THIS IN THE EDITOR YOU DIPSHIT

    public float damperStiffness;

    public float steeringAngle;
    public float wheelAngle;
    public float steeringSpeed;

    public float _gravitanionalForce;
    private float _normalForceValue;
    #endregion

    public bool isGrounded;

    private float suspensionLength;
    private float maxSuspL;

    #region Tire Movement stuff
    [Space(10)]
    [Header("Tire Settings")]
    public float rollingResistanceCoefficient;

    public bool willRenderMesh = true;
    public Vector3 _wheelVelocityLocal;
    public float wheelRPM = 0;
    private float angularVelocity;
    #endregion

    private float _forceForwards, _forceSidewards;

    [HideInInspector] public Vector3 actualForce = Vector3.zero, forceSide = Vector3.zero, forceUp = Vector3.zero, forceForward = Vector3.zero;
    [HideInInspector] public float forwardInputTorque;
    [HideInInspector] public bool isSlipping;
    [HideInInspector] public TrailRenderer _trailRenderer;
    [HideInInspector] public ParticleSystem _smokes;

    private float staticFriction = 0, dynamicFriction = 0;
    private float _dynamicFrictionForceValue;
    private float _staticFrictionForceValue;
    private float forceToAddFromTorque;
    private Vector3 wheelToGroundContactPos;
    private Vector3 _RollResForce;

    void Start()
    {
        rb = transform.root.GetComponent<Rigidbody>();
        _wheelMeshRenderer = GetComponentInChildren<MeshRenderer>();
        _wheel = _wheelMeshRenderer.gameObject;
        _trailRenderer = GetComponentInChildren<TrailRenderer>();
        _smokes = GetComponentInChildren<ParticleSystem>();
    }

    private void Update()
    {
        wheelAngle = Mathf.Lerp(wheelAngle, steeringAngle, Time.deltaTime * steeringSpeed);
        transform.localRotation = Quaternion.Euler(Vector3.up * wheelAngle);

        _wheel.transform.position = transform.position - transform.up * suspensionLength;
        _wheelMeshRenderer.enabled = willRenderMesh;

        wheelRPM = 60 * _wheelVelocityLocal.z / (2 * Mathf.PI * wheelRadius);

        forceToAddFromTorque = forwardInputTorque;
        if (willReceiveTorque)
        {
            if (Mathf.Abs(forwardInputTorque) > 0)
            {
                AddForceToThisWheel();
            }
            else
            {
                _forceForwards = 0;
            }
        }

        angularVelocity = MathF.Sqrt(forceToAddFromTorque / (wheelRadius * rb.mass / 4));
    }

    private void FixedUpdate()
    {
        if (Mathf.Abs(_forceSidewards) < staticFriction * _normalForceValue * gripPercentage)
        {
            isSlipping = false;
            _forceSidewards = _wheelVelocityLocal.x * staticFriction * _normalForceValue;
        }
        else
        {
            isSlipping = true;
            _forceSidewards = Mathf.Sign(_wheelVelocityLocal.x) * dynamicFriction * _normalForceValue;
        }

        SuspensionsSphere();
        RollingResistance();
    }
    void RollingResistance()
    {
        if (_wheelVelocityLocal.z > -1 && _wheelVelocityLocal.z < 1)
        {
            _RollResForce = (-transform.forward * _wheelVelocityLocal.z) * (_normalForceValue * rollingResistanceCoefficient);
        }
        else
        {
            _RollResForce = (-transform.forward * Mathf.Sign(_wheelVelocityLocal.z)) * (_normalForceValue * rollingResistanceCoefficient);
        }
    }
    private void SuspensionsSphere()
    {
        RaycastHit hit;
        maxSuspL = restLength + springTravel;
        if (Physics.SphereCast(transform.position, wheelRadius, -transform.up, out hit, maxSuspL, groundLayer))
        {
            wheelToGroundContactPos = hit.point;
            suspensionLength = hit.distance;
            float compressionRatio = (restLength - suspensionLength) / springTravel;
            float springForce = compressionRatio * springStiffness;

            float springVelocity = Vector3.Dot(rb.GetPointVelocity(transform.position), transform.up);
            float damperForce = damperStiffness * springVelocity;

            float netForce = (springForce - damperForce);

            float angle = Vector3.Angle(hit.transform.up, Vector3.up);
            _normalForceValue = (rb.mass / 4f) * _gravitanionalForce * Mathf.Cos(angle * Mathf.Deg2Rad);

            _wheelVelocityLocal = transform.InverseTransformDirection(rb.GetPointVelocity(hit.point));

            staticFriction = hit.collider.material.staticFriction;
            _staticFrictionForceValue = staticFriction * _normalForceValue * 10f;
            dynamicFriction = hit.collider.material.dynamicFriction;
            _dynamicFrictionForceValue = dynamicFriction * _normalForceValue * 10f;

            forceUp = netForce * transform.up;
            forceForward = transform.forward * _forceForwards;
            forceSide = -transform.right * _forceSidewards;
            actualForce = forceSide + forceUp + forceForward;

            rb.AddForceAtPosition(actualForce + _RollResForce, _wheel.transform.position);

            OnGround();
            Vfx(isSlipping);
        }
        else
        {
            suspensionLength = maxSuspL;
            NotOnGround();
        }
    }

    void OnGround()
    {
        isGrounded = true;

        if (isSlipping)
        {
            _wheel.transform.rotation *=
                Quaternion.Euler(Vector3.right * (angularVelocity / (2 * Mathf.PI * wheelRadius)) * 360 * Time.fixedDeltaTime);
        }
        else
        {
            _wheel.transform.rotation *=
                Quaternion.Euler(Vector3.right * (_wheelVelocityLocal.z / (2 * Mathf.PI * wheelRadius)) * 360 * Time.fixedDeltaTime);
        }
    }
    void NotOnGround()
    {
        isGrounded = false;
        suspensionLength = restLength;
    }

    public void AddForceToThisWheel()
    {
        //axleWidth is important for this
        if (Mathf.Abs(forceToAddFromTorque) <= _staticFrictionForceValue)
        {
            _forceForwards = forceToAddFromTorque;
            isSlipping = false;
        }
        else
        {
            _forceForwards = Mathf.Sign(forceToAddFromTorque) * _dynamicFrictionForceValue;
            isSlipping = true;
        }
    }

    public void Braking(float brakeForce)
    {
        //this works fine for now except if you turn the wheels while braking
        if (Mathf.Abs(_wheelVelocityLocal.z) < 1f)
        {
            _forceForwards = forceToAddFromTorque - (_wheelVelocityLocal.z * brakeForce);
        }
        else
        {
            _forceForwards = forceToAddFromTorque - (Mathf.Sign(_wheelVelocityLocal.z) * brakeForce);
        }
    }
    public void Handbrake(float handbrakepower)
    {
        if (Mathf.Abs(_wheelVelocityLocal.z) < 1f)
        {
            _forceForwards = forceToAddFromTorque - (_wheelVelocityLocal.z * handbrakepower);
        }
        else
        {
            _forceForwards = forceToAddFromTorque - handbrakepower;
        }
    }

    void Vfx(bool toggle)
    {
        _trailRenderer.emitting = toggle;
        if (toggle)
        {
            if (!_smokes.isPlaying)
            {
                _smokes.Play();
            }
        }
        else
        {
            _smokes.Stop();
        }
    }

    //Don't need gizmos anymore
    private void OnDrawGizmos()
    {
        //if (isGrounded)
        //{
        //    Gizmos.color = Color.green;
        //}
        //else
        //{
        //    Gizmos.color = Color.red;
        //}
        Gizmos.DrawWireSphere(transform.position - transform.up * suspensionLength, wheelRadius);
        Gizmos.DrawSphere(wheelToGroundContactPos, .1f);
        Gizmos.color = Color.black;
        Gizmos.DrawRay(_wheel.transform.position, actualForce);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(_wheel.transform.position, forceUp.normalized);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(_wheel.transform.position, forceSide.normalized);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(_wheel.transform.position, _RollResForce * 100f);
        if (willReceiveTorque)
        {
            Gizmos.color = Color.purple;
            Gizmos.DrawRay(_wheel.transform.position, forceForward.normalized * 2f);
        }
    }
}
