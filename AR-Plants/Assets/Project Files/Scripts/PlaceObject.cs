using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;

[RequireComponent(requiredComponent: typeof(ARRaycastManager),
   requiredComponent2: typeof(ARPlaneManager))]
public class PlaceObject : MonoBehaviour
{
    [SerializeField]
    private GameObject objectToPlace;

    private ARRaycastManager aRRaycastManager;
    private ARPlaneManager aRPlaneManager;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        aRRaycastManager = GetComponent<ARRaycastManager>();
        aRPlaneManager = GetComponent<ARPlaneManager>();
    }

    private void OnEnable()
    {
        EnhancedTouch.TouchSimulation.Enable();
        EnhancedTouch.EnhancedTouchSupport.Enable();
        EnhancedTouch.Touch.onFingerDown += FingerDown;
    }

    void OnDestroy()
    {
        EnhancedTouch.TouchSimulation.Disable();
        EnhancedTouch.EnhancedTouchSupport.Disable();
        EnhancedTouch.Touch.onFingerDown -= FingerDown;

    }

    private void FingerDown(EnhancedTouch.Finger finger)
    {
        if (finger.index != 0)
            return;
        if (aRRaycastManager.Raycast(finger.currentTouch.screenPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            foreach (ARRaycastHit hit in hits)
            {
                Pose pose = hit.pose;
                // Raise the object a bit above the plane to avoid z-fighting
                GameObject obj = Instantiate(objectToPlace, pose.position, pose.rotation);
                obj.transform.position = new Vector3(obj.transform.position.x, obj.transform.position.y + 1.5f, obj.transform.position.z);
            }

        }
    }
}
