using UnityEngine;

public class ClothingPieceHat : ClothingPiece
{
	[SerializeField]
	private bool m_isHidesHair;

	public bool IsHidesHair()
	{
		return m_isHidesHair;
	}
}
