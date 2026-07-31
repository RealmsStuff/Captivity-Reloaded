using UnityEngine;

public class RaperMarksman : RaperSmasher
{
	[SerializeField]
	private AudioClip m_audioSlap;

	private void Slap()
	{
		m_player.GainLibido(3f);
		m_npc.PlayAudioSFX(m_audioSlap);
	}
}
