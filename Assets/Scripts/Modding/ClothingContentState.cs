using Newtonsoft.Json;

namespace CaptivityReloaded.Modding
{
	[JsonObject(MemberSerialization.OptIn)]
	public sealed class ClothingContentState
	{
		[JsonProperty("unlocked")]
		public bool Unlocked { get; set; }

		[JsonProperty("equipped")]
		public bool Equipped { get; set; }

		public string ToJson()
		{
			return JsonConvert.SerializeObject(this, Formatting.None);
		}

		public static bool TryParse(string i_json, out ClothingContentState o_state)
		{
			o_state = null;
			try
			{
				o_state = JsonConvert.DeserializeObject<ClothingContentState>(string.IsNullOrWhiteSpace(i_json) ? "{}" : i_json,
					new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Error });
				return o_state != null;
			}
			catch (JsonException)
			{
				return false;
			}
		}
	}
}
