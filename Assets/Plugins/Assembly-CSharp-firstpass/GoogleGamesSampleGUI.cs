using System.Collections.Generic;
using UnityEngine;

public class GoogleGamesSampleGUI : MonoBehaviour
{
	private GoogleGame m_GameDetails;

	private bool m_IsSignedIn;

	private string m_CurrentPlayerId;

	private string m_CurrentPlayerName;

	private List<GoogleLeaderboard> m_AllLeaderboards;

	private List<GoogleAchievement> m_AllAchievements;

	private bool m_IsSubmittingScore;

	private long m_ScoreBeingSubmitted = -1L;

	private string m_LeaderboardBeingSubmittedTo = string.Empty;

	private bool m_FinishedSubmittingScore;

	private int m_ScoreSubmissionStatus = -1;

	private GoogleLeaderboard m_LastScoreLeaderboardLoaded;

	private List<GoogleLeaderboardScore> m_LastScoresLoaded;

	private int m_LastLoadMoreScoresIndex = -1;

	private GoogleAchievement m_AchievementBeingUnlocked;

	private int m_AchievementUpdateStatusCode = -1;

	private void Awake()
	{
		m_IsSignedIn = GoogleGamesBinding.GetIsSignedIn();
		GoogleGamesManager.OnSignInSucceeded += OnSignInSucceeded;
		GoogleGamesManager.OnSignInFailed += OnSignInFailed;
		GoogleGamesManager.OnGamesLoaded += OnGamesLoaded;
		GoogleGamesManager.OnSignedOut += OnSignedOut;
		GoogleGamesManager.OnLeaderboardScoresLoaded += OnLeaderboardScoresLoaded;
		GoogleGamesManager.OnScoreSubmitted += OnScoreSubmitted;
		GoogleGamesManager.OnLeaderboardMetadataLoaded += OnLeaderboardMetadataLoaded;
		GoogleGamesManager.OnAchievementsLoaded += OnAchievementsLoaded;
		GoogleGamesManager.OnAchievementUpdated += OnAchievementUpdated;
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
		GoogleGamesManager.OnAchievementsLoaded -= OnAchievementsLoaded;
		GoogleGamesManager.OnAchievementUpdated -= OnAchievementUpdated;
	}

	private Rect GetButtonPosition(int a_Column, int a_Row)
	{
		int num = 2;
		int num2 = 6;
		a_Column = Mathf.Clamp(a_Column, 0, num - 1);
		a_Row = Mathf.Clamp(a_Row, 0, num2 - 1);
		float num3 = 50f;
		float num4 = 10f;
		float num5 = 0f;
		float num6 = 0f;
		float num7 = Screen.width / num;
		float num8 = ((float)Screen.height - num3) / (float)num2;
		num5 = num7 * (float)a_Column + num4;
		num6 = num3 + num8 * (float)a_Row + num4;
		return new Rect(num5, num6, num7 - num4 * 2f, num8 - num4 * 2f);
	}

