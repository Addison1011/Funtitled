using UnityEngine;
using UnityEngine.UIElements;

public class UIEventHandler : MonoBehaviour

{

    [SerializeField]

    private UIDocument m_UIDocument;



    private Label m_Label;

    private int m_ButtonClickCount = 0;

    private Toggle m_Toggle;

    private Button m_Button;



    public void Start()

    {

        var rootElement = m_UIDocument.rootVisualElement;



        m_Button = rootElement.Q<Button>("EventButton");



        m_Button.clickable.clicked += OnButtonClicked;





        m_Toggle = rootElement.Query<Toggle>("ColorToggle");




        m_Toggle.RegisterValueChangedCallback(OnToggleValueChanged);



        m_Label = rootElement.Q<Label>("IncrementLabel");

        m_Label.text = m_ButtonClickCount.ToString();

    }



    private void OnDestroy()

    {

        m_Button.clickable.clicked -= OnButtonClicked;

        m_Toggle.UnregisterValueChangedCallback(OnToggleValueChanged);

    }



    private void OnButtonClicked()

    {

        m_ButtonClickCount++;

        m_Label.text = m_ButtonClickCount.ToString();

    }



    private void OnToggleValueChanged(ChangeEvent<bool> evt)

    {

        Debug.Log("New toggle value is: " + evt.newValue);

    }

}
