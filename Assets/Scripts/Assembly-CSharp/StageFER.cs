using System.Collections.Generic;
using UnityEngine;

public class StageFER : Stage
{
	private bool m_useBuiltInLabSequence = true;

	public void SetUseBuiltInLabSequence(bool i_useBuiltInLabSequence)
	{
		m_useBuiltInLabSequence = i_useBuiltInLabSequence;
	}

	[SerializeField]
	private List<GameObject> m_objectsToActivateInLab = new List<GameObject>();

	[SerializeField]
	private List<Interactable> m_interactablesToEnableInLab = new List<Interactable>();

	[SerializeField]
	private AudioClip m_audioActivateLab;

	private List<FuseBox> m_fuseBoxesToActivate = new List<FuseBox>();

	private void Start()
	{
		FuseBox[] componentsInChildren = GetComponentsInChildren<FuseBox>();
		foreach (FuseBox fuseBox in componentsInChildren)
		{
			fuseBox.OnActivate += OnFuseBoxActivate;
			m_fuseBoxesToActivate.Add(fuseBox);
		}
	}

	private void OnFuseBoxActivate(Interactable i_interactable)
	{
		FuseBox item = (FuseBox)i_interactable;
		m_fuseBoxesToActivate.Remove(item);
		if (m_fuseBoxesToActivate.Count == 0)
		{
			TurnOnLab();
		}
	}

	private void TurnOnLab()
	{
		if (!m_useBuiltInLabSequence) return;
		foreach (GameObject item in m_objectsToActivateInLab)
		{
			item.SetActive(value: true);
		}
		foreach (Interactable item2 in m_interactablesToEnableInLab)
		{
			item2.SetIsUnInteractable(i_isUnInteractable: false);
		}
		CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(m_audioActivateLab);
	}
}
