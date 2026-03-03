//script to load the plants in a category when said category is clicked
using UnityEditor;
using UnityEngine;

public class LoadCategoriesMenu : MonoBehaviour
{
    public PlantTypes plantTypes;
    [SerializeField] private GameObject CategoriesRoot;
    [SerializeField] private GameObject MainMenuPopulatorie;
    void Start()
    {
        Debug.Log("Loaded category: " + plantTypes.typeName + " with ID: " + plantTypes.typeID);
    }


    public void OnClick()
    {
        SoundManager.Instance.PlayDefaultButtonSound();
        SelectedCategory data = GameObject.FindGameObjectWithTag("SelectedCategory").GetComponent<SelectedCategory>();
        data.plantTypes = this.plantTypes;
        Debug.Log("Button clicked for " + data.plantTypes.typeName);
        /*Instantiate(Resources.Load<GameObject>("MainMenuPopulator"));*/
        GameObject categories = GameObject.Find("categMenu(Clone)");
        if(categories != null)
        {
            categories.SetActive(false);
        }

        GameObject populator = GameObject.Find("MainMenuPopulator");
        if(populator != null)
        {
            populator.SetActive(true);
        }

        Debug.Log("Switched to main menu page");
    }


    

    void Update()
    {
        
    }
}