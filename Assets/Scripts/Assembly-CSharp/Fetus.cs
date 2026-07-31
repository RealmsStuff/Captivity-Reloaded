using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fetus : MonoBehaviour
{
	private int m_idNpcParent;

	private List<Actor> m_actors = new List<Actor>();

	private string m_nameFetus;

	private float m_durationPregnant;

	[SerializeField]
	private float m_timeLeftPregnant;

	private float m_timeBypassForTriggerLabor = 0.2f;

	private bool m_isPlayTimer;

	private bool m_isTimerStarted;

	private bool m_isLaborTriggered;

	private void OnEnable()
	{
		if (!m_isLaborTriggered)
		{
			if (m_timeLeftPregnant < m_timeBypassForTriggerLabor && !m_isLaborTriggered && m_isTimerStarted)
			{
				TriggerLabor();
			}
			else if (m_isTimerStarted)
			{
				m_isPlayTimer = true;
				StartCoroutine(CoroutineTimerPregnant());
			}
		}
	}

	public void InitializeFetus(NPC i_npcParent, Actor i_actorInFetus, float i_timePregnant, string i_nameFetus)
	{
		if (i_npcParent != null)
		{
			m_idNpcParent = i_npcParent.GetId();
		}
		else
		{
			m_idNpcParent = -1;
		}
		m_actors.Clear();
		m_actors.Add(i_actorInFetus);
		m_nameFetus = m_actors[0].GetName();
		m_durationPregnant = i_timePregnant;
		m_timeLeftPregnant = i_timePregnant;
	}

	public void InitializeFetus(List<Actor> i_actorsInFetus, float i_timePregnant, string i_nameFetus)
	{
		m_actors.Clear();
		m_actors = i_actorsInFetus;
		m_nameFetus = i_nameFetus;
		m_durationPregnant = i_timePregnant;
		m_timeLeftPregnant = i_timePregnant;
	}

	public void StartTimerPregnant()
	{
		m_isTimerStarted = true;
		m_isPlayTimer = true;
		if (base.isActiveAndEnabled)
		{
			StartCoroutine(CoroutineTimerPregnant());
		}
		CommonReferences.Instance.GetPlayer().OnRapeEnd += OnRapeEnd;
		CommonReferences.Instance.GetPlayer().OnBirthEnd += OnBirthEnd;
	}

	private void OnRapeEnd()
	{
		if (!m_isLaborTriggered)
		{
			if (m_timeLeftPregnant < m_timeBypassForTriggerLabor && !m_isLaborTriggered && m_isTimerStarted)
			{
				TriggerLabor();
			}
			else if (m_isTimerStarted)
			{
				m_isPlayTimer = true;
				StartCoroutine(CoroutineTimerPregnant());
			}
		}
	}

	private void OnBirthEnd()
	{
		if (m_timeLeftPregnant < m_timeBypassForTriggerLabor && !m_isLaborTriggered && m_isTimerStarted)
		{
			TriggerLabor();
		}
	}

	private void PauseTimer()
	{
		m_isPlayTimer = false;
	}

	private void ResumeTimer()
	{
		if (!m_isLaborTriggered)
		{
			if (m_timeLeftPregnant < m_timeBypassForTriggerLabor && !m_isLaborTriggered && m_isTimerStarted)
			{
				TriggerLabor();
			}
			else
			{
				m_isPlayTimer = true;
			}
		}
	}

	private IEnumerator CoroutineTimerPregnant()
	{
		Player l_player = CommonReferences.Instance.GetPlayer();
		while (m_timeLeftPregnant > 0f)
		{
			yield return new WaitForEndOfFrame();
			l_player.GetStatePlayerCurrent();
			if (m_isPlayTimer)
			{
				float deltaTime = Time.deltaTime;
				m_timeLeftPregnant -= deltaTime;
			}
		}
		TriggerLabor();
	}

	private void TriggerLabor()
	{
		if (CommonReferences.Instance.GetPlayer().GetStatePlayerCurrent() != StatePlayer.Labor && !CommonReferences.Instance.GetPlayer().GetIsBeingRaped())
		{
			CommonReferences.Instance.GetPlayer().StartLabor(this);
			m_isLaborTriggered = true;
		}
	}

	private void OnDestroy()
	{
		CommonReferences.Instance.GetManagerHud().GetManagerFetusHud().DestroyFetusItem(this);
		CommonReferences.Instance.GetPlayer().OnLabor -= PauseTimer;
		CommonReferences.Instance.GetPlayer().OnBirthEnd -= ResumeTimer;
		CommonReferences.Instance.GetPlayer().OnBirthEnd -= OnBirthEnd;
		CommonReferences.Instance.GetPlayer().OnRapeEnd -= OnRapeEnd;
	}

	public List<Actor> GetActorsInFetus()
	{
		return m_actors;
	}

	public string GetNameFetus()
	{
		return m_nameFetus;
	}

	public float GetTimePregnantMax()
	{
		return m_durationPregnant;
	}

	public float GetTimePregnantLeft()
	{
		return m_timeLeftPregnant;
	}

	public bool IsReadyToBirth()
	{
		if (m_timeLeftPregnant <= 0f)
		{
			return true;
		}
		return false;
	}

	public NPC GetNpcParent()
	{
		foreach (NPC allNpc in Library.Instance.Actors.GetAllNpcs())
		{
			if (allNpc.GetId() == m_idNpcParent)
			{
				return allNpc;
			}
		}
		return null;
	}
}
