using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Threading.Tasks;
using CaptivityReloaded.Modding;
using UnityEngine;
using UnityEngine.Networking;

#if !UNITY_WEBGL && !UNITY_ANDROID
using Mono.Data.Sqlite;
#endif

public static class ManagerDB
{
    public delegate void DelInteraction(int i_idInteraction, NPC i_npc);
    public delegate void DelStartGame();

#if !UNITY_WEBGL && !UNITY_ANDROID
    private static SqliteConnection m_con = new SqliteConnection();
    private static string m_pathToSaveFile;
    private static string m_pathToSaveFileUri;
#endif

    public static event DelInteraction OnInteraction;
    public static event DelStartGame OnStartGame;

#if UNITY_WEBGL || UNITY_ANDROID
	// --- WEBGL/ANDROID SQL EMULATOR STORAGE STRUCTURES ---
	[System.Serializable]
	public class WebDatabaseState
	{
		public int skinColor = 1;
		public int eyeColor = 0;
		public int idNpcDeflower = -1;
		public int isFirstTimeStart = 1;
		public string codeKeypad = "1234";

		public List<RelationshipData> relationships = new List<RelationshipData>();
		public List<InteractionRelationshipData> interactions = new List<InteractionRelationshipData>();
		public List<BirthData> births = new List<BirthData>();
		public List<ClothingData> clothing = new List<ClothingData>();
		public List<ChallengeData> challenges = new List<ChallengeData>();
		public List<StageData> stages = new List<StageData>();
		public List<InputData> inputs = new List<InputData>();
		public List<ContentStateData> contentStates = new List<ContentStateData>();
	}

	[System.Serializable]
	public class RelationshipData
	{
		public int idNpc;
		public int timesKilled;
		public int damageTaken;
		public int timesKO;
		public int timesRaped;
		public int timesOrgasmed;
		public float litreCumTaken;
		public int numOfFetusInserted;
		public int timesMindBroken;
	}

	[System.Serializable]
	public class InteractionRelationshipData
	{
		public int idInteraction;
		public int idNpc;
		public int timesInteraction;
	}

	[System.Serializable]
	public class BirthData
	{
		public int idNpcOffspring;
		public int idNpc;
		public int isEgg;
		public int timesBirth;
	}

	[System.Serializable]
	public class ClothingData
	{
		public int idClothing;
		public int isUnlocked;
		public int isEquipped;
	}

	[System.Serializable]
	public class ChallengeData
	{
		public int idChallenge;
		public int stateChallenge;
	}

	[System.Serializable]
	public class StageData
	{
		public int idStage;
		public int highscore;
	}

	[System.Serializable]
	public class InputData
	{
		public string nameKey;
		public string keyCode;
	}

	[System.Serializable]
	public class ContentStateData
	{
		public string contentId;
		public string category;
		public string stateJson;
	}

	private static WebDatabaseState state = new WebDatabaseState();

	private static void LoadWebState()
	{
#if UNITY_WEBGL
		if (PlayerPrefs.HasKey("WebSaveData"))
		{
			string json = PlayerPrefs.GetString("WebSaveData");
			state = JsonUtility.FromJson<WebDatabaseState>(json);
			if (state.contentStates == null) state.contentStates = new List<ContentStateData>();
		}
		else
		{
			ResetSave();
		}
#elif UNITY_ANDROID
		string path = System.IO.Path.Combine(Application.persistentDataPath, "save_state.json");
		if (File.Exists(path))
		{
			try
			{
				string json = File.ReadAllText(path);
				state = JsonUtility.FromJson<WebDatabaseState>(json);
				if (state.contentStates == null) state.contentStates = new List<ContentStateData>();
			}
			catch (Exception e)
			{
				Debug.LogError("ManagerDB: Error loading Android save file: " + e.Message);
				ResetSave();
			}
		}
		else
		{
			ResetSave();
		}
#endif
	}

	private static void SaveWebState()
	{
		string json = JsonUtility.ToJson(state);
#if UNITY_WEBGL
		PlayerPrefs.SetString("WebSaveData", json);
		PlayerPrefs.Save();
#elif UNITY_ANDROID
		try
		{
			string path = System.IO.Path.Combine(Application.persistentDataPath, "save_state.json");
			File.WriteAllText(path, json);
		}
		catch (Exception e)
		{
			Debug.LogError("ManagerDB: Error saving file on Android: " + e.Message);
		}
#endif
	}

	private static RelationshipData GetWebRelationship(int id)
	{
		foreach (var r in state.relationships)
		{
			if (r.idNpc == id) return r;
		}
		var newR = new RelationshipData { idNpc = id };
		state.relationships.Add(newR);
		return newR;
	}

	private static InteractionRelationshipData GetWebInteraction(int interactId, int npcId)
	{
		foreach (var inter in state.interactions)
		{
			if (inter.idInteraction == interactId && inter.idNpc == npcId) return inter;
		}
		var newI = new InteractionRelationshipData { idInteraction = interactId, idNpc = npcId };
		state.interactions.Add(newI);
		return newI;
	}

	private static BirthData GetWebBirth(int npcId, int offspringId, int isEgg)
	{
		foreach (var b in state.births)
		{
			if (b.idNpc == npcId && b.idNpcOffspring == offspringId && b.isEgg == isEgg) return b;
		}
		var newB = new BirthData { idNpc = npcId, idNpcOffspring = offspringId, isEgg = isEgg };
		state.births.Add(newB);
		return newB;
	}

	private static ClothingData GetWebClothing(int id)
	{
		foreach (var c in state.clothing)
		{
			if (c.idClothing == id) return c;
		}
		var newC = new ClothingData { idClothing = id };
		state.clothing.Add(newC);
		return newC;
	}

	private static ChallengeData GetWebChallenge(int id)
	{
		foreach (var c in state.challenges)
		{
			if (c.idChallenge == id) return c;
		}
		var newC = new ChallengeData { idChallenge = id };
		state.challenges.Add(newC);
		return newC;
	}

	private static StageData GetWebStage(int id)
	{
		foreach (var s in state.stages)
		{
			if (s.idStage == id) return s;
		}
		var newS = new StageData { idStage = id };
		state.stages.Add(newS);
		return newS;
	}

	private static int ParseId(string query, string marker)
	{
		int index = query.IndexOf(marker);
		if (index == -1) return -1;
		string sub = query.Substring(index + marker.Length).Trim();
		string digits = "";
		for (int i = 0; i < sub.Length; i++)
		{
			if (char.IsDigit(sub[i]))
			{
				digits += sub[i];
			}
			else if (digits.Length > 0)
			{
				break;
			}
		}
		int val;
		if (int.TryParse(digits, out val)) return val;
		return -1;
	}

	private static object EmulateQuery(string query, int ordinal, string column)
	{
		string q = query.ToLowerInvariant();
		
		if (q.Contains("sum(") && q.Contains("tbl_relationship"))
		{
			float sum = 0;
			foreach (var r in state.relationships)
			{
				if (q.Contains("timeskilled")) sum += r.timesKilled;
				if (q.Contains("damagetaken")) sum += r.damageTaken;
				if (q.Contains("timesko")) sum += r.timesKO;
				if (q.Contains("timesraped")) sum += r.timesRaped;
				if (q.Contains("timesorgasmed")) sum += r.timesOrgasmed;
				if (q.Contains("litrecumtaken")) sum += r.litreCumTaken;
				if (q.Contains("timesmindbroken")) sum += r.timesMindBroken;
				if (q.Contains("numoffetusinserted")) sum += r.numOfFetusInserted;
			}
			return sum;
		}
		
		if (q.Contains("max(") && q.Contains("tbl_relationship"))
		{
			int maxId = -1;
			float maxVal = -1;
			foreach (var r in state.relationships)
			{
				float val = 0;
				if (q.Contains("timesraped")) val = r.timesRaped;
				if (q.Contains("numoffetusinserted")) val = r.numOfFetusInserted;
				if (val > maxVal)
				{
					maxVal = val;
					maxId = r.idNpc;
				}
			}
			if (maxId == -1) return DBNull.Value;
			if (ordinal == 0 || column == "idNpc") return maxId;
			return maxVal;
		}

