using UnityEngine;

public class ClothingPieceHat : ClothingPiece
{
	[SerializeField]
	private bool m_isHidesHair;

	public bool IsHidesHair()
	{
		return m_isHidesHair;
	}

	public void ConfigureModHat(bool i_hidesHair) { m_isHidesHair = i_hidesHair; }
}
