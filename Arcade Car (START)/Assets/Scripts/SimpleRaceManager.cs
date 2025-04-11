using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleRaceManager : MonoBehaviour
{
    public static SimpleRaceManager Instance;
    
    private bool raceFinished = false;
    private string winner = "";

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // This method is called by a car when it finishes the race.
    public void DeclareWinner(string carName)
    {
        if (raceFinished) return;  // Only the first finisher is the winner.
        raceFinished = true;
        winner = carName;
        Debug.Log("Winner: " + carName);
    }

    private void OnGUI()
    {
        if (raceFinished)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 40;
            style.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(Screen.width / 2 - 150, Screen.height / 2 - 100, 300, 100),
                "Winner: " + winner, style);

            if (GUI.Button(new Rect(Screen.width / 2 - 50, Screen.height / 2 + 20, 100, 30), "Play Again"))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }
}
