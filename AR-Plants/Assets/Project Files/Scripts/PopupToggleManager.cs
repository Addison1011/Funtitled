using UnityEngine;

public class PopupToggleManager : MonoBehaviour
{
    public RectTransform infoPanel;
    public Vector2 hiddenPosition;
    public Vector2 shownPosition;
    private bool isVisible = false;
    public float slidespeed = 500f;

    void Start()
    {
        infoPanel.anchoredPosition = hiddenPosition;
    }

    public void TogglePanel()
    {
        Debug.Log("Info panel reference: " + infoPanel);
        Debug.Log("Toggling panel. New state: " + isVisible);
        isVisible = !isVisible;
        StopAllCoroutines();
        StartCoroutine(SlidePanel(isVisible ? shownPosition : hiddenPosition));
    }

    private System.Collections.IEnumerator SlidePanel(Vector2 target)
    {
        while (Vector2.Distance(infoPanel.anchoredPosition, target) > 0.1f)
        {
            infoPanel.anchoredPosition = Vector2.Lerp(infoPanel.anchoredPosition, target, Time.deltaTime * 10f);
            yield return null;
        }
        infoPanel.anchoredPosition = target;
        Debug.Log("Arrived at: " + infoPanel.anchoredPosition);
    }

}