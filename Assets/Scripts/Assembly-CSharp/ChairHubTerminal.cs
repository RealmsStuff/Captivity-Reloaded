using System.Collections;
using UnityEngine;

public class ChairHubTerminal : Interactable
{
	protected override void HandleActivation(Actor i_initiator, InteractableActivationType i_activationType)
	{
		Sit();
	}

	private void Sit()
	{
		CommonReferences.Instance.GetPlayer().SetIsForceIgnoreInput(i_isForceIgnoreInput: true);
		CommonReferences.Instance.GetPlayer().SetIsFacingLeft(i_isFacingLeft: false);
		CommonReferences.Instance.GetPlayer().GetRigidbody2D().bodyType = RigidbodyType2D.Static;
		StartCoroutine(CoroutineWaitForSit());
	}

	private IEnumerator CoroutineWaitForSit()
	{
		CommonReferences.Instance.GetPlayer().SetPos(new Vector2(base.transform.position.x, CommonReferences.Instance.GetPlayer().GetPos().y));
		CommonReferences.Instance.GetPlayer().GetAnimator().Play("SitChair");
		yield return new WaitForSeconds(1f);
		GameObject l_overlay = CommonReferences.Instance.GetManagerHud().GetManagerOverlay().PlayOverlay(new Color(1f, 0f, 0f, 1f), i_isUseOverlayWithHole: false, i_isDestroyOverlayAfterAnimation: false, 0.15f, 0f, 1f);
		yield return new WaitForSeconds(0.15f);
		OpenHubMainMenu();
		Object.Destroy(l_overlay);
	}

	private void OpenHubMainMenu()
	{
		CommonReferences.Instance.GetManagerHud().OpenHubMainMenu();
	}

	public void CloseHubMainMenu()
	{
		CommonReferences.Instance.GetManagerHud().CloseHubMainMenu();
		GetUp();
	}

	private void GetUp()
	{
		StartCoroutine(CoroutineWaitForGetUp());
	}

	private IEnumerator CoroutineWaitForGetUp()
	{
		CommonReferences.Instance.GetManagerHud().GetManagerOverlay().PlayOverlay(new Color(1f, 0f, 0f, 1f), i_isUseOverlayWithHole: false, i_isDestroyOverlayAfterAnimation: true, 0.5f, 1f, 0f);
		CommonReferences.Instance.GetPlayer().GetAnimator().Play("GetUpChair");
		yield return new WaitForSeconds(CommonReferences.Instance.GetPlayer().GetAnimator().GetCurrentAnimatorClipInfo(0)[0].clip.length);
		CommonReferences.Instance.GetPlayer().SetIsForceIgnoreInput(i_isForceIgnoreInput: false);
		CommonReferences.Instance.GetPlayer().GetRigidbody2D().bodyType = RigidbodyType2D.Dynamic;
	}
}
