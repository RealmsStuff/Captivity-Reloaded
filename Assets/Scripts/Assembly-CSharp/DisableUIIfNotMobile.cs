using UnityEngine;

public class DisableUIIfNotMobile : MonoBehaviour
{
    private void Start()
    {
        // Automatically finds the PlayerController in the scene
        PlayerController playerController = FindObjectOfType<PlayerController>();

        if (playerController != null)
        {
            // Deactivates this parent object (and all its child UI elements) if not on mobile
            gameObject.SetActive(playerController.GetIsMobileControlsEnabled());
        }
    }
}