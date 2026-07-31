using System.Collections;
using UnityEngine;

public class ManagerFacePlayer : MonoBehaviour
{
	private Player m_player;

	private ManagerPlayerMouth m_managerPlayerMouth;

	[SerializeField]
	private SpriteRenderer m_iris;

	[SerializeField]
	private Color m_colorBlue;

	[SerializeField]
	private Color m_colorBrown;

	[SerializeField]
	private Color m_colorGreen;

	[SerializeField]
	private Color m_colorYellow;

	private Animator m_animatorFace;

	private EyeColor m_eyeColor;

	private Color m_colorEye;

	private bool m_isKO;

	private Coroutine m_coroutineOrgasm;

	private void Awake()
	{
		m_animatorFace = GetComponent<Animator>();
		m_managerPlayerMouth = GetComponent<ManagerPlayerMouth>();
	}

	private void Start()
	{
		m_player = CommonReferences.Instance.GetPlayer();
	}

	private void Update()
	{
		HandleFace();
		HandleAnimatorParameters();
	}

	private void LateUpdate()
	{
		if (!m_player.IsHypnotized())
		{
			UpdateEyeColor();
		}
	}

	private void UpdateEyeColor()
	{
		m_iris.color = m_colorEye;
	}

	private void HandleAnimatorParameters()
	{
		if (m_player.GetIsBeingRaped())
		{
			m_animatorFace.SetBool("IsRape", value: true);
		}
		else
		{
			m_animatorFace.SetBool("IsRape", value: false);
		}
		if (m_player.IsDead())
		{
			m_animatorFace.SetBool("IsDead", value: true);
		}
	}

	public void TakeDamage()
	{
		if (m_player.GetStateActorCurrent() == StateActor.Ragdoll && !m_player.GetIsBeingRaped() && !m_isKO)
		{
			KO();
		}
		else if (!m_isKO)
		{
			m_animatorFace.SetTrigger("TakeHit");
			m_managerPlayerMouth.TakeDamage();
		}
	}

	public void KO()
	{
		m_animatorFace.SetLayerWeight(m_animatorFace.GetLayerIndex("KO"), 1f);
		m_isKO = true;
		m_managerPlayerMouth.KO();
	}

	public void UnKO()
	{
		m_animatorFace.SetLayerWeight(m_animatorFace.GetLayerIndex("KO"), 0f);
		m_isKO = false;
		m_managerPlayerMouth.UnKO();
	}

	public void Orgasm()
	{
		if (!m_isKO)
		{
			m_animatorFace.SetTrigger("Orgasm");
			if (m_coroutineOrgasm != null)
			{
				StopCoroutine(m_coroutineOrgasm);
			}
			m_coroutineOrgasm = StartCoroutine(CoroutineOrgasm());
		}
	}

	private IEnumerator CoroutineOrgasm()
	{
		yield return new WaitForSeconds(3f);
		float num;
		if (m_player.IsDead() || m_player.GetNumOfHeartsCurrent() == 0)
		{
			num = 1f;
		}
		else
		{
			float num2 = m_player.GetLibidoCurrent() / m_player.GetLibidoMax();
			float num3 = m_player.GetPleasureCurrent() / m_player.GetPleasureMax();
			num = (num2 + num3) / 2f;
		}
		float l_weightFrom = 1f;
		float l_weightTo = num;
		float l_timeToMove = 3f;
		float l_timeCurrent = 0f;
		while (l_timeCurrent < l_timeToMove)
		{
			l_timeCurrent += Time.fixedDeltaTime;
			float i_time = l_timeCurrent / l_timeToMove;
			float weight = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Smooth, l_weightFrom, l_weightTo, i_time);
			m_animatorFace.SetLayerWeight(2, weight);
			yield return new WaitForFixedUpdate();
		}
		m_coroutineOrgasm = null;
	}

	private void HandleFace()
	{
		if (m_coroutineOrgasm == null)
		{
			if (m_player.GetNumOfHeartsCurrent() == 0)
			{
				m_animatorFace.SetLayerWeight(2, 1f);
				return;
			}
			float num = m_player.GetLibidoCurrent() / m_player.GetLibidoMax();
			float num2 = m_player.GetPleasureCurrent() / m_player.GetPleasureMax();
			float weight = (num + num2) / 2f;
			m_animatorFace.SetLayerWeight(2, weight);
		}
	}

	public void ApplyRaper(Raper i_raper)
	{
		m_managerPlayerMouth.ApplyRaper(i_raper);
	}

	public void WakeUp()
	{
		m_animatorFace.Play("WakeUp");
	}

	public void WakeUpGameOver()
	{
		m_animatorFace.Play("WakeUpGameOver");
	}

	public Animator GetAnimatorFace()
	{
		return m_animatorFace;
	}

	public void SetColorIris(EyeColor i_color)
	{
		m_eyeColor = i_color;
		switch (i_color)
		{
		case EyeColor.Blue:
			m_colorEye = m_colorBlue;
			break;
		case EyeColor.Brown:
			m_colorEye = m_colorBrown;
			break;
		case EyeColor.Green:
			m_colorEye = m_colorGreen;
			break;
		case EyeColor.Yellow:
			m_colorEye = m_colorYellow;
			break;
		}
	}

	public Color GetColorEye(EyeColor i_eyeColor)
	{
		switch (i_eyeColor)
		{
		case EyeColor.Blue:
			return m_colorBlue;
		case EyeColor.Brown:
			return m_colorBrown;
		case EyeColor.Green:
			return m_colorGreen;
		case EyeColor.Yellow:
			return m_colorYellow;
		default:
			return Color.white;
		}
	}

	public bool IsKO()
	{
		return m_isKO;
	}

	public ManagerPlayerMouth GetManagerPlayerMouth()
	{
		return m_managerPlayerMouth;
	}
}
