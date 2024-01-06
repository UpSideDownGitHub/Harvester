using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public GameObject[] screens;
    public int current = 0;


    /// <summary>
    /// called when the object is enabled and will set curent to 0 to ensure startning from the start
    /// </summary>
    public void OnEnable()
    {
        current = 0;
    }

    /// <summary>
    /// called by button press and will activate the next tutorial screen, but will loop to start 
    /// if end reached
    /// </summary>
    public void Next()
    {
        screens[current].SetActive(false);
        current = current + 1 >= screens.Length ? 0 : current + 1;
        screens[current].SetActive(true);
    }

    /// <summary>
    /// called by button press and will activate the previous tutorial screen, but will loop to
    /// end if start is reached.
    /// </summary>
    public void Previous()
    {
        screens[current].SetActive(false);
        current = current - 1 < 0 ? screens.Length - 1 : current - 1;
        screens[current].SetActive(true);
    }
}