		if (q.Contains("tbl_relationship") && q.Contains("idnpc ="))
		{
			int id = ParseId(q, "idnpc =");
			var r = GetWebRelationship(id);
			if (q.Contains("timeskilled")) return r.timesKilled;
			if (q.Contains("damagetaken")) return r.damageTaken;
			if (q.Contains("timesko")) return r.timesKO;
			if (q.Contains("timesraped")) return r.timesRaped;
			if (q.Contains("timesorgasmed")) return r.timesOrgasmed;
			if (q.Contains("litrecumtaken")) return r.litreCumTaken;
			if (q.Contains("timesmindbroken")) return r.timesMindBroken;
			if (q.Contains("numoffetusinserted")) return r.numOfFetusInserted;
		}

		if (q.Contains("idnpcdeflower") && q.Contains("tbl_player"))
		{
			return state.idNpcDeflower == -1 ? DBNull.Value : (object)state.idNpcDeflower;
		}

		if (q.Contains("timesinteraction") && q.Contains("tbl_interactionrelationship"))
		{
			int npcId = ParseId(q, "idnpc =");
			int interactId = ParseId(q, "idinteraction =");
			var inter = GetWebInteraction(interactId, npcId);
			return inter.timesInteraction;
		}

		if (q.Contains("sum(timesbirth)") && q.Contains("tbl_birth"))
		{
			int isEgg = q.Contains("isegg = 1") ? 1 : 0;
			int sum = 0;
			foreach (var b in state.births)
			{
				if (b.isEgg == isEgg) sum += b.timesBirth;
			}
			return sum;
		}

		if (q.Contains("max(timesbirth)") && q.Contains("tbl_birth"))
		{
			int maxId = -1;
			int maxVal = -1;
			foreach (var b in state.births)
			{
				if (b.timesBirth > maxVal)
				{
					maxVal = b.timesBirth;
					maxId = b.idNpcOffspring;
				}
			}
			if (maxId == -1) return DBNull.Value;
			if (ordinal == 0 || column == "idNpcOffspring") return maxId;
			return maxVal;
		}

		return 0;
	}
#endif

    public static async Task Initialize()
    {
#if UNITY_WEBGL || UNITY_ANDROID
		LoadWebState();
        await Task.Yield();
#else
        m_pathToSaveFile = System.IO.Path.Combine(UnityEngine.Application.persistentDataPath, "xGameDB.db");
        m_pathToSaveFileUri = "URI=file:" + m_pathToSaveFile;
        CheckForSaveFile();
		EnsureContentStateSchema();
        await Task.Yield();
#endif
    }

#if !UNITY_WEBGL && !UNITY_ANDROID
	private static void EnsureContentStateSchema()
	{
		ExecuteNonQuery("CREATE TABLE IF NOT EXISTS tbl_contentState (contentId TEXT NOT NULL, category TEXT NOT NULL, stateJson TEXT NOT NULL DEFAULT '{}', PRIMARY KEY (contentId, category));");
	}

    private static void OpenCon()
    {
        m_con = new SqliteConnection(m_pathToSaveFileUri);
        m_con.Open();
    }

    private static void CloseCon()
    {
        m_con.Close();
    }

    private static void CheckForSaveFile()
    {
        if (!File.Exists(m_pathToSaveFile))
        {
            CreateSaveFile();
        }
    }
#endif

    public static int ExecuteNonQuery(string i_query)
    {
#if UNITY_WEBGL || UNITY_ANDROID
		return 0;
#else
        if (m_con.State != ConnectionState.Open)
        {
            OpenCon();
        }
        SqliteCommand sqliteCommand = m_con.CreateCommand();
        sqliteCommand.CommandText = i_query;
        return ExecuteNonQuery(sqliteCommand);
#endif
    }

#if !UNITY_WEBGL && !UNITY_ANDROID
    private static int ExecuteNonQuery(SqliteCommand i_cmd)
    {
        using (i_cmd)
        {
            return i_cmd.ExecuteNonQuery();
        }
    }

    private static async Task<int> ExecuteNonQueryAsync(string i_query)
    {
        if (m_con.State != ConnectionState.Open)
        {
            OpenCon();
        }
        SqliteCommand sqliteCommand = m_con.CreateCommand();
        sqliteCommand.CommandText = i_query;
        return await ExecuteNonQueryAsync(sqliteCommand);
    }

    private static Task<int> ExecuteNonQueryAsync(SqliteCommand i_cmd)
    {
        using (i_cmd)
        {
            return Task.FromResult(i_cmd.ExecuteNonQuery());
        }
    }

    private static async Task<DbDataReader> ExecuteReaderAsync(string i_query)
    {
        if (m_con.State != ConnectionState.Open)
        {
            OpenCon();
        }
        SqliteCommand sqliteCommand = m_con.CreateCommand();
        sqliteCommand.CommandText = i_query;
        return await ExecuteReaderAsync(sqliteCommand);
    }

    private static Task<DbDataReader> ExecuteReaderAsync(SqliteCommand i_cmd)
    {
        return Task.FromResult<DbDataReader>(ExecuteReader(i_cmd));
    }

    public static DbDataReader ExecuteReader(string i_query)
    {
        if (m_con.State != ConnectionState.Open)
        {
            OpenCon();
        }
        SqliteCommand sqliteCommand = m_con.CreateCommand();
        sqliteCommand.CommandText = i_query;
        return ExecuteReader(sqliteCommand);
    }

    private static DbDataReader ExecuteReader(SqliteCommand i_cmd)
    {
        using (i_cmd)
        using (DbDataReader dbDataReader = i_cmd.ExecuteReader())
        {
            DataTable dataTable = new DataTable();
            dataTable.Load(dbDataReader);
            return dataTable.CreateDataReader();
        }
    }
#endif

    public static object GetExecuteReader(string i_query, int i_ordinal)
    {
#if UNITY_WEBGL || UNITY_ANDROID
		return EmulateQuery(i_query, i_ordinal, "");
#else
        DbDataReader dbDataReader = ExecuteReader(i_query);
        object result = null;
        while (dbDataReader.Read())
        {
            result = dbDataReader.GetValue(i_ordinal);
        }
        return result;
#endif
    }

    public static object GetExecuteReader(string i_query, string i_column)
    {
#if UNITY_WEBGL || UNITY_ANDROID
		return EmulateQuery(i_query, -1, i_column);
#else
        DbDataReader dbDataReader = ExecuteReader(i_query);
        object result = null;
        while (dbDataReader.Read())
        {
            result = dbDataReader[i_column];
        }
        return result;
#endif
    }

    public static async void Rape(NPC i_npc)
    {
#if UNITY_WEBGL || UNITY_ANDROID
		var r = GetWebRelationship(i_npc.GetId());
		r.timesRaped++;
		if (state.idNpcDeflower == -1)
		{
			Deflower(i_npc);
		}
		SaveWebState();
		await Task.Yield();
#else
        await ExecuteNonQueryAsync("Update tbl_relationship SET timesRaped = timesRaped + '1' WHERE idNpc = " + i_npc.GetId());
        if (await IsPlayerVirgin())
        {
            Deflower(i_npc);
        }
#endif
    }

#if !UNITY_WEBGL && !UNITY_ANDROID
    private static async Task<bool> IsPlayerVirgin()
    {
        DbDataReader dbDataReader = await ExecuteReaderAsync("SELECT idNpcDeflower FROM tbl_player WHERE idPlayer = 1");
        while (dbDataReader.Read())
        {
            if (dbDataReader["idNpcDeflower"] == DBNull.Value)
            {
                return true;
            }
        }
        return false;
    }
#endif

