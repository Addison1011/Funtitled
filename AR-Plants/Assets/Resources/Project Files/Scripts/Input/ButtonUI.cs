using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Generic button styling and functionality manager.
/// Can be used on any button to apply custom colors, text, and icons.
/// </summary>
public class ButtonUI : MonoBehaviour
{
    [Header("Button References")]
    [SerializeField] private Button button;
    [SerializeField] private Image buttonImage;
    [SerializeField] private TextMeshProUGUI buttonText;

    [Header("Style Settings")]
    [SerializeField] private Color buttonColor = new Color(0.2f, 0.7f, 1f, 1f); // Light blue
    [SerializeField] private Color buttonHoverColor = new Color(0.1f, 0.6f, 0.9f, 1f); // Darker blue
    [SerializeField] private Color buttonPressedColor = new Color(0f, 0.5f, 0.8f, 1f); // Even darker blue
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private string buttonLabel = "";

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

        // Get button Text component if it exists
        if (buttonText == null)
        {
            buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
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

        // Style the text if it exists
        if (buttonText != null && !string.IsNullOrEmpty(buttonLabel))
        {
            buttonText.text = buttonLabel;
            buttonText.color = textColor;
        }

        // Set button icon if provided
        if (buttonIcon != null && buttonImage != null)
        {
            buttonImage.sprite = buttonIcon;
        }
    }

    /// <summary>
    /// Update the button's color at runtime
    /// </summary>
    public void SetButtonColor(Color newColor)
    {
        buttonColor = newColor;
        if (buttonImage != null)
        {
            buttonImage.color = newColor;
        }
    }

    /// <summary>
    /// Update the button label text
    /// </summary>
    public void SetButtonLabel(string newLabel)
    {
        buttonLabel = newLabel;
        if (buttonText != null)
        {
            buttonText.text = newLabel;
        }
    }

    /// <summary>
    /// Update the button icon
    /// </summary>
    public void SetButtonIcon(Sprite newIcon)
    {
        buttonIcon = newIcon;
        if (buttonImage != null)
        {
            buttonImage.sprite = newIcon;
        }
    }
}
