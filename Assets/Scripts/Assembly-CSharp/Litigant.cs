public class Litigant : Walker
{
	protected override void AddXAIComponent()
	{
		m_xAI = base.gameObject.AddComponent<XAILitigant>();
		m_xAI.Initialize(this);
	}
}
