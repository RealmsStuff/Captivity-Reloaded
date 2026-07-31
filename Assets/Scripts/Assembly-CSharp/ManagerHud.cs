using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManagerHud : MonoBehaviour
{
	[SerializeField]
	private Canvas m_canvasOverlay;

	[SerializeField]
	private Canvas m_canvasCamera;

	[SerializeField]
	private GameObject m_heartHudDefault;

	[SerializeField]
	private Sprite m_spriteHeartAlive;

	[SerializeField]
	private Sprite m_spriteHeartDead;

	[SerializeField]
	private HealthBar m_healthBar;

	[SerializeField]
	private StaminaBar m_staminaBar;

	[SerializeField]
	private LibidoHeart m_libidoHeart;

	[SerializeField]
	private PleasureBar m_pleasureBar;

	[SerializeField]
	private StrengthBar m_strengthBar;

	[SerializeField]
	private MoneyHud m_moneyHud;

	[SerializeField]
	private ManagerHudRapeGames m_managerHudRapeGames;

	[SerializeField]
	private ExhaustedHud m_exhaustedHud;

	[SerializeField]
	private ManagerNotification m_managerNotification;

	[SerializeField]
	private ManagerHealthDisplay m_managerHealthDisplay;

	[SerializeField]
	private KeypadHud m_keypadHud;

	[SerializeField]
	private InspectHud m_inspectHud;

	[SerializeField]
	private ManagerOverlay m_managerOverlay;

	[SerializeField]
	private ManagerFetusHud m_managerFetusHud;

	[SerializeField]
	private StatusPlayerHud m_statusPlayerHud;

	[SerializeField]
	private ManagerDamageNumber m_managerDamageNumber;

	[SerializeField]
	private ManagerAmmoHud m_managerAmmoHud;

	[SerializeField]
	private ManagerSpeechBubble m_managerSpeechBubble;

	[SerializeField]
	private HubMainMenu m_hubMainMenu;

	[SerializeField]
	private GameObject m_pauseMenu;

	[SerializeField]
	private Text[] m_txtsWaveNumber;

	private bool m_isPaused;

	private float m_distanceBetweenHearts;

	private List<HeartHud> m_heartsHud = new List<HeartHud>();

	private Player m_player;

	private void Start()
	{
		StartCoroutine(CoroutineWaitInitialization());
		CommonReferences.Instance.GetManagerActor().OnPlayerChange += OnPlayerChange;
	}

	private IEnumerator CoroutineWaitInitialization()
	{
		yield return new WaitForFixedUpdate();
		yield return new WaitForFixedUpdate();
		yield return new WaitForFixedUpdate();
		Initialize();
	}

	private void Initialize()
	{
		m_player = CommonReferences.Instance.GetPlayer();
		SetIsShowWaveNumber(i_isShow: false);
		m_managerSpeechBubble.ClearSpeechBubbles();
		m_managerHealthDisplay.ClearHealthDisplayItems();
		m_statusPlayerHud.RestartGameAfterGameOver();
		GetWaveHud().Clear();
		GetChallengeHud().Clear();
		GetManagerHudRapeGames().HideHudSmasher();
		m_exhaustedHud.Hide();
		GetHealthBar().ResetBar();
		GetPleasureBar().ResetBar();
		GetStrengthBar().ResetBar();
		m_libidoHeart.ResetLibidoHeart();
		HideHud();
		ShowHud();
		BuildHearts();
		ClosePauseMenu();
	}

	public void RefreshHud()
	{
		Initialize();
	}

	private void OnPlayerChange()
	{
		Initialize();
	}

	private void LateUpdate()
	{
		m_player = CommonReferences.Instance.GetPlayer();
	}

	public void BuildHearts()
	{
		m_heartHudDefault.SetActive(value: false);
		m_distanceBetweenHearts = 4f;
		for (int i = 0; i < m_player.GetNumOfHeartsCurrent(); i++)
		{
			GameObject gameObject = Object.Instantiate(m_heartHudDefault, m_heartHudDefault.transform.parent);
			gameObject.SetActive(value: true);
			Vector3 vector = gameObject.GetComponent<RectTransform>().anchoredPosition;
			vector.x += (float)i * (gameObject.GetComponent<RectTransform>().sizeDelta.x + m_distanceBetweenHearts);
			gameObject.GetComponent<RectTransform>().anchoredPosition = vector;
			m_heartsHud.Add(gameObject.GetComponent<HeartHud>());
		}
	}

	public void DestroyAHeart()
	{
		m_heartsHud[m_player.GetNumOfHeartsCurrent() - 1].Kill();
	}

	public Sprite GetImageHeartDead()
	{
		return m_spriteHeartDead;
	}

	public Sprite GetImageHeartAlive()
	{
		return m_spriteHeartAlive;
	}

	public void HideHud()
	{
		foreach (HeartHud item in m_heartsHud)
		{
			item.gameObject.SetActive(value: false);
		}
		m_healthBar.Hide();
		m_staminaBar.Hide();
		m_libidoHeart.Hide();
		m_pleasureBar.Hide();
		m_strengthBar.Hide();
		m_managerFetusHud.Hide();
		m_statusPlayerHud.Hide();
		m_moneyHud.Hide();
		m_txtsWaveNumber[0].enabled = false;
		m_txtsWaveNumber[1].enabled = false;
	}

	public void ShowHud()
	{
		foreach (HeartHud item in m_heartsHud)
		{
			item.gameObject.SetActive(value: true);
		}
		m_healthBar.Show();
		m_staminaBar.Show();
		m_libidoHeart.Show();
		m_pleasureBar.Show();
		m_strengthBar.Show();
		m_managerFetusHud.Show();
		m_statusPlayerHud.Show();
		m_managerNotification.Show();
		m_managerHealthDisplay.Show();
		m_moneyHud.Show();
		m_txtsWaveNumber[0].enabled = true;
		m_txtsWaveNumber[1].enabled = true;
		m_managerOverlay.ShowOverlay();
	}

	public void RapeHud()
	{
		m_managerHealthDisplay.Hide();
		m_managerNotification.Hide();
		m_staminaBar.Hide();
	}

	public void DeadHud()
	{
		HideHud();
		m_libidoHeart.Show();
		m_pleasureBar.Show();
		m_managerFetusHud.Show();
		m_statusPlayerHud.Show();
		m_managerOverlay.ShowOverlay();
		string text = "Mind broken at Wave " + CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetManagerWave()
			.GetNumWaveCurrent();
		if (CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetManagerWave()
			.IsHighscoreAchieved())
		{
			text += ". NEW HIGHSCORE!";
		}
		text += "\n'Esc' for rescue...";
		m_txtsWaveNumber[0].enabled = true;
		m_txtsWaveNumber[1].enabled = true;
		m_txtsWaveNumber[0].text = text;
		m_txtsWaveNumber[1].text = m_txtsWaveNumber[0].text;
	}

	public void ClearHud()
	{
	}

	public void PlayerCum()
	{
		m_managerOverlay.PlayOverlayFlash(Color.white, i_isUseOverlayWithHole: true, i_isDestroyOverlayAfterAnimation: true, 4, 0.05f, 0.05f, 0.45f);
	}

	public void GainPleasure()
	{
		m_pleasureBar.Increase();
	}

	public void GainLibido()
	{
		m_libidoHeart.FlashGainLibido(1);
	}

	public void LoseLibido()
	{
		m_libidoHeart.FlashLoseLibido(1);
	}

	public HealthBar GetHealthBar()
	{
		return m_healthBar;
	}

	public StaminaBar GetStaminaBar()
	{
		return m_staminaBar;
	}

	public void TakeDamage()
	{
		m_healthBar.Increase();
	}

	public PleasureBar GetPleasureBar()
	{
		return m_pleasureBar;
	}

	public StrengthBar GetStrengthBar()
	{
		return m_strengthBar;
	}

	public ManagerHudRapeGames GetManagerHudRapeGames()
	{
		return m_managerHudRapeGames;
	}

	public void StartExhaustedState()
	{
		m_exhaustedHud.StartExhaustionGame();
	}

	public void InterruptExhaustedState()
	{
		m_exhaustedHud.Interrupt();
	}

	public void AddHealthDisplay(NPC i_npc)
	{
		m_managerHealthDisplay.AddNPCHealthDisplay(i_npc);
	}

	public ManagerHealthDisplay GetManagerHealthDisplay()
	{
		return m_managerHealthDisplay;
	}

	public ManagerNotification GetManagerNotification()
	{
		return m_managerNotification;
	}

	public MoneyHud GetMoneyHud()
	{
		return m_moneyHud;
	}

	public VendorHud GetVendorHud()
	{
		return GetComponentInChildren<VendorHud>();
	}

	public void GameOver()
	{
	}

	public void Retry()
	{
		foreach (HeartHud item in m_heartsHud)
		{
			item.Live();
		}
	}

	public void OpenPauseMenu()
	{
		m_isPaused = true;
		m_pauseMenu.SetActive(value: true);
	}

	public void ClosePauseMenu()
	{
		m_isPaused = false;
		m_pauseMenu.SetActive(value: false);
	}

	public void OpenKeypad(Keypad i_keypad)
	{
		m_keypadHud.Show(i_keypad);
	}

	public void ShowNote(Note i_note)
	{
		m_inspectHud.ShowNote(i_note);
	}

	public void InspectObject(InspectionObject i_inspectionObject)
	{
		m_inspectHud.InspectObject(i_inspectionObject);
	}

	public void CreateDamageNumber(float i_dmgNum, Vector2 i_posSpawn)
	{
		m_managerDamageNumber.CreateDamageNumber(i_dmgNum, i_posSpawn);
	}

	public void DisplayAmmoGun(Gun i_gun)
	{
		m_managerAmmoHud.DisplayAmmoGun(i_gun);
	}

	public void HideAmmoDisplay()
	{
		m_managerAmmoHud.HideAmmoDisplay();
	}

	public HubMainMenu GetHubMainMenu()
	{
		return m_hubMainMenu;
	}

	public void OpenHubMainMenu()
	{
		m_hubMainMenu.Open();
	}

	public void CloseHubMainMenu()
	{
		m_hubMainMenu.Close();
	}

	public ManagerEquippablesHud GetManagerEquippablesHud()
	{
		return GetComponentInChildren<ManagerEquippablesHud>();
	}

	public ManagerDamageNumber GetManagerDamageNumber()
	{
		return m_managerDamageNumber;
	}

	public void CreateSpeechBubble(string i_text, SpeechBubbleTextColor i_textColor, Vector2 i_pos, bool i_isToLeft)
	{
		m_managerSpeechBubble.CreateSpeechBubble(i_text, i_textColor, i_pos, i_isToLeft);
	}

	public ManagerSpeechBubble GetManagerSpeechBubble()
	{
		return m_managerSpeechBubble;
	}

	public ManagerOverlay GetManagerOverlay()
	{
		return m_managerOverlay;
	}

	public ManagerFetusHud GetManagerFetusHud()
	{
		return m_managerFetusHud;
	}

	public StatusPlayerHud GetStatusPlayerHud()
	{
		return GetComponentInChildren<StatusPlayerHud>();
	}

	public WaveHud GetWaveHud()
	{
		return GetComponentInChildren<WaveHud>();
	}

	public void CompleteChallenge(Challenge i_challenge)
	{
		GetComponentInChildren<ChallengeHud>().CompleteChallenge(i_challenge);
	}

	public WardrobeHud GetWardrobeHud()
	{
		return GetComponentInChildren<WardrobeHud>();
	}

	public void ShowWardrobeHud()
	{
		GetComponentInChildren<WardrobeHud>().Show();
	}

	public void SetIsShowWaveNumber(bool i_isShow)
	{
		m_txtsWaveNumber[0].gameObject.SetActive(i_isShow);
		m_txtsWaveNumber[1].gameObject.SetActive(i_isShow);
	}

	public void SetWaveNumber(int i_numWave, bool i_isHighscore)
	{
		string text = "Wave " + i_numWave;
		if (i_isHighscore)
		{
			text += " HIGHSCORE";
		}
		m_txtsWaveNumber[0].text = text;
		m_txtsWaveNumber[1].text = m_txtsWaveNumber[0].text;
	}

	public Canvas GetCanvasOverlay()
	{
		return m_canvasOverlay;
	}

	public Canvas GetCanvasCamera()
	{
		return m_canvasCamera;
	}

	public ChallengeHud GetChallengeHud()
	{
		return GetComponentInChildren<ChallengeHud>();
	}
}
