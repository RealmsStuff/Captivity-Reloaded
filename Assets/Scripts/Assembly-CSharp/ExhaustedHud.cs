using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ExhaustedHud : MonoBehaviour
{
	[SerializeField]
	private Image m_imgMeterBg;

	[SerializeField]
	private Image m_imgMeterCurrent;

	[SerializeField]
	private Image m_imgMeterBar;

	[SerializeField]
	private Text m_txtLeft;

	[SerializeField]
	private Text m_txtRight;

	[SerializeField]
	private Color m_colorArrowToPress;

	[SerializeField]
	private Color m_colorArrowToNotPress;

	[SerializeField]
	private GameObject m_posXMax;

	[SerializeField]
	private GameObject m_posXMin;

	[SerializeField]
	private AudioClip m_audioHit;

	private float m_meterMax;

	private float m_meterCurrent;

	private KeyCode m_keyCurrent;
	private Image m_mobileInputGlyph;

	public void StartExhaustionGame()
	{
		m_meterMax = 100f;
		m_meterCurrent = 0f;
		m_keyCurrent = KeyCode.A;
		base.gameObject.SetActive(value: true);
	}

	private void Update()
	{
		if (CommonReferences.Instance.GetManagerScreens().GetScreenGame().GetIsInventoryOpen())
		{
			return;
		}
		PlayerController controller = CommonReferences.Instance.GetPlayerController();
		bool mobile = controller != null && controller.GetIsMobileControlsEnabled();
		ManagerInput input = CommonReferences.Instance.GetManagerInput();
		bool controllerInput = !mobile && input != null && input.IsControllerLastUsed();
		bool useRecoveryButton = mobile || controllerInput;
		if (mobile)
		{
			if (controller.GetIsMobileJumpPressed()) HandleHit();
		}
		else if (controllerInput)
		{
			// Controller directions are continuous axes, so IsButtonDown(MoveLeft/MoveRight)
			// can never advance this alternating-key recovery game. Use the mapped Jump
			// action just like the mobile recovery control instead.
			if (input.IsButtonDown(InputButton.Jump)) HandleHit();
		}
		else if (m_keyCurrent == KeyCode.D)
		{
			if (input.IsButtonDown(InputButton.MoveLeft))
			{
				m_keyCurrent = KeyCode.A;
				HandleHit();
			}
		}
		else if (input.IsButtonDown(InputButton.MoveRight))
		{
			m_keyCurrent = KeyCode.D;
			HandleHit();
		}
		RefreshRecoveryPrompt(useRecoveryButton);
		if (!useRecoveryButton && m_keyCurrent == KeyCode.A)
		{
			m_txtLeft.color = m_colorArrowToPress;
			m_txtRight.color = m_colorArrowToNotPress;
		}
		else if (!useRecoveryButton)
		{
			m_txtLeft.color = m_colorArrowToNotPress;
			m_txtRight.color = m_colorArrowToPress;
		}
		if (m_meterCurrent >= m_meterMax)
		{
			Win();
		}
		UpdateMeter();
	}

	private void RefreshRecoveryPrompt(bool i_useRecoveryButton)
	{
		m_txtLeft.gameObject.SetActive(!i_useRecoveryButton);
		m_txtRight.gameObject.SetActive(!i_useRecoveryButton);
		if (m_mobileInputGlyph == null)
		{
			m_mobileInputGlyph = InputGlyphLibrary.GetOrCreateImage(transform, "MobileKnockoutJumpGlyph");
			RectTransform rect = m_mobileInputGlyph.rectTransform;
			rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
			rect.pivot = new Vector2(0.5f, 0.5f);
			rect.anchoredPosition = new Vector2(0f, 62f);
			rect.sizeDelta = new Vector2(64f, 64f);
		}
		m_mobileInputGlyph.sprite = i_useRecoveryButton ? InputGlyphLibrary.GetPromptSprite(InputButton.Jump) : null;
		m_mobileInputGlyph.gameObject.SetActive(i_useRecoveryButton && m_mobileInputGlyph.sprite != null);
	}

	private void UpdateMeter()
	{
		Vector3 vector = m_imgMeterCurrent.GetComponent<RectTransform>().anchoredPosition;
		vector.x = m_imgMeterBar.GetComponent<RectTransform>().rect.width / m_meterMax * m_meterCurrent - m_imgMeterBar.GetComponent<RectTransform>().rect.width;
		m_imgMeterCurrent.GetComponent<RectTransform>().anchoredPosition = vector;
	}

	private IEnumerator CoroutineDecreaseMeter()
	{
		while (true)
		{
			yield return new WaitForSeconds(0.01f);
			m_meterCurrent -= 1f;
			if (m_meterCurrent < 0f)
			{
				m_meterCurrent = 0f;
			}
		}
	}

	private void HandleHit()
	{
		m_meterCurrent += 10f;
		CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(m_audioHit);
	}

	private void Win()
	{
		CommonReferences.Instance.GetPlayer().WinExhaustionGame();
		Hide();
	}

	public void Interrupt()
	{
		Hide();
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}
}
