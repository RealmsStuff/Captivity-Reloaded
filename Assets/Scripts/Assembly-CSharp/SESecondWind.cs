using UnityEngine;

public class SESecondWind : StatusEffect
{
	private AudioClip m_audioKo;
	private float m_healthRestore;

	public SESecondWind(string i_name, string i_source, TypeStatusEffect i_type, float i_duration, bool i_isStackable, AudioClip i_audioKo, float i_healthRestore = 0f)
		: base(i_name, i_source, i_type, i_duration, i_isStackable)
	{
		m_audioKo = i_audioKo;
		m_healthRestore = i_healthRestore;
	}

	public override void Activate()
	{
		base.Activate();
		CommonReferences.Instance.GetPlayer().OnGetHit += OnGetHit;
	}

	private void OnGetHit()
	{
		if (CommonReferences.Instance.GetPlayer().GetStateActorCurrent() == StateActor.Ragdoll)
		{
			CommonReferences.Instance.GetPlayer().BecomeInvulnerable("Second Wind Invulnerability", "SecondWind", 5f, i_isAffectSkeleton: true);
			CommonReferences.Instance.GetPlayer().InterruptRagdoll();
			if (m_healthRestore > 0f) CommonReferences.Instance.GetPlayer().RestoreHealth(m_healthRestore);
			CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(m_audioKo);
			CommonReferences.Instance.GetManagerHud().GetManagerOverlay().PlayOverlayPopup(new Color(0.4f, 0.4f, 1f), i_isUseOverlayWithHole: true, i_isDestroyOverlayAfterAnimation: true, 0.25f, 0f, 0.5f, 1f, 0.5f, 0f, 0f);
			End();
		}
	}

	public override void End()
	{
		base.End();
		CommonReferences.Instance.GetPlayer().OnGetHit -= OnGetHit;
	}
}
