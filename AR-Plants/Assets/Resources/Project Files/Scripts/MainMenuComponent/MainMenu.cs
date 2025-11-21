using UnityEngine;
using UnityEngine.UIElements;

public class UIEventHandler : MonoBehaviour

{

    [SerializeField]

    private UIDocument m_UIDocument;



    public void Start()

    {

        Instantiate(Resources.Load<GameObject>("MainMenu"));
        var root = GameObject.FindGameObjectWithTag("MainMenu").GetComponent<UIDocument>().rootVisualElement;
        VisualElement plantCard = root.Q<VisualElement>("plant-card");

        VisualElement content = root.Q<VisualElement>("content");

    }



    private void OnDestroy()

    {
    }


    private void OnButtonClicked()

    {



    }




}
