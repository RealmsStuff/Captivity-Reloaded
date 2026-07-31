using System.Collections.Generic;

public class XAIOrc : XAIWalker
{
	private Orc m_orc;

	public override void Initialize(NPC i_npc)
	{
		base.Initialize(i_npc);
		m_orc = (Orc)i_npc;
		m_player.OnShoot += OnPlayerShoot;
		m_npc.OnDie += OnDie;
	}

	protected override void HandleStateChase()
	{
		base.HandleStateChase();
		HandleShield();
	}

	private void HandleShield()
	{
		if (m_orc.IsShieldRaised() && m_orc.GetIsPlayerBackTurnedToMe() && m_orc.IsCanLowerBackturnedPlayer())
		{
			m_orc.OnAttack -= OnOrcAttack;
			LowerShield();
		}
	}

	private void OnPlayerShoot(List<Bullet> i_bulletsShot)
	{
		if (m_xAIStateCurrent != XAIState.Chase || m_orc.GetIsAttacking())
		{
			return;
		}
		foreach (Bullet item in i_bulletsShot)
		{
			if (item.IsActorHit(m_npc))
			{
				RaiseShield();
				break;
			}
		}
	}

	private void RaiseShield()
	{
		m_orc.RaiseShield();
		m_player.OnShoot -= OnPlayerShoot;
		m_orc.OnAttack += OnOrcAttack;
	}

	private void OnOrcAttack()
	{
		m_orc.OnAttack -= OnOrcAttack;
		LowerShield();
	}

	private void LowerShield()
	{
		m_orc.LowerShield();
		m_player.OnShoot += OnPlayerShoot;
	}

	protected override void OnDie()
	{
		base.OnDie();
		if (!m_orc.IsShieldRaised())
		{
			m_player.OnShoot -= OnPlayerShoot;
		}
	}
}
