using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;


public class ARUIController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    public ARSession arSession;
    SelectedPlantData selectedPlantData;
    private Button backBtn, refreshBtn;
    private Button generalBtn, flowerBtn, stemBtn, leafBtn;

    private VisualElement dropdownArea;

    private VisualElement generalDrop, flowerDrop, stemDrop, leafDrop;

    // General fields (multi-field)
    private Label generalCommonName, generalScientificName, generalType, generalMaxSize, generalWatering, generalDescription;

    // Single text fields
    private Label flowerText, stemText, leafText;

    public enum Tab { None, General, Flower, Stem, Leaf }
    public Tab current = Tab.None;
    private ARInputController inputController;
    private VisualElement root;


    private const string SelectedClass = "tab-selected";

    private void Awake()
    {
        selectedPlantData = GameObject.FindGameObjectWithTag("SelectedPlantData").GetComponent<SelectedPlantData>();
        inputController = GameObject.FindGameObjectWithTag("ARInputController").GetComponent<ARInputController>();
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;

        backBtn = root.Q<Button>("BackButton");
        refreshBtn = root.Q<Button>("RefreshButton");

        generalBtn = root.Q<Button>("GeneralButton");
        flowerBtn = root.Q<Button>("FlowerButton");
        stemBtn = root.Q<Button>("StemButton");
        leafBtn = root.Q<Button>("LeafButton");

        dropdownArea = root.Q<VisualElement>("DropdownArea");

        generalDrop = root.Q<VisualElement>("GeneralDropdown");
        flowerDrop = root.Q<VisualElement>("FlowerDropdown");
        stemDrop = root.Q<VisualElement>("StemDropdown");
        leafDrop = root.Q<VisualElement>("LeafDropdown");

        // General multi-fields
        generalCommonName = root.Q<Label>("GeneralCommonName");
        generalScientificName = root.Q<Label>("GeneralScientificName");
        //generalType = root.Q<Label>("GeneralType");
        //generalMaxSize = root.Q<Label>("GeneralMaxSize");
        //generalWatering = root.Q<Label>("GeneralWatering");
        generalDescription = root.Q<Label>("GeneralDescription");

        // Single fields
        flowerText = root.Q<Label>("FlowerText");
        stemText = root.Q<Label>("StemText");
        leafText = root.Q<Label>("LeafText");

        // Apply theme to AR UI
        if (ColorThemeManager.Instance != null)
        {
            ColorThemeManager.Instance.ApplyThemeToUIDocument(root);
            // Subscribe to theme changes to reapply theme when it changes
            ColorThemeManager.Instance.SubscribeToThemeChange(OnThemeChanged);
        }

        // Start: nothing selected
        SetTab(Tab.None);

        generalBtn.clicked += () => Toggle(Tab.General);
        flowerBtn.clicked += () => Toggle(Tab.Flower);
        stemBtn.clicked += () => Toggle(Tab.Stem);
        leafBtn.clicked += () => Toggle(Tab.Leaf);

        // --- Back Button Highlight (works on Button because TrickleDown) ---
        backBtn.RegisterCallback<PointerDownEvent>(_ =>
        {
            backBtn.AddToClassList(SelectedClass);
        }, TrickleDown.TrickleDown);

        backBtn.RegisterCallback<PointerUpEvent>(_ =>
        {
            backBtn.RemoveFromClassList(SelectedClass);
            OnExitButtonPressed();
        }, TrickleDown.TrickleDown);

        backBtn.RegisterCallback<PointerCancelEvent>(_ =>
        {
            backBtn.RemoveFromClassList(SelectedClass);
        }, TrickleDown.TrickleDown);

        backBtn.RegisterCallback<PointerCaptureOutEvent>(_ =>
        {
            backBtn.RemoveFromClassList(SelectedClass);
        }, TrickleDown.TrickleDown);


        // --- Refresh Button Highlight (works on Button because TrickleDown) ---
        refreshBtn.RegisterCallback<PointerDownEvent>(_ =>
        {
            refreshBtn.AddToClassList(SelectedClass);
        }, TrickleDown.TrickleDown);

        refreshBtn.RegisterCallback<PointerUpEvent>(_ =>
        {
            refreshBtn.RemoveFromClassList(SelectedClass);
            inputController.RefreshSession();
        }, TrickleDown.TrickleDown);

        refreshBtn.RegisterCallback<PointerCancelEvent>(_ =>
        {
            refreshBtn.RemoveFromClassList(SelectedClass);
        }, TrickleDown.TrickleDown);

        refreshBtn.RegisterCallback<PointerCaptureOutEvent>(_ =>
        {
            refreshBtn.RemoveFromClassList(SelectedClass);
        }, TrickleDown.TrickleDown);

        SetPartInfo();
        SetGeneralInfo();

    }

    public void Toggle(Tab t)
    {
        SetTab(current == t ? Tab.None : t);

        if (inputController.activePlant != null)
        {

            if (current == Tab.Leaf && selectedPlantData.selectedPart != PlantPart.Leaf)
            {
                inputController.DisableAllSelectionEffects(inputController.activePlant);
                selectedPlantData.selectedPart = PlantPart.Leaf;
                GameObject child = null;
                foreach (Transform transform in inputController.activePlant.transform)
                {
                    if (transform.CompareTag("Leaf"))
                    {
                        child = transform.gameObject;
                        if (child != null)
                        {
                            child.GetComponentInChildren<ParticleSystem>().Play();
                            SoundManager.Instance.PlaySelectLeafSound();
                        }
                        break;
                    }
                    //inputController.activePlant;
                }
                if (child == null)
                {
                    SoundManager.Instance.PlayDefaultButtonSound();
                }

            }
            // Stem selection plays stem effects and sets selectedPart to Stem, which disables individual part selection until another tab is selected or the plant is deselected
            else if (current == Tab.Stem && selectedPlantData.selectedPart != PlantPart.Stem)
            {
                inputController.DisableAllSelectionEffects(inputController.activePlant);
                selectedPlantData.selectedPart = PlantPart.Stem;
                GameObject child = null;
                foreach (Transform transform in inputController.activePlant.transform)
                {
                    if (transform.CompareTag("Stem"))
                    {
                        child = transform.gameObject;
                        if (child != null)
                        {
                            child.GetComponentInChildren<ParticleSystem>().Play();
                            SoundManager.Instance.PlaySelectBranchSound();
                        }
                        break;
                    }

                    //inputController.activePlant;
                }
                if (child == null)
                {
                    SoundManager.Instance.PlayDefaultButtonSound();
                }

            }
            //Flower selection plays flower effects and sets selectedPart to Flower, which disables individual part selection until another tab is selected or the plant is deselected
            else if (current == Tab.Flower && selectedPlantData.selectedPart != PlantPart.Flower)
            {
                inputController.DisableAllSelectionEffects(inputController.activePlant);
                selectedPlantData.selectedPart = PlantPart.Flower;
                GameObject child = null;
                foreach (Transform transform in inputController.activePlant.transform)
                {
                    if (transform.CompareTag("Flower"))
                    {
                        child = transform.gameObject;
                        if (child != null)
                        {
                            child.GetComponentInChildren<ParticleSystem>().Play();
                            SoundManager.Instance.PlaySelectFlowerSound();
                        }
                        break;
                    }

                    //inputController.activePlant;
                }
                if (child == null)
                {
                    SoundManager.Instance.PlayDefaultButtonSound();
                }



            }
            // General selection plays all effects and sets selectedPart to General, which disables individual part selection until another tab is selected or the plant is deselected
            else if (current == Tab.General && selectedPlantData.selectedPart != PlantPart.General)
            {
                inputController.DisableAllSelectionEffects(inputController.activePlant);
                selectedPlantData.selectedPart = PlantPart.General;
                GameObject child = null;
                foreach (Transform transform in inputController.activePlant.transform)
                {
                    if (transform.CompareTag("Flower"))
                    {
                        child = transform.gameObject;
                        if (child != null)
                        {
                            child.GetComponentInChildren<ParticleSystem>().Play();
                        }
                    }
                    if (transform.CompareTag("Stem"))
                    {
                        child = transform.gameObject;
                        if (child != null)
                        {
                            child.GetComponentInChildren<ParticleSystem>().Play();
                        }
                    }
                    if (transform.CompareTag("Leaf"))
                    {
                        child = transform.gameObject;
                        if (child != null)
                        {
                            child.GetComponentInChildren<ParticleSystem>().Play();
                        }
                    }
                    //inputController.activePlant;
                }
                SoundManager.Instance.PlayDefaultButtonSound();


            }
            else
            {
                inputController.DisableAllSelectionEffects(inputController.activePlant);
                selectedPlantData.selectedPart = PlantPart.None;
            }
        }
        else
        {
            SoundManager.Instance.PlayDefaultButtonSound();
        }
        //SoundManager.Instance.PlayDefaultButtonSound();
    }


    public bool IsScreenPointOverAnyUIButton(Vector2 screenPos)
    {
        if (uiDocument == null) return false;

        var root = uiDocument.rootVisualElement;
        var panel = root?.panel;
        if (panel == null) return false;

        // Convert screen -> panel coords
        Vector2 panelPos = RuntimePanelUtils.ScreenToPanel(panel, screenPos);

        // Some devices/panels end up inverted on Y depending on panel settings,
        // so we also test a flipped-Y position.
        Vector2 panelPosFlipped = RuntimePanelUtils.ScreenToPanel(panel, new Vector2(screenPos.x, Screen.height - screenPos.y));

        return IsOverButton(backBtn, panelPos) || IsOverButton(refreshBtn, panelPos) ||
               IsOverButton(generalBtn, panelPos) || IsOverButton(flowerBtn, panelPos) ||
               IsOverButton(stemBtn, panelPos) || IsOverButton(leafBtn, panelPos) ||
               IsOverButton(backBtn, panelPosFlipped) || IsOverButton(refreshBtn, panelPosFlipped) ||
               IsOverButton(generalBtn, panelPosFlipped) || IsOverButton(flowerBtn, panelPosFlipped) ||
               IsOverButton(stemBtn, panelPosFlipped) || IsOverButton(leafBtn, panelPosFlipped);
    }

    private bool IsOverButton(Button btn, Vector2 panelPos)
    {
        return btn != null && btn.worldBound.Contains(panelPos);
    }



    public void SetTab(Tab t)
    {
        current = t;

        // Clear selection borders
        generalBtn.RemoveFromClassList(SelectedClass);
        flowerBtn.RemoveFromClassList(SelectedClass);
        stemBtn.RemoveFromClassList(SelectedClass);
        leafBtn.RemoveFromClassList(SelectedClass);

        // Hide all dropdown cards
        generalDrop.style.display = DisplayStyle.None;
        flowerDrop.style.display = DisplayStyle.None;
        stemDrop.style.display = DisplayStyle.None;
        leafDrop.style.display = DisplayStyle.None;

        if (t == Tab.None)
        {
            dropdownArea.style.display = DisplayStyle.None;
            return;
        }

        dropdownArea.style.display = DisplayStyle.Flex;

        switch (t)
        {
            case Tab.General:
                generalBtn.AddToClassList(SelectedClass);
                generalDrop.style.display = DisplayStyle.Flex;
                break;

            case Tab.Flower:
                //selectedPlantData.selectedPart = PlantPart.Flower;
                flowerBtn.AddToClassList(SelectedClass);
                flowerDrop.style.display = DisplayStyle.Flex;
                break;

            case Tab.Stem:
                //selectedPlantData.selectedPart = PlantPart.Stem;
                stemBtn.AddToClassList(SelectedClass);
                stemDrop.style.display = DisplayStyle.Flex;
                break;

            case Tab.Leaf:
                //selectedPlantData.selectedPart = PlantPart.Leaf;
                leafBtn.AddToClassList(SelectedClass);
                leafDrop.style.display = DisplayStyle.Flex;
                break;
        }
    }

    public void SetGeneralInfo()
    {
        generalCommonName.text = selectedPlantData.plantInfo.plantName;
        generalScientificName.text = selectedPlantData.plantInfo.scientificName;
        //generalType.text = selectedPlantData.plantInfo.;
        //generalMaxSize.text = maxSize;
        //generalWatering.text = watering;
        generalDescription.text = selectedPlantData.plantInfo.plantDesc;
    }

    public void SetPartInfo()
    {
        flowerText.text = selectedPlantData.plantInfo.flower;
        stemText.text = selectedPlantData.plantInfo.stem;
        leafText.text = selectedPlantData.plantInfo.leaf;
    }

    public void EndARSession()
    {
        if (arSession != null)
        {
            arSession.enabled = false;
            Debug.Log("AR Session ended.");
        }
    }

    public void OnExitButtonPressed()
    {
        EndARSession();

        SoundManager.Instance.PlayDefaultButtonSound();

        ReturnToMainMenu();
    }

    private void ReturnToMainMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    private void OnThemeChanged(ColorThemeManager.ColorTheme newTheme)
    {
        // Reapply theme when it changes
        if (root != null && ColorThemeManager.Instance != null)
        {
            ColorThemeManager.Instance.ApplyThemeToUIDocument(root);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from theme changes to prevent memory leaks
        if (ColorThemeManager.Instance != null)
        {
            ColorThemeManager.Instance.UnsubscribeFromThemeChange(OnThemeChanged);
        }
    }

}