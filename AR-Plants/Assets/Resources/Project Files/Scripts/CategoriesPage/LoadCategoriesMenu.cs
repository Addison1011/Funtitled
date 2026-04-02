//script to load the plants in a category when said category is clicked
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class LoadCategoriesMenu : MonoBehaviour
{
    public PlantTypes plantTypes;
    public GameObject MainMenuPopulatorPrefab;
    public GameObject CategoriesMenuObject;

    void Start()
    {
        Debug.Log("Loaded category: " + plantTypes.typeName + " with ID: " + plantTypes.typeID);
    }


    public void OnClick()
    {
        //--------------------------------
        // Clear the current plant buttons and repopulate with the plants of the selected category
        //--------------------------------
        SoundManager.Instance.PlayDefaultButtonSound();

        SelectedCategory data = GameObject.FindGameObjectWithTag("SelectedCategory")?.GetComponent<SelectedCategory>();
        if (data != null)
        {
            data.plantTypes = this.plantTypes;
            Debug.Log("Category clicked: " + data.plantTypes.typeName);
        }
        else
        {
            Debug.LogWarning("SelectedCategory object not found or missing component.");
        }

        var populator = FindAnyObjectByType<PopulateMenuTest>();
        if (populator == null)
        {
            if (MainMenuPopulatorPrefab != null)
            {
                var populatorObj = Instantiate(MainMenuPopulatorPrefab);
                populator = populatorObj.GetComponent<PopulateMenuTest>();
                if (populator == null)
                {
                    Debug.LogError("MainMenuPopulatorPrefab did not contain PopulateMenuTest component.");
                    return;
                }
            }
            else
            {
                Debug.LogError("Populate menus: no existing PopulateMenuTest and MainMenuPopulatorPrefab is null.");
                return;
            }
        }

        if (populator.mainMenu != null)
        {
            populator.mainMenu.SetActive(true);
            populator.mainMenu.GetComponent<UIDocument>().panelSettings.sortingOrder = 1;
        }
        else if (populator.MainMenuPrefab != null)
        {
            var newMainMenu = Instantiate(populator.MainMenuPrefab);
            populator.SetMainMenuAndRefresh(newMainMenu);
            if (populator.mainMenu != null)
            {
                populator.mainMenu.GetComponent<UIDocument>().panelSettings.sortingOrder = 1;
                Debug.Log("PopulateMenuTest: mainMenu instantiated from MainMenuPrefab.");
            }
            else
            {
                Debug.LogError("PopulateMenuTest: instantiated mainMenu is null after SetMainMenuAndRefresh.");
            }
        }
        else
        {
            Debug.LogWarning("PopulateMenuTest mainMenu is null and no prefab available.");
        }

        if (populator.mainMenu == null)
        {
            Debug.LogError("LoadCategoriesMenu.OnClick: cannot continue because mainMenu is null.");
            return;
        }

        if (CategoriesMenuObject != null)
        {
            CategoriesMenuObject.SetActive(false);
            var categoriesPanel = CategoriesMenuObject.GetComponent<UIDocument>();
            if (categoriesPanel != null)
            {
                categoriesPanel.panelSettings.sortingOrder = 0;
            }
        }

        // ensure category menu reference is preserved
        categories.CurrentCategoriesMenu = CategoriesMenuObject;

        populator.ClearAndPopulateMenuWithCategoryPlants(this.plantTypes);
    }




    void Update()
    {

    }
}