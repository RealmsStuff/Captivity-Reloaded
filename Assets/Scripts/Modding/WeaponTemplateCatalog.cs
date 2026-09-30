using System;
using System.Collections.Generic;

namespace CaptivityReloaded.Modding
{
	/// <summary>Stable V1 sprite slots for Core gun prefabs that external weapons may inherit.</summary>
	public static class WeaponTemplateCatalog
	{
		private static readonly Dictionary<string, Dictionary<string, string>> s_templates =
			new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal)
			{
				{
					"core:item/weapon/pistol",
					new Dictionary<string, string>(StringComparer.Ordinal)
					{
						{ "body", null },
						{ "slide", "slide" },
						{ "magazine", "magazine" },
						{ "base", "base" }
					}
				},
				{
					"core:item/weapon/tenelli-so3",
					new Dictionary<string, string>(StringComparer.Ordinal)
					{
						{ "body", null },
						{ "base", "base" },
						{ "magazine", "magazine" },
						{ "shell", "shell" }
					}
				},
				{
					"core:item/weapon/revolver-44",
					new Dictionary<string, string>(StringComparer.Ordinal)
					{
						{ "body", null },
						{ "hammer", "hammer" },
						{ "chamber", "chamber" },
						{ "base", "base" },
						{ "bullet", "bullet" },
						{ "bullet-1", "bullet (1)" },
						{ "bullet-2", "bullet (2)" },
						{ "bullet-3", "bullet (3)" },
						{ "bullet-4", "bullet (4)" },
						{ "bullet-5", "bullet (5)" }
					}
				}
			};

		public static bool IsSupportedTemplate(ContentId i_template)
		{
			return s_templates.ContainsKey(i_template.ToString());
		}

		public static bool IsPublished(ContentId i_template, string i_slot)
		{
			return s_templates.TryGetValue(i_template.ToString(), out Dictionary<string, string> slots)
				&& i_slot != null && slots.ContainsKey(i_slot);
		}

		public static bool TryGetCorePartName(ContentId i_template, string i_slot, out string o_partName)
		{
			o_partName = null;
			return s_templates.TryGetValue(i_template.ToString(), out Dictionary<string, string> slots)
				&& i_slot != null && slots.TryGetValue(i_slot, out o_partName);
		}

		public static IEnumerable<string> GetSlots(ContentId i_template)
		{
			if (s_templates.TryGetValue(i_template.ToString(), out Dictionary<string, string> slots))
				return slots.Keys;
			return new string[0];
		}
	}
}
