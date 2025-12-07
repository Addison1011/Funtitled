using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;


public class PlantDescriptionUI : MonoBehaviour
{
    public VisualTreeAsset uxmlDocument; // Assign your UXML file in the Inspector
    private PlantInfo plantInfo;
    void OnEnable()
    {
        PlantInfo plantInfo = GameObject.FindGameObjectWithTag("SelectedPlantData").GetComponent<SelectedPlantData>().plantInfo;
        // Load the UXML and get the root VisualElement
        VisualElement root = GameObject.FindGameObjectWithTag("PlantDescription").GetComponent<UIDocument>().rootVisualElement;

        plantInfo = GameObject.FindGameObjectWithTag("SelectedPlantData").GetComponent<SelectedPlantData>().plantInfo;
        Label commonNameLabel = root.Q<Label>("CommonName");
        Label scientificNameLabel = root.Q<Label>("ScientificName");
        Label descriptionLabel = root.Q<Label>("PlantDescription");

        commonNameLabel.text = plantInfo.plantName;
        scientificNameLabel.text = plantInfo.scientificName;
        descriptionLabel.text = plantInfo.plantDesc;
        // Find the button by its name
        Button arButton = root.Q<Button>("ARButton");
        Button backButton = root.Q<Button>("BackButton");
        Button view3DButton = root.Q<Button>("View3DButton");

        // Register the event handler for the 'clicked' event
        if (arButton != null && backButton != null)
        {
            arButton.clicked += OnARButtonClicked;
            backButton.clicked += OnBackButtonClicked;
            view3DButton.clicked += OnView3DButtonClicked;
        }
    }

    // This method will be called when the button is pressed
    private void OnARButtonClicked()
    {
        Debug.Log("Button 'myButton' was clicked!");
        GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().sceneCounter += 1;
        SoundManager.Instance.PlayDefaultButtonSound();
        SceneManager.LoadScene("MainScene00");
    }

    private void OnBackButtonClicked()
    {
        Debug.Log("Button 'myButton' was clicked!");
        SoundManager.Instance.PlayDefaultButtonSound();
        Destroy(this.gameObject);
    }

    private void OnView3DButtonClicked()
    {
        Debug.Log("Button 'myButton' was clicked!");
        GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().sceneCounter += 1;
        SoundManager.Instance.PlayDefaultButtonSound();
        SceneManager.LoadScene("View3D");
    }




}