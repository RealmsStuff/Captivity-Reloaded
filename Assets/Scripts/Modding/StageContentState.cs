using Newtonsoft.Json;

namespace CaptivityReloaded.Modding
{
	[JsonObject(MemberSerialization.OptIn)]
	public sealed class StageContentState
	{
		[JsonProperty("highscore")]
		public int Highscore { get; set; }

		public string ToJson()
		{
			return JsonConvert.SerializeObject(this, Formatting.None);
		}

		public static bool TryParse(string i_json, out StageContentState o_state)
		{
			o_state = null;
			try
			{
				o_state = JsonConvert.DeserializeObject<StageContentState>(string.IsNullOrWhiteSpace(i_json) ? "{}" : i_json,
					new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Error });
				if (o_state == null || o_state.Highscore < 0)
				{
					o_state = null;
					return false;
				}
				return true;
			}
			catch (JsonException)
			{
				return false;
			}
		}
	}
}
