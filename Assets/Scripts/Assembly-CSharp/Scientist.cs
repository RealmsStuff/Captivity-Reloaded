using UnityEngine;

public class Scientist : Interactable
{
	[SerializeField]
	private Animator m_animatorSkeletonHeadHumper;

	[SerializeField]
	private AudioClip m_audioEmptySack;

	[SerializeField]
	private GameObject m_boneHead;

	private new void Start()
	{
		m_animatorSkeletonHeadHumper.Play("HeadHugging");
		GetComponentInChildren<AudioSource>().maxDistance = 42f;
	}

	protected override void HandleActivation(Actor i_initiator, InteractableActivationType i_activationType)
	{
		m_animatorSkeletonHeadHumper.gameObject.SetActive(value: false);
		HeadHumper component = Library.Instance.Actors.GetActorDupe("Head Humper").GetComponent<HeadHumper>();
		component.transform.SetParent(CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetActorsParent());
		component.SetPos(base.transform.position);
		component.gameObject.SetActive(value: true);
		component.Spawn(i_isFadeIn: false);
		GetComponent<Animator>().Play("Dead");
	}

	private void AnimEventEmptySack()
	{
		GetComponentInChildren<AudioSource>().PlayOneShot(m_audioEmptySack);
		HeadHumper component = Library.Instance.Actors.GetActor("Head Humper").GetComponent<HeadHumper>();
		ParticleSystem particleSystem = CommonReferences.Instance.GetUtilityTools().CreateParticleDuplicateAndPlay(component.GetParticleEmptySack(), m_boneHead.transform);
		particleSystem.transform.localPosition = Vector3.zero;
		particleSystem.transform.localEulerAngles = Vector3.zero;
		CommonReferences.Instance.GetUtilityTools().DestroyObjectAfterTime(particleSystem.gameObject, particleSystem.main.duration + 5f);
	}
}