	private void OnGUI()
	{
		if (m_CurrentPlayerId != null && m_CurrentPlayerName != null)
		{
			GUI.Label(new Rect(10f, 10f, (float)Screen.width / 2f, 20f), "PlayerId = " + m_CurrentPlayerId);
			GUI.Label(new Rect(10f, 20f, (float)Screen.width / 2f, 20f), "PlayerName = " + m_CurrentPlayerName);
		}
		else
		{
			GUI.Label(new Rect(10f, 25f, (float)Screen.width / 2f, 30f), "Sign in to get your player details");
		}
		if (m_GameDetails != null)
		{
			GUI.Label(new Rect((float)Screen.width / 2f + 10f, 10f, (float)Screen.width / 2f, 20f), "GameName = " + m_GameDetails.GetDisplayName());
			GUI.Label(new Rect((float)Screen.width / 2f + 10f, 20f, (float)Screen.width / 2f, 20f), "DeveloperName = " + m_GameDetails.GetDeveloperName());
			GUI.Label(new Rect((float)Screen.width / 2f + 10f, 30f, (float)Screen.width / 2f, 20f), "ApplicationId = " + m_GameDetails.GetApplicationId());
		}
		else
		{
			GUI.Label(new Rect((float)Screen.width / 2f + 10f, 25f, (float)Screen.width / 2f, 30f), "Sign in to load the game details");
		}
		if (!m_IsSignedIn)
		{
			if (GUI.Button(GetButtonPosition(0, 0), "Sign in"))
			{
				GoogleGamesBinding.DoSignIn();
			}
			return;
		}
		if (GUI.Button(GetButtonPosition(0, 0), "Sign out"))
		{
			GoogleGamesBinding.DoSignOut();
		}
		Rect buttonPosition = GetButtonPosition(1, 0);
		buttonPosition.width /= 2f;
		if (GUI.Button(buttonPosition, "Show Leaderboards GUI"))
		{
			GoogleGamesBinding.DoShowAllLeaderboards();
		}
		buttonPosition.x += buttonPosition.width;
		if (GUI.Button(buttonPosition, "Show Achievements GUI"))
		{
			GoogleGamesBinding.DoShowAllAchievements();
		}
		if (GUI.Button(GetButtonPosition(0, 1), "Load Leaderboards Metadata"))
		{
			GoogleGamesBinding.DoLoadLeaderboardMetadata();
		}
		if (GUI.Button(GetButtonPosition(0, 2), "Load Random Leaderboard Scores"))
		{
			LoadRandomScores();
		}
		if (GUI.Button(GetButtonPosition(0, 3), "Load more scores..."))
		{
			LoadMoreScores();
		}
		Rect buttonPosition2 = GetButtonPosition(0, 4);
		if (m_AllLeaderboards == null)
		{
			GUI.Label(buttonPosition2, "Load leaderboard metadata to load scores...");
		}
		else if (m_AllLeaderboards.Count == 0)
		{
			GUI.Label(buttonPosition2, "No leaderboards found for this app!");
		}
		else if (m_AllLeaderboards == null)
		{
			GUI.Label(buttonPosition2, "Load leaderboard metadata first...");
		}
		else if (m_LastLoadMoreScoresIndex < 0)
		{
			GUI.Label(buttonPosition2, "Load some scores to see the results...");
		}
		else
		{
			string text = "Unknown";
			if (m_LastScoreLeaderboardLoaded != null)
			{
				text = m_LastScoreLeaderboardLoaded.GetDisplayName();
			}
			int num = ((m_LastScoresLoaded != null) ? m_LastScoresLoaded.Count : 0);
			string text2 = string.Empty;
			if (m_LastScoresLoaded != null)
			{
				foreach (GoogleLeaderboardScore item in m_LastScoresLoaded)
				{
					string text3 = text2;
					text2 = text3 + "\n" + item.GetScoreHolderDisplayName() + " - " + item.GetDisplayRank() + " - " + item.GetDisplayScore() + " - " + item.GetTimestampMillis();
				}
			}
			GUI.Label(buttonPosition2, "Received total of " + num + " scores from leaderboard " + text + ". Scores:" + text2);
		}
		if (GUI.Button(GetButtonPosition(0, 5), "Submit Random Leaderboard Score"))
		{
			SubmitRandomScore();
		}
		Rect buttonPosition3 = GetButtonPosition(1, 5);
		if (m_IsSubmittingScore)
		{
			if (m_FinishedSubmittingScore)
			{
				GUI.Label(buttonPosition3, "Finished submitting score of " + m_ScoreBeingSubmitted + " to leaderboard " + m_LeaderboardBeingSubmittedTo + " with a status code of " + m_ScoreSubmissionStatus + ".");
			}
			else
			{
				GUI.Label(buttonPosition3, "Submitting score of " + m_ScoreBeingSubmitted + " to leaderboard " + m_LeaderboardBeingSubmittedTo + "...");
			}
		}
		else if (m_AllLeaderboards == null)
		{
			GUI.Label(buttonPosition3, "Load leaderboard metadata to submit a score...");
		}
		else if (m_AllLeaderboards.Count == 0)
		{
			GUI.Label(buttonPosition3, "No leaderboards found for this app!");
		}
		else
		{
			GUI.Label(buttonPosition3, "Submit a score to see more information.");
		}
		if (GUI.Button(GetButtonPosition(1, 1), "Load Achievements"))
		{
			GoogleGamesBinding.DoLoadAchievements();
		}
		Rect buttonPosition4 = GetButtonPosition(1, 2);
		if (m_AllAchievements == null)
		{
			GUI.Label(buttonPosition4, "Load Achievement metadata to see the data...");
		}
		else if (m_AllAchievements.Count == 0)
		{
			GUI.Label(buttonPosition4, "No Achievements found for this app!");
		}
		else
		{
			string text4 = "Found a total of " + m_AllAchievements.Count + " Achievements:";
			foreach (GoogleAchievement allAchievement in m_AllAchievements)
			{
				string text3 = text4;
				text4 = text3 + "\n" + allAchievement.GetAchievementId() + " - " + allAchievement.GetName() + " - " + allAchievement.GetDescription() + " - " + ((!allAchievement.IsIncrementalAchievement()) ? "Standard" : "Incremental") + " - ";
				if (allAchievement.IsHidden())
				{
					text4 += "Hidden";
				}
				else if (allAchievement.IsUnlocked())
				{
					text4 += "Unlocked";
				}
				else if (allAchievement.IsRevealed())
				{
					text4 += "Revealed";
				}
			}
			GUI.Label(buttonPosition4, text4);
		}
		if (GUI.Button(GetButtonPosition(1, 3), "Unlock Random Achievement"))
		{
			UnlockRandomAchievement();
		}
		Rect buttonPosition5 = GetButtonPosition(1, 4);
		if (m_AllAchievements == null)
		{
			GUI.Label(buttonPosition5, "Load Achievement metadata to see the data...");
		}
		else if (m_AllAchievements.Count == 0)
		{
			GUI.Label(buttonPosition5, "No Achievements found for this app!");
		}
		else if (m_AchievementBeingUnlocked == null)
		{
			GUI.Label(buttonPosition5, "Press unlock achievement to see the status...");
		}
		else if (m_AchievementUpdateStatusCode < 0)
		{
			if (m_AchievementBeingUnlocked.IsIncrementalAchievement())
			{
				GUI.Label(buttonPosition5, "Now incrementing the steps for achievement " + m_AchievementBeingUnlocked.GetName() + "...");
			}
			else
			{
				GUI.Label(buttonPosition5, "Now unlocking achievement " + m_AchievementBeingUnlocked.GetName() + "...");
			}
		}
		else if (m_AchievementBeingUnlocked.IsIncrementalAchievement())
		{
			GUI.Label(buttonPosition5, "Finished incrementing the steps for achievement " + m_AchievementBeingUnlocked.GetName() + " with status " + m_AchievementUpdateStatusCode);
		}
		else
		{
			GUI.Label(buttonPosition5, "Finished unlocking achievement " + m_AchievementBeingUnlocked.GetName() + " with status " + m_AchievementUpdateStatusCode);
		}
	}

