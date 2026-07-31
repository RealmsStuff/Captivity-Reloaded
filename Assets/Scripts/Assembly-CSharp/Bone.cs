using System.Collections.Generic;
using UnityEngine;

public class Bone : MonoBehaviour
{
	private BodyPart m_bodyPart;

	private HingeJoint2D m_hingeJoint2DRight;

	private HingeJoint2D m_hingeJoint2DLeft;

	private Rigidbody2D m_rigidbody2d;

	private void Awake()
	{
		m_bodyPart = GetComponentInChildren<BodyPart>();
		if (GetComponents<HingeJoint2D>().Length != 0)
		{
			m_hingeJoint2DRight = GetComponents<HingeJoint2D>()[0];
		}
		m_rigidbody2d = GetComponent<Rigidbody2D>();
	}

	public Rigidbody2D GetRigidbody2D()
	{
		if (m_rigidbody2d == null)
		{
			m_rigidbody2d = GetComponent<Rigidbody2D>();
		}
		return m_rigidbody2d;
	}

	public BodyPart GetBodyPart()
	{
		if (m_bodyPart == null)
		{
			m_bodyPart = GetComponentInChildren<BodyPart>(includeInactive: true);
		}
		return m_bodyPart;
	}

	public HingeJoint2D GetHingeJoint2D()
	{
		if (GetBodyPart().GetOwner().GetIsFacingLeft())
		{
			if (m_hingeJoint2DLeft == null)
			{
				if (GetComponents<HingeJoint2D>().Length < 1)
				{
					return null;
				}
				m_hingeJoint2DLeft = GetComponents<HingeJoint2D>()[1];
			}
			return m_hingeJoint2DLeft;
		}
		if (m_hingeJoint2DRight == null)
		{
			if (GetComponents<HingeJoint2D>().Length < 1)
			{
				return null;
			}
			m_hingeJoint2DRight = GetComponents<HingeJoint2D>()[0];
		}
		return m_hingeJoint2DRight;
	}

	public HingeJoint2D GetHingeJoint2D(bool i_left)
	{
		if (i_left)
		{
			if (m_hingeJoint2DLeft == null)
			{
				if (GetComponents<HingeJoint2D>().Length < 1)
				{
					return null;
				}
				m_hingeJoint2DLeft = GetComponents<HingeJoint2D>()[1];
			}
			return m_hingeJoint2DLeft;
		}
		if (m_hingeJoint2DRight == null)
		{
			if (GetComponents<HingeJoint2D>().Length < 1)
			{
				return null;
			}
			m_hingeJoint2DRight = GetComponents<HingeJoint2D>()[0];
		}
		return m_hingeJoint2DRight;
	}

	public Bone GetBoneParent()
	{
		Bone[] componentsInParent = GetComponentsInParent<Bone>(includeInactive: true);
		foreach (Bone bone in componentsInParent)
		{
			if (bone.transform == base.transform.parent)
			{
				return bone;
			}
		}
		return null;
	}

	public List<Bone> GetBonesChildren()
	{
		List<Bone> list = new List<Bone>();
		Bone[] componentsInChildren = GetComponentsInChildren<Bone>();
		foreach (Bone bone in componentsInChildren)
		{
			if (bone.transform.parent == base.transform)
			{
				list.Add(bone);
			}
		}
		return list;
	}
}
