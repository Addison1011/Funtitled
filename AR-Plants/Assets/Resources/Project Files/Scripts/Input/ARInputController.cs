using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem.EnhancedTouch;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.XR.ARCore;
using System.Collections;
using UnityEngine.InputSystem;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;
using UnityEngine.XR.Interaction.Toolkit.Attachment;
public enum PlantPart
{
    Stem,
    Leaf,
    Root,
    Flower,
    None
}
[RequireComponent(typeof(ARRaycastManager))]

public class ARInputController : MonoBehaviour
{
    public ARSession arSession;
    [Header("References")]
    [SerializeField] private Camera arCamera;                   // AR Camera
    [SerializeField] private GameObject selectedPlantModel; // Prefab to place
    [SerializeField] private SoundManager soundManager;
    private SelectedPlantData selectedPlantData;

    [SerializeField] private GameObject selectedPlantDataHandle;
    private ParticleSystem placementEffect;



    [Header("Tuning")]
    [SerializeField] private float yOffsetMeters = 0.02f;       // lift to avoid z-fighting
    [SerializeField] private float followLerp = 14f;            // smoothing for drag
    private float initialPinchDistance;
    private Vector3 initialScale;
    public Color emissionColor = Color.white;
    public float emissionIntensity = 1f;

    [Header("Hold / Tap Settings")]
    [Tooltip("Hold duration (seconds) required on the plant to start dragging.")]
    [SerializeField] private float holdToDragSeconds = 0.4f;
    [Tooltip("Max finger movement (pixels) still considered a tap/hold (pre-drag).")]
    [SerializeField] private float tapSlopPixels = 12f;



    [Header("Tap Callback")]
    public UnityEvent onPlantTapped; // hook UI, selection, etc.

    private ARRaycastManager aRRaycastManager;
    [Header("Placement Area Toggle")]
    [SerializeField] private ARPlaneManager arPlaneManager;
    [SerializeField] private bool allowHorizontalUp = true;
    [SerializeField] private bool allowHorizontalDown = false;
    [SerializeField] private bool allowVertical = false;
    private readonly List<ARRaycastHit> hits = new();

    // Placement state
    private bool isPlantPlaced = false;
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

        EnsureCamera();
        if (GameObject.FindWithTag("SelectedPlantData") != null)
        {
            selectedPlantDataHandle = GameObject.FindWithTag("SelectedPlantData");
        }


        //Gets the SelectedPlantData script from the SelectedPlantData GameObject
        selectedPlantData = selectedPlantDataHandle.GetComponent<SelectedPlantData>();

