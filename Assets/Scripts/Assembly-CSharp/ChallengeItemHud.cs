using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChallengeItemHud : MonoBehaviour
{
	[SerializeField]
	private Text m_txtName;

	[SerializeField]
	private Text m_txtDescription;

	[SerializeField]
	private Image m_imgHudItem;

	[SerializeField]
	private Image m_imgRewardBorder;

	[SerializeField]
	private Image m_imgReward;

	[SerializeField]
	private Text m_txtReward;

	private Challenge m_challenge;

	private Color m_colorOpen = Color.white;

	private Color m_colorCompleted = Color.green;

	private Color m_colorReward = new Color(1f, 0.5f, 0f);

	public void Initialize(Challenge i_challenge)
	{
		m_challenge = i_challenge;
		if (m_challenge == null)
		{
			gameObject.SetActive(value: false);
			return;
		}
		m_txtName.text = m_challenge.GetName();
		Clothing clothing = GetFirstValidReward();
		if (clothing != null)
		{
			m_imgReward.sprite = clothing.GetIcon();
			m_txtReward.text = clothing.GetCatergoryClothing().ToString();
		}
		else
		{
			m_imgReward.enabled = false;
			m_imgRewardBorder.enabled = false;
			m_txtReward.enabled = false;
		}
		switch (m_challenge.GetState())
		{
		case 0:
			SetToOpen();
			break;
		case 1:
			SetToCompletedPending();
			break;
		case 2:
			SetToCompletedSeen();
			break;
		}
		if (GetValidRewards().Count > 1)
		{
			StartCoroutine(CoroutineAnimateIterateThroughRewards());
		}
	}

	private List<Clothing> GetValidRewards()
	{
		List<Clothing> list = new List<Clothing>();
		foreach (Clothing item in m_challenge.GetRewardsClothing())
		{
			if (item != null)
			{
				list.Add(item);
			}
		}
		return list;
	}

	private Clothing GetFirstValidReward()
	{
		List<Clothing> validRewards = GetValidRewards();
		return validRewards.Count > 0 ? validRewards[0] : null;
	}

	private void SetToOpen()
	{
		m_imgHudItem.color = m_colorOpen;
		m_imgRewardBorder.color = m_colorReward;
		if (m_challenge.IsHiddenDescription())
		{
			m_txtDescription.text = "???";
		}
		else
		{
			m_txtDescription.text = m_challenge.GetDescription();
		}
	}

	private void SetToCompletedPending()
	{
		SetColorsAndTextToComplete();
		StartCoroutine(CoroutineAnimatePending());
	}

	private IEnumerator CoroutineAnimatePending()
	{
		int l_timesToBlink = 8;
		int l_timesBlinked = 0;
		Color l_colorFrom = m_colorOpen;
		l_colorFrom.a = 0f;
		for (; l_timesBlinked <= l_timesToBlink; l_timesBlinked++)
		{
			Color l_weightFrom = m_imgHudItem.color;
			Color l_weightTo = l_colorFrom;
			float l_timeToMove = 0.25f;
			float l_timeCurrent = 0f;
			while (l_timeCurrent < l_timeToMove)
			{
				l_timeCurrent += Time.fixedDeltaTime;
				float i_time = l_timeCurrent / l_timeToMove;
				float r = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Smooth, l_weightFrom.r, l_weightTo.r, i_time);
				float g = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Smooth, l_weightFrom.g, l_weightTo.g, i_time);
				float b = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Smooth, l_weightFrom.b, l_weightTo.b, i_time);
				float a = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Smooth, l_weightFrom.a, l_weightTo.a, i_time);
				Color color = new Color(r, g, b, a);
				m_imgHudItem.color = color;
				m_imgRewardBorder.color = color;
				m_txtName.color = color;
				m_txtDescription.color = color;
				m_txtReward.color = color;
				m_imgReward.color = color;
				yield return new WaitForFixedUpdate();
			}
			l_weightFrom = l_colorFrom;
			l_weightTo = m_colorCompleted;
			l_timeCurrent = 0f;
			while (l_timeCurrent < l_timeToMove)
			{
				l_timeCurrent += Time.fixedDeltaTime;
				float i_time2 = l_timeCurrent / l_timeToMove;
				float r2 = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Smooth, l_weightFrom.r, l_weightTo.r, i_time2);
				float g2 = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Smooth, l_weightFrom.g, l_weightTo.g, i_time2);
				float b2 = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Smooth, l_weightFrom.b, l_weightTo.b, i_time2);
				float a2 = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Smooth, l_weightFrom.a, l_weightTo.a, i_time2);
				Color color2 = new Color(r2, g2, b2, a2);
				m_imgHudItem.color = color2;
				m_imgRewardBorder.color = color2;
				m_txtName.color = color2;
				m_txtDescription.color = color2;
				m_txtReward.color = color2;
				m_imgReward.color = color2;
				yield return new WaitForFixedUpdate();
			}
		}
		SetColorsAndTextToComplete();
		m_imgReward.color = Color.white;
	}

	private void SetToCompletedSeen()
	{
		SetColorsAndTextToComplete();
	}

	private void SetColorsAndTextToComplete()
	{
		m_txtName.color = m_colorCompleted;
		m_txtDescription.color = m_colorCompleted;
		m_imgHudItem.color = m_colorCompleted;
		m_imgRewardBorder.color = m_colorCompleted;
		m_txtDescription.text = m_challenge.GetDescription();
		m_txtReward.color = m_colorCompleted;
	}

	private IEnumerator CoroutineAnimateIterateThroughRewards()
	{
		List<Clothing> validRewards = GetValidRewards();
		if (validRewards.Count < 2)
		{
			yield break;
		}
		int l_index = 0;
		while (true)
		{
			Color l_colorInvisible = new Color(1f, 1f, 1f, 0f);
			Color l_weightFrom = m_imgReward.color;
			Color l_weightTo = l_colorInvisible;
			float l_timeToMove = 0.25f;
			float l_timeCurrent = 0f;
			while (l_timeCurrent < l_timeToMove)
			{
				l_timeCurrent += Time.fixedDeltaTime;
				float i_time = l_timeCurrent / l_timeToMove;
				float r = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Smooth, l_weightFrom.r, l_weightTo.r, i_time);
				float g = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Smooth, l_weightFrom.g, l_weightTo.g, i_time);
				float b = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Smooth, l_weightFrom.b, l_weightTo.b, i_time);
				float a = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Smooth, l_weightFrom.a, l_weightTo.a, i_time);
				Color color = new Color(r, g, b, a);
				m_imgReward.color = color;
				yield return new WaitForFixedUpdate();
			}
			l_index++;
			if (l_index > validRewards.Count - 1)
			{
				l_index = 0;
			}
			m_imgReward.sprite = validRewards[l_index].GetIcon();
			m_txtReward.text = validRewards[l_index].GetCatergoryClothing().ToString();
			l_weightFrom = l_colorInvisible;
			l_weightTo = new Color(1f, 1f, 1f, 1f);
			l_timeCurrent = 0f;
			while (l_timeCurrent < l_timeToMove)
			{
				l_timeCurrent += Time.fixedDeltaTime;
				float i_time2 = l_timeCurrent / l_timeToMove;
				float r2 = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Smooth, l_weightFrom.r, l_weightTo.r, i_time2);
				float g2 = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Smooth, l_weightFrom.g, l_weightTo.g, i_time2);
				float b2 = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Smooth, l_weightFrom.b, l_weightTo.b, i_time2);
				float a2 = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Smooth, l_weightFrom.a, l_weightTo.a, i_time2);
				Color color2 = new Color(r2, g2, b2, a2);
				m_imgReward.color = color2;
				yield return new WaitForFixedUpdate();
			}
			yield return new WaitForSeconds(2.5f);
		}
	}
}
