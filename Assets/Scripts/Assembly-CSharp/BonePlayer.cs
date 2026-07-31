using UnityEngine;

public class BonePlayer : Bone
{
	[SerializeField]
	private BoneTypePlayer m_bonePlayer;

	public BodyPartPlayer GetBodyPartPlayer()
	{
		return (BodyPartPlayer)GetBodyPart();
	}

	public BoneTypePlayer GetBoneType()
	{
		return m_bonePlayer;
	}
}
