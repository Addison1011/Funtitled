//program to load the selected category
using UnityEngine;
public class SelectedCategory : MonoBehaviour{
    public static SelectedCategory Instance;

    public PlantTypes plantTypes;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if(Instance != this)
        {
            Destroy(gameObject);
        }
    }
}