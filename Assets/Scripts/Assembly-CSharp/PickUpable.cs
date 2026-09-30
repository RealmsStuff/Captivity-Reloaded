using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpable : Item
{
	public delegate void DelOnPickUp();

	public delegate void DelOnDrop();

	[SerializeField]
	private AudioClip m_audioPickUp;

	[SerializeField]
	protected bool m_isCanDrop;

	[SerializeField]
	protected bool m_isStackAble;

	[SerializeField]
	private int m_value;

	[SerializeField]
	private int m_weight;

	[SerializeField]
	protected int m_amountAdditional;

	[SerializeField]
	protected List<GameObject> m_objectsToEnableAfterPickUp = new List<GameObject>();

	protected bool m_isPickedUp;

	protected bool m_isPickUpable = true;

	protected bool m_isPickedUpOnceAlready;

	protected Actor m_owner;

	protected ParticleSystem m_particleEffectPickUp;

	protected AudioSource m_audioSourceSFX;

	protected bool m_isVisible;

	private Coroutine m_coroutineAnimateOutline;

	public event DelOnPickUp OnPickUp;

	public event DelOnDrop OnDrop;

	public void ConfigureModWorldPickup(int i_amount)
	{
		StopAllCoroutines();
		m_owner = null;
		m_isPickedUp = false;
		m_isPickedUpOnceAlready = false;
		m_isPickUpable = true;
		m_amountAdditional = Mathf.Max(0, i_amount - 1);
		if ((bool)GetComponent<Rigidbody2D>())
		{
			GetComponent<Rigidbody2D>().isKinematic = false;
			GetComponent<Rigidbody2D>().velocity = Vector2.zero;
		}
		foreach (Collider2D itemCollider in GetComponentsInChildren<Collider2D>(includeInactive: true))
			itemCollider.enabled = true;
		foreach (SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>(includeInactive: true))
			renderer.sortingLayerName = "Item";
		Show();
		ShowOutline();
	}

	public void ConfigureModRewardAmount(int i_amount)
	{
		m_amountAdditional = Mathf.Max(0, i_amount - 1);
	}

	protected virtual void Awake()
	{
		ShowOutline();
		_ = m_particleEffectPickUp == null;
		m_audioSourceSFX = CommonReferences.Instance.GetManagerAudio().CreateAndAddAudioSourceSFX(base.gameObject);
	}

	private void OnEnable()
	{
		if (m_owner == null && m_isPickUpable) ShowOutline();
	}

	private void AddParticleEffect()
	{
		GameObject gameObject = CreateParticleEffect();
		if ((bool)GetComponent<SpriteRenderer>())
		{
			gameObject.transform.position = GetComponent<SpriteRenderer>().bounds.center;
		}
		else
		{
			gameObject.transform.position = base.transform.position;
		}
		gameObject.SetActive(value: true);
		m_particleEffectPickUp = gameObject.GetComponent<ParticleSystem>();
		if (m_owner != null)
		{
			m_particleEffectPickUp.gameObject.SetActive(value: false);
		}
	}

	private GameObject CreateParticleEffect()
	{
		if (this is Weapon)
		{
			return Object.Instantiate(ResourceContainer.Resources.m_particleEquippable, base.transform);
		}
		if (this is Usable)
		{
			return Object.Instantiate(ResourceContainer.Resources.m_particleUsable, base.transform);
		}
		return null;
	}

	public virtual void PickUp(Actor i_owner)
	{
		m_isPickedUp = true;
		if (!m_isPickedUpOnceAlready)
		{
			foreach (GameObject item in m_objectsToEnableAfterPickUp)
			{
				item.SetActive(value: true);
				if ((bool)item.GetComponent<NPC>())
				{
					item.GetComponent<NPC>().Spawn(i_isFadeIn: true);
				}
			}
		}
		m_isPickedUpOnceAlready = true;
		m_owner = i_owner;
		base.gameObject.transform.parent = i_owner.transform;
		base.gameObject.transform.position = i_owner.transform.position;
		base.gameObject.SetActive(value: false);
		if ((bool)GetComponent<Rigidbody2D>())
		{
			GetComponent<Rigidbody2D>().isKinematic = true;
		}
		Collider2D[] components = GetComponents<Collider2D>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].enabled = false;
		}
		if (m_owner is Player)
		{
			CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(Resources.Load<AudioClip>("Audio/Pickup1"));
			if (m_audioPickUp != null)
			{
				CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(m_audioPickUp);
			}
		}
		if (m_particleEffectPickUp != null)
		{
			m_particleEffectPickUp.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
			m_particleEffectPickUp.gameObject.SetActive(value: false);
		}
		if (this.OnPickUp != null)
		{
			this.OnPickUp();
		}
		HideOutline();
	}

	public void SetOwner(Actor i_owner)
	{
		m_owner = i_owner;
	}

	public virtual void Drop(Vector2 i_forceDrop)
	{
		m_isPickedUp = false;
		base.gameObject.SetActive(value: true);
		base.transform.position = m_owner.transform.position;
		m_owner = null;
		GetComponent<Rigidbody2D>().velocity = i_forceDrop;
		if ((bool)GetComponent<Rigidbody2D>())
		{
			GetComponent<Rigidbody2D>().isKinematic = false;
		}
		Collider2D[] components = GetComponents<Collider2D>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].enabled = true;
		}
		base.transform.SetParent(CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetItemsParent());
		if ((bool)GetComponent<SpriteRenderer>())
		{
			GetComponent<SpriteRenderer>().sortingLayerName = "Item";
		}
		SpriteRenderer[] componentsInChildren = GetComponentsInChildren<SpriteRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].sortingLayerName = "Item";
		}
		if (m_particleEffectPickUp != null && m_isPickUpable)
		{
			m_particleEffectPickUp.Clear(withChildren: true);
			m_particleEffectPickUp.gameObject.SetActive(value: true);
			m_particleEffectPickUp.Play();
		}
		if (this.OnDrop != null)
		{
			this.OnDrop();
		}
		if (!m_isVisible)
		{
			Show();
		}
		ShowOutline();
	}

	public virtual void Drop(float i_powerDrop01)
	{
		m_isPickedUp = false;
		float num = i_powerDrop01 * 10f;
		float x = Random.Range(0f - num, num);
		base.gameObject.SetActive(value: true);
		base.transform.position = m_owner.transform.position;
		m_owner = null;
		Vector2 velocity = GetComponent<Rigidbody2D>().velocity;
		velocity.y = num;
		velocity.x = x;
		GetComponent<Rigidbody2D>().velocity = velocity;
		if ((bool)GetComponent<Rigidbody2D>())
		{
			GetComponent<Rigidbody2D>().isKinematic = false;
		}
		Collider2D[] components = GetComponents<Collider2D>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].enabled = true;
		}
		base.transform.SetParent(CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetItemsParent());
		if ((bool)GetComponent<SpriteRenderer>())
		{
			GetComponent<SpriteRenderer>().sortingLayerName = "Item";
		}
		SpriteRenderer[] componentsInChildren = GetComponentsInChildren<SpriteRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].sortingLayerName = "Item";
		}
		if (m_particleEffectPickUp != null && m_isPickUpable)
		{
			m_particleEffectPickUp.gameObject.SetActive(value: true);
			m_particleEffectPickUp.Clear(withChildren: true);
			m_particleEffectPickUp.Play();
		}
		if (this.OnDrop != null)
		{
			this.OnDrop();
		}
		if (!m_isVisible)
		{
			Show();
		}
		ShowOutline();
	}

	public virtual void Drop()
	{
		m_isPickedUp = false;
		float num = 1f * 10f;
		float x = Random.Range(0f - num, num);
		base.gameObject.SetActive(value: true);
		base.transform.position = m_owner.transform.position;
		m_owner = null;
		Vector2 velocity = GetComponent<Rigidbody2D>().velocity;
		velocity.y = num;
		velocity.x = x;
		GetComponent<Rigidbody2D>().velocity = velocity;
		if ((bool)GetComponent<Rigidbody2D>())
		{
			GetComponent<Rigidbody2D>().isKinematic = false;
		}
		Collider2D[] components = GetComponents<Collider2D>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].enabled = true;
		}
		base.transform.SetParent(CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetItemsParent());
		if ((bool)GetComponent<SpriteRenderer>())
		{
			GetComponent<SpriteRenderer>().sortingLayerName = "Item";
		}
		SpriteRenderer[] componentsInChildren = GetComponentsInChildren<SpriteRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].sortingLayerName = "Item";
		}
		if (m_particleEffectPickUp != null && m_isPickUpable)
		{
			m_particleEffectPickUp.Clear(withChildren: true);
			m_particleEffectPickUp.gameObject.SetActive(value: true);
			m_particleEffectPickUp.Play();
		}
		if (this.OnDrop != null)
		{
			this.OnDrop();
		}
		if (!m_isVisible)
		{
			Show();
		}
		ShowOutline();
	}

	protected void ShowOutline()
	{
		if (!(m_owner != null))
		{
			Color value = Color.white;
			if (this is Gun)
			{
				value = Color.red;
			}
			if (this is Usable)
			{
				value = Color.magenta;
			}
			if (this is Consumable)
			{
				value = Color.yellow;
			}
			if ((bool)GetComponent<SpriteRenderer>())
			{
				GetComponent<SpriteRenderer>().material = Resources.Load<Material>("Materials/SpriteOutline");
				GetComponent<SpriteRenderer>().material.SetColor("_OutlineColor", value);
			}
			SpriteRenderer[] componentsInChildren = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
			foreach (SpriteRenderer obj in componentsInChildren)
			{
				obj.material = Resources.Load<Material>("Materials/SpriteOutline");
				obj.material.SetColor("_OutlineColor", value);
			}
			if (isActiveAndEnabled)
			{
				if (m_coroutineAnimateOutline != null) StopCoroutine(m_coroutineAnimateOutline);
				m_coroutineAnimateOutline = StartCoroutine(CoroutineAnimateOutline());
			}
		}
	}

	protected void HideOutline()
	{
		if (m_coroutineAnimateOutline != null)
		{
			StopCoroutine(m_coroutineAnimateOutline);
		}
		if ((bool)GetComponent<SpriteRenderer>())
		{
			GetComponent<SpriteRenderer>().material = Resources.Load<Material>("Materials/SpriteDiffuse");
		}
		SpriteRenderer[] componentsInChildren = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].material = Resources.Load<Material>("Materials/SpriteDiffuse");
		}
	}

	private IEnumerator CoroutineAnimateOutline()
	{
		SpriteRenderer l_rendererOnObj = GetComponent<SpriteRenderer>();
		List<SpriteRenderer> l_renderersInChildren = new List<SpriteRenderer>();
		SpriteRenderer[] componentsInChildren = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
		foreach (SpriteRenderer item in componentsInChildren)
		{
			l_renderersInChildren.Add(item);
		}
		while (m_owner == null)
		{
			float l_weightFrom = 0f;
			float l_weightTo = 0.75f;
			float l_timeToMove = 0.5f;
			float l_timeCurrent = 0f;
			while (l_timeCurrent < l_timeToMove)
			{
				l_timeCurrent += Time.fixedDeltaTime;
				float i_time = l_timeCurrent / l_timeToMove;
				float value = AnimationTools.CalculateOverTime(AnimationTools.Transition.Smooth, AnimationTools.Transition.Steep, l_weightFrom, l_weightTo, i_time);
				if (l_rendererOnObj != null)
				{
					l_rendererOnObj.material.SetFloat("_OutlineThickness", value);
				}
				for (int j = 0; j < l_renderersInChildren.Count; j++)
				{
					l_renderersInChildren[j].material.SetFloat("_OutlineThickness", value);
				}
				yield return new WaitForFixedUpdate();
			}
			l_weightFrom = 0.75f;
			l_weightTo = 0f;
			l_timeToMove = 0.5f;
			l_timeCurrent = 0f;
			while (l_timeCurrent < l_timeToMove)
			{
				l_timeCurrent += Time.fixedDeltaTime;
				float i_time2 = l_timeCurrent / l_timeToMove;
				float value2 = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Smooth, l_weightFrom, l_weightTo, i_time2);
				if (l_rendererOnObj != null)
				{
					l_rendererOnObj.material.SetFloat("_OutlineThickness", value2);
				}
				for (int k = 0; k < l_renderersInChildren.Count; k++)
				{
					l_renderersInChildren[k].material.SetFloat("_OutlineThickness", value2);
				}
				yield return new WaitForFixedUpdate();
			}
		}
	}

	public virtual void Show()
	{
		SpriteRenderer[] componentsInChildren = GetComponentsInChildren<SpriteRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = true;
		}
		m_isVisible = true;
	}

	public virtual void Hide()
	{
		SpriteRenderer[] componentsInChildren = GetComponentsInChildren<SpriteRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = false;
		}
		m_isVisible = false;
	}

	public bool GetIsCanDrop()
	{
		return m_isCanDrop;
	}

	public Sprite GetSpriteIcon()
	{
		return m_sprItem;
	}

	public bool GetIsStackable()
	{
		return m_isStackAble;
	}

	public int GetAmount()
	{
		return m_amountAdditional + 1;
	}

	public void IncreaseAmount(int i_amount)
	{
		m_amountAdditional += i_amount;
	}

	public void DecreaseAmount(int i_amount)
	{
		m_amountAdditional -= i_amount;
		if (m_amountAdditional < 0)
		{
			Object.Destroy(base.gameObject);
		}
	}

	public int GetValue()
	{
		return ExternalRuleProfileFactory.ApplyPickupValue(this, m_value);
	}

	public int GetWeight()
	{
		return m_weight;
	}

	public void ConfigureModEconomy(int? i_weight, int? i_value)
	{
		if (i_weight.HasValue) m_weight = i_weight.Value;
		if (i_value.HasValue) m_value = i_value.Value;
	}

	public void ConfigureModPickup(bool i_canDrop, bool i_stackable = false)
	{
		m_isCanDrop = i_canDrop;
		m_isStackAble = i_stackable;
	}

	public bool GetIsPickUpable()
	{
		return m_isPickUpable;
	}

	public void SetIsPickUpable(bool i_isPickUpable)
	{
		m_isPickUpable = i_isPickUpable;
	}

	public bool IsPickedUp()
	{
		return m_isPickedUp;
	}
}
