using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
	[SerializeField]
	private bool m_isClimbableLeftLedge;

	[SerializeField]
	private bool m_isClimbableRightLedge;

	private GameObject m_edgeLeft;

	private GameObject m_edgeRight;

	private GameObject m_ledgeLeft;

	private GameObject m_ledgeRight;

	public void ConfigureModPlatform(bool i_climbableLeftLedge, bool i_climbableRightLedge)
	{
		m_isClimbableLeftLedge = i_climbableLeftLedge;
		m_isClimbableRightLedge = i_climbableRightLedge;
	}

	public void CreateLedgesAndEdges()
	{
		CreateLedges();
		CreateEdges();
		if (m_isClimbableLeftLedge)
		{
			GameObject obj = Object.Instantiate(ResourceContainer.Resources.m_particleLedge, m_ledgeLeft.transform);
			obj.transform.localPosition = Vector3.zero;
			obj.SetActive(value: true);
		}
		if (m_isClimbableRightLedge)
		{
			GameObject obj2 = Object.Instantiate(ResourceContainer.Resources.m_particleLedge, m_ledgeRight.transform);
			obj2.transform.localPosition = Vector3.zero;
			obj2.SetActive(value: true);
		}
	}

	private void CreateLedges()
	{
		m_ledgeLeft = new GameObject("LedgeLeft");
		m_ledgeLeft.transform.parent = base.transform;
		Vector2 posTop = GetPosTop();
		posTop.x -= GetComponent<BoxCollider2D>().bounds.size.x / 2f;
		posTop.x += 0.25f;
		m_ledgeLeft.transform.position = posTop;
		if (m_isClimbableLeftLedge)
		{
			m_ledgeLeft.AddComponent<Ledge>().Initialize(i_isLeft: true, i_isClimable: true);
		}
		else
		{
			m_ledgeLeft.AddComponent<Ledge>().Initialize(i_isLeft: true, i_isClimable: false);
		}
		m_ledgeRight = new GameObject("LedgeRight");
		m_ledgeRight.transform.parent = base.transform;
		Vector2 posTop2 = GetPosTop();
		posTop2.x += GetComponent<BoxCollider2D>().bounds.size.x / 2f;
		posTop2.x -= 0.25f;
		m_ledgeRight.transform.position = posTop2;
		if (m_isClimbableRightLedge)
		{
			m_ledgeRight.AddComponent<Ledge>().Initialize(i_isLeft: false, i_isClimable: true);
		}
		else
		{
			m_ledgeRight.AddComponent<Ledge>().Initialize(i_isLeft: false, i_isClimable: false);
		}
	}

	private void CreateEdges()
	{
		m_edgeLeft = new GameObject("EdgeLeft");
		m_edgeLeft.transform.parent = base.transform;
		Vector2 posTop = GetPosTop();
		posTop.x -= GetComponent<BoxCollider2D>().bounds.size.x / 2f;
		posTop.x -= 2f;
		m_edgeLeft.transform.position = posTop;
		m_edgeRight = new GameObject("EdgeRight");
		m_edgeRight.transform.parent = base.transform;
		Vector2 posTop2 = GetPosTop();
		posTop2.x += GetComponent<BoxCollider2D>().bounds.size.x / 2f;
		posTop2.x += 2f;
		m_edgeRight.transform.position = posTop2;
	}

	public Vector2 GetPosTop()
	{
		Vector2 result = GetComponent<BoxCollider2D>().bounds.center;
		result.y += GetComponent<BoxCollider2D>().size.y / 2f;
		return result;
	}

	public Transform GetClosestEdgeToWalkOffFrom(Vector2 i_posToCheckFrom)
	{
		if (Vector2.Distance(i_posToCheckFrom, m_ledgeLeft.transform.position) < Vector2.Distance(i_posToCheckFrom, m_ledgeRight.transform.position))
		{
			return m_edgeLeft.transform;
		}
		return m_edgeRight.transform;
	}

	public List<Ledge> GetLedgesClimbable()
	{
		List<Ledge> list = new List<Ledge>();
		if (m_isClimbableLeftLedge)
		{
			list.Add(m_ledgeLeft.GetComponent<Ledge>());
		}
		if (m_isClimbableRightLedge)
		{
			list.Add(m_ledgeRight.GetComponent<Ledge>());
		}
		return list;
	}

	public bool GetIsClimbableLeftLedge()
	{
		return m_isClimbableLeftLedge;
	}

	public bool GetIsClimbableRightLedge()
	{
		return m_isClimbableRightLedge;
	}

	public List<Ledge> GetLedges()
	{
		return new List<Ledge>
		{
			m_ledgeLeft.GetComponent<Ledge>(),
			m_ledgeRight.GetComponent<Ledge>()
		};
	}
}
