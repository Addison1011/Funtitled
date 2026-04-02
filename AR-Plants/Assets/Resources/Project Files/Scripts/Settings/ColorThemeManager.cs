using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;

public class ColorThemeManager : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void EnsureInstanceExists()
    {
        if (Instance == null)
        {
            var go = new GameObject("ColorThemeManager");
            go.AddComponent<ColorThemeManager>();
            UnityEngine.Object.DontDestroyOnLoad(go);
            Debug.Log("ColorThemeManager: auto-created Instance before scene load.");
        }
        else
        {
            Debug.Log("ColorThemeManager: instance already exists at startup.");
        }
    }
    public static ColorThemeManager Instance { get; private set; }

    // Callback for when theme changes
    public event Action<ColorTheme> OnThemeChanged;

    [System.Serializable]
    public class ButtonTheme
    {
        public Color normalColor = new Color(0.2f, 0.7f, 1f, 1f);
        public Color hoverColor = new Color(0.1f, 0.6f, 0.9f, 1f);
        public Color pressedColor = new Color(0f, 0.5f, 0.8f, 1f);
    }

    [System.Serializable]
    public class UITheme
    {
        public Color textColor = new Color(0, 0, 0, 1f);          // Black text
        public Color backgroundColor = new Color(0.94f, 0.94f, 0.96f, 1f);  // Light gray background
        public Color headerBackgroundColor = new Color(0.635f, 0.949f, 0.741f, 1f);  // Light green header
    }

    [System.Serializable]
    public class ColorTheme
    {
        public string themeName = "Default";
        public ButtonTheme buttonTheme = new ButtonTheme();
        public UITheme uiTheme = new UITheme();
    }

    private ColorTheme normalTheme;
    private ColorTheme highContrastTheme;
    private ColorTheme currentTheme;
    private bool isHighContrast = false;

    private List<ButtonUI> registeredButtons = new List<ButtonUI>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            UnityEngine.Object.Destroy(gameObject);
            return;
        }

        Instance = this;
        InitializeThemes();
    }

    private void InitializeThemes()
    {
        // Normal theme
        normalTheme = new ColorTheme
        {
            themeName = "Normal",
            buttonTheme = new ButtonTheme
            {
                normalColor = new Color(0.173f, 0.396f, 0.149f, 1f),    //#2C6526 - Medium green
                hoverColor = new Color(0.937f, 0.992f, 0.929f, 1f),     //#EFFDED - Light green
                pressedColor = new Color(0f, 0.404f, 0.106f, 1f)        //#00671B - Dark green
            },
            uiTheme = new UITheme
            {
                textColor = new Color(0f, 0f, 0f, 1f),                    // Black text
                backgroundColor = new Color(0.937f, 0.992f, 0.929f, 1f),  //#EFFDED - Light green background
                headerBackgroundColor = new Color(0f, 0.404f, 0.106f, 1f) //#00671B - Dark green header
            }
        };

        // High Contrast theme
        highContrastTheme = new ColorTheme
        {
            themeName = "High Contrast",
            buttonTheme = new ButtonTheme
            {
                normalColor = new Color(1f, 0.5f, 0f, 1f),       // Bright orange
                hoverColor = new Color(1f, 0.7f, 0f, 1f),        // Lighter orange
                pressedColor = new Color(0.8f, 0.3f, 0f, 1f)     // Darker orange
            },
            uiTheme = new UITheme
            {
                textColor = new Color(1f, 1f, 1f, 1f),                    // White text
                backgroundColor = new Color(0f, 0f, 0f, 1f),              // Pure black background
                headerBackgroundColor = new Color(1f, 0.5f, 0f, 1f)       // Bright orange header
            }
        };

        currentTheme = normalTheme;
        isHighContrast = false;
        Debug.Log($"ColorThemeManager initialized with theme: {currentTheme.themeName}");
    }

    /// Get the current active theme
    public ColorTheme GetCurrentTheme()
    {
        return currentTheme;
    }

    /// Toggle between Normal and High Contrast themes
    public void ToggleTheme()
    {
        isHighContrast = !isHighContrast;
        currentTheme = isHighContrast ? highContrastTheme : normalTheme;
        ApplyThemeToAllButtons();
        OnThemeChanged?.Invoke(currentTheme);
        Debug.Log($"Theme changed to: {currentTheme.themeName}");
    }

    /// Set theme to High Contrast
    public void SetHighContrast(bool enabled)
    {
        Debug.Log($"ColorThemeManager.SetHighContrast called with: {enabled}");
        if (enabled != isHighContrast)
        {
            ToggleTheme();
            Debug.Log($"ColorThemeManager state changed. HighContrast now: {isHighContrast}");
        }
    }

    /// Register a button to receive theme updates
    public void RegisterButton(ButtonUI button)
    {
        if (button != null && !registeredButtons.Contains(button))
        {
            registeredButtons.Add(button);
            button.ApplyTheme(); // Apply current theme immediately
            Debug.Log($"ColorThemeManager.RegisterButton called for: {button.name}");
        }
    }

    /// Apply theme to all registered buttons
    private void ApplyThemeToAllButtons()
    {
        foreach (ButtonUI button in registeredButtons)
        {
            if (button != null)
            {
                button.ApplyTheme();
            }
        }
        registeredButtons.RemoveAll(b => b == null);
    }

    /// Get current theme's button colors
    public ButtonTheme GetButtonTheme()
    {
        return currentTheme.buttonTheme;
    }

    /// Get current theme's UI styling
    public UITheme GetUITheme()
    {
        return currentTheme.uiTheme;
    }

    /// Check if high contrast is enabled
    public bool IsHighContrast()
    {
        return isHighContrast;
    }

    /// Apply theme colors to a UI Document's root element
    public void ApplyThemeToUIDocument(VisualElement root)
    {
        if (root == null || currentTheme == null)
            return;

        var uiTheme = currentTheme.uiTheme;
        var buttonTheme = currentTheme.buttonTheme;

        // Apply theme to root and content areas
        VisualElement rootElement = root.Q<VisualElement>("root");
        if (rootElement != null)
            rootElement.style.backgroundColor = uiTheme.backgroundColor;

        VisualElement header = root.Q<VisualElement>("header");
        if (header != null)
            header.style.backgroundColor = uiTheme.headerBackgroundColor;

        VisualElement footer = root.Q<VisualElement>("footer");
        if (footer != null)
            footer.style.backgroundColor = uiTheme.headerBackgroundColor;

        // Apply background color to plant description card (main container)
        VisualElement plantDescription = root.Q<VisualElement>(className: "plant-description");
        if (plantDescription != null)
            plantDescription.style.backgroundColor = uiTheme.backgroundColor;

        // Apply background color to content containers
        VisualElement contentContainer = root.Q<VisualElement>("content");
        if (contentContainer != null)
            contentContainer.style.backgroundColor = uiTheme.backgroundColor;

        // Categories page uses a dedicated container for category cards.
        VisualElement categoryContainer = root.Q<VisualElement>("cateCont");
        if (categoryContainer != null)
            categoryContainer.style.backgroundColor = uiTheme.backgroundColor;

        VisualElement plantElement = root.Q<VisualElement>("Plant");
        if (plantElement != null)
            plantElement.style.backgroundColor = uiTheme.backgroundColor;

        // Apply background color to dropdown cards and info containers
        var dropdownCards = root.Query<VisualElement>(className: "dropdown-card").ToList();
        foreach (VisualElement card in dropdownCards)
        {
            card.style.backgroundColor = uiTheme.backgroundColor;
        }

        // Apply text color to all labels with explicit opacity
        var labels = root.Query<Label>().ToList();
        foreach (Label label in labels)
        {
            label.style.color = uiTheme.textColor;
            label.style.opacity = 1f;
        }

        // Apply text color to toggles
        var toggles = root.Query<Toggle>().ToList();
        foreach (Toggle toggle in toggles)
        {
            toggle.style.color = uiTheme.textColor;
        }

        // Apply text and background color to buttons
        var buttons = root.Query<Button>().ToList();
        foreach (Button button in buttons)
        {
            button.style.color = uiTheme.textColor;
            button.style.backgroundColor = buttonTheme.normalColor;
        }

        Debug.Log($"ColorThemeManager: applied UI theme to document -> {currentTheme.themeName}");
    }

    /// Subscribe to theme changes
    public void SubscribeToThemeChange(Action<ColorTheme> callback)
    {
        if (callback != null)
        {
            OnThemeChanged += callback;
        }
    }

    /// Unsubscribe from theme changes
    public void UnsubscribeFromThemeChange(Action<ColorTheme> callback)
    {
        if (callback != null)
        {
            OnThemeChanged -= callback;
        }
    }
}
