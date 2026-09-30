using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenTitle : Screen
{
	[SerializeField]
	private GameObject m_parentLoading;

	[SerializeField]
	private GameObject m_parentTitle;

	[SerializeField]
	private Image m_imgBlackOverlay;

	[SerializeField]
	private AudioClip m_audioScreenMenu;

	[SerializeField]
	private Text m_txtVersion;

	private void Start()
	{
	}

	public override void Open()
	{
		base.Open();
		CommonReferences.Instance.GetManagerAudio().PlayAudioMusic(m_audioScreenMenu);
		StartCoroutine(CoroutineAnimateFadeIn());
		ManagerDB.Initialize();
		CheckPrefs();
		CommonReferences.Instance.GetManagerAudio().SetVolumesToSaved();
		m_parentTitle.SetActive(value: true);
		m_parentLoading.SetActive(value: false);
		m_txtVersion.text = "version " + Application.version;
	}

	private void CheckPrefs()
	{
		if (!PlayerPrefs.HasKey("VolumeMaster"))
		{
			ManagerDB.ResetVolumes();
		}
		if (!PlayerPrefs.HasKey("Difficulty"))
		{
			ManagerDB.SetDifficulty(Difficulty.Normal);
		}
		if (!PlayerPrefs.HasKey("IsReduceGunFlash"))
		{
			ManagerDB.SetIsReduceGunFlash(i_isReduce: false);
		}
		DisplaySettings.EnsureDefaults();
	}

	public void OpenOptions()
	{
		m_parentTitle.SetActive(value: false);
		GetComponentInChildren<ManagerOptions>(includeInactive: true).Open();
	}

	public void CloseOptions()
	{
		m_parentTitle.SetActive(value: true);
		GetComponentInChildren<ManagerOptions>(includeInactive: true).Close();
	}

	public void SetTitleMenuVisible(bool i_isVisible)
	{
		m_parentTitle.SetActive(i_isVisible);
	}

	public void ShowLoading()
	{
		m_parentTitle.SetActive(value: false);
		m_parentLoading.SetActive(value: true);
	}

	private IEnumerator CoroutineAnimateFadeIn()
	{
		float l_tranparencyFrom = 1f;
		float l_tranparencyTo = 0f;
		float l_timeToMove = 0.5f;
		float l_timeCurrent = 0f;
		m_imgBlackOverlay.color = new Color(m_imgBlackOverlay.color.r, m_imgBlackOverlay.color.g, m_imgBlackOverlay.color.b, l_tranparencyFrom);
		m_imgBlackOverlay.gameObject.SetActive(value: true);
		yield return new WaitForSecondsRealtime(0.5f);
		while (l_timeCurrent < l_timeToMove)
		{
			l_timeCurrent += Time.fixedDeltaTime;
			float i_time = l_timeCurrent / l_timeToMove;
			float a = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Steep, l_tranparencyFrom, l_tranparencyTo, i_time);
			m_imgBlackOverlay.color = new Color(m_imgBlackOverlay.color.r, m_imgBlackOverlay.color.g, m_imgBlackOverlay.color.b, a);
			yield return new WaitForFixedUpdate();
		}
		m_imgBlackOverlay.gameObject.SetActive(value: false);
	}

	private IEnumerator CoroutineAnimateFadeInScreenGame()
	{
		float l_tranparencyFrom = 0f;
		float l_tranparencyTo = 1f;
		float l_timeToMove = 0.5f;
		float l_timeCurrent = 0f;
		m_imgBlackOverlay.color = new Color(m_imgBlackOverlay.color.r, m_imgBlackOverlay.color.g, m_imgBlackOverlay.color.b, l_tranparencyFrom);
		m_imgBlackOverlay.gameObject.SetActive(value: true);
		while (l_timeCurrent < l_timeToMove)
		{
			l_timeCurrent += Time.fixedDeltaTime;
			float i_time = l_timeCurrent / l_timeToMove;
			float a = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Steep, l_tranparencyFrom, l_tranparencyTo, i_time);
			m_imgBlackOverlay.color = new Color(m_imgBlackOverlay.color.r, m_imgBlackOverlay.color.g, m_imgBlackOverlay.color.b, a);
			yield return new WaitForFixedUpdate();
		}
		CommonReferences.Instance.GetManagerScreens().OpenScreenGame();
	}

	public void ExitGame()
	{
		Application.Quit();
	}
}
