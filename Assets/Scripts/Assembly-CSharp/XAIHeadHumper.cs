using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XAIHeadHumper : XAIWalker
{
	private bool m_isPanicking;

	private List<StatModifier> m_modifiersPanic = new List<StatModifier>();

	public override void HandleIntelligence()
	{
		if (m_npc.IsOnFire())
		{
			Panic();
		}
		else
		{
			base.HandleIntelligence();
		}
	}

	private void Panic()
	{
		if (m_modifiersPanic.Count == 0)
		{
			m_modifiersPanic.Add(m_npc.AddStatModifier("SpeedAccel", 6f));
			m_modifiersPanic.Add(m_npc.AddStatModifier("SpeedMax", 6f));
		}
		if (!m_isPanicking)
		{
			bool i_isLeft = ((Random.Range(0, 2) != 0) ? true : false);
			StartCoroutine(CoroutinePanic(i_isLeft, Random.Range(0.1f, 0.35f)));
		}
	}

	private IEnumerator CoroutinePanic(bool i_isLeft, float i_duration)
	{
		m_isPanicking = true;
		for (float l_durationPassed = 0f; l_durationPassed < i_duration; l_durationPassed += Time.deltaTime)
		{
			m_npc.MoveHorizontal(i_isLeft);
			yield return new WaitForEndOfFrame();
		}
		m_isPanicking = false;
		m_npc.RemoveStatModifier(m_modifiersPanic);
		m_modifiersPanic.Clear();
	}
}
