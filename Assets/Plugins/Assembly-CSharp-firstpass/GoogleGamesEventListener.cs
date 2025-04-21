using System.Collections.Generic;
using UnityEngine;

public class GoogleGamesEventListener : MonoBehaviour
{
	private void Awake()
	{
		Object.DontDestroyOnLoad(base.gameObject);
		GoogleGamesManager.OnSignInSucceeded += OnSignInSucceeded;
		GoogleGamesManager.OnSignInFailed += OnSignInFailed;
		GoogleGamesManager.OnGamesLoaded += OnGamesLoaded;
		GoogleGamesManager.OnSignedOut += OnSignedOut;
		GoogleGamesManager.OnLeaderboardScoresLoaded += OnLeaderboardScoresLoaded;
		GoogleGamesManager.OnScoreSubmitted += OnScoreSubmitted;
		GoogleGamesManager.OnLeaderboardMetadataLoaded += OnLeaderboardMetadataLoaded;
		GoogleGamesManager.OnAchievementUpdated += OnAchievementUpdated;
		GoogleGamesManager.OnAchievementsLoaded += OnAchievementsLoaded;
	}

	private void OnDestroy()
	{
		GoogleGamesManager.OnSignInSucceeded -= OnSignInSucceeded;
		GoogleGamesManager.OnSignInFailed -= OnSignInFailed;
		GoogleGamesManager.OnGamesLoaded -= OnGamesLoaded;
		GoogleGamesManager.OnSignedOut -= OnSignedOut;
		GoogleGamesManager.OnLeaderboardScoresLoaded -= OnLeaderboardScoresLoaded;
		GoogleGamesManager.OnScoreSubmitted -= OnScoreSubmitted;
		GoogleGamesManager.OnLeaderboardMetadataLoaded -= OnLeaderboardMetadataLoaded;
		GoogleGamesManager.OnAchievementUpdated -= OnAchievementUpdated;
		GoogleGamesManager.OnAchievementsLoaded -= OnAchievementsLoaded;
	}

	private void OnSignInSucceeded()
	{
		Debug.Log("OnSignInSucceeded");
	}

	private void OnSignInFailed()
	{
		Debug.Log("OnSignInFailed");
	}

	private void OnSignedOut()
	{
		Debug.Log("OnSignedOut");
	}

	private void OnGamesLoaded(int a_StatusCode, List<GoogleGame> a_Games)
	{
		Debug.Log("OnGamesLoaded with status [" + a_StatusCode + "], [" + ((a_Games != null) ? a_Games.Count : 0) + "] games.");
		string text = "Games:";
		foreach (GoogleGame a_Game in a_Games)
		{
			string text2 = text;
			text = text2 + "\n" + a_Game.GetDisplayName() + " - " + a_Game.GetDeveloperName() + " - " + a_Game.GetApplicationId();
		}
		Debug.Log(text);
	}

	private void OnLeaderboardScoresLoaded(int a_StatusCode, List<GoogleLeaderboard> a_Leaderboards, List<GoogleLeaderboardScore> a_Scores, int a_LoadMoreScoresIndex)
	{
		Debug.Log("OnLeaderboardScoresLoaded with status [" + a_StatusCode + "], [" + ((a_Leaderboards != null) ? a_Leaderboards.Count : 0) + "] leaderboards, and [" + ((a_Scores != null) ? a_Scores.Count : 0) + "] scores. With load more scores index [" + a_LoadMoreScoresIndex + "].");
		string text = "Leaderboards:";
		foreach (GoogleLeaderboard a_Leaderboard in a_Leaderboards)
		{
			string text2 = text;
			text = text2 + "\n" + a_Leaderboard.GetDisplayName() + " - " + a_Leaderboard.GetLeaderboardId();
		}
		Debug.Log(text);
		string text3 = "Scores:";
		foreach (GoogleLeaderboardScore a_Score in a_Scores)
		{
			string text2 = text3;
			text3 = text2 + "\n" + a_Score.GetScoreHolderDisplayName() + " - " + a_Score.GetDisplayRank() + " - " + a_Score.GetDisplayScore() + " - " + a_Score.GetTimestampMillis();
		}
		Debug.Log(text3);
	}

