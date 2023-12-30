using TMPro;
using UnityEngine;
public class Timer : MonoBehaviour
{
    public float timeRemaining = 10;
    public TMP_Text timeText;

/// <summary>
/// Manages a countdown timer and updates a UI text component to display the remaining time.
/// </summary>
/// <remarks>
/// This class is responsible for tracking time in seconds and formatting it into a user-friendly
/// format (minutes, seconds, and milliseconds). It specifically targets a Unity UI text component
/// for displaying the countdown. The countdown is initiated upon instantiation, and the text is
/// continuously updated in the Update method.
/// </remarks>
    void Update()
    {
        timeRemaining -= Time.deltaTime;
        float minutes = Mathf.FloorToInt(timeRemaining / 60);
        float seconds = Mathf.FloorToInt(timeRemaining % 60);
        float milliSeconds = (timeRemaining % 1) * 1000;
        timeText.text = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliSeconds);
    }
}