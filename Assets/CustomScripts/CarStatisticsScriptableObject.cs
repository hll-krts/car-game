using UnityEngine;

[CreateAssetMenu(fileName = "_Statistics", menuName = "Scriptable Objects/CarStatisticsScriptableObject")]
public class CarStatisticsScriptableObject : ScriptableObject
{
    [Space(10)]
    public int maxSpeed; //The maximum speed that the car can reach in km/h.
    public int maxReverseSpeed; //The maximum speed that the car can reach while going on reverse in km/h.    
    public int motorTorque; // How fast the car can accelerate. 1 is a slow acceleration and 10 is the fastest.

    [Space(10)]
    public int maxSteeringAngle; // The maximum angle that the tires can reach while rotating the steering wheel.
    public float steeringSpeed; // How fast the steering wheel turns.

    [Space(10)]
    public int brakeForce; // The strength of the wheel brakes.
    public int decelerationMultiplier; // How fast the car decelerates when the user is not using the throttle.
    public int handbrakeDriftMultiplier; // How much grip the car loses when the user hit the handbrake.

    [Space(10)]
    public int carWeight;
}
