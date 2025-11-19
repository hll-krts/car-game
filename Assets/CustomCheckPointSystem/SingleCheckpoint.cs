using UnityEngine;

public class SingleCheckpoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Checkpoint triggered by: " + other.name);
        if (other.GetComponentInParent<CarStatisticsClass>())
        {
            Debug.Log("Checkpoint reached by car: " + other.name);
        }
    }
}
