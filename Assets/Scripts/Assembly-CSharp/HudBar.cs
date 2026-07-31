using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public abstract class HudBar : MonoBehaviour
{
	[SerializeField]
	protected Image m_imageValue;

	[SerializeField]
	protected Image m_imageValueDelayed;

	[SerializeField]
	protected Image m_imageValueOverlay;

	[SerializeField]
	protected Image m_imageBar;

	[SerializeField]
	protected float m_secsDelayBeforeMoveValueDelayed;

	[SerializeField]
	protected float m_timeToMoveValueDelayed;

	protected Vector2 m_posValue;

	protected float m_valueMax;

	protected float m_valueCurrent;

	private Coroutine m_coroutineValueDelayed;

	protected virtual void Update()
	{
		UpdateValues();
		UpdatePosValues();
	}

	protected abstract void UpdateValues();

	private void UpdatePosValues()
	{
		Vector3 vector = m_imageValue.GetComponent<RectTransform>().anchoredPosition;
		vector.x = m_imageBar.GetComponent<RectTransform>().rect.width / m_valueMax * m_valueCurrent - m_imageBar.GetComponent<RectTransform>().rect.width;
		m_imageValue.GetComponent<RectTransform>().anchoredPosition = vector;
		if (m_imageValueOverlay != null)
		{
			m_imageValueOverlay.GetComponent<RectTransform>().anchoredPosition = vector;
		}
		m_posValue = vector;
	}

	private IEnumerator CoroutineValueDelayed()
	{
		yield return new WaitForSeconds(m_secsDelayBeforeMoveValueDelayed);
		float l_posXFrom = m_imageValueDelayed.GetComponent<RectTransform>().anchoredPosition.x;
		float l_posXTo = m_posValue.x;
		float l_timeToMove = m_timeToMoveValueDelayed;
		float l_timeCurrent = 0f;
		while (l_timeCurrent < l_timeToMove)
		{
			l_timeCurrent += Time.fixedDeltaTime;
			float i_time = l_timeCurrent / l_timeToMove;
			Vector2 anchoredPosition = new Vector3(AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Steep, l_posXFrom, l_posXTo, i_time), m_imageValueDelayed.GetComponent<RectTransform>().anchoredPosition.y);
			m_imageValueDelayed.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;
			yield return new WaitForFixedUpdate();
		}
		m_coroutineValueDelayed = null;
	}

	public void Show()
	{
		Image[] componentsInChildren = GetComponentsInChildren<Image>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = true;
		}
	}

	public void Hide()
	{
		Image[] componentsInChildren = GetComponentsInChildren<Image>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = false;
		}
	}

	public virtual void Increase()
	{
		UpdateValues();
		RestartValueDelayed();
	}

	public virtual void Decrease()
	{
		UpdateValues();
		RestartValueDelayed();
	}

	private void RestartValueDelayed()
	{
		if (m_coroutineValueDelayed != null)
		{
			StopCoroutine(m_coroutineValueDelayed);
		}
		m_coroutineValueDelayed = StartCoroutine(CoroutineValueDelayed());
	}

	public virtual void ResetBar()
	{
		if (m_coroutineValueDelayed != null)
		{
			StopCoroutine(m_coroutineValueDelayed);
			m_coroutineValueDelayed = null;
		}
		StartCoroutine(CoroutineResetValueDelayed());
		if (m_imageValueOverlay != null)
		{
			m_imageValueOverlay.color = new Color(m_imageValueOverlay.color.r, m_imageValueOverlay.color.g, m_imageValueOverlay.color.b, 0f);
		}
	}

	private IEnumerator CoroutineResetValueDelayed()
	{
		yield return new WaitForSeconds(0.25f);
		m_imageValueDelayed.GetComponent<RectTransform>().anchoredPosition = m_imageValue.GetComponent<RectTransform>().anchoredPosition;
	}
}
