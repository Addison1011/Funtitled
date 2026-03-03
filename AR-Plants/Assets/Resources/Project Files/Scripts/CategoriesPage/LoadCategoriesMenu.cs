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
        SoundManager.Instance.PlayDefaultButtonSound();
        SelectedCategory data = GameObject.FindGameObjectWithTag("SelectedCategory").GetComponent<SelectedCategory>();
        data.plantTypes = this.plantTypes;
        Debug.Log("Button clicked for " + data.plantTypes.typeName);
        Instantiate(Resources.Load<GameObject>("MainMenu"));
    }

    void Update()
    {
        
    }
}