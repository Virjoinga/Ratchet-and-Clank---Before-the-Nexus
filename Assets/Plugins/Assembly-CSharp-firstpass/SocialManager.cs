using GameCenter;
using UnityEngine;

public class SocialManager : MonoBehaviour
{
	public static SocialManager instance;

	public bool isSignedIn;

	public string CurrentPlayerId;

	public string CurrentPlayerName;

	private void Awake()
	{
		if ((bool)instance)
		{
			Debug.LogError("SocialManager: Multiple instances spawned");
			Object.Destroy(base.gameObject);
		}
		else
		{
			instance = this;
		}
	}

	public string GetFriendChallengeName()
	{
		return string.Empty;
	}

	public string GetFriendChallengeScore()
	{
		return string.Empty;
	}

	public int GetFriendScoreToBeat()
	{
		return 0;
	}

	private void Start()
	{
		isSignedIn = false;
	}

	public void OnConnectionState(bool isOnline)
	{
		if (isOnline)
		{
			OnSignInSuccess();
		}
		else
		{
			OnSignInFailed();
		}
	}

	private void OnScoresSuccess(string _playerID, long _scores, string _categoryID)
	{
		UIStartMenu startMenu = UIManager.instance.GetStartMenu();
		if (startMenu != null)
		{
			startMenu.UpdateFriendChallengeButton();
		}
	}

	private void OnScoresFailed(string _playerID, string _error, string _categoryID)
	{
	}

	private void OnAchievemetsSuccess(Achievement[] _Achievements)
	{
	}

	private void OnAchievemetsFailed(string _Error)
	{
	}

	private void LoadFriends(bool authenticated)
	{
		if (!authenticated)
		{
		}
	}

	public void SignIn()
	{
	}

	public void SignOut()
	{
	}

	private void OnSignInFailed()
	{
		Debug.Log("Sign in failed");
		CurrentPlayerId = null;
		CurrentPlayerName = null;
		isSignedIn = false;
	}

	private void OnSignInSuccess()
	{
		Debug.Log("Sign in Success");
		isSignedIn = true;
		UIStartMenu startMenu = UIManager.instance.GetStartMenu();
		if (startMenu != null)
		{
			startMenu.UpdateFriendChallengeButton();
		}
	}

	private void OnSignOut()
	{
		CurrentPlayerId = null;
		CurrentPlayerName = null;
		isSignedIn = false;
	}

	public void CompleteAchievement(string achID)
	{
		if (!isSignedIn)
		{
		}
	}

	public void LoadFriendChallengeScore(bool authenticated = true)
	{
	}

	public void SubmitScore(int leaderboardNum, long score)
	{
	}

	public void ShowLeaderboards()
	{
	}
}
