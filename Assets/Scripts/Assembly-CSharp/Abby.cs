using UnityEngine;

public class Abby : Android
{
	[SerializeField]
	private AudioClip m_audioAbbySong;

	private AudioSource m_audioSourceAbbySong;

	protected override void AddXAIComponent()
	{
		m_xAI = base.gameObject.AddComponent<XAIAbby>();
		m_xAI.Initialize(this);
	}

	public override void Start()
	{
		base.Start();
		AddAbbySong();
	}

	private void AddAbbySong()
	{
		m_audioSourceAbbySong = CommonReferences.Instance.GetManagerAudio().CreateAndAddAudioSourceSFX(base.gameObject);
		m_audioSourceAbbySong.loop = true;
		m_audioSourceAbbySong.clip = m_audioAbbySong;
		m_audioSourceAbbySong.volume /= 6f;
		m_audioSourceAbbySong.Play();
	}

	public void RunAway()
	{
		if (!CommonReferences.Instance.GetPlayer().IsDead() && CommonReferences.Instance.GetPlayer().GetStateActorCurrent() != StateActor.Ragdoll)
		{
			((XAIAbby)m_xAI).RunToRandomDestination();
		}
	}

	public override void Die()
	{
		base.Die();
		m_audioSourceAbbySong.Stop();
	}
}
