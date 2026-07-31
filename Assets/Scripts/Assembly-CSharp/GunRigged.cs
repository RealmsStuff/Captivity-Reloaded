using System.Collections;
using UnityEngine;


public class GunRigged : Interactable
{
	[SerializeField]
	private AudioClip m_audioShoot;

	[SerializeField]
	private SpriteRenderer m_spriteRendererMuzzleFlash;

	[SerializeField]
	private UnityEngine.Rendering.Universal.Light2D m_lightFire;

	protected override void HandleActivation(Actor i_initiator, InteractableActivationType i_activationType)
	{
		Fire();
	}

	private void Fire()
	{
		StartCoroutine(CoroutineFire());
	}

	private IEnumerator CoroutineFire()
	{
		yield return new WaitForSeconds(0.75f);
		CommonReferences.Instance.GetPlayer().GetSkeletonPlayer().GetBone(BoneTypePlayer.Head)
			.GetBodyPart()
			.Explode();
		CommonReferences.Instance.GetPlayer().GetSkeletonPlayer().GetBone(BoneTypePlayer.Ear)
			.GetBodyPart()
			.Explode();
		CommonReferences.Instance.GetPlayer().GetSkeletonPlayer().GetManagerFacePlayer()
			.gameObject.SetActive(value: false);
		CommonReferences.Instance.GetPlayer().EnableRagdoll();
		CommonReferences.Instance.GetPlayer().GetSkeletonPlayer().GetBone(BoneTypePlayer.Head)
			.GetRigidbody2D()
			.AddForce(new Vector2(-150f, 0f), ForceMode2D.Impulse);
		CommonReferences.Instance.GetPlayer().GetSkeletonPlayer().GetBone(BoneTypePlayer.Hips)
			.GetRigidbody2D()
			.AddForce(new Vector2(-50f, 0f), ForceMode2D.Impulse);
		CommonReferences.Instance.GetPlayerController().SetIsForceIgnoreInput(i_isForceIgnoreInput: true);
		CommonReferences.Instance.GetPlayer().Die();
		m_audioSourceSFX.PlayOneShot(m_audioShoot);
		m_spriteRendererMuzzleFlash.enabled = true;
		m_lightFire.enabled = true;
		yield return new WaitForEndOfFrame();
		m_spriteRendererMuzzleFlash.enabled = false;
		yield return new WaitForEndOfFrame();
		m_lightFire.enabled = false;
		yield return new WaitForSeconds(1f);
		CommonReferences.Instance.GetManagerScreens().OpenScreenEnd();
	}
}
