//script to load the plants in a category when said category is clicked
using UnityEditor;
using UnityEngine;

public class LoadCategoriesMenu : MonoBehaviour
{
    public PlantTypes plantTypes;
    
    void Start()
    {
        Debug.Log("Loaded category: " + plantTypes.typeName + " with ID: " + plantTypes.typeID);
    }


    public void OnClick()
    {
        GameObject categories = GameObject.Find("categMenu(Clone)");
        GameObject populator = GameObject.Find("MainMenuPopulator");

        SoundManager.Instance.PlayDefaultButtonSound();
        SelectedCategory data = GameObject.FindGameObjectWithTag("SelectedCategory").GetComponent<SelectedCategory>();
        data.plantTypes = this.plantTypes;
        Debug.Log("Button clicked for " + data.plantTypes.typeName);
        /*Instantiate(Resources.Load<GameObject>("MainMenuPopulator"));*/
        
        if(categories != null)
        {
            categories.SetActive(false);
            Debug.Log("categories set to inactive");
        }

        
        if(populator != null)
        {
            populator.SetActive(true);
            Debug.Log("Switched to main menu page");
        }
        else
        {
            Debug.Log("populator is null:/");
        }

        
    }


    

    void Update()
    {
        
    }
}