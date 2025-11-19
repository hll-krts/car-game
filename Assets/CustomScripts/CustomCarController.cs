using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class CarController : MonoBehaviour
{    
    public enum DriveType { FWD, RWD, AWD }
    public DriveType driveType;
    private CustomDefaultActions inputActions;

    private Rigidbody playerRB;
    public WheelColliders colliders;
    public WheelMeshes wheelMeshes;
    public WheelParticles wheelParticles;
    public float gasInput;
    public float brakeInput;
    public float steeringInput;
    public GameObject smokePrefab;
    public float motorPower, brakePower, slipAngle, speed;
    public AnimationCurve steeringCurve;

    // Start is called before the first frame update
    void Start()
    {
        playerRB = gameObject.GetComponent<Rigidbody>();
        InstantiateSmoke();
        inputActions = new CustomDefaultActions();

        inputActions.CarPlaying.Enable();
    }

    void InstantiateSmoke()
    {
        wheelParticles.FRWheel = Instantiate(smokePrefab, colliders.FRWheel.transform.position - Vector3.up * colliders.FRWheel.radius, Quaternion.identity, colliders.FRWheel.transform)
            .GetComponent<ParticleSystem>();
        wheelParticles.FLWheel = Instantiate(smokePrefab, colliders.FLWheel.transform.position - Vector3.up * colliders.FRWheel.radius, Quaternion.identity, colliders.FLWheel.transform)
            .GetComponent<ParticleSystem>();
        wheelParticles.RRWheel = Instantiate(smokePrefab, colliders.RRWheel.transform.position - Vector3.up * colliders.FRWheel.radius, Quaternion.identity, colliders.RRWheel.transform)
            .GetComponent<ParticleSystem>();
        wheelParticles.RLWheel = Instantiate(smokePrefab, colliders.RLWheel.transform.position - Vector3.up * colliders.FRWheel.radius, Quaternion.identity, colliders.RLWheel.transform)
            .GetComponent<ParticleSystem>();
    }
    // Update is called once per frame

    void Update()
    {
        speed = playerRB.linearVelocity.magnitude;
        CheckParticles();
        ApplyWheelPositions();
    }
    private void FixedUpdate()
    {
        CheckInput();
        ApplyMotor();
        ApplyBrake();
        ApplySteering();       
    }

    void CheckInput()
    {
        gasInput = inputActions.CarPlaying.Gas.ReadValue<float>(); //Input.GetAxis("Vertical");
        steeringInput = inputActions.CarPlaying.Turn.ReadValue<float>();

        slipAngle = Vector3.Angle(transform.forward, playerRB.linearVelocity - transform.forward);

        //fixed code to brake even after going on reverse by Andrew Alex 
        float movingDirection = Vector3.Dot(transform.forward, playerRB.linearVelocity);
        if (movingDirection < -0.5f && gasInput > 0)
        {
            brakeInput = Mathf.Abs((float)gasInput);
        }
        else if (movingDirection > 0.5f && gasInput < 0)
        {
            brakeInput = Mathf.Abs((float)gasInput);
        }
        else
        {
            brakeInput = 0;
        }
    }

    void ApplyBrake()
    {
        colliders.FRWheel.brakeTorque = brakeInput * brakePower * 0.7f;
        colliders.FLWheel.brakeTorque = brakeInput * brakePower * 0.7f;

        //colliders.RRWheel.brakeTorque = brakeInput * brakePower * 0.3f;
        //colliders.RLWheel.brakeTorque = brakeInput * brakePower * 0.3f;
    }
    void ApplyMotor()
    {
        if (driveType == DriveType.RWD)
        {
            colliders.RRWheel.motorTorque = motorPower * (float)gasInput;
            colliders.RLWheel.motorTorque = motorPower * (float)gasInput; 
        }
    }
    void ApplySteering()
    {
        float steeringAngle = inputActions.CarPlaying.Turn.ReadValue<float>() * steeringCurve.Evaluate(speed);


        //AUTOMATIC DRIFTING COUNTERSTEER, MIGHT ADD TO MY OWN
        if (slipAngle < 120f)
        {
            steeringAngle += Vector3.SignedAngle(transform.forward, playerRB.linearVelocity + transform.forward, Vector3.up);
        }

        steeringAngle = Mathf.Clamp(steeringAngle, -90f, 90f);
        colliders.FRWheel.steerAngle = steeringAngle;
        colliders.FLWheel.steerAngle = steeringAngle;
    }

    void ApplyWheelPositions()
    {
        UpdateWheel(colliders.FRWheel, wheelMeshes.FRWheel);
        UpdateWheel(colliders.FLWheel, wheelMeshes.FLWheel);
        UpdateWheel(colliders.RRWheel, wheelMeshes.RRWheel);
        UpdateWheel(colliders.RLWheel, wheelMeshes.RLWheel);
    }

    void CheckParticles()
    {
        WheelHit[] wheelHits = new WheelHit[4];
        colliders.FRWheel.GetGroundHit(out wheelHits[0]);
        colliders.FLWheel.GetGroundHit(out wheelHits[1]);

        colliders.RRWheel.GetGroundHit(out wheelHits[2]);
        colliders.RLWheel.GetGroundHit(out wheelHits[3]);

        float slipAllowance = 0.5f;
        if ((Mathf.Abs(wheelHits[0].sidewaysSlip) + Mathf.Abs(wheelHits[0].forwardSlip) > slipAllowance))
        {
            wheelParticles.FRWheel.Play();
        }
        else
        {
            wheelParticles.FRWheel.Stop();
        }
        if ((Mathf.Abs(wheelHits[1].sidewaysSlip) + Mathf.Abs(wheelHits[1].forwardSlip) > slipAllowance))
        {
            wheelParticles.FLWheel.Play();
        }
        else
        {
            wheelParticles.FLWheel.Stop();
        }
        if ((Mathf.Abs(wheelHits[2].sidewaysSlip) + Mathf.Abs(wheelHits[2].forwardSlip) > slipAllowance))
        {
            wheelParticles.RRWheel.Play();
        }
        else
        {
            wheelParticles.RRWheel.Stop();
        }
        if ((Mathf.Abs(wheelHits[3].sidewaysSlip) + Mathf.Abs(wheelHits[3].forwardSlip) > slipAllowance))
        {
            wheelParticles.RLWheel.Play();
        }
        else
        {
            wheelParticles.RLWheel.Stop();
        }
    }

    void UpdateWheel(WheelCollider coll, MeshRenderer wheelMesh)
    {
        Quaternion quat;
        Vector3 position;
        coll.GetWorldPose(out position, out quat);
        Quaternion.Lerp(wheelMesh.transform.rotation, quat, Time.deltaTime * 5f);
        wheelMesh.transform.position = position;
        //wheelMesh.transform.rotation = quat;
    }
}
[System.Serializable]
public class WheelColliders
{
    public WheelCollider FRWheel;
    public WheelCollider FLWheel;
    public WheelCollider RRWheel;
    public WheelCollider RLWheel;
}
[System.Serializable]
public class WheelMeshes
{
    public MeshRenderer FRWheel;
    public MeshRenderer FLWheel;
    public MeshRenderer RRWheel;
    public MeshRenderer RLWheel;
}
[System.Serializable]
public class WheelParticles
{
    public ParticleSystem FRWheel;
    public ParticleSystem FLWheel;
    public ParticleSystem RRWheel;
    public ParticleSystem RLWheel;

}