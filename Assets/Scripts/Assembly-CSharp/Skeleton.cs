using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Skeleton : MonoBehaviour
{
	protected List<Bone> m_bones = new List<Bone>();

	protected Dictionary<Bone, Vector3> m_dicBonePositionsOriginal = new Dictionary<Bone, Vector3>();

	protected Dictionary<BodyPart, int> m_dicBodyPartSortOrderOriginal = new Dictionary<BodyPart, int>();

	protected bool m_isFacingLeft;

	protected bool m_isRagdoll;

	protected Dictionary<Bone, Transform> m_poseRagdoll = new Dictionary<Bone, Transform>();

	protected bool m_isElectrocute;

	protected List<ParticleSystem> m_particlesIgnition = new List<ParticleSystem>();

	protected bool m_isOnFire;

	private Coroutine m_coroutineElectrocute;

	private Coroutine m_coroutineAnimateElectrocuteColor;

	protected virtual void Awake()
	{
		CheckAllBones();
		AddLeftJointAngleLimits();
		DisablePhysicsOnAllBones();
		RememberSortOrdersBodyParts();
	}

	private void Start()
	{
		m_isFacingLeft = CommonReferences.Instance.GetPlayer().GetIsFacingLeft();
	}

	private void CheckAllBones()
	{
		Bone[] componentsInChildren = GetComponentsInChildren<Bone>();
		foreach (Bone bone in componentsInChildren)
		{
			m_bones.Add(bone);
			m_dicBonePositionsOriginal.Add(bone, bone.transform.localPosition);
		}
	}

	private void RememberSortOrdersBodyParts()
	{
		foreach (Bone bone in m_bones)
		{
			if ((bool)bone.GetBodyPart() && !m_dicBodyPartSortOrderOriginal.ContainsKey(bone.GetBodyPart()))
			{
				m_dicBodyPartSortOrderOriginal.Add(bone.GetBodyPart(), bone.GetBodyPart().GetComponent<SpriteRenderer>().sortingOrder);
			}
		}
	}

	private void DisablePhysicsOnAllBones()
	{
		foreach (Bone bone in m_bones)
		{
			if (bone.GetBodyPart() != null && (bool)bone.GetBodyPart().GetCollider())
			{
				bone.GetBodyPart().GetCollider().isTrigger = true;
			}
			if ((bool)bone.GetRigidbody2D())
			{
				bone.GetRigidbody2D().isKinematic = true;
			}
		}
	}

	private void AddLeftJointAngleLimits()
	{
		for (int i = 0; i < m_bones.Count; i++)
		{
			Bone bone = m_bones[i];
			if ((bool)bone.GetHingeJoint2D(i_left: false))
			{
				JointAngleLimits2D limits = bone.GetHingeJoint2D(i_left: false).limits;
				float num = 360f - limits.min;
				float num2 = 360f - limits.max;
				if (num > num2)
				{
					float num3 = num2;
					num2 = num;
					num = num3;
				}
				num -= 360f;
				num2 -= 360f;
				HingeJoint2D hingeJoint2D = bone.gameObject.AddComponent<HingeJoint2D>();
				hingeJoint2D.connectedBody = bone.GetHingeJoint2D(i_left: false).connectedBody;
				hingeJoint2D.autoConfigureConnectedAnchor = true;
				hingeJoint2D.connectedAnchor = bone.GetHingeJoint2D(i_left: false).connectedAnchor;
				hingeJoint2D.limits = new JointAngleLimits2D
				{
					min = num,
					max = num2
				};
				hingeJoint2D.breakForce = float.PositiveInfinity;
				hingeJoint2D.breakTorque = float.PositiveInfinity;
				hingeJoint2D.enabled = false;
			}
		}
	}

	public void EnableRagdoll()
	{
		m_poseRagdoll = CopyCurrentPoseTransform();
		GetComponent<Animator>().enabled = false;
		for (int i = 0; i < m_bones.Count; i++)
		{
			Bone bone = m_bones[i];
			if (bone.GetBodyPart() != null)
			{
				if ((bool)bone.GetBodyPart().GetCollider())
				{
					bone.GetBodyPart().GetCollider().isTrigger = false;
					bone.GetBodyPart().GetCollider().enabled = true;
				}
				if ((bool)bone.GetHingeJoint2D(m_isFacingLeft))
				{
					bone.GetHingeJoint2D(m_isFacingLeft).useLimits = true;
				}
			}
		}
		m_isRagdoll = true;
		HandleEnableRagdoll();
	}

	public void DisableRagdoll()
	{
		CenterSkeleton();
		GetComponent<Animator>().enabled = true;
		CenterSkeleton();
		for (int i = 0; i < m_bones.Count; i++)
		{
			Bone bone = m_bones[i];
			m_dicBonePositionsOriginal.TryGetValue(bone, out var value);
			bone.transform.localPosition = value;
			if (bone.GetBodyPart() != null)
			{
				if ((bool)bone.GetBodyPart().GetCollider())
				{
					bone.GetBodyPart().GetCollider().isTrigger = true;
				}
				if ((bool)bone.GetHingeJoint2D(m_isFacingLeft))
				{
					bone.GetHingeJoint2D(m_isFacingLeft).useLimits = false;
				}
			}
			if ((bool)bone.GetRigidbody2D())
			{
				bone.GetRigidbody2D().isKinematic = true;
			}
		}
		m_isRagdoll = false;
		if (m_isElectrocute)
		{
			DisableElectrocute();
		}
	}

	protected abstract void HandleEnableRagdoll();

	protected abstract void CenterSkeleton();

	public Dictionary<Bone, Vector3[]> CopyCurrentPose()
	{
		Dictionary<Bone, Vector3[]> dictionary = new Dictionary<Bone, Vector3[]>();
		for (int i = 0; i < m_bones.Count; i++)
		{
			Bone bone = m_bones[i];
			dictionary.Add(bone, new Vector3[3]
			{
				bone.transform.position,
				bone.transform.eulerAngles,
				bone.transform.localScale
			});
		}
		return dictionary;
	}

	public Dictionary<Bone, Transform> CopyCurrentPoseTransform()
	{
		Dictionary<Bone, Transform> dictionary = new Dictionary<Bone, Transform>();
		for (int i = 0; i < m_bones.Count; i++)
		{
			Bone bone = m_bones[i];
			dictionary.Add(bone, bone.transform);
		}
		return dictionary;
	}

	public void ApplyPose(Dictionary<Bone, Vector3[]> i_poseToApply)
	{
		for (int i = 0; i < m_bones.Count; i++)
		{
			Bone bone = m_bones[i];
			Vector3[] value = new Vector3[3];
			if (i_poseToApply.TryGetValue(bone, out value))
			{
				bone.transform.position = value[0];
				bone.transform.eulerAngles = value[1];
				bone.transform.localScale = value[2];
			}
		}
	}

	public virtual void SetIsFacingLeft(bool i_isFacingLeft)
	{
		if (m_isFacingLeft == i_isFacingLeft)
		{
			return;
		}
		for (int i = 0; i < m_bones.Count; i++)
		{
			if ((bool)m_bones[i].GetHingeJoint2D(i_left: false))
			{
				if (i_isFacingLeft)
				{
					m_bones[i].GetHingeJoint2D(i_left: true).enabled = true;
					m_bones[i].GetHingeJoint2D(i_left: false).enabled = false;
				}
				else
				{
					m_bones[i].GetHingeJoint2D(i_left: true).enabled = false;
					m_bones[i].GetHingeJoint2D(i_left: false).enabled = true;
				}
			}
		}
		m_isFacingLeft = i_isFacingLeft;
	}

	public void SetBodyPartSortOrdersToOriginal()
	{
		for (int i = 0; i < m_bones.Count; i++)
		{
			m_dicBodyPartSortOrderOriginal.TryGetValue(m_bones[i].GetBodyPart(), out var value);
			m_bones[i].GetBodyPart().ApplySortOrder(value);
		}
	}

	public Bone GetBone(string i_nameBone)
	{
		for (int i = 0; i < m_bones.Count; i++)
		{
			Bone bone = m_bones[i];
			if (bone.gameObject.name == i_nameBone)
			{
				return bone;
			}
		}
		return null;
	}

	public Bone GetRandomBone()
	{
		int index = Random.Range(0, m_bones.Count);
		return m_bones[index];
	}

	public Actor GetOwner()
	{
		return GetComponentInParent<Actor>();
	}

	public void DisableCollisionOnAllBodyParts()
	{
		for (int i = 0; i < m_bones.Count; i++)
		{
			Bone bone = m_bones[i];
			if ((bool)bone.GetBodyPart())
			{
				BodyPart bodyPart = bone.GetBodyPart();
				if ((bool)bodyPart.GetCollider())
				{
					bodyPart.GetCollider().enabled = false;
				}
			}
		}
	}

	public void EnableCollisionOnAllBodyParts()
	{
		for (int i = 0; i < m_bones.Count; i++)
		{
			Bone bone = m_bones[i];
			if ((bool)bone.GetBodyPart())
			{
				BodyPart bodyPart = bone.GetBodyPart();
				if ((bool)bodyPart.GetCollider())
				{
					bodyPart.GetCollider().enabled = true;
				}
			}
		}
	}

	public List<Bone> GetAllBones()
	{
		List<Bone> list = new List<Bone>();
		foreach (Bone bone in m_bones)
		{
			list.Add(bone);
		}
		return list;
	}

	public List<BodyPart> GetAllBodyParts()
	{
		List<BodyPart> list = new List<BodyPart>();
		foreach (Bone bone in m_bones)
		{
			list.Add(bone.GetBodyPart());
		}
		return list;
	}

	public virtual void SetInvulnerable(bool i_isInvulnerable)
	{
		if (i_isInvulnerable)
		{
			for (int i = 0; i < m_bones.Count; i++)
			{
				Bone bone = m_bones[i];
				Color color = bone.GetBodyPart().GetComponent<SpriteRenderer>().color;
				color.a = 0.5f;
				bone.GetBodyPart().GetComponent<SpriteRenderer>().color = color;
			}
			return;
		}
		for (int j = 0; j < m_bones.Count; j++)
		{
			Bone bone2 = m_bones[j];
			if (bone2.GetBodyPart() != null)
			{
				Color color2 = bone2.GetBodyPart().GetComponent<SpriteRenderer>().color;
				color2.a = 1f;
				bone2.GetBodyPart().GetComponent<SpriteRenderer>().color = color2;
			}
		}
	}

	public void EnableElectrocute(float i_intensity01, Color i_colorElectrocution)
	{
		m_isElectrocute = true;
		m_coroutineElectrocute = StartCoroutine(CoroutineElectrocute(i_intensity01));
		m_coroutineAnimateElectrocuteColor = StartCoroutine(CoroutineAnimateElectrocuteColor(i_colorElectrocution));
	}

	private IEnumerator CoroutineElectrocute(float i_intensity01)
	{
		Dictionary<Bone, Transform> l_pose = CopyCurrentPoseTransform();
		for (int i = 0; i < m_bones.Count; i++)
		{
			_ = m_bones[i] == GetBone("hips");
		}
		while (m_isElectrocute)
		{
			for (int l_index = 0; l_index < m_bones.Count; l_index++)
			{
				Bone bone = m_bones[l_index];
				_ = bone == GetBone("hips");
				float num = 180f * i_intensity01;
				float torque = Random.Range(0f - num, num);
				l_pose.TryGetValue(bone, out var _);
				if ((bool)bone.GetRigidbody2D())
				{
					bone.GetRigidbody2D().AddTorque(torque);
				}
				bool flag = true;
				if ((bool)bone.GetComponentInChildren<ParticleSystem>() && bone.GetComponentInChildren<ParticleSystem>().gameObject.name == ResourceContainer.Resources.m_particleSmokeElectrocution.name)
				{
					flag = false;
				}
				if (flag)
				{
					GameObject obj = Object.Instantiate(ResourceContainer.Resources.m_particleSmokeElectrocution, bone.transform);
					obj.transform.localPosition = Vector3.zero;
					obj.SetActive(value: true);
					Object.Destroy(obj, 1f);
				}
				yield return new WaitForEndOfFrame();
			}
		}
	}

	public void DisableElectrocute()
	{
		m_isElectrocute = false;
		if (m_coroutineElectrocute != null)
		{
			StopCoroutine(m_coroutineElectrocute);
		}
		if (m_coroutineAnimateElectrocuteColor != null)
		{
			StopCoroutine(m_coroutineAnimateElectrocuteColor);
		}
		for (int i = 0; i < m_bones.Count; i++)
		{
			m_bones[i].GetBodyPart().GetComponent<SpriteRenderer>().color = Color.white;
		}
	}

	private IEnumerator CoroutineAnimateElectrocuteColor(Color i_colorElectrocution)
	{
		Color l_colorNormal = new Color(0.75f, 0.75f, 0.75f);
		for (int i = 0; i < m_bones.Count; i++)
		{
			m_bones[i].GetBodyPart().GetComponent<SpriteRenderer>().color = i_colorElectrocution;
		}
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		for (int j = 0; j < m_bones.Count; j++)
		{
			m_bones[j].GetBodyPart().GetComponent<SpriteRenderer>().color = l_colorNormal;
		}
		while (m_isElectrocute)
		{
			List<Bone> l_bonesColored = new List<Bone>();
			for (int l_index = 0; l_index < m_bones.Count; l_index++)
			{
				Bone bone = m_bones[l_index];
				bone.GetBodyPart().GetComponent<SpriteRenderer>().color = i_colorElectrocution;
				l_bonesColored.Add(bone);
				yield return new WaitForEndOfFrame();
				if (l_bonesColored.Count < m_bones.Count / 2)
				{
					continue;
				}
				foreach (Bone bone2 in m_bones)
				{
					bone2.GetBodyPart().GetComponent<SpriteRenderer>().color = l_colorNormal;
				}
				yield return new WaitForEndOfFrame();
				yield return new WaitForEndOfFrame();
				foreach (Bone bone3 in m_bones)
				{
					bone3.GetBodyPart().GetComponent<SpriteRenderer>().color = i_colorElectrocution;
				}
				yield return new WaitForEndOfFrame();
				foreach (Bone bone4 in m_bones)
				{
					bone4.GetBodyPart().GetComponent<SpriteRenderer>().color = l_colorNormal;
				}
				l_bonesColored.Clear();
			}
		}
	}

	public void EnableIgnition()
	{
		if (!m_isOnFire)
		{
			m_isOnFire = true;
			Bone bone = GetBone("hips");
			bool flag = true;
			if ((bool)bone.GetComponentInChildren<ParticleSystem>() && bone.GetComponentInChildren<ParticleSystem>().gameObject.name == ResourceContainer.Resources.m_particleFire.name)
			{
				flag = false;
			}
			if (flag)
			{
				GameObject gameObject = Object.Instantiate(ResourceContainer.Resources.m_particleFire, bone.transform);
				gameObject.transform.localPosition = Vector3.zero;
				gameObject.SetActive(value: true);
				m_particlesIgnition.Add(gameObject.GetComponent<ParticleSystem>());
			}
		}
	}

	public void DisableIgnition()
	{
		m_isOnFire = false;
		foreach (ParticleSystem item in m_particlesIgnition)
		{
			item.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmitting);
			Object.Destroy(item.gameObject, 1f);
		}
		m_particlesIgnition.Clear();
	}
}
