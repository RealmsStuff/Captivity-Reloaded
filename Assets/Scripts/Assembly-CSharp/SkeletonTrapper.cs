public class SkeletonTrapper : SkeletonActor
{
	public void AnimEventPlaceTrap()
	{
		GetComponentInParent<Trapper>().PlaceTrap();
	}
}