    public static int GetTimesRaped()
    {
#if UNITY_WEBGL || UNITY_ANDROID
		int total = 0;
		foreach (var r in state.relationships) total += r.timesRaped;
		return total;
#else
        DbDataReader dbDataReader = ExecuteReader("SELECT SUM(timesRaped) FROM tbl_relationship WHERE idPlayer = 1");
        int result = -1;
        while (dbDataReader.Read())
        {
            result = Convert.ToInt32(dbDataReader[0]);
        }
        return result;
#endif
    }

    private static async void Deflower(NPC i_npc)
    {
#if UNITY_WEBGL || UNITY_ANDROID
		state.idNpcDeflower = i_npc.GetId();
		SaveWebState();
		await Task.Yield();
#else
        await ExecuteNonQueryAsync("UPDATE tbl_player SET idNpcDeflower = " + i_npc.GetId() + " WHERE idPlayer = 1");
#endif
    }

    public static async void MindBreak(NPC i_npc)
    {
#if UNITY_WEBGL || UNITY_ANDROID
		var r = GetWebRelationship(i_npc.GetId());
		r.timesMindBroken++;
		SaveWebState();
		await Task.Yield();
#else
        await ExecuteNonQueryAsync("UPDATE tbl_relationship SET timesMindBroken = timesMindBroken + 1 WHERE idNpc = " + i_npc.GetId());
#endif
    }

    public static int GetTimesMindBroken()
    {
#if UNITY_WEBGL || UNITY_ANDROID
		int total = 0;
		foreach (var r in state.relationships) total += r.timesMindBroken;
		return total;
#else
        DbDataReader dbDataReader = ExecuteReader("SELECT SUM(timesMindBroken) FROM tbl_relationship WHERE idPlayer = 1");
        int result = -1;
        while (dbDataReader.Read())
        {
            result = Convert.ToInt32(dbDataReader[0]);
        }
        return result;
#endif
    }

    public static async Task AddNpcs(List<NPC> i_npcs)
    {
        foreach (NPC i_npc in i_npcs)
        {
            if (i_npc.GetId() != -1)
            {
                await AddNpcIfNotExists(i_npc);
            }
        }
    }

    private static async Task AddNpc(NPC i_npc)
    {
#if UNITY_WEBGL || UNITY_ANDROID
		await Task.Yield();
#else
        await ExecuteNonQueryAsync("INSERT INTO tbl_npc VALUES (" + i_npc.GetId() + ", \"" + i_npc.GetName() + "\")");
#endif
    }

    private static async Task AddRelationship(NPC i_npc)
    {
#if UNITY_WEBGL || UNITY_ANDROID
		await Task.Yield();
#else
        await ExecuteNonQueryAsync("INSERT INTO tbl_relationship (idPlayer, idNpc)VALUES (1, " + i_npc.GetId() + ")");
#endif
    }

    public static Relationship GetRelationShip(NPC i_npc)
    {
#if UNITY_WEBGL || UNITY_ANDROID
		var wr = GetWebRelationship(i_npc.GetId());
		Relationship r = new Relationship();
		r.Id = wr.idNpc;
		r.IdPlayer = 1;
		r.IdNpc = wr.idNpc;
		r.TimesKilled = wr.timesKilled;
		r.DamageTaken = wr.damageTaken;
		r.TimesKO = wr.timesKO;
		r.TimesRaped = wr.timesRaped;
		r.TimesOrgasmed = wr.timesOrgasmed;
		r.LitreCumTaken = (decimal)wr.litreCumTaken;
		r.NumOfFetusInserted = wr.numOfFetusInserted;
		return r;
#else
        DbDataReader dbDataReader = ExecuteReader("SELECT * FROM tbl_relationship WHERE idPlayer = 1 AND idNpc = " + i_npc.GetId());
        if (dbDataReader.Read())
        {
            return new Relationship
            {
                Id = Convert.ToInt32(dbDataReader["idRelationship"]),
                IdPlayer = Convert.ToInt32(dbDataReader["idPlayer"]),
                IdNpc = Convert.ToInt32(dbDataReader["idNpc"]),
                TimesKilled = Convert.ToInt32(dbDataReader["timesKilled"]),
                DamageTaken = Convert.ToInt32(dbDataReader["damageTaken"]),
                TimesKO = Convert.ToInt32(dbDataReader["timesKo"]),
                TimesRaped = Convert.ToInt32(dbDataReader["timesRaped"]),
                TimesOrgasmed = Convert.ToInt32(dbDataReader["timesOrgasmed"]),
                LitreCumTaken = Convert.ToDecimal(dbDataReader["litreCumTaken"]),
                NumOfFetusInserted = Convert.ToInt32(dbDataReader["numOfFetusInserted"])
            };
        }
        return null;
#endif
    }

    public static List<Relationship> GetAllRelationShips()
    {
#if UNITY_WEBGL || UNITY_ANDROID
		List<Relationship> list = new List<Relationship>();
		foreach (var wr in state.relationships)
		{
			Relationship r = new Relationship();
			r.Id = wr.idNpc;
			r.IdPlayer = 1;
			r.IdNpc = wr.idNpc;
			r.TimesKilled = wr.timesKilled;
			r.DamageTaken = wr.damageTaken;
			r.TimesKO = wr.timesKO;
			r.TimesRaped = wr.timesRaped;
			r.TimesOrgasmed = wr.timesOrgasmed;
			r.LitreCumTaken = (decimal)wr.litreCumTaken;
			r.NumOfFetusInserted = wr.numOfFetusInserted;
			list.Add(r);
		}
		return list;
#else
        DbDataReader dbDataReader = ExecuteReader("SELECT * FROM tbl_relationship WHERE idPlayer = 1");
        List<Relationship> list = new List<Relationship>();
        while (dbDataReader.Read())
        {
            Relationship relationship = new Relationship();
            relationship.Id = Convert.ToInt32(dbDataReader["idRelationship"]);
            relationship.IdPlayer = Convert.ToInt32(dbDataReader["idPlayer"]);
            relationship.IdNpc = Convert.ToInt32(dbDataReader["idNpc"]);
            relationship.TimesKilled = Convert.ToInt32(dbDataReader["timesKilled"]);
            relationship.DamageTaken = Convert.ToInt32(dbDataReader["damageTaken"]);
            relationship.TimesKO = Convert.ToInt32(dbDataReader["timesKo"]);
            relationship.TimesRaped = Convert.ToInt32(dbDataReader["timesRaped"]);
            relationship.TimesOrgasmed = Convert.ToInt32(dbDataReader["timesOrgasmed"]);
            relationship.LitreCumTaken = Convert.ToDecimal(dbDataReader["litreCumTaken"]);
            relationship.NumOfFetusInserted = Convert.ToInt32(dbDataReader["numOfFetusInserted"]);
            list.Add(relationship);
        }
        return list;
#endif
    }

    public static async void Kill(NPC i_npcKilled)
    {
#if UNITY_WEBGL || UNITY_ANDROID
		var r = GetWebRelationship(i_npcKilled.GetId());
		r.timesKilled++;
		SaveWebState();
		await Task.Yield();
#else
        await ExecuteNonQueryAsync("UPDATE tbl_relationship SET timesKilled = timesKilled + '1' WHERE idNpc = " + i_npcKilled.GetId());
#endif
    }

    public static int GetKillsTotal()
    {
#if UNITY_WEBGL || UNITY_ANDROID
		int total = 0;
		foreach (var r in state.relationships) total += r.timesKilled;
		return total;
#else
        DbDataReader dbDataReader = ExecuteReader("SELECT SUM(timesKilled) FROM tbl_relationship WHERE idPlayer = 1");
        int result = -1;
        while (dbDataReader.Read())
        {
            result = Convert.ToInt32(dbDataReader[0]);
        }
        return result;
#endif
    }

    public static async void Orgasm(NPC i_npc)
    {
#if UNITY_WEBGL || UNITY_ANDROID
		var r = GetWebRelationship(i_npc.GetId());
		r.timesOrgasmed++;
		SaveWebState();
		await Task.Yield();
#else
        await ExecuteNonQueryAsync("UPDATE tbl_relationship SET timesOrgasmed = timesOrgasmed + '1' WHERE idNpc = " + i_npc.GetId());
#endif
    }

