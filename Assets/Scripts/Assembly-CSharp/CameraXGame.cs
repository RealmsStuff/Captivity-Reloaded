using System.Collections;
using UnityEngine;

public class CameraXGame : MonoBehaviour
{
	[SerializeField]
	private GameObject m_focusedObject;

	[SerializeField]
	private bool m_isFollowObject;

	[SerializeField]
	private bool m_isSway;

	[SerializeField]
	private float m_swayAmount;

	[SerializeField]
	private SmoothLevel m_smoothLevel;

	[SerializeField]
	private bool m_isRotateWithFocusedObject;

	private Camera m_camera;

	private float m_distanceBetweenObjectCamera;

	private float m_distanceXBetweenObjectCamera;

	private float m_distanceYBetweenObjectCamera;

	private Vector3 m_velocity = Vector3.zero;

	private Vector2 m_velocityOffset = Vector3.zero;

	private float m_zoomOriginal;

	private float m_orthographicSizeOriginal;

	private float m_zoomAuthored;

	private float m_orthographicSizeAuthored;

	private float m_swayAmountOriginal;

	private Vector2 m_offsetCamera = Vector2.zero;

	private Coroutine m_coroutineZoom;
	private bool m_hasWorldBounds;
	private Rect m_worldBounds;

	private void Awake()
	{
		m_camera = GetComponent<Camera>();
		m_zoomAuthored = m_camera.fieldOfView;
		m_orthographicSizeAuthored = m_camera.orthographicSize;
		ApplyRuleProfileZoom();
		m_swayAmountOriginal = m_swayAmount;
	}

	public void ApplyRuleProfileZoom()
	{
		if (m_camera == null) m_camera = GetComponent<Camera>();
		if (m_camera == null) return;
		if (m_zoomAuthored <= 0f) m_zoomAuthored = m_camera.fieldOfView;
		if (m_orthographicSizeAuthored <= 0f) m_orthographicSizeAuthored = m_camera.orthographicSize;
		float multiplier = ExternalRuleProfileFactory.ApplyCameraZoom(1f);
		m_zoomOriginal = m_zoomAuthored * multiplier;
		m_orthographicSizeOriginal = m_orthographicSizeAuthored * multiplier;
		m_camera.fieldOfView = m_zoomOriginal;
		m_camera.orthographicSize = m_orthographicSizeOriginal;
		ApplyWorldBounds();
	}

	private void LateUpdate()
	{
		if (!(m_focusedObject == null) && m_isFollowObject && m_focusedObject.activeInHierarchy)
		{
			m_distanceBetweenObjectCamera = Vector3.Distance(base.transform.position, m_focusedObject.transform.position) - 10f;
			m_distanceXBetweenObjectCamera = m_focusedObject.transform.position.x - base.transform.position.x;
			m_distanceYBetweenObjectCamera = m_focusedObject.transform.position.y - base.transform.position.y;
			FollowObject3();
		}
		ApplyWorldBounds();
	}

	private void FollowObject()
	{
		Vector3 position = m_focusedObject.transform.position;
		Vector3 position2 = base.transform.position;
		if (m_isSway && m_swayAmount > 0f)
		{
			if (position2.x > position.x)
			{
				position2.x += m_distanceXBetweenObjectCamera / m_swayAmount;
			}
			if (position2.x < position.x)
			{
				position2.x += m_distanceXBetweenObjectCamera / m_swayAmount;
			}
			if (position2.y > position.y)
			{
				position2.y += m_distanceYBetweenObjectCamera / m_swayAmount;
			}
			if (position2.y < position.y)
			{
				position2.y += m_distanceYBetweenObjectCamera / m_swayAmount;
			}
		}
		else
		{
			position2.x = position.x;
			position2.y = position.y;
		}
		base.transform.position = position2;
	}

	private void FollowObject3()
	{
		Vector3 target = m_focusedObject.transform.position + new Vector3(m_offsetCamera.x, m_offsetCamera.y, base.transform.position.z);
		float smoothTime = 0f;
		switch (m_smoothLevel)
		{
		case SmoothLevel.None:
			smoothTime = 0f;
			break;
		case SmoothLevel.Low:
			smoothTime = 0.1f;
			break;
		case SmoothLevel.Medium:
			smoothTime = 0.125f;
			break;
		case SmoothLevel.High:
			smoothTime = 0.2f;
			break;
		}
		Vector3 position = Vector3.SmoothDamp(base.transform.position, target, ref m_velocity, smoothTime);
		position.z = base.transform.position.z;
		base.transform.position = position;
		if (m_isRotateWithFocusedObject)
		{
			RotateWithFocusedObject();
		}
		else
		{
			base.transform.localEulerAngles = Vector3.zero;
		}
	}

	public void SetIsRotateWithFocusedObject(bool i_isRotate)
	{
		m_isRotateWithFocusedObject = i_isRotate;
	}

	private void RotateWithFocusedObject()
	{
		base.transform.eulerAngles = m_focusedObject.transform.eulerAngles;
	}

	public void SetFollowObject(bool i_follow)
	{
		m_isFollowObject = i_follow;
	}

	public void SetObjectFocused(GameObject i_object)
	{
		m_focusedObject = i_object;
	}

	public void SetIsSway(bool i_isSway)
	{
		m_isSway = i_isSway;
	}

	public void SetSwayAmount(float i_swayAmount)
	{
		m_swayAmount = i_swayAmount;
	}

