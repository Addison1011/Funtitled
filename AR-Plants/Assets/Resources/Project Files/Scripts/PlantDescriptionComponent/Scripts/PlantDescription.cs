using UnityEngine;
using UnityEngine.UIElements;


public class PlantDescription : VisualElement
{

    public new class UxmlFactory : UxmlFactory<PlantDescription, UxmlTraits> { }

    // Define UXML attributes (props)
    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        UxmlStringAttributeDescription m_CommonName = new UxmlStringAttributeDescription { name = "common-name", defaultValue = "Plant Name" };
        UxmlStringAttributeDescription m_ScientificName = new UxmlStringAttributeDescription { name = "scientific-name", defaultValue = "Scientific name" };
        UxmlStringAttributeDescription m_Description = new UxmlStringAttributeDescription { name = "description", defaultValue = "Plant description..." };
        UxmlStringAttributeDescription m_Location = new UxmlStringAttributeDescription { name = "location", defaultValue = "Location" };
        UxmlStringAttributeDescription m_HeaderImagePath = new UxmlStringAttributeDescription { name = "header-image" };
        UxmlStringAttributeDescription m_MapImagePath = new UxmlStringAttributeDescription { name = "map-image" };

        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);
            var card = ve as PlantDescription;

            card.CommonName = m_CommonName.GetValueFromBag(bag, cc);
            card.ScientificName = m_ScientificName.GetValueFromBag(bag, cc);
            card.Description = m_Description.GetValueFromBag(bag, cc);
            card.Location = m_Location.GetValueFromBag(bag, cc);
            card.HeaderImagePath = m_HeaderImagePath.GetValueFromBag(bag, cc);
            card.MapImagePath = m_MapImagePath.GetValueFromBag(bag, cc);
        }
    }

    // Properties (like React props)
    private string m_CommonName;
    private string m_ScientificName;
    private string m_Description;
    private string m_Location;
    private string m_HeaderImagePath;
    private string m_MapImagePath;

    // UI Element references
    private Label commonNameLabel;
    private Label scientificNameLabel;
    private Label descriptionLabel;
    private Label locationLabel;
    private VisualElement headerImageElement;
    private VisualElement mapImageElement;
    private Button backButton;
    private Button arButton;


    public string CommonName
    {
        get => m_CommonName;
        set
        {
            m_CommonName = value;
            if (commonNameLabel != null)
                commonNameLabel.text = value;
        }
    }

    public string ScientificName
    {
        get => m_ScientificName;
        set
        {
            m_ScientificName = value;
            if (scientificNameLabel != null)
                scientificNameLabel.text = value;
        }
    }

    public string Description
    {
        get => m_Description;
        set
        {
            m_Description = value;
            if (descriptionLabel != null)
                descriptionLabel.text = value;
        }
    }

    public string Location
    {
        get => m_Location;
        set
        {
            m_Location = value;
            if (locationLabel != null)
                locationLabel.text = value;
        }
    }

    public string HeaderImagePath
    {
        get => m_HeaderImagePath;
        set
        {
            m_HeaderImagePath = value;
            UpdateHeaderImage(value);
        }
    }

    public string MapImagePath
    {
        get => m_MapImagePath;
        set
        {
            m_MapImagePath = value;
            UpdateMapImage(value);
        }
    }

    public PlantDescription()
    {
        // Load the UXML template
        var visualTree = Resources.Load<VisualTreeAsset>("PlantDescription");
        visualTree.CloneTree(this);

        // Load the stylesheet
        var styleSheet = Resources.Load<StyleSheet>("PlantDescriptionStyles");
        if (styleSheet != null)
            styleSheets.Add(styleSheet);

        // Get references to child elements
        commonNameLabel = this.Q<Label>("CommonName");
        scientificNameLabel = this.Q<Label>("ScientificName");
        descriptionLabel = this.Q<Label>("Description");
        locationLabel = this.Q<Label>("LocationText");
        headerImageElement = this.Q<VisualElement>("HeaderImage");
        mapImageElement = this.Q<VisualElement>("MapImage");
        backButton = this.Q<Button>("BackButton");
        arButton = this.Q<Button>("ARButton");


        if (backButton != null)
            backButton.clicked += OnBackClicked;

        if (arButton != null)
            arButton.clicked += OnARClicked;


        Render();
    }


    private void Render()
    {
        if (commonNameLabel != null)
            commonNameLabel.text = m_CommonName;

        if (scientificNameLabel != null)
            scientificNameLabel.text = m_ScientificName;

        if (descriptionLabel != null)
            descriptionLabel.text = m_Description;

        if (locationLabel != null)
            locationLabel.text = m_Location;

        UpdateHeaderImage(m_HeaderImagePath);
        UpdateMapImage(m_MapImagePath);
    }

    private void UpdateHeaderImage(string path)
    {
        if (headerImageElement == null || string.IsNullOrEmpty(path))
            return;

        var texture = Resources.Load<Texture2D>(path);
        if (texture != null)
            headerImageElement.style.backgroundImage = new StyleBackground(texture);
    }

    private void UpdateMapImage(string path)
    {
        if (mapImageElement == null || string.IsNullOrEmpty(path))
            return;

        var texture = Resources.Load<Texture2D>(path);
        if (texture != null)
            mapImageElement.style.backgroundImage = new StyleBackground(texture);
    }

    // Event handlers (like React event handlers)
    private void OnBackClicked()
    {
        using (var evt = NavigationBackEvent.GetPooled())
        {
            evt.target = this;
            SendEvent(evt);
        }
    }

    private void OnARClicked()
    {
        using (var evt = new ARViewRequestedEvent())
        {
            evt.target = this;
            SendEvent(evt);
        }
    }

    public class ARViewRequestedEvent : EventBase<ARViewRequestedEvent>
    {
        public ARViewRequestedEvent() { }
    }

    public class NavigationBackEvent : EventBase<NavigationBackEvent>
    {
        public NavigationBackEvent() { }
    }
}