    public static async void AddLitreCumTaken(NPC i_npc, float i_litreCum)
    {
#if UNITY_WEBGL || UNITY_ANDROID
		var r = GetWebRelationship(i_npc.GetId());
		r.litreCumTaken += i_litreCum;
		SaveWebState();
		await Task.Yield();
#else
        string text = Math.Round((decimal)i_litreCum, 3, MidpointRounding.AwayFromZero).ToString().Replace(',', '.');
        await ExecuteNonQueryAsync("UPDATE tbl_relationship SET litreCumTaken = litreCumTaken + " + text + " WHERE idNpc = '" + i_npc.GetId() + "'");
#endif
    }

    public static async Task<decimal> GetLitreCumTaken()
    {
#if UNITY_WEBGL || UNITY_ANDROID
		decimal total = 0m;
		foreach (var r in state.relationships) total += (decimal)r.litreCumTaken;
		return await Task.FromResult(total);
#else
        DbDataReader dbDataReader = await ExecuteReaderAsync("SELECT SUM(litreCumTaken) FROM tbl_relationship WHERE idPlayer = 1");
        decimal result = -1m;
        while (dbDataReader.Read())
        {
            result = Convert.ToDecimal(dbDataReader[0]);
        }
        return result;
#endif
    }

    public static async void KO(NPC i_npc)
    {
#if UNITY_WEBGL || UNITY_ANDROID
		var r = GetWebRelationship(i_npc.GetId());
		r.timesKO++;
		SaveWebState();
		await Task.Yield();
#else
        await ExecuteNonQueryAsync("UPDATE tbl_relationship SET timesKO = timesKO + '1' WHERE idNpc = '" + i_npc.GetId() + "'");
#endif
    }

    public static async void Interaction(int i_idInteraction, NPC i_npc)
    {
#if UNITY_WEBGL || UNITY_ANDROID
		ManagerDB.OnInteraction?.Invoke(i_idInteraction, i_npc);
		var inter = GetWebInteraction(i_idInteraction, i_npc.GetId());
		inter.timesInteraction++;
		SaveWebState();
		await Task.Yield();
#else
        ManagerDB.OnInteraction?.Invoke(i_idInteraction, i_npc);
        if (!(await ExecuteReaderAsync("SELECT idInteractionRelationship FROM tbl_interactionRelationship WHERE idPlayer = 1 AND idNpc = " + i_npc.GetId() + " AND idInteraction = " + i_idInteraction)).HasRows)
        {
            await AddInteractionRelationship(i_idInteraction, i_npc.GetId());
        }
        await ExecuteNonQueryAsync("UPDATE tbl_interactionRelationship SET timesInteraction = timesInteraction + '1' WHERE idPlayer = 1 AND idNpc = " + i_npc.GetId() + " AND idInteraction = " + i_idInteraction);
#endif
    }

    public static async Task AddInteractions(List<Interaction> i_interactions)
    {
        foreach (Interaction i_interaction in i_interactions)
        {
            await AddInteractionIfNotExists(i_interaction);
        }
    }

    private static async Task AddInteractionIfNotExists(Interaction i_interaction)
    {
#if UNITY_WEBGL || UNITY_ANDROID
		await Task.Yield();
#else
        if (!(await ExecuteReaderAsync("SELECT idInteraction FROM tbl_interaction WHERE idInteraction = " + i_interaction.GetId())).HasRows)
        {
            await ExecuteNonQueryAsync("INSERT INTO tbl_interaction (idInteraction, nameInteraction, descriptionInteraction) VALUES (\"" + i_interaction.GetId() + "\", \"" + i_interaction.GetName() + "\", \"" + i_interaction.GetDescription() + "\")");
        }
#endif
    }

    private static async Task AddInteractionRelationship(int i_idInteraction, int i_idNpc)
    {
#if UNITY_WEBGL || UNITY_ANDROID
		await Task.Yield();
#else
        await ExecuteNonQueryAsync("INSERT INTO tbl_interactionRelationship (idInteraction, idPlayer, idNpc) VALUES (" + i_idInteraction + ", " + 1 + ", " + i_idNpc + ")");
#endif
    }

    public static async void AddFetusCount(NPC i_npcParent)
    {
#if UNITY_WEBGL || UNITY_ANDROID
		var r = GetWebRelationship(i_npcParent.GetId());
		r.numOfFetusInserted++;
		SaveWebState();
		await Task.Yield();
#else
        await ExecuteNonQueryAsync("UPDATE tbl_relationship SET numOfFetusInserted = numOfFetusInserted + '1' WHERE idNpc = " + i_npcParent.GetId());
#endif
    }

    public static async void Birth(Fetus i_fetusBirthed, NPC i_npcOffspring)
    {
#if UNITY_WEBGL || UNITY_ANDROID
		NPC l_npcParent = i_fetusBirthed.GetNpcParent();
		if (i_npcOffspring.GetId() != -1)
		{
			int parentId = l_npcParent ? l_npcParent.GetId() : -1;
			var birth = GetWebBirth(parentId, i_npcOffspring.GetId(), (i_fetusBirthed.GetActorsInFetus()[0] is Egg) ? 1 : 0);
			birth.timesBirth++;
			SaveWebState();
		}
		await Task.Yield();
#else
        NPC l_npcParent = i_fetusBirthed.GetNpcParent();
        if (i_npcOffspring.GetId() != -1)
        {
            string l_parentPredicate = ((!l_npcParent) ? "idNpc IS NULL" : ("idNpc = " + l_npcParent.GetId()));
            string l_query = "SELECT idBirth FROM tbl_birth WHERE " + l_parentPredicate + " AND idNpcOffspring = " + i_npcOffspring.GetId();
            if (!(await ExecuteReaderAsync(l_query)).HasRows)
            {
                await AddBirth(i_fetusBirthed, i_npcOffspring);
            }
            DbDataReader dbDataReader = await ExecuteReaderAsync(l_query);
            while (dbDataReader.Read())
            {
                Convert.ToInt32(dbDataReader[0]);
            }
            l_query = "UPDATE tbl_birth SET timesBirth = timesBirth + '1' WHERE " + l_parentPredicate + " AND idNpcOffspring = " + i_npcOffspring.GetId();
            await ExecuteNonQueryAsync(l_query);
        }
#endif
    }

    private static async Task AddBirth(Fetus i_fetusBirthed, NPC i_npcOffspring)
    {
#if UNITY_WEBGL || UNITY_ANDROID
		await Task.Yield();
#else
        int num = 0;
        if (i_fetusBirthed.GetActorsInFetus()[0] is Egg)
        {
            num = 1;
        }
        string i_query = ((!i_fetusBirthed.GetNpcParent()) ? ("INSERT INTO tbl_birth (idNpcOffspring, idPlayer, idNpc, isEgg) VALUES (" + i_npcOffspring.GetId() + ", " + 1 + ", NULL, " + num + ")") : ("INSERT INTO tbl_birth (idNpcOffspring, idPlayer, idNpc, isEgg) VALUES (" + i_npcOffspring.GetId() + ", " + 1 + ", " + i_fetusBirthed.GetNpcParent().GetId() + ", " + num + ")"));
        await ExecuteNonQueryAsync(i_query);
#endif
    }

    public static async Task<int> GetTimesBirth()
    {
#if UNITY_WEBGL || UNITY_ANDROID
		int total = 0;
		foreach (var b in state.births) total += b.timesBirth;
		return await Task.FromResult(total);
#else
        DbDataReader dbDataReader = await ExecuteReaderAsync("SELECT SUM(timesBirth) FROM tbl_birth WHERE idPlayer = 1");
        int result = -1;
        while (dbDataReader.Read())
        {
            try
            {
                result = Convert.ToInt32(dbDataReader[0]);
            }
            catch
            {
                result = 0;
            }
        }
        return result;
#endif
    }

