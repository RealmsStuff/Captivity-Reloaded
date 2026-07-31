using System;
using UnityEngine;

public class TriggerSecret : Trigger
{
	[SerializeField]
	private AudioSource m_audioSecretRoom;

	[SerializeField]
	private NPC m_betsy;

	protected override void HandleTriggerEnter()
	{
		CommonReferences.Instance.GetPlayer().GetSkeletonPlayer().RemoveAllClothing();
		UnityEngine.Object.Instantiate(CommonReferences.Instance.GetPlayer().GetSkeletonPlayer().GetBone(BoneTypePlayer.Head), CommonReferences.Instance.GetPlayer().GetSkeletonPlayer().GetBone(BoneTypePlayer.Head)
			.transform.parent).GetBodyPart().Explode();
		CommonReferences.Instance.GetPlayer().GetSkeletonPlayer().GetBone(BoneTypePlayer.Head)
			.GetBodyPart()
			.gameObject.SetActive(value: false);
		CommonReferences.Instance.GetPlayer().GetSkeletonPlayer().GetBone(BoneTypePlayer.Ear)
			.GetBodyPart()
			.Explode();
		CommonReferences.Instance.GetPlayer().GetSkeletonPlayer().GetManagerFacePlayer()
			.gameObject.SetActive(value: false);
		CommonReferences.Instance.GetPlayer().Die();
		CommonReferences.Instance.GetPlayer().GetAnimator().runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Raper/RapeAnimator");
		CommonReferences.Instance.GetPlayer().GetAnimator().Play("Sqoid7");
		CommonReferences.Instance.GetPlayerController().SetIsForceIgnoreInput(i_isForceIgnoreInput: true);
		CommonReferences.Instance.GetManagerHud().HideHud();
		m_audioSecretRoom.Play();
		m_betsy.gameObject.SetActive(value: true);
	}

	protected override void HandleTriggerExit()
	{
		throw new NotImplementedException();
	}
}
