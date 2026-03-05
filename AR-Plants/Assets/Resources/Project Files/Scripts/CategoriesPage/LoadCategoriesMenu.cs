//script to load the plants in a category when said category is clicked
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class LoadCategoriesMenu : MonoBehaviour
{
    public PlantTypes plantTypes;
    public GameObject MainMenuPopulatorPrefab;

    void Start()
    {
        Debug.Log("Loaded category: " + plantTypes.typeName + " with ID: " + plantTypes.typeID);
    }


    public void OnClick()
    {
        //--------------------------------
        //EDITED THIS FUNCTION TO CLEAR THE CURRENT PLANT BUTTONS AND REPOPULATE WITH THE PLANTS OF THE SELECTED CATEGORY
        //--------------------------------
        GameObject categories = GameObject.Find("categMenu(Clone)");
        GameObject populator = GameObject.FindGameObjectWithTag("MainMenuPopulator");
        GameObject.FindGameObjectWithTag("MainMenu").GetComponent<UIDocument>().panelSettings.sortingOrder = 1;
        categories.GetComponent<UIDocument>().panelSettings.sortingOrder = 0;

        SoundManager.Instance.PlayDefaultButtonSound();
        SelectedCategory data = GameObject.FindGameObjectWithTag("SelectedCategory").GetComponent<SelectedCategory>();
        data.plantTypes = this.plantTypes;
        Debug.Log("Button clicked for " + data.plantTypes.typeName);

        //Added function to clear current plant buttons and repopulate with plants of the selected category
        populator.GetComponent<PopulateMenuTest>().ClearAndPopulateMenuWithCategoryPlants(this.plantTypes);

    }




    void Update()
    {

    }
}