    private static async Task AddNpcIfNotExists(NPC i_npc)
    {
#if UNITY_WEBGL || UNITY_ANDROID
		GetWebRelationship(i_npc.GetId());
		SaveWebState();
		await Task.Yield();
#else
        if (!IsNpcExists(i_npc))
        {
            await AddNpc(i_npc);
        }
        if (!ExecuteReader("SELECT * FROM tbl_relationship WHERE idNpc = " + i_npc.GetId()).HasRows)
        {
            await AddRelationship(i_npc);
        }
#endif
    }

#if !UNITY_WEBGL && !UNITY_ANDROID
    private static bool IsNpcExists(NPC i_npc)
    {
        if (ExecuteReader("SELECT idNpc FROM tbl_npc WHERE idNpc = " + i_npc.GetId()).HasRows)
        {
            return true;
        }
        return false;
    }
#endif

    public static async void AddDamageTaken(NPC i_npc, int i_damageTakenToAdd)
    {
#if UNITY_WEBGL || UNITY_ANDROID
		var r = GetWebRelationship(i_npc.GetId());
		r.damageTaken += i_damageTakenToAdd;
		SaveWebState();
		await Task.Yield();
#else
        await ExecuteNonQueryAsync("UPDATE tbl_relationship SET damageTaken = damageTaken + " + i_damageTakenToAdd + " WHERE idNpc = " + i_npc.GetId());
#endif
    }

    public static async Task AddClothes(List<Clothing> i_clothes)
    {
        foreach (Clothing i_clothe in i_clothes)
        {
            await AddClothingIfNotExists(i_clothe);
        }
    }

    private static async Task AddClothingIfNotExists(Clothing i_clothing)
    {
		if (TryGetExternalClothingId(i_clothing, out ContentId contentId))
		{
			if (!TryGetClothingContentState(contentId, out _))
			{
				bool unlocked = false;
				foreach (ClothingDefinition definition in ModLoaderRuntime.ClothingDefinitions)
					if (definition.Id == contentId) unlocked = definition.UnlockedByDefault;
				SaveClothingContentState(contentId, new ClothingContentState { Unlocked = unlocked });
			}
			await Task.Yield();
			return;
		}
#if UNITY_WEBGL || UNITY_ANDROID
		GetWebClothing(i_clothing.GetId());
		SaveWebState();
		await Task.Yield();
#else
        if (!IsClothingExists(i_clothing))
        {
            await AddClothing(i_clothing);
        }
#endif
    }

#if !UNITY_WEBGL && !UNITY_ANDROID
    private static async Task AddClothing(Clothing i_clothing)
    {
        await ExecuteNonQueryAsync("INSERT INTO tbl_clothing VALUES (" + i_clothing.GetId() + ", 0, 0)");
    }

    private static bool IsClothingExists(Clothing i_clothing)
    {
        if (ExecuteReader("SELECT * FROM tbl_clothing WHERE idClothing = " + i_clothing.GetId()).HasRows)
        {
            return true;
        }
        return false;
    }
#endif

    public static List<int> GetIdsUnlockedClothes()
    {
#if UNITY_WEBGL || UNITY_ANDROID
		List<int> list = new List<int>();
		foreach (var c in state.clothing)
		{
			if (c.isUnlocked == 1) list.Add(c.idClothing);
		}
		return list;
#else
        DbDataReader dbDataReader = ExecuteReader("SELECT * FROM tbl_clothing WHERE isUnlocked = 1");
        List<int> list = new List<int>();
        while (dbDataReader.Read())
        {
            list.Add(Convert.ToInt32(dbDataReader[0]));
        }
        return list;
#endif
    }

	public static List<Clothing> GetUnlockedClothes()
	{
		List<Clothing> result = new List<Clothing>();
		foreach (int id in GetIdsUnlockedClothes())
		{
			Clothing clothing = Library.Instance.Clothes.GetClothing(id);
			if (clothing != null) result.Add(clothing);
		}
		foreach (ClothingDefinition definition in ModLoaderRuntime.ClothingDefinitions)
		{
			if (!TryGetClothingContentState(definition.Id, out ClothingContentState saved) || !saved.Unlocked) continue;
			if (ModLoaderRuntime.Registry.TryGet(definition.Id, out ContentRegistration registration) && registration.RuntimeAsset is Clothing clothing)
				result.Add(clothing);
		}
		return result;
	}

    public static void EquipClothes(List<Clothing> i_clothes)
    {
#if UNITY_WEBGL || UNITY_ANDROID
		foreach (var c in state.clothing)
		{
			c.isEquipped = 0;
		}
		foreach (var ec in i_clothes)
		{
			if (TryGetExternalClothingId(ec, out _)) continue;
			var c = GetWebClothing(ec.GetId());
			c.isEquipped = 1;
		}
		SaveWebState();
#else
        ExecuteNonQuery("UPDATE tbl_clothing SET isEquipped = 0");
        foreach (Clothing i_clothe in i_clothes)
        {
			if (TryGetExternalClothingId(i_clothe, out _)) continue;
            ExecuteNonQuery("UPDATE tbl_clothing SET isEquipped = 1 WHERE idClothing = " + i_clothe.GetId());
        }
#endif
		SaveExternalClothingEquipment(i_clothes);
    }

    public static List<int> GetIdsEquippedClothes()
    {
#if UNITY_WEBGL || UNITY_ANDROID
		List<int> list = new List<int>();
		foreach (var c in state.clothing)
		{
			if (c.isUnlocked == 1 && c.isEquipped == 1) list.Add(c.idClothing);
		}
		return list;
#else
        DbDataReader dbDataReader = ExecuteReader("SELECT idClothing FROM tbl_clothing WHERE isUnlocked = 1 AND isEquipped = 1");
        List<int> list = new List<int>();
        while (dbDataReader.Read())
        {
            list.Add(Convert.ToInt32(dbDataReader[0]));
        }
        return list;
#endif
    }

	public static List<Clothing> GetEquippedClothes()
	{
		List<Clothing> result = new List<Clothing>();
		foreach (int id in GetIdsEquippedClothes())
		{
			Clothing clothing = Library.Instance.Clothes.GetClothing(id);
			if (clothing != null) result.Add(clothing);
		}
		foreach (ClothingDefinition definition in ModLoaderRuntime.ClothingDefinitions)
		{
			if (!TryGetClothingContentState(definition.Id, out ClothingContentState saved) || !saved.Unlocked || !saved.Equipped) continue;
			if (ModLoaderRuntime.Registry.TryGet(definition.Id, out ContentRegistration registration) && registration.RuntimeAsset is Clothing clothing)
				result.Add(clothing);
		}
		return result;
	}

    public static async void UnlockClothing(Clothing i_clothing)
    {
		if (TryGetExternalClothingId(i_clothing, out ContentId contentId))
		{
			if (!TryGetClothingContentState(contentId, out ClothingContentState saved)) saved = new ClothingContentState();
			saved.Unlocked = true;
			SaveClothingContentState(contentId, saved);
			await Task.Yield();
			return;
		}
#if UNITY_WEBGL || UNITY_ANDROID
		var c = GetWebClothing(i_clothing.GetId());
		c.isUnlocked = 1;
		SaveWebState();
		await Task.Yield();
#else
        await ExecuteNonQueryAsync("UPDATE tbl_clothing SET isUnlocked = 1 WHERE idClothing = " + i_clothing.GetId());
#endif
    }

    public static async void UnlockAllClothes()
    {
#if UNITY_WEBGL || UNITY_ANDROID
		foreach (var c in state.clothing)
		{
			c.isUnlocked = 1;
		}
		SaveWebState();
		await Task.Yield();
#else
        await ExecuteNonQueryAsync("UPDATE tbl_clothing SET isUnlocked = 1");
#endif
		foreach (ClothingDefinition definition in ModLoaderRuntime.ClothingDefinitions)
		{
			if (!TryGetClothingContentState(definition.Id, out ClothingContentState saved)) saved = new ClothingContentState();
			saved.Unlocked = true;
			SaveClothingContentState(definition.Id, saved);
		}
    }

