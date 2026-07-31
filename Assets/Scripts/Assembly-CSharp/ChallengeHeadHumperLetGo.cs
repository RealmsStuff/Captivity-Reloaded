public class ChallengeHeadHumperLetGo : Challenge
{
	private Player m_player;

	private HeadHumper m_headHumperHeadHumping;

	protected override void HandleActivation()
	{
		m_player = CommonReferences.Instance.GetPlayer();
		m_player.OnBeingRaped += OnPlayerBeingRaped;
	}

	protected override void TrackCompletion()
	{
	}

	protected override void HandleDeActivation()
	{
		m_player.OnBeingRaped -= OnPlayerBeingRaped;
		m_headHumperHeadHumping = null;
	}

	private void OnPlayerBeingRaped()
	{
		HeadHumper component = Library.Instance.Actors.GetActor("Head Humper").GetComponent<HeadHumper>();
		if (m_player.GetRaperCurrent().GetNPC().GetId() == component.GetId())
		{
			RaperGame obj = (RaperGame)m_player.GetRaperCurrent();
			obj.OnWin += OnHeadHumperRapeWin;
			obj.OnLose += OnHeadHumperRapeLose;
		}
	}

	private void OnHeadHumperRapeWin()
	{
		RaperGame obj = (RaperGame)m_player.GetRaperCurrent();
		obj.OnWin -= OnHeadHumperRapeWin;
		obj.OnLose -= OnHeadHumperRapeLose;
		m_headHumperHeadHumping = (HeadHumper)m_player.GetRaperCurrent().GetNPC();
		m_player.GetRaperCurrent().GetNPC().OnIgnite += OnHeadHumperIgnite;
	}

	private void OnHeadHumperRapeLose()
	{
		RaperGame obj = (RaperGame)m_player.GetRaperCurrent();
		obj.OnWin -= OnHeadHumperRapeWin;
		obj.OnLose -= OnHeadHumperRapeLose;
	}

	private void OnHeadHumperIgnite()
	{
		if (m_headHumperHeadHumping.IsHeadHugging() && m_headHumperHeadHumping.IsOnFire())
		{
			m_headHumperHeadHumping.OnIgnite -= OnHeadHumperIgnite;
			Complete();
		}
	}
}
