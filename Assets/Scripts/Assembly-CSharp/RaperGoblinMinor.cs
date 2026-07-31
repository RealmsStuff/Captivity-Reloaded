using UnityEngine;

public class RaperGoblinMinor : RaperSmasher
{
	[SerializeField]
	private AudioClip m_audioPunchBackOfHead;

	[SerializeField]
	private AudioClip m_audioHitHeadStick;

	[SerializeField]
	private AudioClip m_audioSlapAss;

	public override void BeginRape()
	{
		base.BeginRape();
		GetComponentInParent<GoblinMinor>().GetStick().GetComponent<SpriteRenderer>().sortingLayerName = "Player";
		GetComponentInParent<GoblinMinor>().GetSyringe().GetComponent<SpriteRenderer>().sortingLayerName = "Player";
		base.OnEndRape += FixStickAndSyringe;
	}

	private void PunchBackOfHead()
	{
		m_player.LoseLibido(2f);
		m_player.PlayAudioSFX(m_audioPunchBackOfHead);
		CommonReferences.Instance.GetManagerHud().GetStatusPlayerHud().IsStatusPlayerHudItemExists("Backhead Punch");
	}

	private void StickHitHead()
	{
		m_player.LoseLibido(2f);
		m_player.PlayAudioSFX(m_audioHitHeadStick);
		CommonReferences.Instance.GetManagerHud().GetStatusPlayerHud().IsStatusPlayerHudItemExists("Stick Blow");
	}

	private void AssSlap()
	{
		m_player.GainLibido(1f);
		m_player.PlayAudioSFX(m_audioSlapAss);
		CommonReferences.Instance.GetManagerHud().GetStatusPlayerHud().IsStatusPlayerHudItemExists("Spank");
	}

	private void AssStickHit()
	{
		m_player.GainLibido(4f);
		m_player.PlayAudioSFX(m_audioSlapAss);
	}

	private void InsertStick()
	{
		GetComponentInParent<GoblinMinor>().GetStick().transform.parent = CommonReferences.Instance.GetPlayer().GetSkeletonPlayer().GetBone(BoneTypePlayer.Hips)
			.transform;
	}

	private void StickNeedle()
	{
		GetComponentInParent<GoblinMinor>().StickNeedle();
	}

	private void GrabStick()
	{
		GetComponentInParent<GoblinMinor>().GetStick().transform.parent = GetComponentInParent<GoblinMinor>().GetSkeletonActor().GetBone("rHand").transform;
		GetComponentInParent<GoblinMinor>().GetStick().transform.localPosition = new Vector3(0f, -0.055f, 0f);
	}

	private void FixStickAndSyringe()
	{
		base.OnEndRape -= FixStickAndSyringe;
		GetComponentInParent<GoblinMinor>().GetStick().GetComponent<SpriteRenderer>().sortingLayerName = "Actor";
		GetComponentInParent<GoblinMinor>().GetSyringe().GetComponent<SpriteRenderer>().sortingLayerName = "Actor";
		GetComponentInParent<GoblinMinor>().GetStick().transform.localEulerAngles = Vector3.zero;
		GetComponentInParent<GoblinMinor>().GetStick().transform.localScale = Vector3.one;
	}
}
