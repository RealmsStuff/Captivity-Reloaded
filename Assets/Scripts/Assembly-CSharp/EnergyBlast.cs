using UnityEngine;

public class EnergyBlast : Projectile
{
	[SerializeField]
	private float m_damage;

	private bool m_isDodged;

	protected override void HandleHit(Collider2D i_collider)
	{
		if (!i_collider.GetComponent<BodyPartPlayer>())
		{
			return;
		}
		Player player = CommonReferences.Instance.GetPlayer();
		if (!m_isDodged && player.GetIsInvulnerable() && player.GetStatePlayerCurrent() == StatePlayer.Dashing)
		{
			foreach (Challenge challenge in CommonReferences.Instance.GetManagerChallenge().GetAllChallenges())
			{
				CHSamus cHSamus = challenge as CHSamus;
				if (cHSamus != null && cHSamus.IsActive())
				{
					cHSamus.DodgeEnergyBlast((Sqoid)m_owner);
					m_isDodged = true;
				}
			}
		}
		if (!player.GetIsBeingRaped() && !player.GetIsInvulnerable() && player.GetStateActorCurrent() != StateActor.Ragdoll)
		{
			player.TakeDamage(m_damage);
			if (player.GetStateActorCurrent() != StateActor.Ragdoll)
			{
				player.Ragdoll(2f);
			}
			player.GetSkeleton().EnableElectrocute(0.7f, Color.green);
			Sqoid sqoid = (Sqoid)m_owner;
			CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(sqoid.GetAudioZap());
			CommonReferences.Instance.GetManagerPostProcessing().PlayEffectElectrocute();
			sqoid.GetAllInteractions()[0].Trigger(sqoid);
			Object.Destroy(base.gameObject);
		}
	}
}
