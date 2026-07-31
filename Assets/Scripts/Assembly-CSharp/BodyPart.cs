using System.Collections.Generic;
using UnityEngine;


public class BodyPart : MonoBehaviour
{
	protected Actor m_owner;

	protected virtual void Awake()
	{
		m_owner = GetComponentInParent<Actor>();
	}

	public virtual void Separate()
	{
		Bone componentInParent = GetComponentInParent<Bone>();
		foreach (Bone bonesChild in componentInParent.GetBonesChildren())
		{
			bonesChild.transform.parent = CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetActorsParent()
				.transform;
			if ((bool)bonesChild.GetHingeJoint2D())
			{
				bonesChild.GetHingeJoint2D().enabled = false;
			}
			if (bonesChild.GetBodyPart() != null && bonesChild.GetBodyPart().GetCollider() != null)
			{
				bonesChild.GetBodyPart().GetCollider().isTrigger = false;
			}
			bonesChild.name = "SEPERATED_" + bonesChild.name;
			CommonReferences.Instance.GetUtilityTools().DestroyObjectAfterTime(bonesChild.gameObject, 30f);
			ParticleSystem particleSystem = Object.Instantiate(ResourceContainer.Resources.m_particleBloodLeak, bonesChild.transform);
			particleSystem.transform.localPosition = Vector3.zero;
			particleSystem.gameObject.SetActive(value: true);
			ParticleSystem.MainModule main = particleSystem.main;
			main.startColor = m_owner.GetColorBlood();
			particleSystem.Clear(withChildren: true);
			particleSystem.Play();
			CommonReferences.Instance.GetUtilityTools().DestroyObjectAfterTime(particleSystem.gameObject, 15f);
		}
		if ((bool)componentInParent.GetBoneParent())
		{
			ParticleSystem particleSystem2 = Object.Instantiate(ResourceContainer.Resources.m_particleBloodLeak, componentInParent.GetBoneParent().transform);
			particleSystem2.transform.localPosition = Vector3.zero;
			particleSystem2.gameObject.SetActive(value: true);
			ParticleSystem.MainModule main2 = particleSystem2.main;
			main2.startColor = m_owner.GetColorBlood();
			particleSystem2.Clear(withChildren: true);
			particleSystem2.Play();
			CommonReferences.Instance.GetUtilityTools().DestroyObjectAfterTime(particleSystem2.gameObject, 15f);
		}
	}

	public virtual void Explode()
	{
		GetComponent<SpriteRenderer>().enabled = false;
		GetCollider().enabled = false;
		DisableAllChildElements();
		Bone componentInParent = GetComponentInParent<Bone>();
		ParticleSystem particleSystem = Object.Instantiate(ResourceContainer.Resources.m_particleBloodExplosion, componentInParent.transform);
		particleSystem.transform.localPosition = Vector3.zero;
		particleSystem.gameObject.SetActive(value: true);
		ParticleSystem.MainModule main = particleSystem.main;
		main.startColor = m_owner.GetColorBlood();
		particleSystem.Clear();
		particleSystem.Play();
		CommonReferences.Instance.GetUtilityTools().DestroyObjectAfterTime(particleSystem.gameObject, 10f);
		List<AudioClip> list = new List<AudioClip>();
		list.Add(Resources.Load<AudioClip>("Audio/Combat/BloodHit1"));
		list.Add(Resources.Load<AudioClip>("Audio/Combat/BloodHit2"));
		int index = Random.Range(0, 2);
		CommonReferences.Instance.GetManagerAudio().CreateAndAddAudioSourceSFX(base.gameObject).PlayOneShot(list[index]);
		Separate();
	}

	protected void DisableAllChildElements()
	{
		SpriteRenderer[] componentsInChildren = GetComponentsInChildren<SpriteRenderer>();
		foreach (SpriteRenderer spriteRenderer in componentsInChildren)
		{
			if (spriteRenderer.transform.parent == base.transform)
			{
				spriteRenderer.enabled = false;
			}
		}
		UnityEngine.Rendering.Universal.Light2D[] componentsInChildren2 = GetComponentsInChildren<UnityEngine.Rendering.Universal.Light2D>();
		foreach (UnityEngine.Rendering.Universal.Light2D light2D in componentsInChildren2)
		{
			if (light2D.transform.parent == base.transform)
			{
				light2D.enabled = false;
			}
		}
		ParticleSystem[] componentsInChildren3 = GetComponentsInChildren<ParticleSystem>();
		foreach (ParticleSystem particleSystem in componentsInChildren3)
		{
			if (particleSystem.transform.parent == base.transform)
			{
				particleSystem.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmitting);
			}
		}
	}

	public Actor GetOwner()
	{
		return m_owner;
	}

	public Collider2D GetCollider()
	{
		return GetComponent<Collider2D>();
	}

	public virtual void ApplySortOrder(int i_sortOrder)
	{
		GetComponent<SpriteRenderer>().sortingOrder = i_sortOrder;
	}
}