	public void SetSmoothLevel(SmoothLevel i_smoothLevel)
	{
		m_smoothLevel = i_smoothLevel;
	}

	public float GetSwayAmount()
	{
		return m_swayAmount;
	}

	public void CenterCamera()
	{
		Vector3 position = base.transform.position;
		position.x = m_focusedObject.transform.position.x;
		position.y = m_focusedObject.transform.position.y;
		base.transform.position = position;
		ApplyWorldBounds();
	}

	public void SetWorldBounds(Rect i_bounds)
	{
		m_worldBounds = i_bounds;
		m_hasWorldBounds = i_bounds.width > 0f && i_bounds.height > 0f;
		ApplyWorldBounds();
	}

	public void ClearWorldBounds() { m_hasWorldBounds = false; }

	private void ApplyWorldBounds()
	{
		if (!m_hasWorldBounds || m_camera == null) return;
		float distance = Mathf.Abs(transform.position.z);
		float halfHeight;
		float halfWidth;
		if (m_camera.orthographic)
		{
			halfHeight = m_camera.orthographicSize;
			halfWidth = halfHeight * m_camera.aspect;
		}
		else
		{
			halfHeight = Mathf.Tan(m_camera.fieldOfView * 0.5f * Mathf.Deg2Rad) * Mathf.Max(0.01f, distance);
			halfWidth = halfHeight * m_camera.aspect;
		}
		Vector3 position = transform.position;
		position.x = ClampCameraAxis(position.x, m_worldBounds.xMin + halfWidth, m_worldBounds.xMax - halfWidth, m_worldBounds.center.x);
		position.y = ClampCameraAxis(position.y, m_worldBounds.yMin + halfHeight, m_worldBounds.yMax - halfHeight, m_worldBounds.center.y);
		transform.position = position;
	}

	private static float ClampCameraAxis(float i_value, float i_min, float i_max, float i_center)
	{
		return i_min <= i_max ? Mathf.Clamp(i_value, i_min, i_max) : i_center;
	}

	public void Shake(float i_shakeAmount01, float i_smoothingAmount01)
	{
		float num = 1f * i_shakeAmount01;
		m_offsetCamera = new Vector2(Random.Range(0f - num, num), Random.Range(0f - num, num));
		StartCoroutine(CoroutineDecreaseOffset(i_smoothingAmount01));
	}

	private IEnumerator CoroutineDecreaseOffset(float i_smoothingAmount01)
	{
		bool l_isDone = false;
		while (!l_isDone)
		{
			Vector2 zero = Vector2.zero;
			float smoothTime = 1.5f * i_smoothingAmount01;
			Vector2 offsetCamera = Vector2.SmoothDamp(m_offsetCamera, zero, ref m_velocityOffset, smoothTime);
			m_offsetCamera = offsetCamera;
			if (m_offsetCamera.x <= 0.01f && m_offsetCamera.x >= -0.01f && m_offsetCamera.y <= 0.01f && m_offsetCamera.y >= -0.01f)
			{
				m_offsetCamera.x = 0f;
				m_offsetCamera.y = 0f;
				l_isDone = true;
			}
			yield return new WaitForFixedUpdate();
		}
	}

	public void ZoomToFOV(float i_fovLevel, float i_timeToZoom)
	{
		if (m_coroutineZoom != null)
		{
			StopCoroutine(m_coroutineZoom);
		}
		m_coroutineZoom = StartCoroutine(CoroutineAnimateZoom(i_fovLevel, i_timeToZoom));
	}

	protected IEnumerator CoroutineAnimateZoom(float i_fovLevel, float i_timeToZoom)
	{
		float l_zoomFrom = GetCurrentZoomLevel();
		if (i_timeToZoom <= 0f)
		{
			ApplyZoomLevel(i_fovLevel);
			m_coroutineZoom = null;
			yield break;
		}
		float l_timeCurrent = 0f;
		while (l_timeCurrent < i_timeToZoom)
		{
			l_timeCurrent += Time.fixedDeltaTime;
			float i_time = Mathf.Clamp01(l_timeCurrent / i_timeToZoom);
			float zoom = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Smooth, l_zoomFrom, i_fovLevel, i_time);
			ApplyZoomLevel(zoom);
			yield return new WaitForEndOfFrame();
		}
		ApplyZoomLevel(i_fovLevel);
		m_coroutineZoom = null;
	}

	private float GetCurrentZoomLevel()
	{
		if (!m_camera.orthographic) return m_camera.fieldOfView;
		if (m_orthographicSizeOriginal <= 0f) return m_zoomOriginal;
		return m_zoomOriginal * (m_camera.orthographicSize / m_orthographicSizeOriginal);
	}

	private void ApplyZoomLevel(float i_zoomLevel)
	{
		if (!m_camera.orthographic)
		{
			m_camera.fieldOfView = i_zoomLevel;
			return;
		}
		float ratio = i_zoomLevel / Mathf.Max(0.01f, m_zoomOriginal);
		m_camera.orthographicSize = Mathf.Max(0.01f, m_orthographicSizeOriginal * ratio);
		ApplyWorldBounds();
	}

	public float GetFOVOriginal()
	{
		return m_zoomOriginal;
	}

	public float GetSwayAmountOriginal()
	{
		return m_swayAmountOriginal;
	}

	public Camera GetCameraUnity()
	{
		return m_camera;
	}
}