        selectedPlantModel = Resources.Load<GameObject>(selectedPlantData.plantInfo.scientificName); //default plant
        aRRaycastManager = GetComponent<ARRaycastManager>();
        placementEffect = selectedPlantModel.GetComponentInChildren<ParticleSystem>();
        soundManager = SoundManager.Instance;
    }

    void Start()
    {

    }

    private void OnEnable()
    {
        EnsureCamera();
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
        arCamera = null;
    }

    private void Update()
    {
        ClampPlantSize();
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


        else if (activePlant != null)
        {
            activePlant.transform.position =
                Vector3.Lerp(activePlant.transform.position, desiredWorldPos, Time.deltaTime * followLerp);
        }

    }

    //++++ Touch Handlers ++++
    private void OnFingerDown(Finger finger)
    {
        Vector2 screenPos = finger.currentTouch.screenPosition;



        if (HitActivePlant(finger.currentTouch.screenPosition))
        {
            resizePlantModelOnPinch();
        }
        else
        {
            DisableAllEmission(activePlant);
        }
        // If plant exists and touch is on the plant -> start HOLD candidate
        if (isPlantPlaced && activePlant != null && HitActivePlant(screenPos))
        {
            //resizePlantModelOnPinch();
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
        resizePlantModelOnPinch();
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
            OnPlantTapped(finger); // placeholder behavior
        }
        // Case B: we were NOT holding on the plant (finger down wasn't on plant) -> treat as TAP on empty plane
        else if (!holdCandidate && !isDragging)
        {

            if (TryARRaycastToAllowedPlane(finger.currentTouch.screenPosition, out Pose pose))
            {

                if (!isPlantPlaced)
                {
                    // Place new
                    PlacePlant(pose);
                    /*if (activePlant.GetComponent<Collider>() == null)
                        activePlant.AddComponent<BoxCollider>();*/
                }
                else
                {
                    // Move existing
                    //desiredWorldPos = pose.position + Vector3.up * yOffsetMeters;
                }
            }
        }

        // Reset states
        holdCandidate = false;
        isDragging = false;
        holdFinger = null;
    }

    private void RemovePlant()
    {
        ParticleSystem particleSystem = activePlant.GetComponentInChildren<ParticleSystem>();

        soundManager.PlayRefreshARSceneSound();


        particleSystem.Play();
        particleSystem.transform.parent = null;
        Destroy(particleSystem.gameObject, 3f);
        Destroy(activePlant);
        activePlant = null;
    }

    private void PlacePlant(Pose pose)
    {
        // Place new
        isPlantPlaced = true;
        Debug.Log("rotation: " + pose.rotation.eulerAngles);
        activePlant = Instantiate(selectedPlantModel, pose.position, pose.rotation);
        activePlant.transform.position += Vector3.up * yOffsetMeters;
        desiredWorldPos = pose.position + Vector3.up * yOffsetMeters;
        soundManager.PlayPlantPlacementSound();
        activePlant.GetComponentInChildren<ParticleSystem>().Play();
    }

    public void RefreshSession()
    {

        arSession.Reset();
        isPlantPlaced = false;
        if (activePlant != null)
        {
            RemovePlant();
            //SoundManager.Instance.PlayRefreshARSceneSound();
        }
        else
        {
            SoundManager.Instance.PlayRefreshARSceneSound();
        }
    }



    // helper method to raycast and find a valid plane
    private bool TryARRaycastToAllowedPlane(Vector2 screenPos, out Pose pose)
    {
        pose = default;

        if (!aRRaycastManager.Raycast(screenPos, hits, TrackableType.PlaneWithinPolygon))
            return false;

        foreach (var hit in hits)
        {
            var plane = arPlaneManager?.GetPlane(hit.trackableId);
            if (plane == null) continue;

            var align = plane.alignment; // UnityEngine.XR.ARSubsystems.PlaneAlignment

            bool placementAllowed =
                (allowHorizontalUp && align == PlaneAlignment.HorizontalUp) ||
                (allowHorizontalDown && align == PlaneAlignment.HorizontalDown) ||
                (allowVertical && align == PlaneAlignment.Vertical);

            if (placementAllowed)
            {
                pose = hit.pose;
                pose.position += Vector3.up * yOffsetMeters;
                return true;
            }
        }
        return false;
    }

    // Raycast from screen to check if we tapped the active plant
    private bool HitActivePlant(Vector2 screenPos)
    {
        EnsureCamera();
        if (!arCamera || activePlant == null) return false;

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
    private void OnPlantTapped(Finger finger)
    {
        EnsureCamera();
        // Unity’s destroyed objects compare equal to null
        if (!arCamera) return;

        Ray ray = arCamera.ScreenPointToRay(finger.currentTouch.screenPosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, ~0, QueryTriggerInteraction.Ignore))
        {

            if (hit.collider.gameObject.tag == "Stem")
            {
                selectedPlantData.selectedPart = PlantPart.Stem;
                SoundManager.Instance.PlaySelectBranchSound();
                DisableAllEmission(activePlant);
                EnableEmissionsOnHitObject("Stem");


            }
            else if (hit.collider.gameObject.tag == "Leaf")
            {

                selectedPlantData.selectedPart = PlantPart.Leaf;
                SoundManager.Instance.PlaySelectLeafSound();

                DisableAllEmission(activePlant);
                EnableEmissionsOnHitObject("Leaf");


            }
            else if (hit.collider.gameObject.tag == "Root")
            {
                selectedPlantData.selectedPart = PlantPart.Root;
                SoundManager.Instance.PlaySelectPlantPartSound();

                DisableAllEmission(activePlant);
                EnableEmissionsOnHitObject("Root");

            }
            else if (hit.collider.gameObject.tag == "Flower")
            {
                selectedPlantData.selectedPart = PlantPart.Flower;
                SoundManager.Instance.PlaySelectFlowerSound();

                DisableAllEmission(activePlant);
                EnableEmissionsOnHitObject("Flower");

            }
            else
            {
                selectedPlantData.selectedPart = PlantPart.None;
                DisableAllEmission(activePlant);
            }



            Debug.Log(hit.collider.gameObject.name);
        }

        Debug.Log("Plant tapped (short press) — TODO: handle selection/details UI here.");
    }

    private void EnableEmissionsOnHitObject(string hitPartTag)
    {
        //enable all leaf mesh emmisions if there are multiple leaf meshes


        //GameObject.FindGameObjectsWithTag(hitPartTag);
        Renderer[] renderers = activePlant.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
            if (rend.gameObject.tag == hitPartTag)
            {
                rend.material.SetColor("_EmissionColor", emissionColor * emissionIntensity);
                rend.material.EnableKeyword("_EMISSION");
            }
        }

    }

    private void DisableAllEmission(GameObject activePlant)
    {
        if (activePlant == null) return;

        Renderer[] renderers = activePlant.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
            rend.material.DisableKeyword("_EMISSION");
        }
    }


    private void EnsureCamera()
    {
        if (!arCamera)
        {
            var main = Camera.main;
            if (main) { arCamera = main; return; }

            var tagged = GameObject.FindWithTag("MainCamera");
            if (tagged) arCamera = tagged.GetComponent<Camera>();
        }
    }

    private void ClampPlantSize()
    {
        if (activePlant != null)
        {
            if (activePlant.transform.localScale.x < selectedPlantData.plantInfo.minSize &&
                activePlant.transform.localScale.y < selectedPlantData.plantInfo.minSize &&
                activePlant.transform.localScale.z < selectedPlantData.plantInfo.minSize)
            {
                float minSize = selectedPlantData.plantInfo.minSize;
                activePlant.transform.localScale = new Vector3(minSize, minSize, minSize);
            }
            else if (activePlant.transform.localScale.x > selectedPlantData.plantInfo.maxSize &&
                activePlant.transform.localScale.y > selectedPlantData.plantInfo.maxSize &&
                activePlant.transform.localScale.z > selectedPlantData.plantInfo.maxSize)
            {
                float maxSize = selectedPlantData.plantInfo.maxSize;
                activePlant.transform.localScale = new Vector3(maxSize, maxSize, maxSize);
            }
        }
    }

    private void resizePlantModelOnPinch()
    {
        if (EnhancedTouch.Touch.activeTouches.Count == 2)
        {
            EnhancedTouch.Touch touch0 = EnhancedTouch.Touch.activeTouches[0];
            EnhancedTouch.Touch touch1 = EnhancedTouch.Touch.activeTouches[1];



            Debug.Log("ScaleX: " + activePlant.transform.localScale.x + " ScaleY: " + activePlant.transform.localScale.y + " ScaleZ: " + activePlant.transform.localScale.z);
            //Debug.Log("can scale: " + canScale);

            if (HitActivePlant(touch0.screenPosition) || HitActivePlant(touch1.screenPosition))
            {
                // Ignore if Touch Canceled or Ended 
                if (touch0.phase == TouchPhase.Ended || touch0.phase == TouchPhase.Canceled ||
                   touch1.phase == TouchPhase.Ended || touch1.phase == TouchPhase.Canceled)
                {
                    return;
                }
                // if touch began, record initial distance and scale
                if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
                {
                    initialPinchDistance = Vector2.Distance(touch0.screenPosition, touch1.screenPosition);
                    initialScale = activePlant.transform.localScale;
                }
                else
                {
                    float currentPinchDistance = Vector2.Distance(touch0.screenPosition, touch1.screenPosition);
                    if (Mathf.Approximately(initialPinchDistance, 0))
                        return; // prevent division by zero
                    float scaleFactor = currentPinchDistance / initialPinchDistance;
                    activePlant.transform.localScale = initialScale * scaleFactor;
                }
            }
        }
    }
}
