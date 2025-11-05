using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;


public class PlantDescriptionUI : MonoBehaviour
{
    public VisualTreeAsset uxmlDocument; // Assign your UXML file in the Inspector
    private PlantInfo plantInfo;
    void OnEnable()
    {
        // Load the UXML and get the root VisualElement
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
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

        // Register the event handler for the 'clicked' event
        if (arButton != null && backButton != null)
        {
            arButton.clicked += OnARButtonClicked;
            backButton.clicked += OnBackButtonClicked;
        }
    }

    /*void OnDisable()
    {
        // Unregister the event handler to prevent memory leaks
        Button arButton = GetComponent<UIDocument>().rootVisualElement.Q<Button>("ARButton");
        if (arButton != null)
        {
            arButton.clicked -= OnARButtonClicked;
        }
    }*/

    // This method will be called when the button is pressed
    private void OnARButtonClicked()
    {
        Debug.Log("Button 'myButton' was clicked!");
        GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().sceneCounter += 1;
        SceneManager.LoadScene("MainScene00");
    }

    private void OnBackButtonClicked()
    {
        Debug.Log("Button 'myButton' was clicked!");

        Destroy(this.gameObject);
    }
}