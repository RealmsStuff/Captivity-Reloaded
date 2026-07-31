public class AndroidCrawler : Android
{
	protected override void AddXAIComponent()
	{
		m_xAI = base.gameObject.AddComponent<XAIAndroidCrawler>();
		m_xAI.Initialize(this);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		SetIsCanAttack(i_isCanAttack: false);
		m_miniumXSpeedForAnim /= 3f;
	}

	protected override void SetAllAnimatorBoolsToFalse()
	{
		m_animator.SetBool("IsMoving", value: false);
	}
}
