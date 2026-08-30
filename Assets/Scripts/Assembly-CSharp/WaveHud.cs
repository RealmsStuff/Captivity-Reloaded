using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WaveHud : MonoBehaviour
{
	[SerializeField]
	private Text[] m_txtsTitleWave = new Text[2];

	[SerializeField]
	private Text[] m_txtsTimer = new Text[2];

	private Coroutine m_coroutineTimer;

	private void Awake()
	{
		m_txtsTitleWave[0].color = new Color(m_txtsTitleWave[0].color.r, m_txtsTitleWave[0].color.g, m_txtsTitleWave[0].color.b, 0f);
		m_txtsTitleWave[1].color = new Color(m_txtsTitleWave[1].color.r, m_txtsTitleWave[1].color.g, m_txtsTitleWave[1].color.b, 0f);
		m_txtsTimer[0].color = new Color(m_txtsTitleWave[0].color.r, m_txtsTitleWave[0].color.g, m_txtsTitleWave[0].color.b, 0f);
		m_txtsTimer[1].color = new Color(m_txtsTitleWave[1].color.r, m_txtsTitleWave[1].color.g, m_txtsTitleWave[1].color.b, 0f);
		Clear();
	}

	public void ShowStageName(string i_nameStage)
	{
		m_txtsTitleWave[0].text = i_nameStage;
		m_txtsTitleWave[1].text = m_txtsTitleWave[0].text;
		GetComponent<Animator>().Play("ShowWaveStart");
	}

	public void ShowWaveStart(int i_numWave)
	{
		m_txtsTitleWave[0].text = "Wave " + i_numWave;
		m_txtsTitleWave[1].text = m_txtsTitleWave[0].text;
		if (m_coroutineTimer != null)
		{
			StopCoroutine(m_coroutineTimer);
		}
		GetComponent<Animator>().Play("ShowWaveStart");
	}

	public void ShowWaveEnd(int i_secsWait)
	{
		m_txtsTitleWave[0].text = "Wave End";
		m_txtsTitleWave[1].text = m_txtsTitleWave[0].text;
		GetComponent<Animator>().Play("ShowWaveEnd");
		if (m_coroutineTimer != null)
		{
			StopCoroutine(m_coroutineTimer);
		}
		m_coroutineTimer = StartCoroutine(CoroutineTimer(i_secsWait));
	}

	private IEnumerator CoroutineTimer(int i_secsWait)
	{
		int l_secsPassed = 0;
		while (l_secsPassed < i_secsWait)
		{
			string text = CommonReferences.Instance.GetManagerInput().GetWaveSkipBindingName();
			m_txtsTimer[0].text = i_secsWait - l_secsPassed + " ('" + text + "' to skip)";
			m_txtsTimer[1].text = m_txtsTimer[0].text;
			l_secsPassed++;
			yield return new WaitForSeconds(1f);
		}
		m_coroutineTimer = null;
	}

	public void Clear()
	{
		GetComponent<Animator>().Play("Idle");
		m_txtsTitleWave[0].text = "";
		m_txtsTitleWave[1].text = "";
		m_txtsTimer[0].text = "";
		m_txtsTimer[1].text = "";
		if (m_coroutineTimer != null)
		{
			StopCoroutine(m_coroutineTimer);
			m_coroutineTimer = null;
		}
	}
}
