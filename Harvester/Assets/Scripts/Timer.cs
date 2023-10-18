using TMPro;
using UnityEngine;
public class Timer : MonoBehaviour
{
    public float timeRemaining = 10;
    public TMP_Text timeText;

    void Update()
    {
        timeRemaining -= Time.deltaTime;
        float minutes = Mathf.FloorToInt(timeRemaining / 60);
        float seconds = Mathf.FloorToInt(timeRemaining % 60);
        float milliSeconds = (timeRemaining % 1) * 1000;
        timeText.text = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliSeconds);
    }
}