using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimerController : MonoBehaviour
{
    public static TimerController Instance { get; private set; }

    public event Action<TimeSpan> TimeUpdated;
    public event Action TimerStarted;
    public event Action TimerStopped;
    public event Action TimerPaused;
    public event Action TimerResumed;

    private bool isRunning = false;
    public bool isPaused = false;

    private TimeSpan timeSpan;
    private float elapsedTime;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {

    }

    private void OnDisable()
    {

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timeSpan = TimeSpan.Zero;
        TimeUpdated?.Invoke(timeSpan);
        isRunning = false;
        StartTimer();
    }

    public void StartTimer()
    {
        isRunning = true;
        isPaused = false;
        elapsedTime = 0f;
        timeSpan = TimeSpan.Zero;

        TimerStarted?.Invoke();
        TimeUpdated?.Invoke(timeSpan);

        StartCoroutine(UpdateTimer());
    }

    public void PauseTimer()
    {
        if (!isRunning || isPaused)
        {
            return;
        }

        isPaused = true;
        TimerPaused?.Invoke();
    }

    public void ResumeTimer()
    {
        if (!isRunning || !isPaused)
        {
            return;
        }

        isPaused = false;
        TimerResumed?.Invoke();
    }

    public void StopTimer()
    {
        if (!isRunning)
        {
            return;
        }

        isRunning = false;
        TimerStopped?.Invoke();
    }

    private IEnumerator UpdateTimer()
    {
        while (isRunning)
        {
            if (!isPaused)
            {
                elapsedTime += Time.deltaTime;
                timeSpan = TimeSpan.FromSeconds(elapsedTime);
                TimeUpdated?.Invoke(timeSpan);
            }

            yield return null;
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
