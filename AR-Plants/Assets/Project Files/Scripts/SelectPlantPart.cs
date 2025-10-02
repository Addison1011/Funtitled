using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;

[RequireComponent(requiredComponent: typeof(ARRaycastManager),
   requiredComponent2: typeof(ARPlaneManager))]
public class SelectPlantPart : MonoBehaviour
{
    [SerializeField]
    private GameObject objectToSelect;
    private string selectedPart;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {

    }

    private void OnEnable()
    {
        EnhancedTouch.TouchSimulation.Enable();
        EnhancedTouch.EnhancedTouchSupport.Enable();
        EnhancedTouch.Touch.onFingerDown += FingerDown;
    }

    private void OnDisable()
    {
        EnhancedTouch.TouchSimulation.Disable();
        EnhancedTouch.EnhancedTouchSupport.Disable();
        EnhancedTouch.Touch.onFingerDown -= FingerDown;

    }

    // Called when geting or setting selectedPart
    public string SelectedPart
    {
        get { return selectedPart; }
        set { selectedPart = value; }
    }

    private void FingerDown(EnhancedTouch.Finger finger)
    {
        if (finger.index != 0)
            return;

        // Send out raycast from touch position
        Ray ray = Camera.main.ScreenPointToRay(finger.currentTouch.screenPosition);
        RaycastHit hit;

        // Check if the raycast hits an object
        if (Physics.Raycast(ray, out hit))
        {
            // Check if the hit object has the tag "Branch", "Trunk", or "Leaves"
            if (hit.transform.gameObject.CompareTag("Branch"))
            {
                // Handle selection logic here
                selectedPart = "Branch";
                Debug.Log("Branch");
            }
            if (hit.transform.gameObject.CompareTag("Trunk"))
            {
                // Handle selection logic here
                selectedPart = "Trunk";
                Debug.Log("Trunk");
            }
            if (hit.transform.gameObject.CompareTag("Leaves"))
            {
                // Handle selection logic here
                selectedPart = "Leaves";
                Debug.Log("Leaves");
            }

        }
    }
}