	private void OnSignInSucceeded()
	{
		m_CurrentPlayerId = GoogleGamesBinding.GetCurrentPlayerId();
		m_CurrentPlayerName = GoogleGamesBinding.GetCurrentPlayerDisplayName();
		m_IsSignedIn = true;
		GoogleGamesBinding.DoLoadGame();
	}

	private void OnSignInFailed()
	{
		m_CurrentPlayerId = null;
		m_CurrentPlayerName = null;
		m_IsSignedIn = false;
	}

	private void OnSignedOut()
	{
		m_CurrentPlayerId = null;
		m_CurrentPlayerName = null;
		m_IsSignedIn = false;
	}

	private void OnGamesLoaded(int a_StatusCode, List<GoogleGame> a_Games)
	{
		if (a_Games != null && a_Games.Count > 0)
		{
			m_GameDetails = a_Games[0];
		}
	}

	private void OnLeaderboardScoresLoaded(int a_StatusCode, List<GoogleLeaderboard> a_Leaderboards, List<GoogleLeaderboardScore> a_Scores, int a_LoadMoreScoresIndex)
	{
		m_LastLoadMoreScoresIndex = a_LoadMoreScoresIndex;
		if (a_Leaderboards != null && a_Leaderboards.Count > 0)
		{
			m_LastScoreLeaderboardLoaded = a_Leaderboards[0];
		}
		m_LastScoresLoaded = a_Scores;
	}

