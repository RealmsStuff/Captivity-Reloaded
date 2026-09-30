using System.Collections;
using UnityEngine;

public class UtilityTools : MonoBehaviour
{
	public void DestroyObjectAfterTime(GameObject i_objectToDestroy, float i_secondsBeforeDestroy)
	{
		StartCoroutine(CoroutineDestroyObjectAfterTime(i_objectToDestroy, i_secondsBeforeDestroy));
	}

	private IEnumerator CoroutineDestroyObjectAfterTime(GameObject i_objectToDestroy, float i_secondsBeforeDestroy)
	{
		yield return new WaitForSeconds(i_secondsBeforeDestroy);
		Object.Destroy(i_objectToDestroy);
	}

	public ParticleSystem CreateParticleDuplicateAndPlay(ParticleSystem i_particleToDuplicate, Transform i_parent)
	{
		ParticleSystem particleSystem = Object.Instantiate(i_particleToDuplicate, i_parent);
		particleSystem.transform.position = i_particleToDuplicate.transform.position;
		particleSystem.transform.localScale = i_particleToDuplicate.transform.localScale;
		particleSystem.gameObject.SetActive(value: true);
		particleSystem.Play();
		return particleSystem;
	}

	public ParticleSystem CreateParticleDuplicateInCurrentLevelAndPlay(ParticleSystem i_particleToDuplicate)
	{
		ParticleSystem particleSystem = Object.Instantiate(i_particleToDuplicate, CommonReferences.Instance.GetManagerStages().GetStageCurrent().transform);
		particleSystem.transform.position = i_particleToDuplicate.transform.position;
		particleSystem.transform.localScale = i_particleToDuplicate.transform.localScale;
		particleSystem.gameObject.SetActive(value: true);
		particleSystem.Play();
		return particleSystem;
	}

	public ParticleSystem CreateParticleDuplicateInCurrentLevelAndPlay(ParticleSystem i_particleToDuplicate, Vector2 i_positionCustom)
	{
		ParticleSystem particleSystem = Object.Instantiate(i_particleToDuplicate, CommonReferences.Instance.GetManagerStages().GetStageCurrent().transform);
		particleSystem.transform.position = i_positionCustom;
		particleSystem.transform.localScale = i_particleToDuplicate.transform.localScale;
		particleSystem.gameObject.SetActive(value: true);
		particleSystem.Play();
		return particleSystem;
	}

	public Vector2 GetPosMouse()
	{
		Canvas component = CommonReferences.Instance.GetManagerHud().GetComponent<Canvas>();
		RectTransformUtility.ScreenPointToLocalPointInRectangle(component.transform as RectTransform, Input.mousePosition, component.worldCamera, out var localPoint);
		return component.transform.TransformPoint(localPoint);
	}

	public Vector3 GetPosMousePerspectiveCamera()
	{
		Player player = CommonReferences.Instance.GetPlayer();
		ManagerInput managerInput = CommonReferences.Instance.GetManagerInput();
		if (player != null && managerInput.IsControllerAiming())
		{
			return managerInput.GetAimWorldPosition(player.transform.position);
		}
        // If mobile controls are active, fake the mouse position to be straight in front of the player
        if (CommonReferences.Instance.GetPlayerController().GetIsMobileControlsEnabled())
        {
            if (player != null)
            {
				return CommonReferences.Instance.GetPlayerController().GetMobileAimWorldPosition(player.transform.position);
            }
        }

		return managerInput.GetAimWorldPosition(player != null ? player.transform.position : Vector3.zero);
	}

	public Vector2 GetCalculateTopOfSprite(SpriteRenderer i_sprite)
	{
		return new Vector2(base.transform.position.x, i_sprite.bounds.center.y + i_sprite.bounds.size.y / 2f);
	}

	public IEnumerator CoroutineWaitForAnimationClip(AnimationClip i_animClip)
	{
		yield return new WaitForSeconds(i_animClip.length);
	}

	public IEnumerator CoroutineWaitForAnimationClip(Animator i_animator)
	{
		yield return new WaitForSeconds(i_animator.GetCurrentAnimatorClipInfo(0)[0].clip.length);
	}

	public Vector2 WorldPosToCanvasPos(Vector2 i_posWorld)
	{
		Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(CommonReferences.Instance.GetManagerCamerasXGame().GetCameraXGameCurrent().GetCameraUnity(), i_posWorld);
		RectTransformUtility.ScreenPointToLocalPointInRectangle(CommonReferences.Instance.GetManagerHud().GetCanvasCamera().GetComponent<RectTransform>(), screenPoint, CommonReferences.Instance.GetManagerCamerasXGame().GetCameraXGameCurrent().GetCameraUnity(), out var localPoint);
		return localPoint;
	}
}
