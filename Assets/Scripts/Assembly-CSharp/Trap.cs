using System.Collections;
using UnityEngine;

public class Trap : Interactable
{
	[SerializeField]
	private Sprite m_sprTrapOpen;

	[SerializeField]
	private Sprite m_sprTrapClosed;

	[SerializeField]
	private Collider2D m_colOpen;

	[SerializeField]
	private Collider2D m_colClosed;

	[SerializeField]
	private AudioClip m_audioSetTrap;

	[SerializeField]
	private AudioClip m_audioTriggerHit;

	[SerializeField]
	private AudioClip m_audioTriggerMiss;

	private bool m_isSet;

	private bool m_isActive;

	private Player m_player;

	public void SetTrap()
	{
		GetComponent<SpriteRenderer>().sprite = m_sprTrapOpen;
		m_colOpen.enabled = true;
		m_colClosed.enabled = false;
		m_isSet = true;
		m_player = CommonReferences.Instance.GetPlayer();
		m_audioSourceSFX.PlayOneShot(m_audioSetTrap);
		StartCoroutine(CoroutineWaitBeforeActive());
	}

	private IEnumerator CoroutineWaitBeforeActive()
	{
		yield return new WaitForSeconds(0.5f);
		m_isActive = true;
	}

	protected override void HandleActivation(Actor i_initiator, InteractableActivationType i_activationType)
	{
		if (m_isSet && m_isActive)
		{
			if (i_activationType == InteractableActivationType.Touch)
			{
				TriggerTrap(i_isHitPlayer: true);
			}
			else
			{
				TriggerTrap(i_isHitPlayer: false);
			}
		}
	}

	private void TriggerTrap(bool i_isHitPlayer)
	{
		m_isSet = false;
		GetComponent<SpriteRenderer>().sprite = m_sprTrapClosed;
		m_colOpen.enabled = false;
		m_colClosed.enabled = true;
		Object.Destroy(base.gameObject, 8f);
		if (!i_isHitPlayer)
		{
			m_audioSourceSFX.PlayOneShot(m_audioTriggerMiss);
			Vector2 force = new Vector2(Random.Range(-8f, 8f), Random.Range(3f, 10f));
			GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
			GetComponent<Rigidbody2D>().AddTorque(Random.Range(-100f, 100f));
			base.gameObject.layer = LayerMask.NameToLayer("Item");
			m_colClosed.gameObject.layer = LayerMask.NameToLayer("Item");
			m_colOpen.gameObject.layer = LayerMask.NameToLayer("Item");
			return;
		}
		m_audioSourceSFX.PlayOneShot(m_audioTriggerHit);
		m_player.TakeDamage(15f);
		CommonReferences.Instance.GetManagerHud().GetStatusPlayerHud().CreateAndAddStatus("Bear Trap", "You stepped into a bear trap and got stuck!", StatusPlayerHudItemColor.Combat, 4.5f);
		AttachToPlayer();
		StartCoroutine(CoroutineUnfreeze());
		m_colOpen.enabled = false;
		m_colClosed.enabled = false;
		if (m_player.GetStateActorCurrent() != StateActor.Ragdoll && !m_player.GetIsBeingRaped())
		{
			CommonReferences.Instance.GetPlayer().Ragdoll(2.5f);
		}
		else
		{
			base.gameObject.layer = LayerMask.NameToLayer("Item");
			m_colClosed.gameObject.layer = LayerMask.NameToLayer("Item");
			m_colOpen.gameObject.layer = LayerMask.NameToLayer("Item");
		}
		Library.Instance.Actors.GetNpc("Goblin Trapper").GetAllInteractions()[0].Trigger(Library.Instance.Actors.GetNpc("Goblin Trapper"));
	}

	private void AttachToPlayer()
	{
		PointLegTowardsTrap();
		base.transform.parent = m_player.GetSkeletonPlayer().GetBone(BoneTypePlayer.rFoot).transform;
		GetComponent<SpriteRenderer>().sortingOrder = m_player.GetSkeletonPlayer().GetBone(BoneTypePlayer.rFoot).GetBodyPart()
			.GetComponent<SpriteRenderer>()
			.sortingOrder + 1;
		m_player.GetSkeletonPlayer().GetBone(BoneTypePlayer.rFoot).GetRigidbody2D()
			.constraints = RigidbodyConstraints2D.FreezePosition;
		GetComponent<Rigidbody2D>().isKinematic = true;
	}

	private void PointLegTowardsTrap()
	{
		SkeletonPlayer skeletonPlayer = m_player.GetSkeletonPlayer();
		Vector2 vector = base.transform.position - skeletonPlayer.GetBone(BoneTypePlayer.rLegUpper).transform.position;
		float num = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
		num += 90f;
		skeletonPlayer.GetBone(BoneTypePlayer.rLegUpper).transform.rotation = Quaternion.AngleAxis(num, Vector3.forward);
		skeletonPlayer.GetBone(BoneTypePlayer.rLegLower).transform.localEulerAngles = Vector3.zero;
		skeletonPlayer.GetBone(BoneTypePlayer.rFoot).transform.localEulerAngles = Vector3.zero;
	}

	private IEnumerator CoroutineUnfreeze()
	{
		yield return new WaitForSeconds(2.25f);
		m_player.GetSkeletonPlayer().GetBone(BoneTypePlayer.rFoot).GetRigidbody2D()
			.constraints = RigidbodyConstraints2D.FreezeRotation;
	}
}
