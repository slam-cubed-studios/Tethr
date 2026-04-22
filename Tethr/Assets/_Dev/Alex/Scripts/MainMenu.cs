using System;
using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainMenu : MonoBehaviour
{
    public GameObject GrappleHook;
    //[Range(0.1f, 1f)]
    public float GrappleHookSpeed = 1f;

    Vector3 clickedPosition;
    Vector3 originalHookPosition;

    bool isHookOut = false;
    bool isMenuOpen = true;

    [SerializeField] GameObject levelSelectWindow;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        levelSelectWindow.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePosition = Mouse.current.position.ReadValue();

        if (Mouse.current.leftButton.wasPressedThisFrame && isMenuOpen)
        {
            //Store mouse position at the moment of the click and use that to calculate the new position of the grapple hook
            clickedPosition = mousePosition;

            //Store the original position of the grapple hook to use as a reference for returning it
            originalHookPosition = GrappleHook.transform.GetChild(0).gameObject.transform.position;

            //Vector3 velocity = Vector3.zero;

            //Move the grapple hook towards the clicked position at a speed determined by GrappleHookSpeed, but only if it's not already at that position
            if (GrappleHook.transform.GetChild(0).gameObject.transform.position != clickedPosition)
            {
                StartCoroutine(MoveGrappleHook(GrappleHook.transform.GetChild(0).gameObject.transform.position, clickedPosition));
                isHookOut = true;
            }

            if (GrappleHook.transform.GetChild(0).gameObject.transform.position == clickedPosition)// && it hit a button -> do button functionality here
            {
                //GrappleHook.transform.GetChild(0).gameObject.transform.position = Vector3.MoveTowards(clickedPosition, originalHookPosition, GrappleHookSpeed * Time.deltaTime);
            }

            //Debug.Log((clickedPosition , currentHookPosition));

        }
        else if(Mouse.current.rightButton.wasPressedThisFrame && isMenuOpen)
        {
            //Return the grapple hook to its original position at a speed determined by GrappleHookSpeed, but only if it's not already at that position
            if (GrappleHook.transform.GetChild(0).gameObject.transform.position != originalHookPosition)
            {
                StartCoroutine(ReturnGrappleHook(GrappleHook.transform.GetChild(0).gameObject.transform.position, originalHookPosition));
                isHookOut = false;
            }
        }

        if(!isHookOut && isMenuOpen)
        {
            GrappleHook.transform.rotation = Quaternion.LookRotation(Vector3.forward, mousePosition - GrappleHook.transform.position);
        }

        if(!levelSelectWindow.activeSelf)
        {
            isMenuOpen = true;
        }
    }

    private IEnumerator MoveGrappleHook(Vector3 location, Vector3 target)
    {
        //if (location != target)
        //{
        yield return new WaitForEndOfFrame();
        GrappleHook.transform.GetChild(0).gameObject.transform.position = Vector3.Lerp(GrappleHook.transform.GetChild(0).gameObject.transform.position, target, GrappleHookSpeed * Time.deltaTime);
        //}
    }
    private IEnumerator ReturnGrappleHook(Vector3 location, Vector3 target)
    {
        while (GrappleHook.transform.GetChild(0).gameObject.transform.position != target)
        {
            //yield return new WaitForEndOfFrame();
            GrappleHook.transform.GetChild(0).gameObject.transform.position = Vector3.MoveTowards(GrappleHook.transform.GetChild(0).gameObject.transform.position, target, GrappleHookSpeed * Time.deltaTime);
            if(Vector3.Distance(GrappleHook.transform.GetChild(0).gameObject.transform.position, target) < 5)
            {
                GrappleHook.transform.GetChild(0).gameObject.transform.position = target;
                break;
            }
        }
        yield return new WaitForEndOfFrame();
    }

    public void OpenLevelWindow()
    {
        levelSelectWindow.SetActive(true);
        isMenuOpen = false;
        //gameObject.GetComponent<MainMenu>().enabled = false;
    }
}
