using UnityEngine;
using System.Collections.Generic;

public class ColorThemeManager : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void EnsureInstanceExists()
    {
        if (Instance == null)
        {
            var go = new GameObject("ColorThemeManager");
            go.AddComponent<ColorThemeManager>();
            Object.DontDestroyOnLoad(go);
            Debug.Log("ColorThemeManager: auto-created Instance before scene load.");
        }
        else
        {
            Debug.Log("ColorThemeManager: instance already exists at startup.");
        }
    }
    public static ColorThemeManager Instance { get; private set; }

    [System.Serializable]
    public class ButtonTheme
    {
        public Color normalColor = new Color(0.2f, 0.7f, 1f, 1f);
        public Color hoverColor = new Color(0.1f, 0.6f, 0.9f, 1f);
        public Color pressedColor = new Color(0f, 0.5f, 0.8f, 1f);
    }

    [System.Serializable]
    public class ColorTheme
    {
        public string themeName = "Default";
        public ButtonTheme buttonTheme = new ButtonTheme();
        // TODO: Add more color categories here (particles, UI text, backgrounds, etc.)
        
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
            Destroy(gameObject);
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
                normalColor = new Color(0.2f, 0.8f, 0.3f, 1f),    // Light green
                hoverColor = new Color(0.15f, 0.65f, 0.2f, 1f),   // Medium green
                pressedColor = new Color(0.1f, 0.5f, 0.15f, 1f)   // Dark green
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

    /// Check if high contrast is enabled
    public bool IsHighContrast()
    {
        return isHighContrast;
    }
}