	private void OnScoreSubmitted(int a_StatusCode, GoogleSubmitScoreResult a_SubmitScoreResult)
	{
		m_FinishedSubmittingScore = true;
		m_ScoreSubmissionStatus = a_StatusCode;
	}

	private void OnLeaderboardMetadataLoaded(int a_StatusCode, List<GoogleLeaderboard> a_Leaderboards)
	{
		m_AllLeaderboards = a_Leaderboards;
	}

	private void OnAchievementsLoaded(int a_StatusCode, List<GoogleAchievement> a_Achievements)
	{
		m_AllAchievements = a_Achievements;
	}

	private void OnAchievementUpdated(int a_StatusCode, string a_AchievementId)
	{
		if (m_AchievementBeingUnlocked != null && m_AchievementBeingUnlocked.GetAchievementId() == a_AchievementId)
		{
			m_AchievementUpdateStatusCode = a_StatusCode;
		}
	}

	private void SubmitRandomScore()
	{
		if (m_AllLeaderboards != null && m_AllLeaderboards.Count > 0)
		{
			GoogleLeaderboard googleLeaderboard = m_AllLeaderboards[Random.Range(0, m_AllLeaderboards.Count)];
			if (googleLeaderboard != null)
			{
				m_IsSubmittingScore = true;
				m_FinishedSubmittingScore = false;
				m_ScoreBeingSubmitted = Random.Range(1, 1000);
				m_LeaderboardBeingSubmittedTo = googleLeaderboard.GetDisplayName();
				GoogleGamesBinding.DoSubmitScoreImmediate(googleLeaderboard.GetLeaderboardId(), m_ScoreBeingSubmitted);
			}
		}
		else
		{
			Debug.LogWarning("No leaderboards found!");
		}
	}

	private void UnlockRandomAchievement()
	{
		if (m_AllAchievements != null && m_AllAchievements.Count > 0)
		{
			GoogleAchievement googleAchievement = m_AllAchievements[Random.Range(0, m_AllAchievements.Count)];
			if (googleAchievement != null)
			{
				m_AchievementBeingUnlocked = googleAchievement;
				m_AchievementUpdateStatusCode = -1;
				if (googleAchievement.IsStandardAchievement())
				{
					GoogleGamesBinding.DoUnlockAchievementImmediate(googleAchievement.GetAchievementId());
				}
				else
				{
					GoogleGamesBinding.DoIncrementAchievementImmediate(googleAchievement.GetAchievementId(), 1);
				}
			}
		}
		else
		{
			Debug.LogWarning("No achievements found!");
		}
	}

	private void LoadRandomScores()
	{
		if (m_AllLeaderboards != null && m_AllLeaderboards.Count > 0)
		{
			GoogleLeaderboard googleLeaderboard = m_AllLeaderboards[Random.Range(0, m_AllLeaderboards.Count)];
			if (googleLeaderboard != null)
			{
				GoogleGamesBinding.DoLoadTopScores(googleLeaderboard.GetLeaderboardId(), GoogleGamesBinding.TimeSpan.TIME_SPAN_ALL_TIME, GoogleGamesBinding.LeaderboardCollection.COLLECTION_PUBLIC, 10);
			}
		}
		else
		{
			Debug.LogWarning("No leaderboards found!");
		}
	}

	private void LoadMoreScores()
	{
		if (m_LastLoadMoreScoresIndex >= 0)
		{
			GoogleGamesBinding.DoLoadMoreScores(m_LastLoadMoreScoresIndex, 10, GoogleGamesBinding.PageDirection.NEXT);
		}
		else
		{
			Debug.LogWarning("No scores have been loaded yet!");
		}
	}
}
