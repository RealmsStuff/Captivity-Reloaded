using System.Collections;
using CaptivityReloaded.Modding;
using UnityEngine;
using UnityEngine.InputSystem;

public class RaperSmasher : RaperGame, ISmasherHudSource
{
	[Header("---Smasher---")]
	[SerializeField]
	private float m_meterMax01;

	private AudioClip m_audioHit;

	private float m_meterCurrent;

	private KeyCode m_keyCurrent;

	private float m_secsPast;

	private Coroutine m_coroutineWaitUntilFailure;

	private Coroutine m_coroutineDecreaseMeter;

	private readonly StickCircleGesture m_circleGesture = new StickCircleGesture();

    protected virtual void Update()
    {
        if (m_isDone || !m_isStarted || CommonReferences.Instance.GetManagerScreens().GetScreenGame().IsPaused())
        {
            return;
        }

		PlayerController controller = CommonReferences.Instance.GetPlayerController();
		if (controller.GetIsMobileControlsEnabled())
		{
			if (controller.GetIsMobileJumpPressed()) HandleHit();
			if (m_circleGesture.Update(controller.GetMobileAimInput())) HandleHit(4f);
			return;
		}
		if (CommonReferences.Instance.GetManagerInput().IsControllerLastUsed() && Gamepad.current != null)
		{
			if (m_circleGesture.Update(Gamepad.current.rightStick.ReadValue())) HandleHit(4f);
			return;
		}

        // --- ORIGINAL PC INPUTS ---
        if (CommonReferences.Instance.GetManagerInput().IsButton(InputButton.Jump))
        {
            HandleAutoEscape();
        }
        else if (m_keyCurrent == KeyCode.D)
        {
            if (CommonReferences.Instance.GetManagerInput().IsButtonDown(InputButton.MoveLeft))
            {
                m_keyCurrent = KeyCode.A;
                HandleHit();
            }
        }
        else if (CommonReferences.Instance.GetManagerInput().IsButtonDown(InputButton.MoveRight))
        {
            m_keyCurrent = KeyCode.D;
            HandleHit();
        }
    }

    protected override void HandleRape()
	{
		base.HandleRape();
		if (!m_player.IsDead())
		{
			m_keyCurrent = KeyCode.D;
			m_isLose = false;
			m_isDone = false;
			m_isStarted = true;
			m_meterCurrent = 0f;
			m_circleGesture.Reset();
			m_audioHit = Resources.Load<AudioClip>("Audio/SmasherHit");
			ShowGameOverlay();
			if (m_coroutineWaitUntilFailure != null)
			{
				StopCoroutine(m_coroutineWaitUntilFailure);
			}
			m_coroutineWaitUntilFailure = StartCoroutine(CoroutineWaitUntilFailure());
			HandleRaperAnimation();
			base.OnAdvanceRaperAnimation += HandleRaperAnimation;
		}
	}

	private void HandleRaperAnimation()
	{
		if (m_raperAnimationCurrent.IsUseThrustToResist())
		{
			if (m_coroutineDecreaseMeter != null)
			{
				StopCoroutine(m_coroutineDecreaseMeter);
			}
		}
		else
		{
			m_coroutineDecreaseMeter = StartCoroutine(CoroutineDecreaseMeter());
		}
	}

	protected override void HandleEndRape()
	{
		if (m_coroutineWaitUntilFailure != null)
		{
			StopCoroutine(m_coroutineWaitUntilFailure);
		}
	}

	protected override void Lose()
	{
		base.Lose();
		base.OnAdvanceRaperAnimation -= HandleRaperAnimation;
		m_meterCurrent = 0f;
	}

	protected override void SetStartAudioRaperGame()
	{
		m_audioStartRaperGame = Resources.Load<AudioClip>("Audio/RaperGameSmasherStart");
	}

	protected override void ShowGameOverlay()
	{
		CommonReferences.Instance.GetManagerHud().GetManagerHudRapeGames().ShowHudSmasher(this);
	}

	protected override void HideGameOverlay()
	{
		CommonReferences.Instance.GetManagerHud().GetManagerHudRapeGames().HideHudSmasher();
	}

	private float CalculateHitPower(float modifier = 1f)
	{
		Player player = m_player != null ? m_player : CommonReferences.Instance.GetPlayer();
		if (player == null) return 1f * modifier;

		float strengthCurrent = player.GetStrengthCurrent();
		float strengthMax = player.GetStrengthMax();
		if (strengthMax <= 0f) strengthMax = 100f;

		float num2 = strengthCurrent / strengthMax * 100f;
		float num = m_escapePowerPlayer01 - m_escapePowerPlayer01 / 2f / 100f * (100f - num2);
		num -= num / 4f / 100f * (100f - num2);
		num *= 2f - m_difficulty02;
		if (strengthCurrent <= 0f)
		{
			num *= 0.5f;
		}
		DifficultyDefinition difficulty = DifficultyRegistry.Current;
		num *= difficulty == null ? 1f : difficulty.EscapeStrengthMultiplier;

		if (float.IsNaN(num) || float.IsInfinity(num) || num <= 0f)
		{
			num = 1f;
		}

		return num * modifier;
	}

