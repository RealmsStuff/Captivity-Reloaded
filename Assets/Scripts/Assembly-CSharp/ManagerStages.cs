using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerStages : MonoBehaviour
{
	[SerializeField]
	private int m_numStageStart;

	[SerializeField]
	private Stage m_stageStart;

	private StageHub m_stageHub;

	private List<Stage> m_stages = new List<Stage>();

	private List<Stage> m_stagesDuplicants = new List<Stage>();

	private Stage m_stageCurrent;

	private int m_maxNumOfNpcsOfSameType = 100;

	private void Awake()
	{
		Stage[] componentsInChildren = GetComponentsInChildren<Stage>(includeInactive: true);
		foreach (Stage item in componentsInChildren)
		{
			m_stages.Add(item);
		}
		m_stageHub = GetComponentInChildren<StageHub>(includeInactive: true);
		DisableAllStages();
		m_stageCurrent = m_stageStart;
	}

	private void Start()
	{
		ManagerDB.AddStages(m_stages);
	}

	public void GoToWaypoint(Waypoint i_waypoint)
	{
		if (i_waypoint == null)
		{
			Debug.Log("Waypoint is null");
		}
		else
		{
			StartCoroutine(CoroutineGoToWaypoint(i_waypoint));
		}
	}

	public IEnumerator CoroutineGoToWaypoint(Waypoint i_waypoint)
	{
		Player l_player = CommonReferences.Instance.GetPlayer();
		l_player.StopMoving();
		if (i_waypoint.GetStage() != m_stageCurrent)
		{
			i_waypoint.DisableUntilPlayerExitsCollider();
			l_player.SetIsForceIgnoreInput(i_isForceIgnoreInput: true);
			yield return StartCoroutine(CoroutineAnimateFadeIn());
			CommonReferences.Instance.GetPlayer().StopMoving();
			l_player.SetPos(i_waypoint.GetPos());
			OpenStage(i_waypoint.GetStage());
			l_player.SetIsForceIgnoreInput(i_isForceIgnoreInput: false);
			yield return StartCoroutine(CoroutineAnimateFadeOut());
		}
		else
		{
			i_waypoint.DisableUntilPlayerExitsCollider();
			l_player.SetPos(i_waypoint.GetPos());
			CommonReferences.Instance.GetManagerCamerasXGame().GetCameraXGameCurrent().CenterCamera();
		}
	}

	public void OpenStage(Stage i_stageToOpen)
	{
		Player player = CommonReferences.Instance.GetPlayer();
		if (player.GetIsBeingRaped())
		{
			player.GetRaperCurrent().ForceEndRape();
		}
		CommonReferences.Instance.GetManagerChallenge().DeActivateAllChallenges();
		CommonReferences.Instance.GetManagerHud().GetManagerHealthDisplay().ClearHealthDisplayItems();
		CloseAllStages();
		if (m_stageCurrent.GetIsDuplicate())
		{
			Object.Destroy(m_stageCurrent.gameObject);
			m_stagesDuplicants.Remove(m_stageCurrent);
		}
		Stage stage = CopyStage(i_stageToOpen);
		stage.SetIsDuplicate(i_isDuplicate: true);
		stage.OpenStage();
		m_stageCurrent = stage;
		CommonReferences.Instance.GetManagerCamerasXGame().GetCameraXGameCurrent().CenterCamera();
		CommonReferences.Instance.GetManagerHud().ClosePauseMenu();
		CommonReferences.Instance.GetPlayerController().ResetInventory();
		CommonReferences.Instance.GetPlayer().PlaceFeetOnPos(i_stageToOpen.GetWaypointStart().GetPos());
		CommonReferences.Instance.GetPlayer().EnterStage();
		CommonReferences.Instance.GetPlayer().Spawn();
		CommonReferences.Instance.GetManagerHud().GetManagerOverlay().PlayOverlay(Color.black, i_isUseOverlayWithHole: false, i_isDestroyOverlayAfterAnimation: true, 1f, 1f, 0f);
		CommonReferences.Instance.GetManagerHud().GetManagerNotification().DestroyAllNotifications();
		CommonReferences.Instance.GetManagerChallenge().ActivateChallenges(stage.GetId());
		CommonReferences.Instance.GetManagerAudio().PlayAudioAmbience(m_stageCurrent.GetAudioAmbience());
	}

	public void OpenStage(int i_numStage)
	{
		OpenStage(m_stages[i_numStage]);
	}

	public void OpenStageStart()
	{
		OpenStage(m_stageStart);
	}

	private IEnumerator CoroutineAnimateFadeIn()
	{
		yield return CommonReferences.Instance.GetManagerHud().GetManagerOverlay().CoroutineAnimateBlackOverlay(i_isShow: true);
	}

	private IEnumerator CoroutineAnimateFadeOut()
	{
		yield return CommonReferences.Instance.GetManagerHud().GetManagerOverlay().CoroutineAnimateBlackOverlay(i_isShow: false);
	}

	private Stage CopyStage(Stage i_stage)
	{
		if (m_stagesDuplicants.Contains(i_stage))
		{
			return i_stage;
		}
		Stage stage = Object.Instantiate(i_stage, base.transform);
		m_stagesDuplicants.Add(stage);
		return stage;
	}

	private void CloseAllStages()
	{
		foreach (Stage stagesDuplicant in m_stagesDuplicants)
		{
			stagesDuplicant.CloseStage();
		}
		foreach (Stage stage in m_stages)
		{
			stage.CloseStage();
		}
	}

	private void DisableAllStages()
	{
		foreach (Stage stage in m_stages)
		{
			stage.CloseStage();
		}
	}

	public Stage GetStageCurrent()
	{
		return m_stageCurrent;
	}

	public StageHub GetStageHub()
	{
		return m_stageHub;
	}

	public Stage GetStageFromNumber(int i_numStage)
	{
		return m_stages[i_numStage--];
	}

	public List<Stage> GetAllStages()
	{
		return m_stages;
	}

	public int GetMaxNumberOfNpcsOfSameTypeOnStageAtGivenMoment()
	{
		return m_maxNumOfNpcsOfSameType;
	}
}
