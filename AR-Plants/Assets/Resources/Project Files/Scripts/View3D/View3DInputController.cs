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


public class View3DInputController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera arCamera;                   // AR Camera
    [SerializeField] private GameObject selectedPlantModel; // Prefab to place
    [SerializeField] private SoundManager soundManager;
    private SelectedPlantData selectedPlantData;

    [SerializeField] private GameObject selectedPlantDataHandle;
    private ParticleSystem placementEffect;



    [Header("Tuning")]


    private float initialPinchDistance;
    private Vector3 initialScale;
    public Color emissionColor = Color.white;
    public float emissionIntensity = 1f;

    [Header("Hold / Tap Settings")]
    [Tooltip("Hold duration (seconds) required on the plant to start dragging.")]
    [SerializeField] private float holdToDragSeconds = 0.05f;
    [Tooltip("Max finger movement (pixels) still considered a tap/hold (pre-drag).")]
    [SerializeField] private float tapSlopPixels = 12f;



    [Header("Tap Callback")]
    public UnityEvent onPlantTapped; // hook UI, selection, etc.

    [Header("Rotate Settings")]
    [SerializeField] private float rotationSpeed = 0.2f; // degrees per pixel
    private Vector2 lastDragScreenPos;


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

    private float spawnDistanceFromCamera = 4.0f;


    private void Awake()
    {

        SoundManager.Instance.StopMusic();
        if (SoundManager.Instance.ambientSoundEnabled)
        {
            SoundManager.Instance.PlayAmbientSounds();
        }

        arCamera = Camera.main;

        if (GameObject.FindWithTag("SelectedPlantData") != null)
        {
            selectedPlantDataHandle = GameObject.FindWithTag("SelectedPlantData");
        }


        //Gets the SelectedPlantData script from the SelectedPlantData GameObject
        selectedPlantData = selectedPlantDataHandle.GetComponent<SelectedPlantData>();

        selectedPlantModel = Resources.Load<GameObject>(selectedPlantData.plantInfo.scientificName); //default plant
        activePlant = Instantiate(selectedPlantModel);
        GameObject plantBase = Instantiate(Resources.Load<GameObject>("Base"));
        plantBase.transform.position = new Vector3(arCamera.transform.position.x, 0, arCamera.transform.position.z + spawnDistanceFromCamera);
        isPlantPlaced = true;
        //activePlant.transform.position = arCamera.transform.position + arCamera.transform.forward * spawnDistanceFromCamera;

        placementEffect = selectedPlantModel.GetComponentInChildren<ParticleSystem>();


        soundManager = SoundManager.Instance;
    }

    void Start()
    {

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
                lastDragScreenPos = holdFinger.currentTouch.screenPosition;
                isDragging = true;
            }
        }


        else if (activePlant != null)
        {
            activePlant.transform.position = new Vector3(arCamera.transform.position.x, 0, arCamera.transform.position.z + spawnDistanceFromCamera);
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
            DisableAllSelectionEffects(activePlant);
            //DisableAllEmission(activePlant);
        }
        // If plant exists and touch is on the plant -> start HOLD candidate
        if (isPlantPlaced && activePlant != null)
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

        if (EnhancedTouch.Touch.activeTouches.Count == 1)
        {
            DragFingerToRotate(finger);
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


        // Reset states
        holdCandidate = false;
        isDragging = false;
        holdFinger = null;
    }

    private void RemovePlant()
    {
        ParticleSystem particleSystem = GameObject.FindGameObjectWithTag("PlacementEffect").GetComponent<ParticleSystem>();

        soundManager.PlayRefreshARSceneSound();


        particleSystem.Play();
        particleSystem.transform.parent = null;
        Destroy(particleSystem.gameObject, 3f);
        Destroy(activePlant);
        activePlant = null;
    }



    public void RefreshSession()
    {
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

    // Raycast from screen to check if we tapped the active plant
    private bool HitActivePlant(Vector2 screenPos)
    {
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
        // Unity’s destroyed objects compare equal to null
        if (!arCamera) return;

        Ray ray = arCamera.ScreenPointToRay(finger.currentTouch.screenPosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, ~0, QueryTriggerInteraction.Ignore))
        {

            if (hit.collider.gameObject.tag == "Stem")
            {
                selectedPlantData.selectedPart = PlantPart.Stem;
                SoundManager.Instance.PlaySelectBranchSound();
                DisableAllSelectionEffects(activePlant);
                hit.collider.gameObject.GetComponentInChildren<ParticleSystem>().Play();
            }
            else if (hit.collider.gameObject.tag == "Leaf")
            {

                selectedPlantData.selectedPart = PlantPart.Leaf;
                SoundManager.Instance.PlaySelectLeafSound();

                DisableAllSelectionEffects(activePlant);
                hit.collider.gameObject.GetComponentInChildren<ParticleSystem>().Play();

            }
            else if (hit.collider.gameObject.tag == "Root")
            {
                selectedPlantData.selectedPart = PlantPart.Root;
                SoundManager.Instance.PlaySelectPlantPartSound();

                DisableAllSelectionEffects(activePlant);
                hit.collider.gameObject.GetComponentInChildren<ParticleSystem>().Play();

            }
            else if (hit.collider.gameObject.tag == "Flower")
            {
                selectedPlantData.selectedPart = PlantPart.Flower;
                SoundManager.Instance.PlaySelectFlowerSound();
                DisableAllSelectionEffects(activePlant);
                hit.collider.gameObject.GetComponentInChildren<ParticleSystem>().Play();


            }
            else
            {
                selectedPlantData.selectedPart = PlantPart.None;
                DisableAllSelectionEffects(activePlant);
            }



            Debug.Log(hit.collider.gameObject.name);
        }

        Debug.Log("Plant tapped (short press) — TODO: handle selection/details UI here.");
    }

    private void DisableAllSelectionEffects(GameObject hitObject)
    {
        if (activePlant == null) return;

        GameObject[] effects = GameObject.FindGameObjectsWithTag("SelectionEffect");
        foreach (GameObject eff in effects)
        {
            eff.GetComponent<ParticleSystem>().Stop();
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

    private void DragFingerToRotate(Finger finger)
    {
        if (activePlant == null) return;

        Vector2 currentPos = finger.currentTouch.screenPosition;

        // Horizontal delta in pixels
        float deltaX = currentPos.x - lastDragScreenPos.x;

        // Rotate around Y (world up)
        activePlant.transform.Rotate(Vector3.up, deltaX * rotationSpeed, Space.World);

        // Update last drag position
        lastDragScreenPos = currentPos;
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
