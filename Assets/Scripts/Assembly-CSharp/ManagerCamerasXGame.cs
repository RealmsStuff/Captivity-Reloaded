using UnityEngine;

public class ManagerCamerasXGame : MonoBehaviour
{
	[SerializeField]
	private CameraXGame m_cameraGame;

	[SerializeField]
	private CameraXGame m_cameraArchive;

	private CameraXGame m_cameraCurrent;

	private void UpdateCurrentCamera()
	{
		if (CommonReferences.Instance.GetManagerScreens().GetScreenCurrent() is ScreenGame)
		{
			m_cameraCurrent = m_cameraGame;
		}
		CommonReferences.Instance.GetManagerScreens().GetScreenCurrent();
	}

	public CameraXGame GetCameraXGameCurrent()
	{
		UpdateCurrentCamera();
		return m_cameraCurrent;
	}
}
