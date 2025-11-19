using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TrackCheckpoints : MonoBehaviour
{
    [SerializeField] private CustomTimeManager timeManager;

    public int maxLaps; [SerializeField] private int currentLaps;
    public bool isRacing = true;

    public event EventHandler OnPlayerCorrectCheckpoint;
    public event EventHandler OnPlayerWrongCheckpoint;

    [SerializeField] private List<Transform> carTransformList;

    private List<CheckpointSingle> checkpointSingleList;
    private List<int> nextCheckpointSingleIndexList;
    [SerializeField] private int firstCheckpointIndex;

    [SerializeField] private TextMeshProUGUI CurrentLapText, MaxLapsText;

    private void Awake()
    {
        currentLaps = 1;
        MaxLapsText.text = "" + maxLaps;
        CurrentLapText.text = "" + currentLaps;

        Transform checkpointsTransform = transform.Find("SingleCheckpoints").transform;

        checkpointSingleList = new List<CheckpointSingle>();
        foreach (Transform checkpointSingleTransform in checkpointsTransform)
        {
            CheckpointSingle checkpointSingle = checkpointSingleTransform.GetComponent<CheckpointSingle>();

            checkpointSingle.SetTrackCheckpoints(this);

            checkpointSingleList.Add(checkpointSingle);

            checkpointSingleTransform.gameObject.SetActive(false);
        }

        nextCheckpointSingleIndexList = new List<int>();
        foreach (Transform carTransform in carTransformList)
        {
            nextCheckpointSingleIndexList.Add(firstCheckpointIndex);
            checkpointSingleList[firstCheckpointIndex].gameObject.SetActive(true);
        }

    }

    public void CarThroughCheckpoint(CheckpointSingle checkpointSingle, Transform carTransform)
    {
        int nextCheckpointSingleIndex = nextCheckpointSingleIndexList[carTransformList.IndexOf(carTransform)];
        int _checkPointIndex = checkpointSingleList.IndexOf(checkpointSingle);

        switch (_checkPointIndex)
        {
            case 0:
                timeManager.StartTimer();
                break;
            case int n when (n == checkpointSingleList.Count - 1):
                if (currentLaps == 1)
                {
                    timeManager.BestLapTime();
                }
                timeManager.StopTimer();
                break;
        }

        if (isRacing)
        {
            if (_checkPointIndex == nextCheckpointSingleIndex)
            {
                // Correct checkpoint
                //Debug.Log("Correct");
                CheckpointSingle correctCheckpointSingle = checkpointSingleList[nextCheckpointSingleIndex];
                //correctCheckpointSingle.Hide();
                correctCheckpointSingle.gameObject.SetActive(false);


                nextCheckpointSingleIndexList[carTransformList.IndexOf(carTransform)]
                    = (nextCheckpointSingleIndex + 1) % checkpointSingleList.Count;

                if (nextCheckpointSingleIndexList[carTransformList.IndexOf(carTransform)] == 0)
                {
                    if (currentLaps < maxLaps)
                    {
                        currentLaps++;
                        CurrentLapText.text = "" + currentLaps;
                    }
                    else
                    {
                        CurrentLapText.text = "" + maxLaps;
                        isRacing = false;
                    }
                }

                checkpointSingleList[nextCheckpointSingleIndexList[carTransformList.IndexOf(carTransform)]]
                    .gameObject.SetActive(true);

                //OnPlayerCorrectCheckpoint?.Invoke(this, EventArgs.Empty);
            }
            //else
            //{
            //    // Wrong checkpoint
            //    Debug.Log("Wrong");
            //    //OnPlayerWrongCheckpoint?.Invoke(this, EventArgs.Empty);

            //    CheckpointSingle correctCheckpointSingle = checkpointSingleList[nextCheckpointSingleIndex];
            //    //correctCheckpointSingle.Show();
            //} 
        }
    }


}
