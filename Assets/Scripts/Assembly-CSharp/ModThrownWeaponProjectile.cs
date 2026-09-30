using System.Collections;
using System.Collections.Generic;
using CaptivityReloaded.Modding;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public sealed class ModThrownWeaponProjectile : MonoBehaviour
{
	private Gun m_source;
	private Actor m_owner;
	private Rigidbody2D m_body;
	private Collider2D m_collider;
	private SpriteRenderer m_renderer;
	private AudioSource m_audio;
	private WeaponThrowableDefinition m_definition;
	private List<AudioClip> m_impactSounds;
	private List<AudioClip> m_tickSounds;
	private List<AudioClip> m_explosionSounds;
	private readonly HashSet<Actor> m_impactedActors = new HashSet<Actor>();
	private float m_nextImpactSoundAt;
	private bool m_finished;
	private float m_explodeAt = -1f;
	private float m_destroyAt;
	private float m_nextTickAt;

	public void Configure(Gun i_source, Actor i_owner, Rigidbody2D i_body, Collider2D i_collider,
		SpriteRenderer i_renderer, AudioSource i_audio, Vector2 i_direction, float i_heldSeconds,
		WeaponThrowableDefinition i_definition, IEnumerable<AudioClip> i_impactSounds,
		IEnumerable<AudioClip> i_tickSounds, IEnumerable<AudioClip> i_explosionSounds)
	{
		m_source = i_source;
		m_owner = i_owner;
		m_body = i_body;
		m_collider = i_collider;
		m_renderer = i_renderer;
		m_audio = i_audio;
		m_definition = i_definition;
		m_impactSounds = new List<AudioClip>(i_impactSounds ?? new AudioClip[0]);
		m_tickSounds = new List<AudioClip>(i_tickSounds ?? new AudioClip[0]);
		m_explosionSounds = new List<AudioClip>(i_explosionSounds ?? new AudioClip[0]);

		float speed = m_definition.ThrowSpeed ?? 12f;
		m_body.velocity = i_direction * speed;
		m_body.angularVelocity = (m_owner != null && m_owner.GetIsFacingLeft() ? -1f : 1f)
			* (m_definition.AngularVelocity ?? 180f);
		m_destroyAt = Time.time + (m_definition.LifetimeSeconds ?? 15f);
		if (m_definition.FuseSeconds.HasValue)
		{
			m_explodeAt = Time.time + Mathf.Max(0.01f, m_definition.FuseSeconds.Value - i_heldSeconds);
			m_nextTickAt = Time.time;
		}
		if (m_owner != null)
			foreach (Collider2D ownerCollider in m_owner.GetAllColliders())
				if (ownerCollider != null) Physics2D.IgnoreCollision(m_collider, ownerCollider, true);
	}

	private void Update()
	{
		if (m_finished) return;
		if (m_explodeAt >= 0f)
		{
			if (Time.time >= m_explodeAt)
			{
				Explode();
				return;
			}
			if (m_tickSounds.Count > 0 && Time.time >= m_nextTickAt)
			{
				m_audio.PlayOneShot(m_tickSounds[Random.Range(0, m_tickSounds.Count)]);
				m_nextTickAt = Time.time + (m_definition.TickIntervalSeconds ?? 0.5f);
			}
		}
		if (Time.time >= m_destroyAt) Finish(0f);
	}

	private void OnCollisionEnter2D(Collision2D i_collision)
	{
		if (i_collision == null || i_collision.collider == null) return;
		Vector2 point = i_collision.contactCount > 0 ? i_collision.GetContact(0).point : (Vector2)transform.position;
		HandleImpact(i_collision.collider, point);
	}

	private void OnTriggerEnter2D(Collider2D i_collider)
	{
		HandleImpact(i_collider, transform.position);
	}

	private void HandleImpact(Collider2D i_colliderHit, Vector2 i_point)
	{
		if (m_finished || i_colliderHit == null) return;
		BodyPartActor bodyPart = i_colliderHit.GetComponent<BodyPartActor>() ?? i_colliderHit.GetComponentInParent<BodyPartActor>();
		Actor actor = bodyPart == null ? i_colliderHit.GetComponent<Actor>() ?? i_colliderHit.GetComponentInParent<Actor>() : bodyPart.GetOwner();
		if (actor != null && actor == m_owner) return;
		if (m_impactSounds.Count > 0 && Time.time >= m_nextImpactSoundAt)
		{
			m_audio.PlayOneShot(m_impactSounds[Random.Range(0, m_impactSounds.Count)]);
			m_nextImpactSoundAt = Time.time + 0.08f;
		}
		if (actor != null && m_impactedActors.Add(actor))
		{
			float damage = m_source == null ? 0f : m_source.GetDamage() * (m_definition.ImpactDamageMultiplier ?? 1f);
			if (bodyPart != null) damage *= bodyPart.GetDamageMultiplierAmountBodyPart();
			actor.TakeDamage(damage);
			Rigidbody2D hitBody = i_colliderHit.attachedRigidbody;
			if (hitBody != null) hitBody.AddForce(m_body.velocity.normalized * damage, ForceMode2D.Impulse);
		}
		if (m_definition.ExplodeOnImpact == true)
		{
			Explode();
			return;
		}
		if (m_definition.DestroyOnImpact == true) Finish(LongestClip(m_impactSounds));
	}

	private void Explode()
	{
		if (m_finished) return;
		m_finished = true;
		float radius = m_definition.ExplosionRadius ?? 0f;
		float baseDamage = (m_source == null ? 0f : m_source.GetDamage()) * (m_definition.ExplosionDamageMultiplier ?? 1f);
		float force = m_definition.ExplosionForce ?? baseDamage;
		HashSet<Actor> damaged = new HashSet<Actor>();
		int mask = LayerMask.GetMask("Player", "Actor", "BodyPart");
		foreach (Collider2D hit in Physics2D.OverlapCircleAll(transform.position, radius, mask))
		{
			BodyPartActor part = hit.GetComponent<BodyPartActor>() ?? hit.GetComponentInParent<BodyPartActor>();
			Actor actor = part == null ? hit.GetComponent<Actor>() ?? hit.GetComponentInParent<Actor>() : part.GetOwner();
			float distance01 = radius <= 0f ? 0f : Mathf.Clamp01(Vector2.Distance(transform.position, hit.bounds.center) / radius);
			if (actor != null && damaged.Add(actor))
			{
				actor.TakeDamage(baseDamage * (1f - distance01));
				if (m_definition.IgniteDurationSeconds.HasValue)
					actor.Ignite(m_source == null ? "mod-throwable" : m_source.name,
						m_definition.IgniteDurationSeconds.Value, m_definition.IgniteDamagePerTick ?? 0f,
						m_definition.IgniteTicksPerSecond ?? 1f);
			}
			if (hit.attachedRigidbody != null)
			{
				Vector2 direction = ((Vector2)hit.bounds.center - (Vector2)transform.position).normalized;
				hit.attachedRigidbody.AddForce(direction * force * (1f - distance01), ForceMode2D.Impulse);
			}
		}
		if (m_explosionSounds.Count > 0) m_audio.PlayOneShot(m_explosionSounds[Random.Range(0, m_explosionSounds.Count)]);
		CreateExplosionPresentation(radius);
		Finish(Mathf.Max(0.35f, LongestClip(m_explosionSounds)));
	}

	private void CreateExplosionPresentation(float i_radius)
	{
		Color color = new Color(1f, 0.45f, 0.1f, 1f);
		if (!string.IsNullOrWhiteSpace(m_definition.ExplosionColor)) ColorUtility.TryParseHtmlString(m_definition.ExplosionColor, out color);
		GameObject effect = new GameObject("Throwable Explosion");
		effect.transform.position = transform.position;
		if (transform.parent != null) effect.transform.SetParent(transform.parent, true);
		ParticleSystem particles = effect.AddComponent<ParticleSystem>();
		particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		ParticleSystem.MainModule main = particles.main;
		main.duration = 0.2f;
		main.loop = false;
		main.startLifetime = 0.3f;
		bool fireStyle = m_definition.ExplosionStyle == "fire";
		main.startSpeed = fireStyle ? Mathf.Max(0.5f, i_radius) : Mathf.Max(1f, i_radius * 2f);
		main.startSize = Mathf.Max(0.08f, i_radius * 0.15f);
		main.startColor = color;
		ParticleSystem.EmissionModule emission = particles.emission;
		emission.rateOverTime = 0f;
		emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)(fireStyle ? 28 : 18)) });
		ParticleSystem.ShapeModule shape = particles.shape;
		shape.shapeType = fireStyle ? ParticleSystemShapeType.Cone : ParticleSystemShapeType.Circle;
		shape.radius = Mathf.Max(0.05f, i_radius * 0.15f);
		if (fireStyle) shape.angle = 20f;
		ParticleSystemRenderer particleRenderer = particles.GetComponent<ParticleSystemRenderer>();
		particleRenderer.sortingLayerName = "Projectile";
		particleRenderer.sortingOrder = 60;
		Light2D light = effect.AddComponent<Light2D>();
		light.lightType = Light2D.LightType.Point;
		light.color = color;
		light.intensity = 1.5f;
		light.pointLightOuterRadius = Mathf.Max(0.5f, i_radius);
		particles.Play();
		effect.AddComponent<ModThrowableExplosionFade>().Configure(light, 0.25f);
		Destroy(effect, 1f);
	}

	private void Finish(float i_audioTail)
	{
		m_finished = true;
		if (m_renderer != null) m_renderer.enabled = false;
		if (m_collider != null) m_collider.enabled = false;
		if (m_body != null) m_body.simulated = false;
		Destroy(gameObject, Mathf.Max(0.01f, i_audioTail));
	}

	private static float LongestClip(IEnumerable<AudioClip> i_clips)
	{
		float length = 0f;
		if (i_clips != null) foreach (AudioClip clip in i_clips) if (clip != null) length = Mathf.Max(length, clip.length);
		return length;
	}
}

public sealed class ModThrowableExplosionFade : MonoBehaviour
{
	private Light2D m_light;
	private float m_duration;
	private float m_started;

	public void Configure(Light2D i_light, float i_duration)
	{
		m_light = i_light;
		m_duration = Mathf.Max(0.01f, i_duration);
		m_started = Time.time;
	}

	private void Update()
	{
		if (m_light != null) m_light.intensity = Mathf.Lerp(1.5f, 0f, Mathf.Clamp01((Time.time - m_started) / m_duration));
	}
}
