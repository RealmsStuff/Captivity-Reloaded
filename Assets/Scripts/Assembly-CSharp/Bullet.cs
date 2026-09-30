using System.Collections.Generic;
using UnityEngine;

public class Bullet
{
	private Gun m_gun;

	private Actor m_owner;

	private float m_damage;

	private float m_knockbackX;

	private float m_knockbackY;

	private int m_penetration;

	private List<AudioClip> m_audiosHit = new List<AudioClip>();

	private List<Actor> m_actorsChecked = new List<Actor>();

	private List<Actor> m_actorsHit = new List<Actor>();

	private List<Interactable> m_interactablesHit = new List<Interactable>();

	private int m_aliveActorsHit;

	private Vector2 m_direction;

	private RaycastHit2D[] m_hits;
	private bool m_showLine;

	public Bullet(Gun i_gun, Actor i_owner, Vector2 i_direction, RaycastHit2D[] i_hits, float i_damage, float i_knockbackX, float i_knockbackY, int i_penetration, bool i_showLine = true)
	{
		m_gun = i_gun;
		m_owner = i_owner;
		m_direction = i_direction;
		m_hits = i_hits;
		m_damage = i_damage;
		m_knockbackX = i_knockbackX;
		m_knockbackY = i_knockbackY;
		m_penetration = i_penetration;
		m_showLine = i_showLine;
		m_aliveActorsHit = 0;
	}

	public void HandleHits()
	{
		Vector2 i_posEnd = Vector2.zero;
		RaycastHit2D[] hits = m_hits;
		for (int i = 0; i < hits.Length; i++)
		{
			RaycastHit2D raycastHit2D = hits[i];
			if ((bool)raycastHit2D.collider.GetComponent<BodyPartActor>())
			{
				BodyPartActor component = raycastHit2D.collider.GetComponent<BodyPartActor>();
				if (m_actorsChecked.Contains(component.GetOwner()))
				{
					continue;
				}
				m_actorsChecked.Add(component.GetOwner());
				bool flag = component.GetOwner().IsDead();
				if (HitBodyPart(component, raycastHit2D.point) && !flag)
				{
					i_posEnd = raycastHit2D.point;
				}
				m_gun.HandleActorHit(component, this);
				if (GetNumOfActorsAliveHit() == m_penetration + 1)
				{
					break;
				}
			}
			if ((bool)raycastHit2D.collider.GetComponent<Interactable>())
			{
				Interactable component2 = raycastHit2D.collider.GetComponent<Interactable>();
				HitInteractable(component2);
				if (component2.IsObstructionPaths())
				{
					i_posEnd = raycastHit2D.point;
					break;
				}
			}
			else if ((bool)raycastHit2D.collider.GetComponentInParent<Interactable>())
			{
				Interactable componentInParent = raycastHit2D.collider.GetComponentInParent<Interactable>();
				HitInteractable(componentInParent);
				if (componentInParent.IsObstructionPaths())
				{
					i_posEnd = raycastHit2D.point;
					break;
				}
			}
			if ((bool)raycastHit2D.collider.GetComponent<Platform>() || raycastHit2D.collider.gameObject.layer == LayerMask.NameToLayer("Platform"))
			{
				i_posEnd = raycastHit2D.point;
				break;
			}
			if (GetNumOfActorsAliveHit() == m_penetration + 1)
			{
				i_posEnd = Vector2.zero;
				break;
			}
		}
		if (m_showLine) m_gun.LineShoot(m_direction, i_posEnd);
	}

	public bool HitBodyPartDirect(BodyPartActor i_bodyPart, Vector2 i_pointHit)
	{
		return HitBodyPart(i_bodyPart, i_pointHit);
	}

	private bool HitBodyPart(BodyPartActor i_bodyPart, Vector2 i_pointHit)
	{
		Actor owner = i_bodyPart.GetOwner();
		if (owner == m_owner)
		{
			return false;
		}
		if (i_bodyPart.GetIsBodyPartDestroyed())
		{
			return false;
		}
		if (m_actorsHit.Contains(owner))
		{
			return false;
		}
		if (owner.IsDead())
		{
			bool num = i_bodyPart.TakeHitProjectile(this);
			if (num)
			{
				m_actorsHit.Add(owner);
			}
			return num;
		}
		float originalDamage = m_damage;
		if (m_gun != null) m_damage *= m_gun.GetAmmunitionWeakpointMultiplier(i_bodyPart);
		bool accepted = i_bodyPart.TakeHitProjectile(this);
		m_damage = originalDamage;
		if (!accepted)
		{
			return false;
		}
		m_actorsHit.Add(owner);
		m_aliveActorsHit++;
		m_damage *= 0.75f;
		CommonReferences.Instance.GetManagerHud().CreateDamageNumber(i_bodyPart.GetDamageMultiplierAmountBodyPart(), i_pointHit);
		CommonReferences.Instance.GetManagerAudio().PlayAudioHitsound(i_bodyPart.GetDamageMultiplierAmountBodyPart(), i_isKill: false);
		return true;
	}

	public void HitInteractable(Interactable i_interactable)
	{
		if (!m_interactablesHit.Contains(i_interactable) && i_interactable.GetIsCanBeShotToActivate())
		{
			i_interactable.Activate(m_owner, InteractableActivationType.Shot);
			m_interactablesHit.Add(i_interactable);
		}
	}

	public float GetDamage()
	{
		return m_damage;
	}

	public void SetDamage(float i_damage)
	{
		m_damage = i_damage;
	}

	public float GetKockbackX()
	{
		return m_knockbackX;
	}

	public float GetKockbackY()
	{
		return m_knockbackY;
	}

	public Actor GetOwner()
	{
		return m_owner;
	}

	public int GetNumOfActorsAliveHit()
	{
		return m_aliveActorsHit;
	}

	public List<Actor> GetActorsHit()
	{
		return m_actorsHit;
	}

	public bool IsActorHit(Actor i_actorToCheck)
	{
		if (m_actorsHit.Contains(i_actorToCheck))
		{
			return true;
		}
		return false;
	}
}
