using UnityEngine;

public class RaperZombie : RaperSmasher
{
	[Header("--- Zombie ---")]
	[SerializeField]
	private AudioClip m_audioChew;

	private void Chew()
	{
		m_player.TakeDamage(1f);
		CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(m_audioChew);
		m_npc.GetAllInteractions()[0].Trigger(m_npc);
	}
}
