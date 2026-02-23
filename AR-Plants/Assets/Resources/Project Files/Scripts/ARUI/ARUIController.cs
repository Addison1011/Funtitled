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

    private const string SelectedClass = "tab-selected";

    private void Awake()
    {
        selectedPlantData = GameObject.FindGameObjectWithTag("SelectedPlantData").GetComponent<SelectedPlantData>();
        ARInputController inputController = GameObject.FindGameObjectWithTag("ARInputController").GetComponent<ARInputController>();
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

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
        generalType = root.Q<Label>("GeneralType");
        generalMaxSize = root.Q<Label>("GeneralMaxSize");
        generalWatering = root.Q<Label>("GeneralWatering");
        generalDescription = root.Q<Label>("GeneralDescription");

        // Single fields
        flowerText = root.Q<Label>("FlowerText");
        stemText = root.Q<Label>("StemText");
        leafText = root.Q<Label>("LeafText");

        // Start: nothing selected
        SetTab(Tab.None);

        generalBtn.clicked += () => Toggle(Tab.General);
        flowerBtn.clicked += () => Toggle(Tab.Flower);
        stemBtn.clicked += () => Toggle(Tab.Stem);
        leafBtn.clicked += () => Toggle(Tab.Leaf);

        backBtn.clicked += () => OnExitButtonPressed();
        refreshBtn.clicked += () => inputController.RefreshSession();

        // Example default text (replace with DB values later)
        SetPartInfo(PlantPart.Flower, PlantPart.Stem, PlantPart.Leaf);
    }

    public void Toggle(Tab t)
    {
        SetTab(current == t ? Tab.None : t);
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

    public void SetGeneralInfo(string common, string scientific, string type, string maxSize, string watering, string description)
    {
        generalCommonName.text = common;
        generalScientificName.text = scientific;
        generalType.text = type;
        generalMaxSize.text = maxSize;
        generalWatering.text = watering;
        generalDescription.text = description;
    }

    public void SetPartInfo(PlantPart flower, PlantPart stem, PlantPart leaf)
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



}