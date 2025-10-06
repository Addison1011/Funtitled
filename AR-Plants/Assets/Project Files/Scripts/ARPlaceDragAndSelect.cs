using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem.EnhancedTouch;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;

[RequireComponent(typeof(ARRaycastManager))]
public class ARPlaceDragAndSelect : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera arCamera;                   // AR Camera
    [SerializeField] private GameObject selectedPlant;          // Prefab to place

    [Header("Tuning")]
    [SerializeField] private float yOffsetMeters = 0.02f;       // lift to avoid z-fighting
    [SerializeField] private float followLerp = 14f;            // smoothing for drag
    [SerializeField] private float smoothMovePlantProximity = 0.5f;  // radius around plant to consider it a position to smooth move to
    [Header("Hold / Tap Settings")]
    [Tooltip("Hold duration (seconds) required on the plant to start dragging.")]
    [SerializeField] private float holdToDragSeconds = 0.4f;
    [Tooltip("Max finger movement (pixels) still considered a tap/hold (pre-drag).")]
    [SerializeField] private float tapSlopPixels = 12f;

    [Header("Tap Callback")]
    public UnityEvent onPlantTapped; // hook UI, selection, etc.

    private ARRaycastManager aRRaycastManager;
    private readonly List<ARRaycastHit> hits = new();

    // Placement state
    private bool isPlantPlaced = false;
    private bool smoothMoveEnabled = false; // whether to smooth move the plant on tap
    private GameObject activePlant;

    // Drag state
    private bool isDragging = false;
    private Vector3 desiredWorldPos;

    // Hold-to-drag state
    private bool holdCandidate = false;               // currently holding on plant (might become drag)
    private float holdStartTime;
    private Vector2 holdStartScreenPos;
    private Finger holdFinger;

    private void Awake()
    {
        aRRaycastManager = GetComponent<ARRaycastManager>();
        if (arCamera == null) arCamera = Camera.main;
    }

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
        EnhancedTouch.Touch.onFingerDown += OnFingerDown;
        EnhancedTouch.Touch.onFingerMove += OnFingerMove;
        EnhancedTouch.Touch.onFingerUp += OnFingerUp;
    }

    private void OnDisable()
    {
        EnhancedTouch.Touch.onFingerDown -= OnFingerDown;
        EnhancedTouch.Touch.onFingerMove -= OnFingerMove;
        EnhancedTouch.Touch.onFingerUp -= OnFingerUp;
        EnhancedTouchSupport.Disable();
    }

    private void Update()
    {
        // Promote hold -> drag when time & slop constraints satisfied
        if (holdCandidate && !isDragging && holdFinger != null)
        {
            float heldFor = Time.time - holdStartTime;
            float moved = (holdFinger.currentTouch.screenPosition - holdStartScreenPos).magnitude;

            if (moved <= tapSlopPixels && heldFor >= holdToDragSeconds)
            {
                // Begin dragging
                isDragging = true;

                if (TryARRaycastToAllowedPlane(holdFinger.currentTouch.screenPosition, out Pose pose))
                    desiredWorldPos = pose.position + Vector3.up * yOffsetMeters;
            }
        }

        if (isDragging && activePlant != null || smoothMoveEnabled)
        {
            activePlant.transform.position =
                Vector3.Lerp(activePlant.transform.position, desiredWorldPos, Time.deltaTime * followLerp);
            if (activePlant.transform.position == desiredWorldPos)
            {
                smoothMoveEnabled = false; // stop smoothing once we reach the target
            }
        }


    }

    //++++ Touch Handlers ++++
    private void OnFingerDown(Finger finger)
    {
        Vector2 screenPos = finger.currentTouch.screenPosition;

        // If plant exists and touch is on the plant -> start HOLD candidate
        if (isPlantPlaced && activePlant != null && HitActivePlant(screenPos))
        {
            holdCandidate = true;
            holdStartTime = Time.time;
            holdStartScreenPos = screenPos;
            holdFinger = finger;
            return;
        }

        // Otherwise, we are NOT touching the plant here.
        // We won't move/place immediately; we'll confirm it's a TAP on finger up.
        holdCandidate = false;
        holdFinger = finger; // remember so we can check on FingerUp
    }

    private void OnFingerMove(Finger finger)
    {
        if (!isDragging || activePlant == null || finger != holdFinger) return;

        if (TryARRaycastToAllowedPlane(finger.currentTouch.screenPosition, out Pose planePose))
        {
            desiredWorldPos = planePose.position + Vector3.up * yOffsetMeters;
            // align rotation:
            // activePlant.transform.rotation = planePose.rotation;
        }
    }

    private void OnFingerUp(Finger finger)
    {
        if (finger != holdFinger)
            return;

        // Case A: we were holding on the plant but never transitioned to drag -> treat as a TAP on plant
        if (holdCandidate && !isDragging)
        {
            OnPlantTapped(); // placeholder behavior
        }
        // Case B: we were NOT holding on the plant (finger down wasn't on plant) -> treat as TAP on empty plane
        else if (!holdCandidate && !isDragging)
        {
            if (TryARRaycastToAllowedPlane(finger.currentTouch.screenPosition, out Pose pose))
            {

                if (!isPlantPlaced)
                {
                    // Place new
                    isPlantPlaced = true;
                    activePlant = Instantiate(selectedPlant, pose.position, pose.rotation);
                    activePlant.transform.position += Vector3.up * yOffsetMeters;

                    if (activePlant.GetComponent<Collider>() == null)
                        activePlant.AddComponent<BoxCollider>();
                }
                else if (
                    Mathf.Abs(activePlant.transform.position.x - pose.position.x) <= smoothMovePlantProximity &&
                        Mathf.Abs(activePlant.transform.position.z - pose.position.z) <= smoothMovePlantProximity)
                {
                    // Smooth move to the desired position
                    smoothMoveEnabled = true;
                    desiredWorldPos = pose.position + Vector3.up * yOffsetMeters;
                }
                else
                {
                    // Move existing there
                    activePlant.transform.position =
                            Vector3.Lerp(activePlant.transform.position, desiredWorldPos, Time.deltaTime * followLerp);
                    activePlant.transform.position = pose.position + Vector3.up * yOffsetMeters;
                }
            }
        }

        // Reset states
        holdCandidate = false;
        isDragging = false;
        holdFinger = null;
    }

    // helper method to raycast and find a valid plane
    // only accept planes whose pose Y-rotation == 270.
    private bool TryARRaycastToAllowedPlane(Vector2 screenPos, out Pose pose)
    {
        pose = default;

        if (!aRRaycastManager.Raycast(screenPos, hits, TrackableType.PlaneWithinPolygon))
            return false;

        // Use the closest hit that passes the rotation check
        foreach (var hit in hits)
        {
            var p = hit.pose;
            if (Mathf.Approximately(NormalizeAngle(p.rotation.eulerAngles.y), 270f))
            {
                pose = p;
                return true;
            }
        }
        return false;
    }

    // Normalize angle to be within 0-360 degrees
    private static float NormalizeAngle(float degrees)
    {
        degrees %= 360f;
        if (degrees < 0f) degrees += 360f;
        return degrees;
    }

    // Raycast from screen to check if we tapped the active plant
    private bool HitActivePlant(Vector2 screenPos)
    {
        if (arCamera == null || activePlant == null) return false;

        Ray ray = arCamera.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, ~0, QueryTriggerInteraction.Ignore))
        {
            return hit.collider != null &&
                   (hit.collider.gameObject == activePlant || hit.collider.transform.IsChildOf(activePlant.transform));
        }
        return false;
    }

    // Placeholder tap behavior on the plant (short tap)
    private void OnPlantTapped()
    {
        if (onPlantTapped != null) onPlantTapped.Invoke();
        else Debug.Log("Plant tapped (short press) — TODO: handle selection/details UI here.");
    }


}
