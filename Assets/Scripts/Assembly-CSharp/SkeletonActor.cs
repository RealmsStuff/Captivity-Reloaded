using System.Collections;
using UnityEngine;

public class SkeletonActor : Skeleton
{
    public bool isBlackCharacter = false;
    private Coroutine m_coroutineFlashingLeapAttack;

	protected override void Awake()
	{
		base.Awake();
	}

    void Start()
    {
        if (isBlackCharacter)
        {
            SetColorOnAllBodyParts(Color.black);
        }
    }

    protected override void HandleEnableRagdoll()
	{
		foreach (Bone bone in m_bones)
		{
			if ((bool)bone.GetRigidbody2D())
			{
				bone.GetRigidbody2D().isKinematic = false;
				bone.GetRigidbody2D().velocity = GetOwner().GetVelocity();
			}
		}
	}

	protected override void CenterSkeleton()
	{
		Vector3 position = GetBoneHips().transform.position;
		GetOwner().SetPos(position);
		GetBoneHips().transform.localPosition = Vector3.zero;
	}

	public void StartFlashingLeapAttack()
	{
		if (m_coroutineFlashingLeapAttack != null)
		{
			StopCoroutine(m_coroutineFlashingLeapAttack);
		}
		m_coroutineFlashingLeapAttack = StartCoroutine(CoroutineFlashRed());
	}

	public void StopFlashingLeapAttack()
	{
		if (m_coroutineFlashingLeapAttack != null)
		{
			StopCoroutine(m_coroutineFlashingLeapAttack);
		}
		SetColorOnAllBodyParts(Color.white);
	}

	private IEnumerator CoroutineFlashRed()
	{
		while (true)
		{
			SetColorOnAllBodyParts(Color.red);
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			SetColorOnAllBodyParts(Color.white);
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
		}
	}

	public void FadeBodyIn()
	{
		StartCoroutine(CoroutineFadeBodyIn());
	}

	private IEnumerator CoroutineFadeBodyIn()
	{
		float l_weightFrom = 0f;
		float l_weightTo = 1f;
		float l_timeToMove = 0.5f;
		float l_timeCurrent = 0f;
		while (l_timeCurrent < l_timeToMove)
		{
			l_timeCurrent += Time.fixedDeltaTime;
			float i_time = l_timeCurrent / l_timeToMove;
			float a = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Smooth, l_weightFrom, l_weightTo, i_time);
			SetColorOnAllBodyParts(new Color(1f, 1f, 1f, a));
			yield return new WaitForFixedUpdate();
		}
	}

    public void SetColorOnAllBodyParts(Color i_color)
    {
        // NEW ADDITION: Intercept the color if this is a black character
        if (isBlackCharacter)
        {
            // If the script tries to turn it pure white, turn it pure black instead
            if (i_color == Color.white)
            {
                i_color = Color.black;
            }
            // If the script tries to fade it in using white (1f, 1f, 1f), keep the fade (alpha) but make it black
            else if (i_color.r == 1f && i_color.g == 1f && i_color.b == 1f)
            {
                i_color = new Color(0f, 0f, 0f, i_color.a);
            }
        }

        foreach (Bone bone in m_bones)
        {
            if ((bool)bone.GetBodyPart())
            {
                bone.GetBodyPart().GetComponent<SpriteRenderer>().color = i_color;
            }
        }
    }

    public Bone GetBoneHips()
	{
		return GetBone("hips");
	}

	public Bone GetBoneHead()
	{
		Bone bone = GetBone("head");
		if (bone == null)
		{
			return GetBoneHips();
		}
		return bone;
	}

	public void SetToRape()
	{
		SpriteRenderer[] componentsInChildren = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].sortingLayerName = "Player";
		}
	}

	public void SetToDefault()
	{
		SpriteRenderer[] componentsInChildren = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].sortingLayerName = "Actor";
		}
	}

	private void AnimEventPerformAttack()
	{
		GetComponentInParent<NPC>().PerformAttack(CommonReferences.Instance.GetPlayer());
	}

	private void AnimEventPlayAudioUnique(int i_numAudioUnique)
	{
		GetComponentInParent<Actor>().PlayAudioUnique(i_numAudioUnique - 1);
	}

	private void AnimEventLeap()
	{
		((AttackLeap)GetComponentInParent<NPC>().GetAttackCurrent()).Leap();
	}
}
