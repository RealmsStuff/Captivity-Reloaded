using System.Collections;
using UnityEngine;

public class CHBitch : Challenge
{
	[SerializeField]
	private Interaction m_interactionKnot;

	protected override void HandleActivation()
	{
		ManagerDB.OnInteraction += OnInteraction;
	}

	protected override void HandleDeActivation()
	{
		ManagerDB.OnInteraction -= OnInteraction;
	}

	protected override void TrackCompletion()
	{
	}

	private void OnInteraction(int i_idInteraction, NPC i_npc)
	{
		if (!CommonReferences.Instance.GetPlayer().IsDead() && i_idInteraction == m_interactionKnot.GetId() && CommonReferences.Instance.GetPlayer().GetPleasureCurrent() > 0f)
		{
			StartCoroutine(CoroutineWaitBeforeCheckIfDiedAfterKnot());
		}
	}

	private IEnumerator CoroutineWaitBeforeCheckIfDiedAfterKnot()
	{
		yield return new WaitForSeconds(1f);
		if (CommonReferences.Instance.GetPlayer().GetPleasureCurrent() == 0f)
		{
			Complete();
		}
	}
}