	private void HandleHit(float powerMultiplier = 1f)
	{
		m_meterCurrent += CalculateHitPower(powerMultiplier);

		if (m_audioHit != null)
		{
			CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(m_audioHit);
		}

		if (m_meterCurrent >= GetMeterMax())
		{
			Lose();
		}
	}

	private void HandleAutoEscape()
	{
		m_meterCurrent += CalculateHitPower(0.2f);
		if (m_meterCurrent >= GetMeterMax())
		{
			Lose();
		}
	}

	private IEnumerator CoroutineWaitUntilFailure()
	{
		m_secsPast = 0f;
		Timer l_timer = new Timer(m_timeToEscape);
		StartCoroutine(l_timer.CoroutinePlayAndWaitForEnd());
		while (m_secsPast < m_timeToEscape)
		{
			yield return new WaitForEndOfFrame();
			m_secsPast = l_timer.GetTimePassed();
		}
		if (!m_isLose && !m_isDone)
		{
			base.OnAdvanceRaperAnimation -= HandleRaperAnimation;
			Win();
		}
	}

	private IEnumerator CoroutineDecreaseMeter()
	{
		while (!m_isDone)
		{
			yield return new WaitForEndOfFrame();
			m_meterCurrent -= GetMeterMax() * m_raperAnimationCurrent.GetResistanceRaper01() / 50f;
			if (m_meterCurrent < 0f)
			{
				m_meterCurrent = 0f;
			}
		}
	}

	protected override void Thrust(int i_power0to3)
	{
		base.Thrust(i_power0to3);
		if (m_raperAnimationCurrent.IsUseThrustToResist())
		{
			m_meterCurrent -= GetMeterMax() * m_raperAnimationCurrent.GetResistanceRaper01() / 2f;
			if (m_meterCurrent < 0f)
			{
				m_meterCurrent = 0f;
			}
		}
		if (!m_isDone && !m_player.IsDead())
		{
			CommonReferences.Instance.GetManagerHud().GetManagerHudRapeGames().GetHudSmasher()
				.Thrust();
		}
	}

	protected override void CumThrust(int i_power0to3)
	{
		base.CumThrust(i_power0to3);
		if (m_raperAnimationCurrent.IsUseThrustToResist())
		{
			m_meterCurrent -= GetMeterMax() * m_raperAnimationCurrent.GetResistanceRaper01() / 2f;
			if (m_meterCurrent < 0f)
			{
				m_meterCurrent = 0f;
			}
		}
		if (!m_isDone && !m_player.IsDead())
		{
			CommonReferences.Instance.GetManagerHud().GetManagerHudRapeGames().GetHudSmasher()
				.Thrust();
		}
	}

	public float GetMeterCurrent()
	{
		return m_meterCurrent;
	}

	public float GetMeterMax()
	{
		return m_meterMax01 * 100f;
	}

	public KeyCode GetKeyCodeToPress()
	{
		if (m_keyCurrent == KeyCode.A)
		{
			return KeyCode.D;
		}
		return KeyCode.A;
	}

	public float GetTimeLeft()
	{
		return m_timeToEscape - m_secsPast;
	}

	public bool UsesCircularInput()
	{
		return CommonReferences.Instance.GetPlayerController().GetIsMobileControlsEnabled()
			|| CommonReferences.Instance.GetManagerInput().IsControllerLastUsed();
	}

	public string GetInputPrompt()
	{
		return CommonReferences.Instance.GetPlayerController().GetIsMobileControlsEnabled()
			? "Rotate aim stick or tap Jump"
			: "Rotate right stick";
	}
}

public sealed class StickCircleGesture
{
	private const float ActivationMagnitude = 0.45f;
	private const float CompletionDegrees = 300f;
	private Vector2 m_previous;
	private float m_accumulatedDegrees;
	private bool m_hasPrevious;

	public bool Update(Vector2 i_stick)
	{
		if (i_stick.magnitude < ActivationMagnitude)
		{
			Reset();
			return false;
		}
		Vector2 current = i_stick.normalized;
		if (!m_hasPrevious)
		{
			m_previous = current;
			m_hasPrevious = true;
			return false;
		}
		float delta = Vector2.SignedAngle(m_previous, current);
		m_previous = current;
		if (Mathf.Abs(delta) > 120f)
		{
			m_accumulatedDegrees = 0f;
			return false;
		}
		m_accumulatedDegrees += delta;
		if (Mathf.Abs(m_accumulatedDegrees) < CompletionDegrees) return false;
		m_accumulatedDegrees = 0f;
		return true;
	}

	public void Reset()
	{
		m_previous = Vector2.zero;
		m_accumulatedDegrees = 0f;
		m_hasPrevious = false;
	}
}
