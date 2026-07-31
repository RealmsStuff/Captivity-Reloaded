using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ChallengeCompletionItemHud : MonoBehaviour
{
	[SerializeField]
	private Text m_txtNameChallenge;

	[SerializeField]
	private Text m_txtLabelComplete;

	[SerializeField]
	private Image m_imgItem;

	[SerializeField]
	private Image m_imgReward;

	[SerializeField]
	private Image m_imgRewardBorder;

	private Challenge m_challenge;

	public void Initialize(Challenge i_challenge)
	{
		m_challenge = i_challenge;
		m_txtNameChallenge.text = m_challenge.GetName();
		if (m_challenge.GetRewardsClothing().Count > 0)
		{
			m_imgReward.sprite = m_challenge.GetRewardsClothing()[0].GetIcon();
		}
		StartCoroutine(CoroutineAnimate());
		if (m_challenge.GetRewardsClothing().Count > 1)
		{
			StartCoroutine(CoroutineIterateThroughRewards());
		}
	}

	private IEnumerator CoroutineAnimate()
	{
		yield return CoroutineAnimateIn();
		yield return new WaitForSeconds(8f);
		yield return CoroutineAnimateOut();
		Object.Destroy(base.gameObject);
	}

	private IEnumerator CoroutineAnimateIn()
	{
		float l_weightFrom = 0f;
		float l_weightTo = 1f;
		float l_timeToMove = 0.5f;
		float l_timeCurrent = 0f;
		while (l_timeCurrent < l_timeToMove)
		{
			l_timeCurrent += Time.fixedDeltaTime;
			float i_time = l_timeCurrent / l_timeToMove;
			float a = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Smooth, l_weightFrom, l_weightTo, i_time);
			m_imgItem.color = new Color(m_imgItem.color.r, m_imgItem.color.g, m_imgItem.color.b, a);
			m_txtNameChallenge.color = new Color(m_txtNameChallenge.color.r, m_txtNameChallenge.color.g, m_txtNameChallenge.color.b, a);
			m_txtLabelComplete.color = new Color(m_txtLabelComplete.color.r, m_txtLabelComplete.color.g, m_txtLabelComplete.color.b, a);
			m_imgReward.color = new Color(m_imgReward.color.r, m_imgReward.color.g, m_imgReward.color.b, a);
			m_imgRewardBorder.color = new Color(m_imgRewardBorder.color.r, m_imgRewardBorder.color.g, m_imgRewardBorder.color.b, a);
			yield return new WaitForFixedUpdate();
		}
	}

	private IEnumerator CoroutineAnimateOut()
	{
		float l_weightFrom = 1f;
		float l_weightTo = 0f;
		float l_timeToMove = 0.5f;
		float l_timeCurrent = 0f;
		while (l_timeCurrent < l_timeToMove)
		{
			l_timeCurrent += Time.fixedDeltaTime;
			float i_time = l_timeCurrent / l_timeToMove;
			float a = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Smooth, l_weightFrom, l_weightTo, i_time);
			m_imgItem.color = new Color(m_imgItem.color.r, m_imgItem.color.g, m_imgItem.color.b, a);
			m_txtNameChallenge.color = new Color(m_txtNameChallenge.color.r, m_txtNameChallenge.color.g, m_txtNameChallenge.color.b, a);
			m_txtLabelComplete.color = new Color(m_txtLabelComplete.color.r, m_txtLabelComplete.color.g, m_txtLabelComplete.color.b, a);
			m_imgReward.color = new Color(m_imgReward.color.r, m_imgReward.color.g, m_imgReward.color.b, a);
			m_imgRewardBorder.color = new Color(m_imgRewardBorder.color.r, m_imgRewardBorder.color.g, m_imgRewardBorder.color.b, a);
			yield return new WaitForFixedUpdate();
		}
	}

	private IEnumerator CoroutineIterateThroughRewards()
	{
		int l_index = 0;
		while (true)
		{
			yield return new WaitForSeconds(1.5f);
			l_index++;
			if (l_index >= m_challenge.GetRewardsClothing().Count)
			{
				l_index = 0;
			}
			m_imgReward.sprite = m_challenge.GetRewardsClothing()[l_index].GetIcon();
		}
	}
}
