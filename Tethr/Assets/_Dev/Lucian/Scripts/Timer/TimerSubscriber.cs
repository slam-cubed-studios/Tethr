using System;
using UnityEngine;

public class TimerSubscriber : MonoBehaviour
{
    private void OnEnable()
    {
        if (TimerController.Instance == null)
        {
            return;
        }

        TimerController.Instance.TimeUpdated += HandleTimeUpdated;
        TimerController.Instance.TimerStarted += HandleTimerStarted;
        TimerController.Instance.TimerPaused += HandleTimerPaused;
        TimerController.Instance.TimerResumed += HandleTimerResumed;
        TimerController.Instance.TimerStopped += HandleTimerStopped;
    }

    private void OnDisable()
    {
        if (TimerController.Instance == null)
        {
            return;
        }

        TimerController.Instance.TimeUpdated -= HandleTimeUpdated;
        TimerController.Instance.TimerStarted -= HandleTimerStarted;
        TimerController.Instance.TimerPaused -= HandleTimerPaused;
        TimerController.Instance.TimerResumed -= HandleTimerResumed;
        TimerController.Instance.TimerStopped -= HandleTimerStopped;
    }

    private void HandleTimeUpdated(TimeSpan time)
    {
        Debug.Log($"Time: {time:mm\\:ss\\.ff}");
    }

    private void HandleTimerStarted()
    {
        Debug.Log("Timer started.");
    }

    private void HandleTimerPaused()
    {
        Debug.Log("Timer paused.");
    }

    private void HandleTimerResumed()
    {
        Debug.Log("Timer resumed.");
    }

    private void HandleTimerStopped()
    {
        Debug.Log("Timer stopped.");
    }
}