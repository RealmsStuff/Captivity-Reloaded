public class Musca : Flier
{
	private bool m_isCarryingPlayerToNest;

	protected override void AddXAIComponent()
	{
		m_xAI = base.gameObject.AddComponent<XAIMusca>();
		m_xAI.Initialize(this);
	}

	public void CarryPlayer()
	{
		m_isCarryingPlayerToNest = true;
		m_animator.Play("Carry");
		SetIsThinking(i_isThinking: true);
		SetState(StateActor.Idle);
		SetIsInvulnerable(i_isInvulnerable: false, i_isAffectSkeleton: true);
		m_isCanMove = true;
	}

	public void DropPlayer()
	{
		m_isCarryingPlayerToNest = false;
		m_animator.Play("Idle");
		((RaperMusca)GetRaper()).StopCarrying();
		m_interactions[0].Trigger(this);
	}

	public bool IsCarryingPlayerToNest()
	{
		return m_isCarryingPlayerToNest;
	}
}
