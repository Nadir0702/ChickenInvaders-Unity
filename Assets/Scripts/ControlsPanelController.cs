using UnityEngine;

public class ControlsPanelController : MonoBehaviour
{
    public void OnBackClick()
    {
        UIManager.Instance?.HideControlsPanel();
    }

    public void OnToggleClicked()
    {
        PlayerController.Instance?.ToggleControls();
    }
}