	private static bool TryGetExternalClothingId(Clothing i_clothing, out ContentId o_contentId)
	{
		o_contentId = default;
		return RuntimeContentIdentity.TryResolve(i_clothing, out o_contentId, out ContentCategory category) &&
			category == ContentCategory.Clothing && o_contentId.Namespace != "core";
	}

	private static bool TryGetClothingContentState(ContentId i_id, out ClothingContentState o_state)
	{
		foreach (SavedContentState saved in GetSavedContentStates())
		{
			if (saved.Category == ContentCategory.Clothing && saved.ContentId == i_id)
				return ClothingContentState.TryParse(saved.StateJson, out o_state);
		}
		o_state = null;
		return false;
	}

	private static void SaveClothingContentState(ContentId i_id, ClothingContentState i_state)
	{
		SaveContentState(new SavedContentState(i_id, ContentCategory.Clothing, i_state.ToJson()));
	}

	private static void SaveExternalClothingEquipment(List<Clothing> i_clothes)
	{
		HashSet<ContentId> selected = new HashSet<ContentId>();
		foreach (Clothing clothing in i_clothes)
			if (TryGetExternalClothingId(clothing, out ContentId id)) selected.Add(id);

		foreach (ClothingDefinition definition in ModLoaderRuntime.ClothingDefinitions)
		{
			if (!TryGetClothingContentState(definition.Id, out ClothingContentState saved))
				saved = new ClothingContentState { Unlocked = definition.UnlockedByDefault };
			saved.Equipped = selected.Contains(definition.Id);
			if (saved.Equipped) saved.Unlocked = true;
			SaveClothingContentState(definition.Id, saved);
		}
	}

    public static async void SetSkinColor(SkinColor i_skinColor)
    {
        int num = 1;
        switch (i_skinColor)
        {
            case SkinColor.Pale:
                num = 0;
                break;
            case SkinColor.White:
                num = 1;
                break;
            case SkinColor.Tan:
                num = 2;
                break;
            case SkinColor.Black:
                num = 3;
                break;
			case SkinColor.Olive:
				num = 4;
				break;
			case SkinColor.Brown:
				num = 5;
				break;
			case SkinColor.Deep:
				num = 6;
				break;
        }
#if UNITY_WEBGL || UNITY_ANDROID
		state.skinColor = num;
		SaveWebState();
		await Task.Yield();
#else
        await ExecuteNonQueryAsync("UPDATE tbl_player SET skinColor = " + num);
#endif
    }

    public static SkinColor GetSkinColorPlayer()
    {
#if UNITY_WEBGL || UNITY_ANDROID
		int num = state.skinColor;
#else
        DbDataReader dbDataReader = ExecuteReader("SELECT skinColor FROM tbl_player WHERE idPlayer = 1");
        int num = 1;
        while (dbDataReader.Read())
        {
            num = Convert.ToInt32(dbDataReader[0]);
        }
#endif
        SkinColor result = SkinColor.White;
        switch (num)
        {
            case 0:
                result = SkinColor.Pale;
                break;
            case 1:
                result = SkinColor.White;
                break;
            case 2:
                result = SkinColor.Tan;
                break;
            case 3:
                result = SkinColor.Black;
                break;
			case 4:
				result = SkinColor.Olive;
				break;
			case 5:
				result = SkinColor.Brown;
				break;
			case 6:
				result = SkinColor.Deep;
				break;
        }
        return result;
    }

    public static async void SetEyeColor(EyeColor i_eyeColor)
    {
        int num = 0;
        switch (i_eyeColor)
        {
            case EyeColor.Blue:
                num = 0;
                break;
            case EyeColor.Brown:
                num = 1;
                break;
            case EyeColor.Green:
                num = 2;
                break;
            case EyeColor.Yellow:
                num = 3;
                break;
        }
#if UNITY_WEBGL || UNITY_ANDROID
		state.eyeColor = num;
		SaveWebState();
		await Task.Yield();
#else
        await ExecuteNonQueryAsync("UPDATE tbl_player SET eyeColor = " + num);
#endif
    }

    public static EyeColor GetEyeColorPlayer()
    {
#if UNITY_WEBGL || UNITY_ANDROID
		int num = state.eyeColor;
#else
        DbDataReader dbDataReader = ExecuteReader("SELECT eyeColor FROM tbl_player WHERE idPlayer = 1");
        int num = 0;
        while (dbDataReader.Read())
        {
            num = Convert.ToInt32(dbDataReader[0]);
        }
#endif
        EyeColor result = EyeColor.Blue;
        switch (num)
        {
            case 0:
                result = EyeColor.Blue;
                break;
            case 1:
                result = EyeColor.Brown;
                break;
            case 2:
                result = EyeColor.Green;
                break;
            case 3:
                result = EyeColor.Yellow;
                break;
        }
        return result;
    }

    public static async Task AddChallenges(List<Challenge> i_challenges)
    {
        foreach (Challenge i_challenge in i_challenges)
        {
            await AddChallengeIfNotExists(i_challenge);
        }
    }

    private static async Task AddChallengeIfNotExists(Challenge i_challenge)
    {
#if UNITY_WEBGL || UNITY_ANDROID
		GetWebChallenge(i_challenge.GetId());
		SaveWebState();
		await Task.Yield();
#else
        if (!(await IsChallengeExists(i_challenge)))
        {
            await AddChallenge(i_challenge);
        }
#endif
    }

#if !UNITY_WEBGL && !UNITY_ANDROID
    private static async Task AddChallenge(Challenge i_challenge)
    {
        await ExecuteNonQueryAsync("INSERT INTO tbl_challenge VALUES (" + i_challenge.GetId() + ", 0)");
    }

    private static async Task<bool> IsChallengeExists(Challenge i_challenge)
    {
        if ((await ExecuteReaderAsync("SELECT * FROM tbl_challenge WHERE idChallenge = " + i_challenge.GetId())).HasRows)
        {
            return true;
        }
        return false;
    }
#endif

    public static List<int> GetIdsOpenChallenges()
    {
#if UNITY_WEBGL || UNITY_ANDROID
		List<int> list = new List<int>();
		foreach (var c in state.challenges)
		{
			if (c.stateChallenge == 0) list.Add(c.idChallenge);
		}
		return list;
#else
        DbDataReader dbDataReader = ExecuteReader("SELECT idChallenge FROM tbl_challenge WHERE stateChallenge = 0");
        List<int> list = new List<int>();
        while (dbDataReader.Read())
        {
            list.Add(Convert.ToInt32(dbDataReader[0]));
        }
        return list;
#endif
    }

    public static List<int> GetIdsCompletedPendingChallenges()
    {
#if UNITY_WEBGL || UNITY_ANDROID
		List<int> list = new List<int>();
		foreach (var c in state.challenges)
		{
			if (c.stateChallenge == 1) list.Add(c.idChallenge);
		}
		return list;
#else
        DbDataReader dbDataReader = ExecuteReader("SELECT idChallenge FROM tbl_challenge WHERE stateChallenge = 1");
        List<int> list = new List<int>();
        while (dbDataReader.Read())
        {
            list.Add(Convert.ToInt32(dbDataReader[0]));
        }
        return list;
#endif
    }

    public static List<int> GetIdsCompletedSeenChallenges()
    {
#if UNITY_WEBGL || UNITY_ANDROID
		List<int> list = new List<int>();
		foreach (var c in state.challenges)
		{
			if (c.stateChallenge == 2) list.Add(c.idChallenge);
		}
		return list;
#else
        DbDataReader dbDataReader = ExecuteReader("SELECT idChallenge FROM tbl_challenge WHERE stateChallenge = 2");
        List<int> list = new List<int>();
        while (dbDataReader.Read())
        {
            list.Add(Convert.ToInt32(dbDataReader[0]));
        }
        return list;
#endif
    }

