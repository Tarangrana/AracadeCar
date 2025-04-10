using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;  // Required for scene reloading

public class CheckpointsAndLaps : MonoBehaviour
{
    [Header("Points")]
    public GameObject start;
    public GameObject end;
    public GameObject[] checkpoints;

    [Header("Settings")]
    public float laps = 1;
    public float lapTimeLimit = 30f;      // Time limit to complete a lap (in seconds)
    public float countdownDuration = 3f;  // Pre-game countdown duration

    [Header("Car Control")]
    // Assign your car movement script (e.g., CarController) here via the Inspector.
    // This will be disabled during the countdown.
    public MonoBehaviour carController;

    [Header("Information")]
    private int currentCheckpoint = 0;
    private int currentLap = 1;
    private bool started = false;    // True when the race has started.
    private bool finished = false;   // True when the player finishes the race.
    private bool gameOver = false;   // True if the time limit is exceeded.
    
    // Countdown variables
    private float countdownTime;
    private bool countdownActive = true;

    private float currentLapTime;
    private float bestLapTime;
    private int bestLap;

    private void Start()
    {
        // Initialize race variables
        currentCheckpoint = 0;
        currentLap = 1;
        started = false;
        finished = false;
        gameOver = false;
        currentLapTime = 0;
        bestLapTime = 0;
        bestLap = 0;

        // Start the countdown.
        countdownTime = countdownDuration;
        countdownActive = true;

        // Disable car controls during countdown.
        if (carController != null)
        {
            carController.enabled = false;
        }
    }

    private void Update()
    {
        // Handle the pre-game countdown.
        if (countdownActive)
        {
            countdownTime -= Time.deltaTime;
            if (countdownTime <= 0)
            {
                countdownActive = false;
                started = true;  // Automatically start the race
                Debug.Log("Go!");
                
                // Enable car controls now that the countdown is finished.
                if (carController != null)
                {
                    carController.enabled = true;
                }
            }
            // Skip the rest of Update during countdown.
            return;
        }

        // Update the lap timer if the race is active.
        if (started && !finished && !gameOver)
        {
            currentLapTime += Time.deltaTime;

            // Check if the lap time exceeds the limit.
            if (currentLapTime >= lapTimeLimit)
            {
                Debug.Log("Time up! Lap failed. Game Over.");
                gameOver = true;
            }
        }

        // Optionally update the best lap time if the race is active.
        if (started && !gameOver && !finished)
        {
            if (bestLap == currentLap)
            {
                bestLapTime = currentLapTime;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Ignore trigger events if countdown is active or the game is over or finished.
        if (countdownActive || gameOver || finished)
            return;

        if (other.CompareTag("Checkpoint"))
        {
            GameObject thisCheckpoint = other.gameObject;

            // Check for the start checkpoint (if desired; note that the race auto-starts after countdown).
            if (thisCheckpoint == start && !started)
            {
                Debug.Log("Started");
                started = true;
            }
            // Process the lap finish when the end checkpoint is hit.
            else if (thisCheckpoint == end && started)
            {
                // Verify that all checkpoints were passed.
                if (currentCheckpoint == checkpoints.Length)
                {
                    // If this is the final lap, finish the race.
                    if (currentLap == laps)
                    {
                        if (currentLapTime < bestLapTime)
                        {
                            bestLap = currentLap;
                        }
                        finished = true;
                        Debug.Log("Finished! You Won!");
                    }
                    // Otherwise, start the next lap.
                    else if (currentLap < laps)
                    {
                        if (currentLapTime < bestLapTime)
                        {
                            bestLap = currentLap;
                            bestLapTime = currentLapTime;
                        }
                        currentLap++;
                        currentCheckpoint = 0;
                        currentLapTime = 0;
                        Debug.Log($"Started lap {currentLap}");
                    }
                }
                else
                {
                    Debug.Log("Did not go through all checkpoints");
                }
            }

            // Process checkpoint order.
            for (int i = 0; i < checkpoints.Length; i++)
            {
                if (thisCheckpoint == checkpoints[i])
                {
                    // Correct checkpoint order.
                    if (i == currentCheckpoint)
                    {
                        Debug.Log($"Correct Checkpoint reached: {Mathf.FloorToInt(currentLapTime / 60)}:{currentLapTime % 60:00.000}");
                        currentCheckpoint++;
                    }
                    else
                    {
                        Debug.Log("Incorrect checkpoint");
                    }
                }
            }
        }
    }

    private void OnGUI()
    {
        // Define a GUIStyle for the timer (large text for the top center).
        GUIStyle timerStyle = new GUIStyle(GUI.skin.label);
        timerStyle.fontSize = 40;
        timerStyle.alignment = TextAnchor.UpperCenter;
        timerStyle.normal.textColor = Color.white;

        // Define a separate GUIStyle for final messages (slightly smaller).
        GUIStyle messageStyle = new GUIStyle(GUI.skin.label);
        messageStyle.fontSize = 30;
        messageStyle.alignment = TextAnchor.MiddleCenter;
        messageStyle.normal.textColor = Color.white;

        // Display the countdown timer (top center) while counting down.
        if (countdownActive)
        {
            string countdownDisplay = (countdownTime > 1f) ? Mathf.CeilToInt(countdownTime).ToString() : "Go!";
            GUI.Label(new Rect(Screen.width / 2 - 100, 10, 200, 50), countdownDisplay, timerStyle);
            return;
        }

        // Final results display: if finished or if game over.
        if (finished)
        {
            GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 70, 200, 50), "You Won!", messageStyle);
            if (GUI.Button(new Rect(Screen.width / 2 - 50, Screen.height / 2 - 20, 100, 30), "Play Again"))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            return;
        }

        if (gameOver)
        {
            GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 70, 200, 50), "Game Over!", messageStyle);
            if (GUI.Button(new Rect(Screen.width / 2 - 50, Screen.height / 2 - 20, 100, 30), "Play Again"))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            return;
        }

        // Display only the remaining time at the top center as the timer during active gameplay.
        float timeLeft = lapTimeLimit - currentLapTime;
        string timerDisplay = timeLeft.ToString("F2");  // Format the time to two decimal places.
        GUI.Label(new Rect(Screen.width / 2 - 100, 10, 200, 50), timerDisplay, timerStyle);
    }
}
