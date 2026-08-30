using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenGame : Screen
{
	[SerializeField]
	private Image m_imgBlackOverlay;

	[SerializeField]
	private bool m_isDebugMode;

	[SerializeField]
	private AudioClip m_audioPause;

	[SerializeField]
	private AudioClip m_audioUnPause;

	private bool m_isOpenedBefore;

	private bool m_isInventoryOpen;

	private bool m_isFirstTimeSpawn = true;

	private bool m_isPaused;

	private Coroutine m_coroutineFadeOut;

	private void Awake()
	{
		base.gameObject.SetActive(value: false);
	}

	private void Update()
	{
		if (m_isDebugMode)
		{
			HandleDebugKeys();
		}
		if (Input.GetKeyDown(KeyCode.Escape) || CommonReferences.Instance.GetManagerInput().IsPausePressed())
		{
			HandleEscapeInput();
		}
	}

	public void HandleEscapeInput()
	{
		if (!CommonReferences.Instance.GetManagerHud().GetVendorHud().GetIsOpen() && !CommonReferences.Instance.GetManagerHud().GetWardrobeHud().IsShowing() && !CommonReferences.Instance.GetManagerHud().GetHubMainMenu().IsOpen())
		{
			if (CommonReferences.Instance.GetManagerHud().GetManagerEquippablesHud().GetIsShowing())
			{
				CommonReferences.Instance.GetManagerHud().GetManagerEquippablesHud().Hide();
			}
			else if (CommonReferences.Instance.GetPlayer().IsDead())
			{
				RefreshGame();
				CommonReferences.Instance.GetManagerStages().OpenStage(CommonReferences.Instance.GetManagerStages().GetStageHub());
			}
			else if (m_isPaused)
			{
				ResumeGame();
			}
			else
			{
				PauseGame();
			}
		}
	}

	private void HandleDebugKeys()
	{
		if (Input.GetKeyDown(KeyCode.Keypad1))
		{
			CommonReferences.Instance.GetPlayerController().GainMoney(1000);
		}
		if (Input.GetKeyDown(KeyCode.Keypad2))
		{
			CommonReferences.Instance.GetPlayer().TakeDamage(10f);
		}
		if (Input.GetKeyDown(KeyCode.Keypad3))
		{
			foreach (Challenge allChallenge in CommonReferences.Instance.GetManagerChallenge().GetAllChallenges())
			{
				CommonReferences.Instance.GetManagerChallenge().CompleteChallenge(allChallenge);
			}
		}
		if (Input.GetKeyDown(KeyCode.Keypad4))
		{
			ManagerDB.UnlockAllClothes();
		}
		if (Input.GetKeyDown(KeyCode.Keypad5))
		{
			CommonReferences.Instance.GetPlayer().LoseLibido(100f);
			CommonReferences.Instance.GetPlayer().LosePleasure(100f);
		}
		if (Input.GetKeyDown(KeyCode.Keypad6))
		{
			CommonReferences.Instance.GetPlayer().DamageStrength(100f);
		}
		if (Input.GetKeyDown(KeyCode.Keypad7))
		{
			CommonReferences.Instance.GetPlayer().CreateAndAddFetus(null, Library.Instance.Actors.GetActorDupe("Fly"), 3f, "test");
		}
		Input.GetKeyDown(KeyCode.Keypad8);
		if (Input.GetKeyDown(KeyCode.KeypadPeriod))
		{
			CommonReferences.Instance.GetPlayer().Orgasm();
		}
		if (Input.GetKeyDown(KeyCode.KeypadEnter))
		{
			CommonReferences.Instance.GetPlayer().GainLibido(100f);
			CommonReferences.Instance.GetPlayer().GainPleasureFlat(50f);
		}
		if (Input.GetKeyDown(KeyCode.Mouse3))
		{
			ScreenCapture.CaptureScreenshot(Application.dataPath + "/StreamingAssets/Screenshots/" + DateTime.Now.ToString("HH-mm-ss") + ".png");
		}
	}

	public override void Open()
	{
		base.Open();
		if (!m_isOpenedBefore)
		{
			m_isOpenedBefore = true;
		}
		base.gameObject.SetActive(value: true);
		RefreshGame();
		if (CommonReferences.Instance.GetPlayer().IsDead())
		{
			CommonReferences.Instance.GetManagerStages().OpenStage(CommonReferences.Instance.GetManagerStages().GetStageHub());
		}
		else
		{
			CommonReferences.Instance.GetManagerStages().OpenStageStart();
		}
		m_isFirstTimeSpawn = false;
		CommonReferences.Instance.GetManagerHud().GetManagerOverlay().PlayOverlay(Color.black, i_isUseOverlayWithHole: false, i_isDestroyOverlayAfterAnimation: true, 1.5f, 1f, 0f);
		CommonReferences.Instance.GetManagerInput().SetButtonsToSavedButtons();
		Library.Instance.Clothes.FirstTimeStart();
	}

	public void RefreshGame()
	{
		CommonReferences.Instance.GetManagerAudio().StopMusic();
		CommonReferences.Instance.GetManagerActor().RecreatePlayer();
		CommonReferences.Instance.GetManagerHud().RefreshHud();
		CommonReferences.Instance.GetManagerCamerasXGame().GetCameraXGameCurrent().ZoomToFOV(CommonReferences.Instance.GetManagerCamerasXGame().GetCameraXGameCurrent().GetFOVOriginal(), 0.1f);
		CommonReferences.Instance.GetManagerPostProcessing().ResetAllEffects();
		CommonReferences.Instance.GetPlayerController().ResetInventory();
	}

	public override void Close()
	{
		base.Close();
		CommonReferences.Instance.GetManagerAudio().StopMusic();
		CommonReferences.Instance.GetManagerAudio().StopAudioAmbience();
		CommonReferences.Instance.GetManagerHud().GetManagerOverlay().ClearGeneratedOverlays();
	}

	public void PauseGame()
	{
		Time.timeScale = 0f;
		m_isPaused = true;
		CommonReferences.Instance.GetManagerHud().OpenPauseMenu();
		CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(m_audioPause);
	}

	public void ResumeGame()
	{
		Time.timeScale = 1f;
		m_isPaused = false;
		CommonReferences.Instance.GetManagerHud().ClosePauseMenu();
		CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(m_audioUnPause);
	}

	public void SlowMoDeath()
	{
		CommonReferences.Instance.GetManagerHud().HideHud();
		CommonReferences.Instance.GetManagerPostProcessing().GetEffectDeath().weight = 1f;
		StartCoroutine(CoroutineSlowMoDeath());
	}

	public bool GetIsInventoryOpen()
	{
		return m_isInventoryOpen;
	}

	public bool GetIsFirstTimeSpawn()
	{
		return m_isFirstTimeSpawn;
	}

	private IEnumerator CoroutineSlowMoDeath()
	{
		CommonReferences.Instance.GetManagerAudio().SetVolumeMusicAndAmbienceToMute();
		CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(Resources.Load<AudioClip>("Audio/GlassBreak"));
		Time.timeScale = 0.25f;
		yield return new WaitForSecondsRealtime(3f);
		if (CommonReferences.Instance.GetPlayer().GetIsBeingRaped())
		{
			if ((bool)CommonReferences.Instance.GetPlayer().GetRaperCurrent().GetDefeatScene())
			{
				Time.timeScale = 0f;
				ManagerDefeatScene managerDefeatScene = CommonReferences.Instance.GetManagerDefeatScene();
				managerDefeatScene.OpenScene(CommonReferences.Instance.GetPlayer().GetRaperCurrent().GetDefeatScene());
				managerDefeatScene.OnEndDefeatScene += ContinueGameAfterDefeatScene;
			}
			else
			{
				CommonReferences.Instance.GetManagerAudio().SetVolumesToDefault();
				CommonReferences.Instance.GetManagerPostProcessing().PlayEffectDeathAway();
				Time.timeScale = 1f;
			}
		}
		else
		{
			CommonReferences.Instance.GetManagerAudio().SetVolumesToDefault();
			CommonReferences.Instance.GetManagerPostProcessing().PlayEffectDeathAway();
			Time.timeScale = 1f;
		}
	}

	private void ContinueGameAfterDefeatScene()
	{
		CommonReferences.Instance.GetManagerDefeatScene().OnEndDefeatScene -= ContinueGameAfterDefeatScene;
		StartCoroutine(CoroutineContinueAfterDefeatScene());
	}

	private IEnumerator CoroutineContinueAfterDefeatScene()
	{
		Time.timeScale = 0.25f;
		yield return new WaitForSecondsRealtime(1.5f);
		CommonReferences.Instance.GetManagerAudio().SetVolumesToDefault();
		Time.timeScale = 1f;
	}

	public void OpenStage(int i_numStage)
	{
		CommonReferences.Instance.GetManagerStages().OpenStage(i_numStage);
	}

	public void OpenStage(Stage i_stage)
	{
		CommonReferences.Instance.GetManagerStages().OpenStage(i_stage);
	}

	private IEnumerator CoroutineAnimateFadeOut()
	{
		yield return new WaitForSecondsRealtime(0f);
		Image l_imgOverlay = m_imgBlackOverlay.GetComponentInChildren<Image>();
		float l_tranparencyFrom = 1f;
		float l_tranparencyTo = 0f;
		float l_timeToMove = 0.5f;
		float l_timeCurrent = 0f;
		m_imgBlackOverlay.color = new Color(l_imgOverlay.color.r, l_imgOverlay.color.g, l_imgOverlay.color.b, l_tranparencyFrom);
		m_imgBlackOverlay.gameObject.SetActive(value: true);
		while (l_timeCurrent < l_timeToMove)
		{
			l_timeCurrent += Time.fixedDeltaTime;
			float i_time = l_timeCurrent / l_timeToMove;
			float a = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Steep, l_tranparencyFrom, l_tranparencyTo, i_time);
			m_imgBlackOverlay.color = new Color(l_imgOverlay.color.r, l_imgOverlay.color.g, l_imgOverlay.color.b, a);
			yield return new WaitForFixedUpdate();
		}
		m_coroutineFadeOut = null;
	}

	public bool IsPaused()
	{
		return m_isPaused;
	}
}
