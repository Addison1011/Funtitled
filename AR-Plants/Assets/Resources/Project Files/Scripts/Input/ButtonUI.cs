using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// Generic button styling and functionality manager.
/// Can be used on any button to apply custom colors, text, and icons.
public class ButtonUI : MonoBehaviour
{
    [Header("Button References")]
    [SerializeField] private Button button;
    [SerializeField] private Image buttonImage;

    [Header("Style Settings")]
    [SerializeField] private Color buttonColor = new Color(0.2f, 0.7f, 1f, 1f); // Light blue
    [SerializeField] private Color buttonHoverColor = new Color(0.1f, 0.6f, 0.9f, 1f); // Darker blue
    [SerializeField] private Color buttonPressedColor = new Color(0f, 0.5f, 0.8f, 1f); // Even darker blue

    [Header("Button Icon (Optional)")]
    [SerializeField] private Sprite buttonIcon;

    private void OnEnable()
    {
        InitializeButton();
    }

    private void InitializeButton()
    {
        // Find the button if not assigned
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (button == null)
        {
            Debug.LogError("ButtonUI: Could not find Button component!");
            return;
        }

        // Get button Image component for styling
        if (buttonImage == null)
        {
            buttonImage = button.GetComponent<Image>();
        }

        // Register with ColorThemeManager
        if (ColorThemeManager.Instance != null)
        {
            ColorThemeManager.Instance.RegisterButton(this);
        }

        // Apply styling
        ApplyButtonStyle();
    }

    private void ApplyButtonStyle()
    {
        // Style the button image/background
        if (buttonImage != null)
        {
            buttonImage.color = buttonColor;
        }

        // Set up button transition colors
        ColorBlock colors = button.colors;
        colors.normalColor = buttonColor;
        colors.highlightedColor = buttonHoverColor;
        colors.pressedColor = buttonPressedColor;
        colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        button.colors = colors;

        // Set button icon if provided
        if (buttonIcon != null && buttonImage != null)
        {
            buttonImage.sprite = buttonIcon;
        }
    }

    /// Apply theme colors from ColorThemeManager
    public void ApplyTheme()
    {
        if (ColorThemeManager.Instance == null)
            return;

        var buttonTheme = ColorThemeManager.Instance.GetButtonTheme();
        buttonColor = buttonTheme.normalColor;
        buttonHoverColor = buttonTheme.hoverColor;
        buttonPressedColor = buttonTheme.pressedColor;

        ApplyButtonStyle();
    }

    /// Update the button's color at runtime
    /// Might delete this method after testing ColorThemeManager
    public void SetButtonColor(Color newColor)
    {
        buttonColor = newColor;
        if (buttonImage != null)
        {
            buttonImage.color = newColor;
        }
    }

    /// Update the button icon
    public void SetButtonIcon(Sprite newIcon)
    {
        buttonIcon = newIcon;
        if (buttonImage != null)
        {
            buttonImage.sprite = newIcon;
        }
    }
}
