using System.Collections.Generic;
using UnityEngine;

public class Grapple : Gun
{
	[SerializeField]
	private Sprite m_sprGrappleIdle;

	[SerializeField]
	private Sprite m_sprGrappleShot;

	[SerializeField]
	private Sprite m_sprIconGrappleIdle;

	[SerializeField]
	private Sprite m_sprIconGrappleShot;

	[SerializeField]
	private Hook m_hook;

	[SerializeField]
	private Rope m_rope;

	[SerializeField]
	private GameObject m_pointShootHook;

	[SerializeField]
	private AudioClip m_audioShoot;

	[SerializeField]
	private AudioClip m_audioAttach;

	[SerializeField]
	private List<ParticleSystem> m_particlesShoot = new List<ParticleSystem>();

	private StateGrapple m_stateGrappleCurrent;

	private Hook m_hookCurrent;

	private SpringJoint2D m_springJointCurrent;

	private Rope m_ropeCurrent;

	protected override bool HandleUse(bool i_isAltFire)
	{
		switch (m_stateGrappleCurrent)
		{
		case StateGrapple.Idle:
			ShootHook();
			break;
		case StateGrapple.Shooting:
			ReleasePlayerFromHook();
			break;
		case StateGrapple.Attached:
			ReleasePlayerFromHook();
			break;
		}
		return true;
	}

	private void ShootHook()
	{
		m_stateGrappleCurrent = StateGrapple.Shooting;
		m_isCanDrop = false;
		m_sprItem = m_sprIconGrappleShot;
		GetComponent<SpriteRenderer>().sprite = m_sprGrappleShot;
		m_hookCurrent = Object.Instantiate(m_hook, CommonReferences.Instance.GetManagerStages().GetStageCurrent().transform);
		m_hookCurrent.gameObject.SetActive(value: true);
		m_hookCurrent.OnAttach += AttachPlayerToHook;
		m_hookCurrent.transform.position = m_pointShootHook.transform.position;
		m_hookCurrent.transform.rotation = m_pointShootHook.transform.rotation;
		m_hookCurrent.Fly();
		CreateRope();
		CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(m_audioShoot);
		foreach (ParticleSystem item in m_particlesShoot)
		{
			item.Play();
		}
		Player player = CommonReferences.Instance.GetPlayer();
		player.OnGetHit += ReleasePlayerFromHook;
		player.OnBeingRaped += ReleasePlayerFromHook;
		player.OnLabor += ReleasePlayerFromHook;
	}

	private void AttachPlayerToHook()
	{
		Player player = CommonReferences.Instance.GetPlayer();
		if (m_hookCurrent.transform.position.y < player.transform.position.y)
		{
			ReleasePlayerFromHook();
			return;
		}
		m_stateGrappleCurrent = StateGrapple.Attached;
		GetComponent<SpriteRenderer>().sprite = m_sprGrappleShot;
		player.SetStatePlayer(StatePlayer.Grappling);
		player.GetAnimator().Play("Grapple");
		m_springJointCurrent = player.gameObject.AddComponent<SpringJoint2D>();
		m_springJointCurrent.enableCollision = true;
		m_springJointCurrent.connectedBody = m_hookCurrent.GetComponent<Rigidbody2D>();
		m_springJointCurrent.dampingRatio = 1f;
		m_springJointCurrent.frequency = 5f;
		m_springJointCurrent.autoConfigureDistance = false;
		CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(m_audioAttach);
	}

	private void ReleasePlayerFromHook()
	{
		m_stateGrappleCurrent = StateGrapple.Idle;
		m_isCanDrop = true;
		m_sprItem = m_sprIconGrappleIdle;
		GetComponent<SpriteRenderer>().sprite = m_sprGrappleIdle;
		Player player = CommonReferences.Instance.GetPlayer();
		player.SetStateActor(StateActor.Idle);
		player.SetVelocity(player.GetVelocity() * 1.2f);
		Object.Destroy(m_springJointCurrent);
		Object.Destroy(m_hookCurrent.gameObject);
		Object.Destroy(m_ropeCurrent.gameObject);
		player.OnGetHit -= ReleasePlayerFromHook;
		player.OnBeingRaped -= ReleasePlayerFromHook;
		player.OnLabor -= ReleasePlayerFromHook;
	}

	private void CreateRope()
	{
		m_ropeCurrent = Object.Instantiate(m_rope);
		m_ropeCurrent.gameObject.SetActive(value: true);
		m_ropeCurrent.Attach(CommonReferences.Instance.GetPlayer().gameObject, m_hookCurrent.gameObject);
	}

	public StateGrapple GetStateGrappleCurrent()
	{
		return m_stateGrappleCurrent;
	}

	public Hook GetHookCurrent()
	{
		return m_hookCurrent;
	}
}
