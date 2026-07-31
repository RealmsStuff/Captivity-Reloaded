using UnityEngine;

public class Rope : MonoBehaviour
{
	private GameObject m_obj1;

	private GameObject m_obj2;

	private bool m_isAttached;

	public void Attach(GameObject i_object1, GameObject i_object2)
	{
		m_obj1 = i_object1;
		m_obj2 = i_object2;
		m_isAttached = true;
	}

	private void Update()
	{
		if (m_isAttached)
		{
			StretchRope();
		}
	}

	public void StretchRope()
	{
		Vector2 vector = m_obj1.transform.position;
		Vector2 vector2 = m_obj2.transform.position;
		Vector2 vector3 = (vector + vector2) / 2f;
		base.transform.position = vector3;
		Vector2 vector4 = vector2 - vector;
		vector4 = Vector3.Normalize(vector4);
		base.transform.right = vector4;
		Vector2 size = GetComponent<SpriteRenderer>().size;
		size.x = Vector2.Distance(vector, vector2);
		GetComponent<SpriteRenderer>().size = size;
	}
}
