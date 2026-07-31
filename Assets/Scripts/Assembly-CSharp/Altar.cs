using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Altar : Interactable
{
	[SerializeField]
	private AudioClip m_audioPlaceFetish;

	[SerializeField]
	private PickUpable m_pickUpableFetish1;

	[SerializeField]
	private PickUpable m_pickUpableFetish2;

	[SerializeField]
	private PickUpable m_pickUpableFetish3;

	[SerializeField]
	private PickUpable m_pickUpableFetish4;

	[SerializeField]
	private GameObject m_fetish1;

	[SerializeField]
	private GameObject m_fetish2;

	[SerializeField]
	private GameObject m_fetish3;

	[SerializeField]
	private GameObject m_fetish4;

	[SerializeField]
	private GameObject m_effectsWorking;

	[SerializeField]
	private GameObject m_effectsActive;

	[SerializeField]
	private AudioClip m_audioWork;

	[SerializeField]
	private AudioClip m_audioReleaseSpirit;

	[SerializeField]
	private UnityEngine.Rendering.Universal.Light2D m_lightGlobal;

	private bool m_isAltarWorking;

	private bool m_isAltarActive;

	private bool m_isReleasedSpirit;

	protected override void HandleActivation(Actor i_initiator, InteractableActivationType i_activationType)
	{
		if (!m_isReleasedSpirit)
		{
			if (m_isAltarActive)
			{
				ReleaseSpirit();
			}
			else if (!IsAllFetishesPresent())
			{
				FetishPlacement();
			}
		}
	}

	private void ReleaseSpirit()
	{
		m_isReleasedSpirit = true;
		CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetManagerWave()
			.OnWaveStart -= DeActivateAltar;
		CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetManagerWave()
			.OnWaveEnd -= ActivateAltar;
		AltarSpirit componentInChildren = GetComponentInChildren<AltarSpirit>(includeInactive: true);
		componentInChildren.transform.parent = CommonReferences.Instance.GetManagerStages().GetStageCurrent().transform;
		componentInChildren.gameObject.SetActive(value: true);
		componentInChildren.Spawn();
		CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(m_audioReleaseSpirit);
		m_effectsActive.SetActive(value: false);
		m_effectsWorking.SetActive(value: false);
		StartCoroutine(CoroutineReleaseSpirit());
	}

	private IEnumerator CoroutineReleaseSpirit()
	{
		float l_weightFrom = 8f;
		float l_weightTo = m_lightGlobal.intensity;
		float l_timeToMove = 3f;
		float l_timeCurrent = 0f;
		while (l_timeCurrent < l_timeToMove)
		{
			l_timeCurrent += Time.fixedDeltaTime;
			float i_time = l_timeCurrent / l_timeToMove;
			float intensity = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Smooth, l_weightFrom, l_weightTo, i_time);
			m_lightGlobal.intensity = intensity;
			yield return new WaitForFixedUpdate();
		}
	}

	private void WorkAltar()
	{
		m_isAltarWorking = true;
		CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetManagerWave()
			.OnWaveStart += DeActivateAltar;
		CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetManagerWave()
			.OnWaveEnd += ActivateAltar;
		m_effectsWorking.SetActive(value: true);
		CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(m_audioWork);
	}

	private void ActivateAltar()
	{
		m_isAltarActive = true;
		m_effectsActive.SetActive(value: true);
	}

	private void DeActivateAltar()
	{
		m_isAltarActive = false;
		m_effectsActive.SetActive(value: false);
	}

	private void FetishPlacement()
	{
		PlayerController playerController = CommonReferences.Instance.GetPlayerController();
		List<PickUpable> list = new List<PickUpable>();
		foreach (PickUpable allPickUpable in playerController.GetInventory().GetAllPickUpables())
		{
			if (allPickUpable.GetName() == m_pickUpableFetish1.GetName())
			{
				list.Add(allPickUpable);
				m_fetish1.SetActive(value: true);
				CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(m_audioPlaceFetish);
			}
			else if (allPickUpable.GetName() == m_pickUpableFetish2.GetName())
			{
				list.Add(allPickUpable);
				m_fetish2.SetActive(value: true);
				CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(m_audioPlaceFetish);
			}
			else if (allPickUpable.GetName() == m_pickUpableFetish3.GetName())
			{
				list.Add(allPickUpable);
				m_fetish3.SetActive(value: true);
				CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(m_audioPlaceFetish);
			}
			else if (allPickUpable.GetName() == m_pickUpableFetish4.GetName())
			{
				list.Add(allPickUpable);
				m_fetish4.SetActive(value: true);
				CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(m_audioPlaceFetish);
			}
		}
		foreach (PickUpable item in list)
		{
			playerController.RemovePickupAbleFromInventory(item);
		}
		if (IsAllFetishesPresent() && !m_isAltarWorking)
		{
			WorkAltar();
		}
	}

	private bool IsAllFetishesPresent()
	{
		if (m_fetish1.activeSelf && m_fetish2.activeSelf && m_fetish3.activeSelf && m_fetish4.activeSelf)
		{
			return true;
		}
		return false;
	}
}
