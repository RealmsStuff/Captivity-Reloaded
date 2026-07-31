using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AndroidStatue : MonoBehaviour
{
	[SerializeField]
	private List<AndroidStatuePose> m_poses = new List<AndroidStatuePose>();

	private AndroidStatuePose m_poseCurrent;

	private Coroutine m_coroutineCheckIsPlayerOutOfSight;

	private void Start()
	{
		CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetManagerWave()
			.OnWaveEnd += OnWaveEnd;
	}

	private void OnWaveEnd()
	{
		CheckForQueuedPose();
	}

	private void CheckForQueuedPose()
	{
		for (int i = 0; i < m_poses.Count; i++)
		{
			if (m_poses[i].GetNumWave() == CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetManagerWave()
				.GetNumWaveCurrent())
			{
				m_poseCurrent = m_poses[i];
				if (m_coroutineCheckIsPlayerOutOfSight != null)
				{
					StopCoroutine(m_coroutineCheckIsPlayerOutOfSight);
				}
				m_coroutineCheckIsPlayerOutOfSight = StartCoroutine(CoroutineCheckIsPlayerOutOfSight());
				break;
			}
		}
	}

	private IEnumerator CoroutineCheckIsPlayerOutOfSight()
	{
		Player l_player = CommonReferences.Instance.GetPlayer();
		while (m_poseCurrent != null)
		{
			if (Vector2.Distance(m_poseCurrent.GetPos(), l_player.GetPosHips()) > 20f && Vector2.Distance(base.transform.position, l_player.GetPosHips()) > 20f)
			{
				PerformQueuedPose();
			}
			yield return new WaitForSeconds(1f);
		}
		m_coroutineCheckIsPlayerOutOfSight = null;
	}

	private void PerformQueuedPose()
	{
		GetComponentInChildren<Animator>().Play(m_poseCurrent.GetNameStateAnimPose());
		base.transform.position = m_poseCurrent.GetPos();
		m_poseCurrent = null;
	}
}
