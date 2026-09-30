public enum SkinColor
{
	Pale = 0,
	White = 1,
	Tan = 2,
	Black = 3,
	Olive = 4,
	Brown = 5,
	Deep = 6
}

public static class SkinTonePalette
{
	public static UnityEngine.Color GetRendererTint(SkinColor i_skinColor)
	{
		switch (i_skinColor)
		{
		case SkinColor.Olive: return new UnityEngine.Color(0.96f, 0.91f, 0.76f, 1f);
		case SkinColor.Brown: return new UnityEngine.Color(0.78f, 0.72f, 0.68f, 1f);
		case SkinColor.Deep: return new UnityEngine.Color(0.72f, 0.66f, 0.64f, 1f);
		default: return UnityEngine.Color.white;
		}
	}

	public static UnityEngine.Color GetColor(SkinColor i_skinColor)
	{
		switch (i_skinColor)
		{
		case SkinColor.Pale: return new UnityEngine.Color32(246, 208, 181, 255);
		case SkinColor.White: return new UnityEngine.Color32(231, 181, 143, 255);
		case SkinColor.Tan: return new UnityEngine.Color32(185, 121, 79, 255);
		case SkinColor.Black: return new UnityEngine.Color32(106, 63, 44, 255);
		case SkinColor.Olive: return new UnityEngine.Color32(190, 151, 94, 255);
		case SkinColor.Brown: return new UnityEngine.Color32(137, 83, 55, 255);
		case SkinColor.Deep: return new UnityEngine.Color32(72, 42, 34, 255);
		default: return UnityEngine.Color.white;
		}
	}
}
