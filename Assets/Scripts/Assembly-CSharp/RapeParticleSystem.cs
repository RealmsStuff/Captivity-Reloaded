using UnityEngine;

public class RapeParticleSystem : MonoBehaviour
{
	[SerializeField]
	private BoneTypePlayer m_bonePlayerToParentTo;

	[Header("Or...")]
	[SerializeField]
	private Bone m_boneRaperToParentTo;

	public ParticleSystem GetParticleSystem()
	{
		return GetComponent<ParticleSystem>();
	}

	public Transform GetParentBone()
	{
		if (m_boneRaperToParentTo != null)
		{
			return m_boneRaperToParentTo.transform;
		}
		return CommonReferences.Instance.GetPlayer().GetSkeletonPlayer().GetBone(m_bonePlayerToParentTo)
			.transform;
	}
}
