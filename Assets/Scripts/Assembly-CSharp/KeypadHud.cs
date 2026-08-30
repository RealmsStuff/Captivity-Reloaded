using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeypadHud : MonoBehaviour
{
	private Keypad m_keypadCurrent;

	[SerializeField]
	private List<UnityEngine.UI.Button> m_btnKeys = new List<UnityEngine.UI.Button>();

	[SerializeField]
	private GameObject m_keypadObject;

	[SerializeField]
	private GameObject m_imgOverlay;

	[SerializeField]
	private Text m_txtInput;

	private bool m_isCanPressKeys;

	private Coroutine m_coroutineFailCode;

	private string m_lastKeyPressed;

	private float m_nextKeyPressTime;

	private void Awake()
	{
		m_keypadObject.SetActive(value: false);
		m_imgOverlay.SetActive(value: false);
		m_txtInput.text = "";
		m_isCanPressKeys = true;
		m_lastKeyPressed = null;
		m_nextKeyPressTime = 0f;
	}

	private void Update()
	{
	}

	private void ListenToCloseKeypadEvents()
	{
		CommonReferences.Instance.GetPlayer().OnGetHit += Close;
		CommonReferences.Instance.GetPlayer().OnBeingRaped += Close;
		CommonReferences.Instance.GetPlayer().OnLabor += Close;
	}

	private void StopListenToCloseKeypadEvents()
	{
		CommonReferences.Instance.GetPlayer().OnGetHit -= Close;
		CommonReferences.Instance.GetPlayer().OnBeingRaped -= Close;
		CommonReferences.Instance.GetPlayer().OnLabor -= Close;
	}

	public void Show(Keypad i_keypad)
	{
		CommonReferences.Instance.GetPlayer().SetIsForceIgnoreInput(i_isForceIgnoreInput: true);
		m_keypadCurrent = i_keypad;
		if (m_keypadCurrent.GetIsCompleted())
		{
			m_txtInput.text = "OK";
		}
		else
		{
			m_txtInput.text = "";
		}
		m_keypadObject.SetActive(value: true);
		m_imgOverlay.SetActive(value: true);
		m_isCanPressKeys = true;
		m_lastKeyPressed = null;
		m_nextKeyPressTime = 0f;
		ListenToCloseKeypadEvents();
	}

	public void Close()
	{
		CommonReferences.Instance.GetPlayer().SetIsForceIgnoreInput(i_isForceIgnoreInput: false);
		m_keypadObject.SetActive(value: false);
		m_imgOverlay.SetActive(value: false);
		if (m_coroutineFailCode != null)
		{
			StopCoroutine(m_coroutineFailCode);
			m_coroutineFailCode = null;
		}
		StopListenToCloseKeypadEvents();
	}

	public bool IsShowing()
	{
		return m_keypadObject.activeSelf;
	}

	public void PressKey(string i_key)
	{
		if (string.IsNullOrEmpty(i_key) || (m_lastKeyPressed == i_key && Time.unscaledTime < m_nextKeyPressTime))
		{
			return;
		}
		m_lastKeyPressed = i_key;
		m_nextKeyPressTime = Time.unscaledTime + 0.15f;
		if (m_isCanPressKeys && !m_keypadCurrent.GetIsCompleted())
		{
			m_txtInput.text += i_key;
			if (m_txtInput.text.Length == 4)
			{
				CheckIfInputIsCorrect();
			}
		}
	}

	private void CheckIfInputIsCorrect()
	{
		if (m_keypadCurrent.GetCode() == m_txtInput.text)
		{
			m_txtInput.text = "OK";
			m_keypadCurrent.Complete();
			return;
		}
		if (m_coroutineFailCode != null)
		{
			StopCoroutine(m_coroutineFailCode);
		}
		m_coroutineFailCode = StartCoroutine(CoroutineFailCode());
	}

	private IEnumerator CoroutineFailCode()
	{
		m_txtInput.text = "ERROR";
		m_isCanPressKeys = false;
		yield return new WaitForSeconds(1f);
		m_txtInput.text = "";
		m_isCanPressKeys = true;
		m_coroutineFailCode = null;
	}
}
