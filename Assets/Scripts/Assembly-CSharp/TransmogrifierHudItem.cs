using UnityEngine;

public class TransmogrifierHudItem : MonoBehaviour
{
	private Player m_player;

	public void SetPlayer(Player l_player)
	{
		m_player = l_player;
	}

	public Player GetPlayer()
	{
		return m_player;
	}
}
