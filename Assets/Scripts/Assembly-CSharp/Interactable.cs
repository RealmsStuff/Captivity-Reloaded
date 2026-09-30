using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
	public delegate void DelOnActivate(Interactable i_interactble);
	public delegate void DelAnyActivated(Interactable i_interactable, Actor i_initiator, InteractableActivationType i_activationType);

	public static event DelAnyActivated OnAnyActivated;

	[SerializeField]
	protected string m_name;

	[SerializeField]
	protected int m_priceToActivate;

	[SerializeField]
	protected bool m_isSingleUse;

	[SerializeField]
	protected bool m_isContinuesActivation;

	[SerializeField]
	protected bool m_isCanBeUsedToActivate;

	[SerializeField]
	protected bool m_isCanBeTouchedToActivate;

	[SerializeField]
	protected bool m_isCanBeShotToActivate;

	[SerializeField]
	protected bool m_isCanBeActivatedByNPC;

	[SerializeField]
	protected bool m_isShowOutline;

	[SerializeField]
	protected bool m_isHideNotificationInteract;

	[SerializeField]
	protected bool m_isObstructionPaths;

	[SerializeField]
	protected bool m_isUnInteractable;

	[SerializeField]
	protected List<GameObject> m_objectsToEnableAfterActivation = new List<GameObject>();

	[SerializeField]
	protected List<GameObject> m_objectsToDisableAfterActivation = new List<GameObject>();

	[SerializeField]
	protected List<Spawner> m_spawnersToEnableAfterActivation = new List<Spawner>();

	[SerializeField]
	protected List<Interactable> m_interactablesToActivateAfterActivation = new List<Interactable>();

	protected AudioSource m_audioSourceSFX;

	protected bool m_isActivatedOnceAlready;

	private Notification m_notificationInteract;

	protected Coroutine m_coroutineAnimateOutline;

	public event DelOnActivate OnActivate;

	private void Awake()
	{
		AddAudioSource();
		if (m_isCanBeUsedToActivate && m_isShowOutline)
		{
			ShowOutline();
		}
	}

	protected virtual void Start()
	{
	}

	private void AddAudioSource()
	{
		m_audioSourceSFX = CommonReferences.Instance.GetManagerAudio().CreateAndAddAudioSourceSFX(base.gameObject);
	}

	protected virtual void LateUpdate()
	{
		if (!m_isUnInteractable && m_isCanBeUsedToActivate)
		{
			HandleNotification();
		}
	}

	private void HandleNotification()
	{
		if ((m_isSingleUse && m_isActivatedOnceAlready) || (m_priceToActivate > 0 && m_isActivatedOnceAlready) || m_isHideNotificationInteract || CommonReferences.Instance.GetPlayerController().GetIsForceIgnoreInput())
		{
			DestroyNotification();
		}
		else if (m_notificationInteract == null)
		{
			if (Vector2.Distance(CommonReferences.Instance.GetPlayer().GetPos(), base.transform.position) < 2.25f)
			{
				string text = "";
				text = text + "Press " + CommonReferences.Instance.GetManagerInput().GetPromptBindingName(InputButton.Use) + " ";
				if (m_priceToActivate > 0)
				{
					text = text + "(" + m_priceToActivate + "$) ";
				}
				text += "to use";
				m_notificationInteract = CommonReferences.Instance.GetManagerHud().GetManagerNotification().CreateNotification(text, ColorTextNotification.UnlockDoor, i_isContinues: true);
				m_notificationInteract.SetPromptInput(InputButton.Use);
			}
		}
		else if (Vector2.Distance(CommonReferences.Instance.GetPlayer().GetPos(), base.transform.position) <= 2.25f)
		{
			string text2 = "Press " + CommonReferences.Instance.GetManagerInput().GetPromptBindingName(InputButton.Use) + " ";
			if (m_priceToActivate > 0)
			{
				text2 = text2 + "(" + m_priceToActivate + ((char)36).ToString() + ") ";
			}
			text2 += "to use";
			m_notificationInteract.SetText(text2);
		}
		else if (Vector2.Distance(CommonReferences.Instance.GetPlayer().GetPos(), base.transform.position) > 2.25f)
		{
			CommonReferences.Instance.GetManagerHud().GetManagerNotification().DestroyNotification(m_notificationInteract);
			m_notificationInteract = null;
		}
	}

	protected void DestroyNotification()
	{
		if (m_notificationInteract != null)
		{
			CommonReferences.Instance.GetManagerHud().GetManagerNotification().DestroyNotification(m_notificationInteract);
			m_notificationInteract = null;
		}
	}

	protected virtual void OnTriggerEnter2D(Collider2D i_collider)
	{
		if (!m_isContinuesActivation)
		{
			if (m_isCanBeTouchedToActivate && (bool)i_collider.gameObject.GetComponent<BodyPartPlayer>())
			{
				Actor owner = i_collider.gameObject.GetComponent<BodyPartPlayer>().GetOwner();
				Activate(owner, InteractableActivationType.Touch);
			}
			if (m_isCanBeActivatedByNPC && (bool)i_collider.gameObject.GetComponent<BodyPartActor>())
			{
				Actor owner2 = i_collider.gameObject.GetComponent<BodyPartActor>().GetOwner();
				Activate(owner2, InteractableActivationType.NPC);
			}
		}
	}

	protected virtual void OnCollisionEnter2D(Collision2D i_collision)
	{
		if (!m_isContinuesActivation)
		{
			if (m_isCanBeTouchedToActivate && (bool)i_collision.collider.gameObject.GetComponent<BodyPartPlayer>())
			{
				Actor owner = i_collision.collider.gameObject.GetComponent<BodyPartPlayer>().GetOwner();
				Activate(owner, InteractableActivationType.Touch);
			}
			if (m_isCanBeActivatedByNPC && (bool)i_collision.collider.gameObject.GetComponent<BodyPartActor>())
			{
				Actor owner2 = i_collision.collider.gameObject.GetComponent<BodyPartActor>().GetOwner();
				Activate(owner2, InteractableActivationType.NPC);
			}
		}
	}

	protected virtual void OnTriggerStay2D(Collider2D i_collider)
	{
		if (m_isContinuesActivation)
		{
			if (m_isCanBeTouchedToActivate && (bool)i_collider.gameObject.GetComponent<BodyPartPlayer>())
			{
				Actor owner = i_collider.gameObject.GetComponent<BodyPartPlayer>().GetOwner();
				Activate(owner, InteractableActivationType.Touch);
			}
			if (m_isCanBeActivatedByNPC && (bool)i_collider.gameObject.GetComponent<NPC>())
			{
				Actor component2 = i_collider.gameObject.GetComponent<NPC>();
				Activate(component2, InteractableActivationType.NPC);
			}
		}
	}

	protected virtual void OnCollisionStay2D(Collision2D i_collision)
	{
		if (m_isContinuesActivation)
		{
			if (m_isCanBeTouchedToActivate && (bool)i_collision.collider.gameObject.GetComponent<BodyPartPlayer>())
			{
				Actor owner = i_collision.collider.gameObject.GetComponent<BodyPartPlayer>().GetOwner();
				Activate(owner, InteractableActivationType.Touch);
			}
			if (m_isCanBeActivatedByNPC && (bool)i_collision.collider.gameObject.GetComponent<NPC>())
			{
				Actor component2 = i_collision.collider.gameObject.GetComponent<NPC>();
				Activate(component2, InteractableActivationType.NPC);
			}
		}
	}

	public virtual void Use()
	{
		if (m_isUnInteractable || !m_isCanBeUsedToActivate)
		{
			return;
		}
		if (!m_isActivatedOnceAlready && m_priceToActivate > 0)
		{
			if (CommonReferences.Instance.GetPlayerController().GetInventory().GetMoney() < m_priceToActivate)
			{
				CommonReferences.Instance.GetManagerHud().GetManagerNotification().CreateNotification("Not enough money", ColorTextNotification.Other, i_isContinues: false);
				return;
			}
			CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(Resources.Load<AudioClip>("Audio/Buy"));
			CommonReferences.Instance.GetPlayerController().GetInventory().DepleteMoney(m_priceToActivate);
		}
		Activate(CommonReferences.Instance.GetPlayer(), InteractableActivationType.Use);
	}

	public virtual void Activate(Actor i_initiator, InteractableActivationType i_activationType)
	{
		if (m_isUnInteractable || (m_isSingleUse && m_isActivatedOnceAlready))
		{
			return;
		}
		if (this.OnActivate != null)
		{
			this.OnActivate(this);
		}
		if ((bool)GetComponent<TriggerInteractable>())
		{
			GetComponent<TriggerInteractable>().Trigger();
		}
		HandleActivation(i_initiator, i_activationType);
		if (!m_isActivatedOnceAlready)
		{
			foreach (GameObject item in m_objectsToEnableAfterActivation)
			{
				item.SetActive(value: true);
				if ((bool)item.GetComponent<NPC>())
				{
					item.GetComponent<NPC>().Spawn(i_isFadeIn: true);
				}
			}
			foreach (GameObject item2 in m_objectsToDisableAfterActivation)
			{
				item2.SetActive(value: false);
			}
			foreach (Spawner item3 in m_spawnersToEnableAfterActivation)
			{
				item3.Enable();
			}
			foreach (Interactable item4 in m_interactablesToActivateAfterActivation)
			{
				item4.Activate(CommonReferences.Instance.GetPlayer(), InteractableActivationType.Operator);
			}
		}
		m_isActivatedOnceAlready = true;
		OnAnyActivated?.Invoke(this, i_initiator, i_activationType);
	}

	protected abstract void HandleActivation(Actor i_initiator, InteractableActivationType i_activationType);

	protected void ResetModActivationLinks()
	{
		m_objectsToEnableAfterActivation.Clear();
		m_objectsToDisableAfterActivation.Clear();
		m_spawnersToEnableAfterActivation.Clear();
		m_interactablesToActivateAfterActivation.Clear();
		m_isActivatedOnceAlready = false;
	}

	public void RemapModSpawnerActivationLinks(Dictionary<Spawner, Spawner> i_replacements)
	{
		if (i_replacements == null) return;
		for (int index = 0; index < m_spawnersToEnableAfterActivation.Count; index++)
			if (m_spawnersToEnableAfterActivation[index] != null
				&& i_replacements.TryGetValue(m_spawnersToEnableAfterActivation[index], out Spawner replacement))
			{
				m_spawnersToEnableAfterActivation[index] = replacement;
				if (this is Door)
				{
					ModRetainedCoreStageObject retained = GetComponent<ModRetainedCoreStageObject>();
					replacement.AddRequiredOpenDoor(retained != null && !string.IsNullOrEmpty(retained.ObjectId)
						? retained.ObjectId : gameObject.name);
				}
			}
	}

	public bool GetIsCanBeUsedToActivate()
	{
		return m_isCanBeUsedToActivate;
	}

	public bool GetIsCanBeTouchedToActivate()
	{
		return m_isCanBeTouchedToActivate;
	}

	public bool GetIsCanBeShotToActivate()
	{
		return m_isCanBeShotToActivate;
	}

	public bool GetIsCanBeActivatedByNPC()
	{
		return m_isCanBeActivatedByNPC;
	}

	public bool GetIsUnInteractable()
	{
		return m_isUnInteractable;
	}

	public void SetIsUnInteractable(bool i_isUnInteractable)
	{
		m_isUnInteractable = i_isUnInteractable;
	}

	protected void ShowOutline()
	{
		Color blue = Color.blue;
		if ((bool)GetComponent<SpriteRenderer>())
		{
			GetComponent<SpriteRenderer>().material = Resources.Load<Material>("Materials/SpriteOutline");
			GetComponent<SpriteRenderer>().material.SetColor("_OutlineColor", blue);
		}
		SpriteRenderer[] componentsInChildren = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
		foreach (SpriteRenderer obj in componentsInChildren)
		{
			obj.material = Resources.Load<Material>("Materials/SpriteOutline");
			obj.material.SetColor("_OutlineColor", blue);
		}
		m_coroutineAnimateOutline = StartCoroutine(CoroutineAnimateOutline());
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
		while (IsCanBeUsed())
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
				if ((bool)GetComponent<SpriteRenderer>())
				{
					GetComponent<SpriteRenderer>().material.SetFloat("_OutlineThickness", value);
				}
				SpriteRenderer[] componentsInChildren = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].material.SetFloat("_OutlineThickness", value);
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
				if ((bool)GetComponent<SpriteRenderer>())
				{
					GetComponent<SpriteRenderer>().material.SetFloat("_OutlineThickness", value2);
				}
				SpriteRenderer[] componentsInChildren = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].material.SetFloat("_OutlineThickness", value2);
				}
				yield return new WaitForFixedUpdate();
			}
		}
		HideOutline();
	}

	private bool IsCanBeUsed()
	{
		if (!m_isCanBeUsedToActivate)
		{
			return false;
		}
		if (m_isSingleUse && m_isActivatedOnceAlready)
		{
			return false;
		}
		return true;
	}

	public bool IsObstructionPaths()
	{
		return m_isObstructionPaths;
	}
}
