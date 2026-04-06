using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimerController : MonoBehaviour
{
    public static TimerController Instance { get; private set; }

    public TextMeshProUGUI timeCounter; 

    private bool isRunning = false;
    public bool isPaused = false;

    private TimeSpan timeSpan;

    private float elapsedTime;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timeCounter.text = "00:00.00";
        isRunning = false;
        StartTimer();
    }

    public void StartTimer()
    {
        isRunning = true;
        isPaused = false;
        elapsedTime = 0f;

        StartCoroutine(UpdateTimer());
    }

    private IEnumerator UpdateTimer()
    {
        while (isRunning)
        {
            if (!isPaused)
            {
                elapsedTime += Time.deltaTime;
                timeSpan = TimeSpan.FromSeconds(elapsedTime);
                timeCounter.text = string.Format("{0:00}:{1:00}.{2:00}", timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds / 10);
            }
            yield return null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
