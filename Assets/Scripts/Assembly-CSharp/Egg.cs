using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Egg : NPC
{
	[Header("-Egg-")]
	[SerializeField]
	private float m_durationBeforeHatch;

	[SerializeField]
	private Sprite m_sprHatched;

	[SerializeField]
	private List<Sprite> m_sprsHatchedEggBits = new List<Sprite>();

	[SerializeField]
	private AudioClip m_audioHatch;

	[SerializeField]
	private ParticleSystem m_psHatch;

	[Header("Growth")]
	[SerializeField]
	private bool m_isGrows;

	[SerializeField]
	private Vector2 m_localScaleBegin;

	[SerializeField]
	private Vector2 m_localScaleEnd;

	private List<Actor> m_actorsInEgg = new List<Actor>();

	private float m_timeLeftGrowing;

	private bool m_isHatched;

	protected override void OnEnable()
	{
		if (!m_isHatched)
		{
			StartTimer();
		}
	}

	public override void UpdateAnim()
	{
	}

	public void Initialize(List<Actor> i_actorsInEgg, string i_nameEgg)
	{
		m_actorsInEgg = i_actorsInEgg;
		m_name = i_nameEgg;
		m_timeLeftGrowing = m_durationBeforeHatch;
	}

	public void StartTimer()
	{
		if (!m_isHatched)
		{
			base.OnDie += Hatch;
			StartCoroutine(CoroutineTimer());
		}
	}

	private IEnumerator CoroutineTimer()
	{
		while (m_timeLeftGrowing > 0f)
		{
			yield return new WaitForEndOfFrame();
			float deltaTime = Time.deltaTime;
			m_timeLeftGrowing -= deltaTime;
			if (m_isGrows)
			{
				UpdateSize();
			}
		}
		Hatch();
	}

	private void Hatch()
	{
		foreach (Actor item in m_actorsInEgg)
		{
			Actor actor = Object.Instantiate(item, CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetActorsParent()
				.transform);
				actor.SetPos(new Vector2(GetPos().x, GetPos().y + 1f));
				actor.SetStateActor(StateActor.Idle);
				actor.transform.SetParent(CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetActorsParent()
					.transform);
					actor.gameObject.SetActive(value: true);
					actor.SetVelocity(new Vector2(Random.Range(-5f, 5f), Random.Range(2f, 6f)));
					actor.DisableThinking(2f);
					if (actor is NPC)
					{
						NPC obj = (NPC)actor;
						obj.SetIsPlayerParent(i_isPlayerParent: true);
						obj.Spawn(i_isFadeIn: false);
					}
				}
				foreach (Sprite sprsHatchedEggBit in m_sprsHatchedEggBits)
				{
					GameObject obj2 = new GameObject("Egg Bit");
					obj2.transform.position = base.transform.position;
					obj2.transform.parent = CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetActorsParent()
						.transform;
					obj2.AddComponent<SpriteRenderer>();
					obj2.GetComponent<SpriteRenderer>().sprite = sprsHatchedEggBit;
					obj2.GetComponent<SpriteRenderer>().sortingLayerName = "Actor";
					obj2.AddComponent<PolygonCollider2D>();
					obj2.AddComponent<Rigidbody2D>();
					obj2.layer = LayerMask.NameToLayer("Actor");
					obj2.GetComponent<Rigidbody2D>().velocity = new Vector2(Random.Range(-5, 5), 10f);
				}
				CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(m_audioHatch);
				m_psHatch.Play();
				if (m_sprHatched != null)
				{
					GetComponent<SpriteRenderer>().sprite = m_sprHatched;
					Object.Destroy(base.gameObject, 10f);
				}
				else
				{
					Object.Destroy(base.gameObject);
				}
			}

			private void UpdateSize()
			{
				Vector3 zero = Vector3.zero;
				zero.x = m_localScaleBegin.x + (m_localScaleEnd.x - m_localScaleBegin.x) / m_durationBeforeHatch * (m_durationBeforeHatch - m_timeLeftGrowing);
				zero.y = m_localScaleBegin.y + (m_localScaleEnd.y - m_localScaleBegin.y) / m_durationBeforeHatch * (m_durationBeforeHatch - m_timeLeftGrowing);
				zero.z = base.transform.localScale.z;
				base.transform.localScale = zero;
			}

			protected override void HandleThinking()
			{
			}

			public override void TakeDamage(float i_amount)
			{
			}

			protected override void AddXAIComponent()
			{
			}

			public override void MoveToPlayer()
			{
			}

			public override void MoveAwayFromPlayer()
			{
			}
		}
