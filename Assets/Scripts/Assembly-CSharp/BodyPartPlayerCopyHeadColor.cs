using UnityEngine;

public class BodyPartPlayerCopyHeadColor : BodyPartPlayer
{
	private BodyPartPlayer m_head;

	private SpriteRenderer m_renderer;

	private SpriteRenderer m_rendererHead;

	private void Start()
	{
		m_rendererHead = CommonReferences.Instance.GetPlayer().GetSkeletonPlayer().GetBone(BoneTypePlayer.Head)
			.GetBodyPartPlayer()
			.GetComponent<SpriteRenderer>();
		m_renderer = GetComponentInChildren<SpriteRenderer>();
	}

	private void LateUpdate()
	{
		m_renderer.color = m_rendererHead.color;
	}
}
