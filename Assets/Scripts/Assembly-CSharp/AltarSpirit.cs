using System.Collections;
using UnityEngine;

public class AltarSpirit : MonoBehaviour
{
	[SerializeField]
	private AudioClip m_audioImpregnation;

	private bool m_isCaughtPlayer;

	public void Spawn()
	{
		StartCoroutine(CoroutineSpawn());
	}

	private IEnumerator CoroutineSpawn()
	{
		_ = base.transform.position;
		float l_distanceYToMove = 4.5f;
		float l_distanceYMoved = 0f;
		while (l_distanceYMoved < l_distanceYToMove)
		{
			base.transform.position = new Vector3(base.transform.position.x, base.transform.position.y + 0.1f, base.transform.position.z);
			l_distanceYMoved += 0.1f;
			yield return new WaitForFixedUpdate();
		}
		yield return new WaitForSeconds(3f);
		StartChasingPlayerHips();
	}

	private void StartChasingPlayerHips()
	{
		StartCoroutine(CoroutineChasePlayerHips());
	}

	private IEnumerator CoroutineChasePlayerHips()
	{
		while (!m_isCaughtPlayer)
		{
			ChasePlayerHips();
			CheckCloseEnoughToImpregnate();
			yield return new WaitForFixedUpdate();
		}
	}

	private void ChasePlayerHips()
	{
		float num = 25f;
		Player player = CommonReferences.Instance.GetPlayer();
		Vector3.Distance(base.transform.position, player.GetPosHips());
		float num2 = player.GetPosHips().x - base.transform.position.x;
		float num3 = player.GetPosHips().y - base.transform.position.y;
		Vector3 vector = player.GetPosHips();
		Vector3 position = base.transform.position;
		if (position.x > vector.x)
		{
			position.x += num2 / num;
		}
		if (position.x < vector.x)
		{
			position.x += num2 / num;
		}
		if (position.y > vector.y)
		{
			position.y += num3 / num;
		}
		if (position.y < vector.y)
		{
			position.y += num3 / num;
		}
		base.transform.position = position;
	}

	private void CheckCloseEnoughToImpregnate()
	{
		if (Vector2.Distance(base.transform.position, CommonReferences.Instance.GetPlayer().GetPosHips()) < 0.5f)
		{
			m_isCaughtPlayer = true;
			ImpregnatePlayer();
		}
	}

	private void ImpregnatePlayer()
	{
		StartCoroutine(CoroutineImpregnatePlayer());
	}

	private IEnumerator CoroutineImpregnatePlayer()
	{
		Player l_player = CommonReferences.Instance.GetPlayer();
		base.transform.parent = l_player.GetSkeletonPlayer().GetBone(BoneTypePlayer.Hips).transform;
		base.transform.localPosition = Vector3.zero;
		yield return new WaitForSeconds(2f);
		l_player.CreateAndAddFetus(null, Library.Instance.Actors.GetActor("Altar Spirit").GetComponent<Actor>(), 5f, "Altar Spirit");
		l_player.GetRigidbody2D().AddForce(new Vector2(10f, 5f));
		l_player.Ragdoll(2f);
		CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(m_audioImpregnation);
		KillSpirit();
	}

	private void KillSpirit()
	{
		GetComponentInChildren<ParticleSystem>().Stop(withChildren: true, ParticleSystemStopBehavior.StopEmitting);
		Object.Destroy(base.gameObject, 0.5f);
	}
}