	private void OnScoreSubmitted(int a_StatusCode, GoogleSubmitScoreResult a_SubmitScoreResult)
	{
		Debug.Log("OnScoreSubmitted with status [" + a_StatusCode + "].");
		string text = "Score result = [LeaderboardId " + a_SubmitScoreResult.GetLeaderboardId() + " - PlayerId " + a_SubmitScoreResult.GetPlayerId() + "]";
		GoogleSubmitScoreResult.Result scoreResult = a_SubmitScoreResult.GetScoreResult(GoogleGamesBinding.TimeSpan.TIME_SPAN_ALL_TIME);
		GoogleSubmitScoreResult.Result scoreResult2 = a_SubmitScoreResult.GetScoreResult(GoogleGamesBinding.TimeSpan.TIME_SPAN_WEEKLY);
		GoogleSubmitScoreResult.Result scoreResult3 = a_SubmitScoreResult.GetScoreResult(GoogleGamesBinding.TimeSpan.TIME_SPAN_DAILY);
		if (scoreResult == null)
		{
			text += "\nAll Time Result is null.";
		}
		else
		{
			string text2 = text;
			text = text2 + "\nAll Time Result = [" + scoreResult.GetFormattedScore() + " NewBest " + scoreResult.GetNewBest() + "].";
		}
		if (scoreResult2 == null)
		{
			text += "\nWeekly Result is null.";
		}
		else
		{
			string text2 = text;
			text = text2 + "\nWeekly Result = [" + scoreResult2.GetFormattedScore() + " NewBest " + scoreResult2.GetNewBest() + "].";
		}
		if (scoreResult3 == null)
		{
			text += "\nDaily Result is null.";
		}
		else
		{
			string text2 = text;
			text = text2 + "\nDaily Result = [" + scoreResult3.GetFormattedScore() + " NewBest " + scoreResult3.GetNewBest() + "].";
		}
		Debug.Log(text);
	}

	private void OnLeaderboardMetadataLoaded(int a_StatusCode, List<GoogleLeaderboard> a_Leaderboards)
	{
		Debug.Log("OnLeaderboardMetadataLoaded with status [" + a_StatusCode + "], [" + ((a_Leaderboards != null) ? a_Leaderboards.Count : 0) + "] leaderboards");
		string text = "Leaderboards:";
		foreach (GoogleLeaderboard a_Leaderboard in a_Leaderboards)
		{
			string text2 = text;
			text = text2 + "\n" + a_Leaderboard.GetDisplayName() + " - " + a_Leaderboard.GetLeaderboardId();
		}
		Debug.Log(text);
	}

	private void OnAchievementUpdated(int a_StatusCode, string a_AchievementId)
	{
		Debug.Log("OnAchievementUpdated with status [" + a_StatusCode + "], AchievementId [" + a_AchievementId + "]");
	}

	private void OnAchievementsLoaded(int a_StatusCode, List<GoogleAchievement> a_Achievements)
	{
		Debug.Log("OnAchievementsLoaded with status [" + a_StatusCode + "], [" + ((a_Achievements != null) ? a_Achievements.Count : 0) + "] achievements");
		string text = "Achievements:";
		foreach (GoogleAchievement a_Achievement in a_Achievements)
		{
			string text2 = text;
			text = text2 + "\n" + a_Achievement.GetAchievementId() + " - " + a_Achievement.GetName() + " - " + a_Achievement.GetDescription() + " - " + ((!a_Achievement.IsIncrementalAchievement()) ? "Standard" : "Incremental") + " - ";
			if (a_Achievement.IsHidden())
			{
				text += "Hidden";
			}
			else if (a_Achievement.IsUnlocked())
			{
				text += "Unlocked";
			}
			else if (a_Achievement.IsRevealed())
			{
				text += "Revealed";
			}
		}
		Debug.Log(text);
	}
}
