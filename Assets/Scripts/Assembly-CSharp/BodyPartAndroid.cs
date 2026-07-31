using System.Collections.Generic;
using UnityEngine;

public class BodyPartAndroid : BodyPartActor
{
	public override void Separate()
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
			ParticleSystem particleSystem = Object.Instantiate(ResourceContainer.Resources.m_particleAndroidExplosionElectricityLeak, bonesChild.transform);
			particleSystem.transform.localPosition = Vector3.zero;
			particleSystem.gameObject.SetActive(value: true);
			particleSystem.Clear(withChildren: true);
			particleSystem.Play();
			CommonReferences.Instance.GetUtilityTools().DestroyObjectAfterTime(particleSystem.gameObject, 15f);
		}
		if ((bool)componentInParent.GetBoneParent())
		{
			ParticleSystem particleSystem2 = Object.Instantiate(ResourceContainer.Resources.m_particleAndroidExplosionElectricityLeak, componentInParent.GetBoneParent().transform);
			particleSystem2.transform.localPosition = Vector3.zero;
			particleSystem2.gameObject.SetActive(value: true);
			particleSystem2.Clear(withChildren: true);
			particleSystem2.Play();
			CommonReferences.Instance.GetUtilityTools().DestroyObjectAfterTime(particleSystem2.gameObject, 15f);
		}
	}

	public override void Explode()
	{
		GetComponent<SpriteRenderer>().enabled = false;
		GetCollider().enabled = false;
		DisableAllChildElements();
		Bone componentInParent = GetComponentInParent<Bone>();
		ParticleSystem[] array = new ParticleSystem[3]
		{
			Object.Instantiate(ResourceContainer.Resources.m_particleAndroidExplosion, componentInParent.transform),
			Object.Instantiate(ResourceContainer.Resources.m_particleAndroidExplosionSmoke, componentInParent.transform),
			Object.Instantiate(ResourceContainer.Resources.m_particleAndroidExplosionElectricity, componentInParent.transform)
		};
		array[0].transform.localPosition = Vector3.zero;
		array[1].transform.localPosition = Vector3.zero;
		array[2].transform.localPosition = Vector3.zero;
		array[0].gameObject.SetActive(value: true);
		array[1].gameObject.SetActive(value: true);
		array[2].gameObject.SetActive(value: true);
		array[0].Clear();
		array[1].Clear();
		array[2].Clear();
		array[0].Play();
		array[1].Play();
		array[2].Play();
		CommonReferences.Instance.GetUtilityTools().DestroyObjectAfterTime(array[0].gameObject, 10f);
		CommonReferences.Instance.GetUtilityTools().DestroyObjectAfterTime(array[1].gameObject, 10f);
		CommonReferences.Instance.GetUtilityTools().DestroyObjectAfterTime(array[2].gameObject, 10f);
		List<AudioClip> list = new List<AudioClip>();
		list.Add(Resources.Load<AudioClip>("Audio/AndroidExplosion1"));
		list.Add(Resources.Load<AudioClip>("Audio/AndroidExplosion2"));
		list.Add(Resources.Load<AudioClip>("Audio/AndroidExplosion3"));
		list.Add(Resources.Load<AudioClip>("Audio/AndroidExplosion4"));
		int index = Random.Range(0, list.Count);
		CommonReferences.Instance.GetManagerAudio().CreateAndAddAudioSourceSFX(base.gameObject).PlayOneShot(list[index]);
		Separate();
	}
}
