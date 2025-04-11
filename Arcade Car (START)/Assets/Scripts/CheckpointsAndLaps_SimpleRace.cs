using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointsAndLaps_SimpleRace : MonoBehaviour
{
    [Header("Points")]
    public GameObject start;       // The start checkpoint.
    public GameObject end;         // The finish checkpoint.
    public GameObject[] checkpoints;  // Intermediate checkpoints that must be passed in order.

    [Header("Settings")]
    public int laps = 1;  // For testing, set to 1. You can later adjust for multi–lap races.
    
    [Header("Car Info")]
    // Used to identify the car ("Player" or "AI")
    public string carName = "Player";
    
    // Internal variables to track progress.
    private int currentCheckpoint = 0;
    private int currentLap = 1;
    private bool started = false;
    private bool finished = false;

    private void Start()
    {
        currentCheckpoint = 0;
        currentLap = 1;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Only process triggers tagged "Checkpoint"
        if (!other.CompareTag("Checkpoint"))
            return;

        GameObject thisCheckpoint = other.gameObject;
        
        // Start the race by hitting the start checkpoint if not already started.
        if (thisCheckpoint == start && !started)
        {
            started = true;
            Debug.Log(carName + " has started the race!");
        }
        // When the end checkpoint is hit during the race:
        else if (thisCheckpoint == end && started)
        {
            // Only finish if all intermediate checkpoints have been correctly passed.
            if (currentCheckpoint == checkpoints.Length)
            {
                // If this is the final lap:
                if (currentLap == laps)
                {
                    finished = true;
                    Debug.Log(carName + " finished the race!");
                    
                    // Tell the race manager this car is a finisher.
                    if (SimpleRaceManager.Instance != null)
                    {
                        SimpleRaceManager.Instance.DeclareWinner(carName);
                    }
                }
                // Otherwise, if more laps remain:
                else
                {
                    currentLap++;
                    currentCheckpoint = 0;
                    Debug.Log(carName + " started lap " + currentLap);
                }
            }
            else
            {
                Debug.Log(carName + " did not pass through all checkpoints.");
            }
        }

        // Check intermediate checkpoints (they must be passed in order).
        for (int i = 0; i < checkpoints.Length; i++)
        {
            if (thisCheckpoint == checkpoints[i])
            {
                // Only accept checkpoints in sequential order.
                if (i == currentCheckpoint)
                {
                    currentCheckpoint++;
                    Debug.Log(carName + " passed checkpoint " + (i + 1));
                }
                else
                {
                    Debug.Log(carName + " hit an out-of-order checkpoint!");
                }
            }
        }
    }
}
