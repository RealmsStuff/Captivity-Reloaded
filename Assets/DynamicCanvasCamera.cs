using UnityEngine;

public class DynamicCanvasCamera : MonoBehaviour
{
    private Canvas m_canvas;

    void Awake()
    {
        m_canvas = GetComponent<Canvas>();
    }

    void Update()
    {
        // Find the camera manager in your scene and get the currently active camera
        var cameraManager = FindFirstObjectByType<ManagerCamerasXGame>();
        if (cameraManager != null)
        {
            var currentCamera = cameraManager.GetCameraXGameCurrent();
            if (currentCamera != null)
            {
                // Dynamically assign the camera component
                m_canvas.worldCamera = currentCamera.GetComponent<Camera>();
            }
        }
    }
}