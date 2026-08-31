using System;
using System.Collections.Generic;
using System.Text;

namespace CaptivityReloaded.Modding
{
	/// <summary>Published semantic atlas slots supported by V1 Core clothing templates.</summary>
	public static class ClothingSlotCatalog
	{
		private static readonly HashSet<string> s_slots = new HashSet<string>(StringComparer.Ordinal)
		{
			"icon",
			"piece/arm-upper",
			"piece/belt",
			"piece/butt",
			"piece/chest",
			"piece/ear",
			"piece/glasses",
			"piece/hair",
			"piece/head",
			"piece/hips",
			"piece/l-arm-lower",
			"piece/l-arm-upper",
			"piece/l-foot",
			"piece/l-hand",
			"piece/l-leg-lower",
			"piece/l-leg-lower-armor",
			"piece/l-leg-lower-shoes",
			"piece/l-leg-lower-stocking",
			"piece/l-leg-upper",
			"piece/l-leg-upper-stocking",
			"piece/mask",
			"piece/neck",
			"piece/r-arm-lower",
			"piece/r-arm-upper",
			"piece/r-foot",
			"piece/r-hand",
			"piece/r-leg-lower",
			"piece/r-leg-lower-armor",
			"piece/r-leg-lower-shoes",
			"piece/r-leg-lower-stocking",
			"piece/r-leg-upper",
			"piece/r-leg-upper-stocking",
			"piece/shirt-chest",
			"piece/shirt-collar",
			"piece/shirt-l-arm-lower",
			"piece/shirt-l-arm-upper",
			"piece/shirt-neck",
			"piece/shirt-r-arm-lower",
			"piece/shirt-r-arm-upper",
			"piece/shirt-spine",
			"piece/skirt-hips",
			"piece/spine"
		};

		public static IEnumerable<string> Slots => s_slots;

		public static bool IsPublished(string i_slot)
		{
			return i_slot != null && s_slots.Contains(i_slot);
		}

		public static string FromCorePieceName(string i_name)
		{
			string name = i_name != null && i_name.StartsWith("clp_", StringComparison.Ordinal)
				? i_name.Substring(4)
				: i_name ?? string.Empty;
			StringBuilder result = new StringBuilder("piece/");
			for (int index = 0; index < name.Length; index++)
			{
				char character = name[index];
				if (char.IsUpper(character) && index > 0) result.Append('-');
				result.Append(char.ToLowerInvariant(character));
			}
			return result.ToString();
		}
	}
}
