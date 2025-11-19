using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointSingle : MonoBehaviour
{

    private TrackCheckpoints trackCheckpoints;
    private MeshRenderer meshRenderer;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void Start()
    {
        //Hide();
    }

    private void OnTriggerEnter(Collider other)
    {
        CarStatisticsClass carTransform = other.GetComponentInParent<CarStatisticsClass>();

        if (carTransform != null)
        {
            //Debug.Log("Checkpoint reached by car: " + carTransform.name);
            trackCheckpoints.CarThroughCheckpoint(this, carTransform.gameObject.transform);
        }
    }

    public void SetTrackCheckpoints(TrackCheckpoints trackCheckpoints)
    {
        this.trackCheckpoints = trackCheckpoints;
    }

    public void Show()
    {
        meshRenderer.enabled = true;
    }

    public void Hide()
    {
        meshRenderer.enabled = false;
    }

}
