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
        RayCastCarController carTransform = other.GetComponentInParent<RayCastCarController>();

        if (carTransform != null)
        {
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
