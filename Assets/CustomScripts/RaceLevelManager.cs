using UnityEngine;

public class RaceLevelManager : MonoBehaviour
{
    [SerializeField] private DataObject dataObject;
    [SerializeField] private CustomCarUtilities carUtils;
    [SerializeField] private RayCastCarController carController;

    [SerializeField] private int _NumberOfLaps;
    private TrackCheckpoints trackCheckpoints;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        carController = GameObject.FindFirstObjectByType<RayCastCarController>(FindObjectsInactive.Include);
        carController.enabled = false;
        carController.isControllable = false;
        carController.gameObject.SetActive(false);

        trackCheckpoints = GameObject.FindAnyObjectByType<TrackCheckpoints>();
        trackCheckpoints.maxLaps = _NumberOfLaps;

        trackCheckpoints.carTransformList.Add(carController.transform);
    }
    private void Start()
    {
        dataObject = FindFirstObjectByType<DataObject>();
        carController.carStatistics = dataObject.selectedCarStats;

        if (!carController.gameObject.activeSelf)
        {
            carController.enabled = true;
            carController.isControllable = true;
            carController.gameObject.SetActive(true);
        }

        carUtils = FindAnyObjectByType<CustomCarUtilities>(FindObjectsInactive.Include);
        carUtils.dataobj = dataObject;
    }
}
