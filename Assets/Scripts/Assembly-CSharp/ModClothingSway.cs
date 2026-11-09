using CaptivityReloaded.Modding;
using UnityEngine;

/// <summary>Bounded secondary motion for hair and other hanging clothing pieces.</summary>
public sealed class ModClothingSway : MonoBehaviour
{
	[SerializeField] private float m_spring = 45f;
	[SerializeField] private float m_damping = 9f;
	[SerializeField] private float m_gravity = 0.25f;
	[SerializeField] private float m_motionInfluence = 1f;
	[SerializeField] private float m_maxAngle = 30f;
	[SerializeField] private float m_idleAmplitude;
	[SerializeField] private float m_idleFrequency = 1f;
	private float m_baseAngle;
	private float m_angle;
	private float m_velocity;
	private float m_parentAngle;
	private Vector3 m_parentPosition;
	private bool m_needsBaseline;

	public void Configure(ClothingPhysicsDefinition i_definition)
	{
		if (i_definition == null) return;
		m_spring = i_definition.Spring ?? 45f;
		m_damping = i_definition.Damping ?? 9f;
		m_gravity = i_definition.Gravity ?? 0.25f;
		m_motionInfluence = i_definition.MotionInfluence ?? 1f;
		m_maxAngle = i_definition.MaxAngle ?? 30f;
		m_idleAmplitude = i_definition.IdleAmplitude ?? 0f;
		m_idleFrequency = i_definition.IdleFrequency ?? 1f;
	}

	private void OnEnable()
	{
		m_angle = 0f;
		m_velocity = 0f;
		m_needsBaseline = true;
	}

	private void LateUpdate()
	{
		if (transform.parent == null) return;
		if (m_needsBaseline)
		{
			m_baseAngle = transform.localEulerAngles.z;
			m_parentAngle = transform.parent.eulerAngles.z;
			m_parentPosition = transform.parent.position;
			m_needsBaseline = false;
			return;
		}
		float deltaTime = Mathf.Min(Time.deltaTime, 0.05f);
		if (deltaTime <= 0f) return;
		float parentAngle = transform.parent.eulerAngles.z;
		float rotationImpulse = -Mathf.DeltaAngle(m_parentAngle, parentAngle) * m_motionInfluence;
		float horizontalSpeed = (transform.parent.position.x - m_parentPosition.x) / deltaTime;
		float gravityTarget = Mathf.DeltaAngle(parentAngle, 0f) * m_gravity;
		float idleTarget = m_idleAmplitude > 0f && m_idleFrequency > 0f
			? Mathf.Sin(Time.time * m_idleFrequency * Mathf.PI * 2f) * m_idleAmplitude
			: 0f;
		float target = Mathf.Clamp(gravityTarget + rotationImpulse - horizontalSpeed * m_motionInfluence + idleTarget,
			-m_maxAngle, m_maxAngle);
		m_velocity += ((target - m_angle) * m_spring - m_velocity * m_damping) * deltaTime;
		m_angle = Mathf.Clamp(m_angle + m_velocity * deltaTime, -m_maxAngle, m_maxAngle);
		transform.localEulerAngles = new Vector3(0f, 0f, m_baseAngle + m_angle);
		m_parentAngle = parentAngle;
		m_parentPosition = transform.parent.position;
	}
}