    public static async void CompleteChallenge(Challenge i_challenge)
    {
#if UNITY_WEBGL || UNITY_ANDROID
		var c = GetWebChallenge(i_challenge.GetId());
		c.stateChallenge = 1;
		SaveWebState();
		await Task.Yield();
#else
        await ExecuteNonQueryAsync("UPDATE tbl_challenge SET stateChallenge = 1 WHERE idChallenge = " + i_challenge.GetId());
#endif
    }

    public static void AddStages(List<Stage> i_stages)
    {
        foreach (Stage i_stage in i_stages)
        {
            AddStageIfNotExists(i_stage);
        }
    }

    private static void AddStageIfNotExists(Stage i_stage)
    {
		if (i_stage == null || i_stage.GetIsRuntimeTemplate()) return;
		if (TryGetExternalStageId(i_stage, out ContentId contentId))
		{
			if (!TryGetStageContentState(contentId, out _))
				SaveStageContentState(contentId, new StageContentState());
			return;
		}
#if UNITY_WEBGL || UNITY_ANDROID
		if (!IsStageExists(i_stage))
		{
			AddStage(i_stage);
		}
#else
        if (!IsStageExists(i_stage))
        {
            AddStage(i_stage);
        }
#endif
    }

    private static void AddStage(Stage i_stage)
    {
#if UNITY_WEBGL || UNITY_ANDROID
		GetWebStage(i_stage.GetId());
		SaveWebState();
#else
        ExecuteNonQuery("INSERT INTO tbl_stage VALUES (" + i_stage.GetId() + ", 0)");
#endif
    }

    private static bool IsStageExists(Stage i_stage)
    {
#if UNITY_WEBGL || UNITY_ANDROID
		foreach (var s in state.stages)
		{
			if (s.idStage == i_stage.GetId()) return true;
		}
		return false;
#else
        if (ExecuteReader("SELECT * FROM tbl_stage WHERE idStage = " + i_stage.GetId()).HasRows)
        {
            return true;
        }
        return false;
#endif
    }

    public static async void SetChallengeToSeen(Challenge i_challenge)
    {
#if UNITY_WEBGL || UNITY_ANDROID
		var c = GetWebChallenge(i_challenge.GetId());
		c.stateChallenge = 2;
		SaveWebState();
		await Task.Yield();
#else
        await ExecuteNonQueryAsync("UPDATE tbl_challenge SET stateChallenge = 2 WHERE idChallenge = " + i_challenge.GetId());
#endif
    }

    public static int GetHighscore(Stage i_stage)
    {
		if (TryGetExternalStageId(i_stage, out ContentId contentId))
			return TryGetStageContentState(contentId, out StageContentState saved) ? saved.Highscore : 0;
#if UNITY_WEBGL || UNITY_ANDROID
		var s = GetWebStage(i_stage.GetId());
		return s.highscore;
#else
        DbDataReader dbDataReader = ExecuteReader("SELECT highscore FROM tbl_stage WHERE idStage = " + i_stage.GetId());
        int result = 0;
        while (dbDataReader.Read())
        {
            result = Convert.ToInt32(dbDataReader[0]);
        }
        return result;
#endif
    }

    public static async void SetHighscore(Stage i_stage, int i_highScoreNew)
    {
		if (TryGetExternalStageId(i_stage, out ContentId contentId))
		{
			SaveStageContentState(contentId, new StageContentState { Highscore = Math.Max(0, i_highScoreNew) });
			await Task.Yield();
			return;
		}
#if UNITY_WEBGL || UNITY_ANDROID
		var s = GetWebStage(i_stage.GetId());
		s.highscore = i_highScoreNew;
		SaveWebState();
		await Task.Yield();
#else
        await ExecuteNonQueryAsync("UPDATE tbl_stage SET highscore = " + i_highScoreNew + " WHERE idStage = " + i_stage.GetId());
#endif
    }

	private static bool TryGetExternalStageId(Stage i_stage, out ContentId o_contentId)
	{
		o_contentId = default;
		return RuntimeContentIdentity.TryResolve(i_stage, out o_contentId, out ContentCategory category) &&
			category == ContentCategory.Stage && o_contentId.Namespace != "core";
	}

	private static bool TryGetStageContentState(ContentId i_id, out StageContentState o_state)
	{
		foreach (SavedContentState saved in GetSavedContentStates())
		{
			if (saved.Category == ContentCategory.Stage && saved.ContentId == i_id)
				return StageContentState.TryParse(saved.StateJson, out o_state);
		}
		o_state = null;
		return false;
	}

	private static void SaveStageContentState(ContentId i_id, StageContentState i_state)
	{
		SaveContentState(new SavedContentState(i_id, ContentCategory.Stage, i_state.ToJson()));
	}

    public static bool IsFirstTimeStart()
    {
#if UNITY_WEBGL || UNITY_ANDROID
		return state.isFirstTimeStart == 1;
#else
        DbDataReader dbDataReader = ExecuteReader("SELECT isFirstTimeStart FROM tbl_player WHERE idPlayer = 1");
        int num = -1;
        while (dbDataReader.Read())
        {
            num = Convert.ToInt32(dbDataReader[0]);
        }
        if (num == 1)
        {
            return true;
        }
        return false;
#endif
    }

    public static async void FirstTimeStart()
    {
#if UNITY_WEBGL || UNITY_ANDROID
		state.isFirstTimeStart = 0;
		if (!IsInputFilled())
		{
			ResetInput();
			CommonReferences.Instance.GetManagerInput().SetButtonsToDefault();
		}
		SaveWebState();
		await Task.Yield();
#else
        await ExecuteNonQueryAsync("UPDATE tbl_player SET isFirstTimeStart = 0 WHERE idPlayer = 1");
        if (!IsInputFilled())
        {
            ResetInput();
            CommonReferences.Instance.GetManagerInput().SetButtonsToDefault();
        }
#endif
    }

    public static string GetCodeKeypad()
    {
#if UNITY_WEBGL || UNITY_ANDROID
		return state.codeKeypad;
#else
        DbDataReader dbDataReader = ExecuteReader("SELECT codeKeypad FROM tbl_player WHERE idPlayer = 1");
        string result = "";
        while (dbDataReader.Read())
        {
            result = dbDataReader[0].ToString();
        }
        return result;
#endif
    }

    public static void ResetSave()
    {
#if UNITY_WEBGL || UNITY_ANDROID
		int[] array = new int[4];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = UnityEngine.Random.Range(0, 9);
		}
		string text = array[0].ToString() + array[1] + array[2] + array[3];
		state = new WebDatabaseState();
		state.codeKeypad = text;
		state.isFirstTimeStart = 1;
		SaveWebState();
#else
        int[] array = new int[4];
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = UnityEngine.Random.Range(0, 9);
        }
        string text = array[0].ToString() + array[1] + array[2] + array[3];
		ExecuteNonQuery("DELETE FROM tbl_interaction;\r\n        DELETE FROM tbl_interactionRelationship;\r\n        DELETE FROM tbl_npc;\r\n        DELETE FROM tbl_relationship;\r\n        DELETE FROM tbl_birth;\r\n        DELETE FROM tbl_clothing;\r\n        DELETE FROM tbl_challenge;\r\n        DELETE FROM tbl_stage;\r\n        DELETE FROM tbl_contentState;\r\n        DELETE FROM tbl_player;\r\n        INSERT INTO tbl_player (idPlayer, isFirstTimeStart, codeKeypad) VALUES (1, 1, " + text + ");");
