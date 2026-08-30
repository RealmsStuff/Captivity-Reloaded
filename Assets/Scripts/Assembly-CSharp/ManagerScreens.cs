using UnityEngine;

public class ManagerScreens : MonoBehaviour
{
	[SerializeField]
	private ScreenTitle m_screenTitle;

	[SerializeField]
	private ScreenGame m_screenGame;

	[SerializeField]
	private ScreenEnd m_screenEnd;

	[SerializeField]
	private Screen m_screenStart;

	private Screen m_screenCurrent;

	private void Start()
	{
		StartGame();
	}

	private void StartGame()
	{
		OpenScreen(m_screenStart);
	}

	public void OpenScreenTitle()
	{
		Time.timeScale = 1f;
		CommonReferences.Instance.GetManagerInput().BlockControllerSubmitUntilRelease();
		OpenScreen(m_screenTitle);
	}

	public void OpenScreenGame()
	{
		ManagerDB.OnStartGame += OnManagerDBStartGame;
		ManagerDB.StartGame();
	}

	private void OnManagerDBStartGame()
	{
		ManagerDB.OnStartGame -= OnManagerDBStartGame;
		OpenScreen(m_screenGame);
	}

	public void OpenScreenEnd()
	{
		OpenScreen(m_screenEnd);
	}

	public void OpenScreen(Screen i_screen)
	{
		CloseAllScreens();
		m_screenCurrent = i_screen;
		m_screenCurrent.Open();
	}

	private void CloseAllScreens()
	{
		m_screenTitle.Close();
		m_screenGame.Close();
		m_screenEnd.Close();
	}

	public ScreenGame GetScreenGame()
	{
		return m_screenGame;
	}

	public Screen GetScreenCurrent()
	{
		return m_screenCurrent;
	}
}
