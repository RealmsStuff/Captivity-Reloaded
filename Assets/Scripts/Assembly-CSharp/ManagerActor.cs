using System.Collections.Generic;
using UnityEngine;

public class ManagerActor : MonoBehaviour
{
	public delegate void DelOnPlayerChange();

	[SerializeField]
	private List<Player> m_players;

	private GameObject m_playerCurrent;

	public event DelOnPlayerChange OnPlayerChange;

	private void Awake()
	{
		m_players = Library.Instance.Actors.GetAllPlayers();
		DisableAllPlayers();
		m_playerCurrent = Object.Instantiate(m_players[0].gameObject, m_players[0].transform.parent);
		CommonReferences.Instance.GetPlayerController().SetPlayer(m_playerCurrent.GetComponent<Player>());
		CoreAssetSlotBinder.BindAfterSceneObjectsStarted();
	}

	private void DisableAllPlayers()
	{
		foreach (Player player in m_players)
		{
			player.gameObject.SetActive(value: false);
		}
	}

	private void ChoosePlayer(Player i_player)
	{
		Vector2 pos = m_playerCurrent.transform.position;
		m_playerCurrent = Object.Instantiate(i_player.gameObject, i_player.transform.parent);
		CommonReferences.Instance.GetPlayerController().SetPlayer(m_playerCurrent.GetComponent<Player>());
		CoreAssetSlotBinder.BindAfterSceneObjectsStarted();
		m_playerCurrent.GetComponent<Player>().SetPos(pos);
	}

	public void SelectPlayerTransmogrifier(Player i_player)
	{
		Player player = CommonReferences.Instance.GetPlayer();
		ChoosePlayer(i_player);
		MoveAllItemsToPlayer(player, CommonReferences.Instance.GetPlayer());
		player.EnableRagdoll();
		m_playerCurrent.SetActive(value: true);
		this.OnPlayerChange?.Invoke();
		CommonReferences.Instance.GetManagerCamerasXGame().GetCameraXGameCurrent().SetObjectFocused(m_playerCurrent);
	}

	private void MoveAllItemsToPlayer(Player i_playerToMoveFrom, Player i_playerToMoveTo)
	{
		Item[] componentsInChildren = i_playerToMoveFrom.GetComponentsInChildren<Item>(includeInactive: true);
		foreach (Item item in componentsInChildren)
		{
			item.transform.parent = i_playerToMoveTo.transform;
			item.gameObject.SetActive(value: false);
			if (item is PickUpable)
			{
				((PickUpable)item).SetOwner(i_playerToMoveTo);
			}
		}
	}

	public void RecreatePlayer()
	{
		Player i_player = null;
		foreach (Player allPlayer in Library.Instance.Actors.GetAllPlayers())
		{
			if (allPlayer.GetName() == CommonReferences.Instance.GetPlayer().GetName())
			{
				i_player = allPlayer;
				break;
			}
		}
		Player player = CommonReferences.Instance.GetPlayer();
		Object.Destroy(m_playerCurrent);
		ChoosePlayer(i_player);
		MoveAllItemsToPlayer(player, CommonReferences.Instance.GetPlayer());
		m_playerCurrent.SetActive(value: true);
		this.OnPlayerChange?.Invoke();
		CommonReferences.Instance.GetManagerCamerasXGame().GetCameraXGameCurrent().SetObjectFocused(m_playerCurrent);
	}
}
