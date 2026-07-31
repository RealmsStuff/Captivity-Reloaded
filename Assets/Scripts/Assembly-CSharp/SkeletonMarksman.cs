public class SkeletonMarksman : SkeletonActor
{
	private void AnimEventShootDart()
	{
		GetComponentInParent<Marksman>().FireDart();
	}
}
