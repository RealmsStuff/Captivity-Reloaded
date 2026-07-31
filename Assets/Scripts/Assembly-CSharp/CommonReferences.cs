using UnityEngine;

public class CommonReferences : MonoBehaviour
{
	[SerializeField]
	private Bullet m_bullet;

	[SerializeField]
	private ManagerCamerasXGame m_managerCamerasXGame;

	[SerializeField]
	private ManagerHud m_managerHud;

	[SerializeField]
	private ManagerDefeatScene m_managerDefeatScene;

	[SerializeField]
	private ManagerAudio m_managerAudio;

	[SerializeField]
	private ManagerStages m_managerStages;

	[SerializeField]
	private PlayerController m_playerController;

	[SerializeField]
	private ManagerActor m_managerActor;

	[SerializeField]
	private ManagerScreens m_managerScreens;

	[SerializeField]
	private ManagerPostProcessing m_managerPostProcessing;

	[SerializeField]
	private ManagerChallenge m_managerChallenge;

	[SerializeField]
	private ManagerInput m_managerInput;

	[SerializeField]
	private UtilityTools m_utilityTools;

	private static CommonReferences l_instance;

	public static CommonReferences Instance
	{
		get
		{
			if (l_instance == null)
			{
				l_instance = Object.FindObjectOfType<CommonReferences>();
				if (l_instance == null)
				{
					l_instance = new GameObject("CommonReferences").AddComponent<CommonReferences>();
				}
			}
			return l_instance;
		}
	}

	public ManagerCamerasXGame GetManagerCamerasXGame()
	{
		return m_managerCamerasXGame;
	}

	public Bullet GetBulletDefault()
	{
		return m_bullet;
	}

	public ManagerHud GetManagerHud()
	{
		return m_managerHud;
	}

	public ManagerDefeatScene GetManagerDefeatScene()
	{
		return m_managerDefeatScene;
	}

	public ManagerAudio GetManagerAudio()
	{
		return m_managerAudio;
	}

	public PlayerController GetPlayerController()
	{
		return m_playerController;
	}

	public Player GetPlayer()
	{
		return m_playerController.GetPlayer();
	}

	public ManagerActor GetManagerActor()
	{
		return m_managerActor;
	}

	public ManagerStages GetManagerStages()
	{
		return m_managerStages;
	}

	public ManagerScreens GetManagerScreens()
	{
		return m_managerScreens;
	}

	public UtilityTools GetUtilityTools()
	{
		return m_utilityTools;
	}

	public ManagerPostProcessing GetManagerPostProcessing()
	{
		return m_managerPostProcessing;
	}

	public ManagerChallenge GetManagerChallenge()
	{
		return m_managerChallenge;
	}

	public ManagerInput GetManagerInput()
	{
		return m_managerInput;
	}
}
