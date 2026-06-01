using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class UITimerSubscriber : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timeText;

    private bool isSubscribed;

    private void OnEnable()
    {
        StartCoroutine(SubscribeWhenReady());
    }

    private void OnDisable()
    {
        if (!isSubscribed || TimerController.Instance == null)
        {
            return;
        }

        TimerController.Instance.TimeUpdated -= HandleTimeUpdated;
        isSubscribed = false;
    }

    private IEnumerator SubscribeWhenReady()
    {
        while (TimerController.Instance == null)
        {
            yield return null;
        }

        if (isSubscribed)
        {
            yield break;
        }

        TimerController.Instance.TimeUpdated += HandleTimeUpdated;
        isSubscribed = true;

        //Debug.Log("Subscribed to TimerController TimeUpdated event.");
    }

    private void HandleTimeUpdated(TimeSpan time)
    {
        if (timeText == null)
        {
            return;
        }

        timeText.text = string.Format("{0:00}:{1:00}.{2:00}", time.Minutes, time.Seconds, time.Milliseconds / 10);
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (TimerController.Instance != null)
            {
                TimerController.Instance.PauseTimer();
            }
        }

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (TimerController.Instance != null)
            {
                TimerController.Instance.ResumeTimer();
            }
        }

        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            if (TimerController.Instance != null)
            {
                TimerController.Instance.StartTimer();
            }
        }
    }
}