#endif
        ResetVolumes();
        SetDifficulty(Difficulty.Normal);
        SetIsReduceGunFlash(i_isReduce: false);
    }

    public static void ResetVolumes()
    {
        PlayerPrefs.SetInt("VolumeMaster", 10);
        PlayerPrefs.SetInt("VolumeMusic", 8);
        PlayerPrefs.SetInt("VolumeAmbience", 5);
        PlayerPrefs.SetInt("VolumeVoice", 7);
        PlayerPrefs.SetInt("VolumeSFX", 6);
        PlayerPrefs.SetInt("VolumeHitsound", 5);
    }

    public static void SetDifficulty(Difficulty i_difficulty)
    {
		DifficultyRegistry.SetCurrent("core:difficulty/" + i_difficulty.ToString().ToLowerInvariant());
    }

    public static void SetIsReduceGunFlash(bool i_isReduce)
    {
        if (i_isReduce)
        {
            PlayerPrefs.SetInt("IsReduceGunFlash", 1);
        }
        else
        {
            PlayerPrefs.SetInt("IsReduceGunFlash", 0);
        }
    }

    public static string GetDifficulty()
    {
        return PlayerPrefs.GetString("Difficulty");
    }

    public static bool IsReduceGunFlash()
    {
        if (PlayerPrefs.GetInt("IsReduceGunFlash") == 1)
        {
            return true;
        }
        return false;
    }

    public static async void StartGame()
    {
        await AddChallenges(CommonReferences.Instance.GetManagerChallenge().GetAllChallenges());
        CommonReferences.Instance.GetManagerChallenge().UpdateChallenges();
        await AddClothes(Library.Instance.Clothes.GetAllClothes());
        Library.Instance.Clothes.FirstTimeStart();
        await AddNpcs(Library.Instance.Actors.GetAllNpcs());
        await AddInteractions(Library.Instance.Actors.GetAllInteractions());
        ManagerDB.OnStartGame?.Invoke();
    }

    public static bool IsInputFilled()
    {
#if UNITY_WEBGL || UNITY_ANDROID
		return state.inputs.Count > 0;
#else
        return ExecuteReader("SELECT * FROM tbl_input").HasRows;
#endif
    }

    public static void ResetInput()
    {
#if UNITY_WEBGL || UNITY_ANDROID
		state.inputs.Clear();
		ManagerInput managerInput = CommonReferences.Instance.GetManagerInput();
		for (int i = 0; i < managerInput.GetButtonsDefault().Count; i++)
		{
			var inp = new InputData();
			inp.nameKey = managerInput.GetButtonsDefault()[i].GetName();
			inp.keyCode = managerInput.GetButtonsDefault()[i].GetKeyCode().ToString();
			state.inputs.Add(inp);
		}
		SaveWebState();
#else
        string i_query = "DELETE FROM tbl_input";
        ExecuteNonQuery(i_query);
        ManagerInput managerInput = CommonReferences.Instance.GetManagerInput();
        List<string> list = new List<string>();
        for (int i = 0; i < managerInput.GetButtonsDefault().Count; i++)
        {
            i_query = "INSERT INTO tbl_input VALUES ";
            i_query += "(\"";
            i_query = i_query + managerInput.GetButtonsDefault()[i].GetName() + "\", \"" + managerInput.GetButtonsDefault()[i].GetKeyCode();
            i_query += "\")";
            i_query += ";";
            list.Add(i_query);
        }
        for (int j = 0; j < list.Count; j++)
        {
            ExecuteNonQuery(list[j]);
        }
#endif
    }

    public static List<InputButtonXGame> GetInputButtons()
    {
#if UNITY_WEBGL || UNITY_ANDROID
		List<InputButtonXGame> list = new List<InputButtonXGame>();
		foreach (var inp in state.inputs)
		{
			list.Add(new InputButtonXGame(inp.nameKey, inp.keyCode));
		}
		AppendMissingDefaultInputs(list);
		return list;
#else
        DbDataReader dbDataReader = ExecuteReader("SELECT * FROM tbl_input");
        List<InputButtonXGame> list = new List<InputButtonXGame>();
        while (dbDataReader.Read())
        {
            list.Add(new InputButtonXGame((string)dbDataReader["nameKey"], (string)dbDataReader["keyCode"]));
        }
		AppendMissingDefaultInputs(list);
        return list;
#endif
    }

	private static void AppendMissingDefaultInputs(List<InputButtonXGame> io_inputs)
	{
		if (CommonReferences.Instance == null || CommonReferences.Instance.GetManagerInput() == null) return;
#if UNITY_WEBGL || UNITY_ANDROID
		bool changed = false;
#endif
		foreach (InputButtonXGame defaultButton in CommonReferences.Instance.GetManagerInput().GetButtonsDefault())
		{
			bool found = false;
			foreach (InputButtonXGame input in io_inputs)
				if (input.GetInputButton() == defaultButton.GetInputButton()) { found = true; break; }
			if (found) continue;
			io_inputs.Add(defaultButton);
#if UNITY_WEBGL || UNITY_ANDROID
			changed = true;
			state.inputs.Add(new InputData { nameKey = defaultButton.GetName(), keyCode = defaultButton.GetKeyCode().ToString() });
#else
			ExecuteNonQuery("INSERT INTO tbl_input VALUES ('" + defaultButton.GetName() + "', '" + defaultButton.GetKeyCode() + "')");
#endif
		}
#if UNITY_WEBGL || UNITY_ANDROID
		if (changed) SaveWebState();
#endif
	}

    public static void SetInputButton(string i_nameButton, KeyCode i_keyCodeToSet)
    {
#if UNITY_WEBGL || UNITY_ANDROID
		foreach (var inp in state.inputs)
		{
			if (inp.nameKey == i_nameButton)
			{
				inp.keyCode = i_keyCodeToSet.ToString();
				break;
			}
		}
		SaveWebState();
#else
        ExecuteNonQuery("UPDATE tbl_input SET keyCode = '" + i_keyCodeToSet.ToString() + "' WHERE nameKey = '" + i_nameButton + "'");
#endif
    }

	public static void SaveContentState(SavedContentState i_state)
	{
		if (i_state == null) throw new ArgumentNullException(nameof(i_state));
#if UNITY_WEBGL || UNITY_ANDROID
		if (state.contentStates == null) state.contentStates = new List<ContentStateData>();
		ContentStateData stored = null;
		foreach (ContentStateData candidate in state.contentStates)
		{
			if (candidate.contentId == i_state.ContentId.ToString() && candidate.category == i_state.Category.ToString())
			{
				stored = candidate;
				break;
			}
		}
		if (stored == null)
		{
			stored = new ContentStateData();
			state.contentStates.Add(stored);
		}
		stored.contentId = i_state.ContentId.ToString();
		stored.category = i_state.Category.ToString();
		stored.stateJson = i_state.StateJson;
		SaveWebState();
#else
		if (m_con.State != ConnectionState.Open) OpenCon();
		using (SqliteCommand command = m_con.CreateCommand())
		{
			command.CommandText = "INSERT OR REPLACE INTO tbl_contentState (contentId, category, stateJson) VALUES (@contentId, @category, @stateJson);";
			command.Parameters.AddWithValue("@contentId", i_state.ContentId.ToString());
			command.Parameters.AddWithValue("@category", i_state.Category.ToString());
			command.Parameters.AddWithValue("@stateJson", i_state.StateJson);
			command.ExecuteNonQuery();
		}
#endif
	}

	public static List<SavedContentState> GetSavedContentStates()
	{
		List<SavedContentState> result = new List<SavedContentState>();
#if UNITY_WEBGL || UNITY_ANDROID
		if (state.contentStates == null) return result;
		foreach (ContentStateData stored in state.contentStates)
		{
			if (SavedContentState.TryCreate(stored.contentId, stored.category, stored.stateJson, out SavedContentState parsed)) result.Add(parsed);
		}
#else
		DbDataReader reader = ExecuteReader("SELECT contentId, category, stateJson FROM tbl_contentState ORDER BY contentId, category");
		while (reader.Read())
		{
			if (SavedContentState.TryCreate(reader["contentId"].ToString(), reader["category"].ToString(), reader["stateJson"].ToString(), out SavedContentState parsed)) result.Add(parsed);
		}
#endif
		return result;
	}

#if !UNITY_WEBGL && !UNITY_ANDROID
    private static void CreateSaveFile()
    {
        string path = System.IO.Path.Combine(UnityEngine.Application.streamingAssetsPath, "dbScript.sql");
        OpenCon();
        ExecuteNonQuery(File.ReadAllText(path));
        ResetSave();
        CloseCon();
    }
#endif
}
