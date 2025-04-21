using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GoogleGamesManager : MonoBehaviour
{
	[method: MethodImpl(32)]
	public static event Action OnSignInFailed;

	[method: MethodImpl(32)]
	public static event Action OnSignInSucceeded;

	[method: MethodImpl(32)]
	public static event Action OnSignedOut;

	[method: MethodImpl(32)]
	public static event Action<int, List<GoogleGame>> OnGamesLoaded;

	[method: MethodImpl(32)]
	public static event Action<int, List<GooglePlayer>> OnPlayersLoaded;

	[method: MethodImpl(32)]
	public static event Action<int, List<GoogleLeaderboard>, List<GoogleLeaderboardScore>, int> OnLeaderboardScoresLoaded;

	[method: MethodImpl(32)]
	public static event Action<int, GoogleSubmitScoreResult> OnScoreSubmitted;

	[method: MethodImpl(32)]
	public static event Action<int, List<GoogleLeaderboard>> OnLeaderboardMetadataLoaded;

	[method: MethodImpl(32)]
	public static event Action<int, string> OnAchievementUpdated;

	[method: MethodImpl(32)]
	public static event Action<int, List<GoogleAchievement>> OnAchievementsLoaded;

	private static void OnSignInFailedEvent()
	{
		if (GoogleGamesManager.OnSignInFailed != null)
		{
			GoogleGamesManager.OnSignInFailed();
		}
	}

	private static void OnSignInSucceededEvent()
	{
		if (GoogleGamesManager.OnSignInSucceeded != null)
		{
			GoogleGamesManager.OnSignInSucceeded();
		}
	}

	private static void OnSignedOutEvent()
	{
		if (GoogleGamesManager.OnSignedOut != null)
		{
			GoogleGamesManager.OnSignedOut();
		}
	}

	private static void OnGamesLoadedEvent(int a_StatusCode, List<GoogleGame> a_Games)
	{
		if (GoogleGamesManager.OnGamesLoaded != null)
		{
			GoogleGamesManager.OnGamesLoaded(a_StatusCode, a_Games);
		}
	}

	private static void OnPlayersLoadedEvent(int a_StatusCode, List<GooglePlayer> a_Players)
	{
		if (GoogleGamesManager.OnPlayersLoaded != null)
		{
			GoogleGamesManager.OnPlayersLoaded(a_StatusCode, a_Players);
		}
	}

	private static void OnLeaderboardScoresLoadedEvent(int a_StatusCode, List<GoogleLeaderboard> a_Leaderboards, List<GoogleLeaderboardScore> a_Scores, int a_ScoresIndex)
	{
		if (GoogleGamesManager.OnLeaderboardScoresLoaded != null)
		{
			GoogleGamesManager.OnLeaderboardScoresLoaded(a_StatusCode, a_Leaderboards, a_Scores, a_ScoresIndex);
		}
	}

	private static void OnScoreSubmittedEvent(int a_StatusCode, GoogleSubmitScoreResult a_Result)
	{
		if (GoogleGamesManager.OnScoreSubmitted != null)
		{
			GoogleGamesManager.OnScoreSubmitted(a_StatusCode, a_Result);
		}
	}

	private static void OnLeaderboardMetadataLoadedEvent(int a_StatusCode, List<GoogleLeaderboard> a_Leaderboards)
	{
		if (GoogleGamesManager.OnLeaderboardMetadataLoaded != null)
		{
			GoogleGamesManager.OnLeaderboardMetadataLoaded(a_StatusCode, a_Leaderboards);
		}
	}

	private static void OnAchievementUpdatedEvent(int a_StatusCode, string a_AchievementId)
	{
		if (GoogleGamesManager.OnAchievementUpdated != null)
		{
			GoogleGamesManager.OnAchievementUpdated(a_StatusCode, a_AchievementId);
		}
	}

	private static void OnAchievementsLoadedEvent(int a_StatusCode, List<GoogleAchievement> a_Achievements)
	{
		if (GoogleGamesManager.OnAchievementsLoaded != null)
		{
			GoogleGamesManager.OnAchievementsLoaded(a_StatusCode, a_Achievements);
		}
	}

	private void Awake()
	{
		base.gameObject.name = GetType().ToString();
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		GoogleGamesBinding.SetDebug(true);
	}

	private void SignInFailed(string a_Empty)
	{
		OnSignInFailedEvent();
	}

	private void SignInSucceeded(string a_Empty)
	{
		OnSignInSucceededEvent();
	}

	private void SignedOut(string a_Empty)
	{
		OnSignedOutEvent();
	}

	private void GamesLoaded(string a_JSONMessage)
	{
		object obj = MiniJSON.jsonDecode(a_JSONMessage);
		int num = -99;
		List<GoogleGame> list = null;
		IDictionary dictionary = obj as IDictionary;
		if (dictionary != null)
		{
			num = JSONObject.GetIntValueFromDict(dictionary, "statusCode", num);
			ICollection valueFromDict = JSONObject.GetValueFromDict<ICollection>(dictionary, "games", null);
			if (valueFromDict != null)
			{
				list = new List<GoogleGame>();
				foreach (object item in valueFromDict)
				{
					IDictionary dictionary2 = item as IDictionary;
					if (dictionary2 != null)
					{
						GoogleGame googleGame = new GoogleGame();
						googleGame.FromJSON(dictionary2);
						list.Add(googleGame);
					}
				}
			}
		}
		OnGamesLoadedEvent(num, list);
	}

	private void PlayersLoaded(string a_JSONMessage)
	{
		object obj = MiniJSON.jsonDecode(a_JSONMessage);
		int num = -99;
		List<GooglePlayer> list = null;
		IDictionary dictionary = obj as IDictionary;
		if (dictionary != null)
		{
			num = JSONObject.GetIntValueFromDict(dictionary, "statusCode", num);
			ICollection valueFromDict = JSONObject.GetValueFromDict<ICollection>(dictionary, "players", null);
			if (valueFromDict != null)
			{
				list = new List<GooglePlayer>();
				foreach (object item in valueFromDict)
				{
					IDictionary dictionary2 = item as IDictionary;
					if (dictionary2 != null)
					{
						GooglePlayer googlePlayer = new GooglePlayer();
						googlePlayer.FromJSON(dictionary2);
						list.Add(googlePlayer);
					}
				}
			}
		}
		OnPlayersLoadedEvent(num, list);
	}

	private void LeaderboardScoresLoaded(string a_JSONMessage)
	{
		object obj = MiniJSON.jsonDecode(a_JSONMessage);
		int num = -99;
		List<GoogleLeaderboard> list = null;
		List<GoogleLeaderboardScore> list2 = null;
		int a_ScoresIndex = -1;
		IDictionary dictionary = obj as IDictionary;
		if (dictionary != null)
		{
			num = JSONObject.GetIntValueFromDict(dictionary, "statusCode", num);
			ICollection valueFromDict = JSONObject.GetValueFromDict<ICollection>(dictionary, "leaderboards", null);
			if (valueFromDict != null)
			{
				list = new List<GoogleLeaderboard>();
				foreach (object item in valueFromDict)
				{
					IDictionary dictionary2 = item as IDictionary;
					if (dictionary2 != null)
					{
						GoogleLeaderboard googleLeaderboard = new GoogleLeaderboard();
						googleLeaderboard.FromJSON(dictionary2);
						list.Add(googleLeaderboard);
					}
				}
			}
			ICollection valueFromDict2 = JSONObject.GetValueFromDict<ICollection>(dictionary, "scores", null);
			if (valueFromDict2 != null)
			{
				list2 = new List<GoogleLeaderboardScore>();
				foreach (object item2 in valueFromDict2)
				{
					IDictionary dictionary3 = item2 as IDictionary;
					if (dictionary3 != null)
					{
						GoogleLeaderboardScore googleLeaderboardScore = new GoogleLeaderboardScore();
						googleLeaderboardScore.FromJSON(dictionary3);
						list2.Add(googleLeaderboardScore);
					}
				}
			}
			a_ScoresIndex = JSONObject.GetIntValueFromDict(dictionary, "scoreBufferIndex", -1);
		}
		OnLeaderboardScoresLoadedEvent(num, list, list2, a_ScoresIndex);
	}

	private void ScoreSubmitted(string a_JSONMessage)
	{
		object obj = MiniJSON.jsonDecode(a_JSONMessage);
		int num = -99;
		GoogleSubmitScoreResult googleSubmitScoreResult = null;
		IDictionary dictionary = obj as IDictionary;
		if (dictionary != null)
		{
			num = JSONObject.GetIntValueFromDict(dictionary, "statusCode", num);
			IDictionary valueFromDict = JSONObject.GetValueFromDict<IDictionary>(dictionary, "result", null);
			if (valueFromDict != null)
			{
				googleSubmitScoreResult = new GoogleSubmitScoreResult();
				googleSubmitScoreResult.FromJSON(valueFromDict);
			}
		}
		OnScoreSubmittedEvent(num, googleSubmitScoreResult);
	}

	private void LeaderboardMetadataLoaded(string a_JSONMessage)
	{
		object obj = MiniJSON.jsonDecode(a_JSONMessage);
		int num = -99;
		List<GoogleLeaderboard> list = null;
		IDictionary dictionary = obj as IDictionary;
		if (dictionary != null)
		{
			num = JSONObject.GetIntValueFromDict(dictionary, "statusCode", num);
			ICollection valueFromDict = JSONObject.GetValueFromDict<ICollection>(dictionary, "leaderboards", null);
			if (valueFromDict != null)
			{
				list = new List<GoogleLeaderboard>();
				foreach (object item in valueFromDict)
				{
					IDictionary dictionary2 = item as IDictionary;
					if (dictionary2 != null)
					{
						GoogleLeaderboard googleLeaderboard = new GoogleLeaderboard();
						googleLeaderboard.FromJSON(dictionary2);
						list.Add(googleLeaderboard);
					}
				}
			}
		}
		GoogleGamesManager.OnLeaderboardMetadataLoaded(num, list);
	}

	private void AchievementUpdated(string a_JSONMessage)
	{
		object obj = MiniJSON.jsonDecode(a_JSONMessage);
		int num = -99;
		string a_AchievementId = null;
		IDictionary dictionary = obj as IDictionary;
		if (dictionary != null)
		{
			num = JSONObject.GetIntValueFromDict(dictionary, "statusCode", num);
			a_AchievementId = JSONObject.GetValueFromDict<string>(dictionary, "achievementId", null);
		}
		OnAchievementUpdatedEvent(num, a_AchievementId);
	}

	private void AchievementsLoaded(string a_JSONMessage)
	{
		object obj = MiniJSON.jsonDecode(a_JSONMessage);
		int num = -99;
		List<GoogleAchievement> list = null;
		IDictionary dictionary = obj as IDictionary;
		if (dictionary != null)
		{
			num = JSONObject.GetIntValueFromDict(dictionary, "statusCode", num);
			ICollection valueFromDict = JSONObject.GetValueFromDict<ICollection>(dictionary, "achievements", null);
			if (valueFromDict != null)
			{
				list = new List<GoogleAchievement>();
				foreach (object item in valueFromDict)
				{
					IDictionary dictionary2 = item as IDictionary;
					if (dictionary2 != null)
					{
						GoogleAchievement googleAchievement = new GoogleAchievement();
						googleAchievement.FromJSON(dictionary2);
						list.Add(googleAchievement);
					}
				}
			}
		}
		OnAchievementsLoadedEvent(num, list);
	}
}
