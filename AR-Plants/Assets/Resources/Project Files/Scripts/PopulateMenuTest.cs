using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class PopulateMenuTest : MonoBehaviour
{

    [SerializeField] private VisualTreeAsset buttonTemplate;
    [SerializeField] private string buttonHandelName;
    [SerializeField] private string contentHandelName;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instantiate(Resources.Load<GameObject>("MainMenuNew"));
        VisualElement root = GameObject.FindGameObjectWithTag("MainMenuNew").GetComponent<UIDocument>().rootVisualElement;
        VisualElement content = root.Q<VisualElement>(contentHandelName);


        VisualElement newButtonInstance = buttonTemplate.CloneTree();

        Button button = newButtonInstance.Q<Button>(buttonHandelName);




        content.Add(button);

    }

    // Update is called once per frame
    void Update()
    {

    }
}
