using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenEnd : Screen
{
	[SerializeField]
	private Text m_txtCredits;

	[SerializeField]
	private Text m_txtThankYou;

	[SerializeField]
	private UnityEngine.UI.Button m_btn;

	public override void Open()
	{
		base.Open();
		m_txtCredits.gameObject.SetActive(value: false);
		m_txtThankYou.gameObject.SetActive(value: false);
		m_btn.gameObject.SetActive(value: false);
		StartCoroutine(CoroutineOpen());
	}

	private IEnumerator CoroutineOpen()
	{
		yield return new WaitForSeconds(6f);
		m_txtCredits.gameObject.SetActive(value: true);
		yield return new WaitForSeconds(15f);
		m_txtCredits.gameObject.SetActive(value: false);
		m_txtThankYou.gameObject.SetActive(value: true);
		m_btn.gameObject.SetActive(value: true);
	}
}
