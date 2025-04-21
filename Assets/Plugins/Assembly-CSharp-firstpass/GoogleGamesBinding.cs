using UnityEngine;

public class GoogleGamesBinding
{
	public enum TimeSpan
	{
		TIME_SPAN_DAILY = 0,
		TIME_SPAN_WEEKLY = 1,
		TIME_SPAN_ALL_TIME = 2
	}

	public enum LeaderboardCollection
	{
		COLLECTION_PUBLIC = 0,
		COLLECTION_SOCIAL = 1
	}

	public enum PageDirection
	{
		NONE = -1,
		NEXT = 0,
		PREV = 1
	}

	public enum NotificationType
	{
		NOTIFICATION_TYPES_ALL = 0,
		NOTIFICATION_TYPES_MULTIPLAYER = 1,
		NOTIFICATION_TYPE_INVITATION = 2
	}

	private const string CLASS_NAME = "GoogleGamesBinding :: ";

	public const int CLIENT_NONE = 0;

	public const int CLIENT_GAMES = 1;

	public const int CLIENT_PLUS = 2;

	public const int CLIENT_APPSTATE = 4;

	public const int CLIENT_ALL = 7;

	public const int NOTIFICATION_TYPES_ALL = -1;

	public const int NOTIFICATION_TYPES_MULTIPLAYER = 1;

	public const int NOTIFICATION_TYPE_INVITATION = 1;

	public const int GRAVITY_BOTTOM = 80;

	public const int GRAVITY_CENTER = 17;

	public const int GRAVITY_CENTER_HORIZONTAL = 1;

	public const int GRAVITY_CENTER_VERTICAL = 16;

	public const int GRAVITY_LEFT = 3;

	public const int GRAVITY_NO_GRAVITY = 0;

	public const int GRAVITY_RIGHT = 5;

	public const int GRAVITY_TOP = 48;

	public const int GRAVITY_DEFAULT = 49;

	public const int STATUS_MISSING = -99;

	public const int STATUS_OK = 0;

	public const int STATUS_INTERNAL_ERROR = 1;

	public const int STATUS_CLIENT_RECONNECT_REQUIRED = 2;

	public const int STATUS_NETWORK_ERROR_STALE_DATA = 3;

	public const int STATUS_NETWORK_ERROR_NO_DATA = 4;

	public const int STATUS_NETWORK_ERROR_OPERATION_DEFERRED = 5;

	public const int STATUS_NETWORK_ERROR_OPERATION_FAILED = 6;

	public const int STATUS_LICENSE_CHECK_FAILED = 7;

	public const int STATUS_ACHIEVEMENT_UNLOCK_FAILURE = 3000;

	public const int STATUS_ACHIEVEMENT_UNKNOWN = 3001;

	public const int STATUS_ACHIEVEMENT_NOT_INCREMENTAL = 3002;

	public const int STATUS_ACHIEVEMENT_UNLOCKED = 3003;

	public const int STATUS_MULTIPLAYER_ERROR_CREATION_NOT_ALLOWED = 6000;

	public const int STATUS_MULTIPLAYER_ERROR_NOT_TRUSTED_TESTER = 6001;

	public const int STATUS_REAL_TIME_CONNECTION_FAILED = 7000;

	public const int STATUS_REAL_TIME_MESSAGE_SEND_FAILED = 7001;

	public const int STATUS_INVALID_REAL_TIME_ROOM_ID = 7002;

	public const int STATUS_PARTICIPANT_NOT_CONNECTED = 7003;

	public const int STATUS_REAL_TIME_ROOM_NOT_JOINED = 7004;

	public const int STATUS_REAL_TIME_INACTIVE_ROOM = 7005;

	public const int STATUS_REAL_TIME_MESSAGE_FAILED = -1;

	private static AndroidJavaObject m_AndroidGamePlugin;

	private static bool m_Debug;

	static GoogleGamesBinding()
	{
		if (Application.platform != RuntimePlatform.Android)
		{
			return;
		}
		using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
		{
			if (androidJavaClass == null)
			{
				return;
			}
			using (AndroidJavaObject androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity"))
			{
				if (androidJavaObject != null)
				{
					m_AndroidGamePlugin = androidJavaObject.Get<AndroidJavaObject>("mHelper");
					if (m_AndroidGamePlugin == null)
					{
						Debug.LogWarning("AndroidGamePlugin could not be found - have you updated your AndroidManifest file as required?");
					}
				}
			}
		}
	}

	public static void SetDebug(bool a_Debug)
	{
		m_Debug = a_Debug;
		if (Application.platform == RuntimePlatform.Android)
		{
			CallAndroid("SetDebug", a_Debug);
		}
	}

	public static void SetSigningInMessage(string a_SigningInMessage)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			CallAndroid("SetSigningInMessage", a_SigningInMessage);
		}
	}

	public static void SetSigningOutMessage(string a_SigningOutMessage)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			CallAndroid("SetSigningOutMessage", a_SigningOutMessage);
		}
	}

	public static void SetUnknownErrorMessage(string a_UnknownErrorMessage)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			CallAndroid("SetUnknownErrorMessage", a_UnknownErrorMessage);
		}
	}

	public static void DoSignIn()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			CallAndroid("DoSignIn");
		}
	}

	public static void DoSignOut()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			CallAndroid("DoSignOut");
		}
	}

	public static string GetAppId()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			return CallAndroidWithReturn<string>("GetAppId", null, new object[0]);
		}
		return null;
	}

	public static string GetCurrentAccountName()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			return CallAndroidWithReturn<string>("GetCurrentAccountName", null, new object[0]);
		}
		return null;
	}

	public static bool GetIsSignedIn()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			return CallAndroidWithReturn("GetIsSignedIn", false);
		}
		return false;
	}

	public static bool GetIsConnected()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			return CallAndroidWithReturn("GetIsConnected", false);
		}
		return false;
	}

	public static bool GetIsConnecting()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			return CallAndroidWithReturn("GetIsConnecting", false);
		}
		return false;
	}

	public static void DoClearAllNotifications()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			CallAndroid("DoClearAllNotifications");
		}
	}

	public static void DoClearNotifications(NotificationType a_NotificationType)
	{
		switch (a_NotificationType)
		{
		case NotificationType.NOTIFICATION_TYPES_ALL:
			DoClearNotifications(-1);
			break;
		case NotificationType.NOTIFICATION_TYPES_MULTIPLAYER:
			DoClearNotifications(1);
			break;
		case NotificationType.NOTIFICATION_TYPE_INVITATION:
			DoClearNotifications(1);
			break;
		default:
			DoClearNotifications(-1);
			break;
		}
	}

	public static void DoClearNotifications(int a_NotificationTypes)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			CallAndroid("DoClearNotifications", a_NotificationTypes);
		}
	}

	public static void SetDefaultGravityForPopups()
	{
		SetGravityForPopups(49);
	}

	public static void SetGravityForPopups(int a_Gravity)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			CallAndroid("SetGravityForPopups", a_Gravity);
		}
	}

	public static void SetUseNewPlayerNotificationsFirstParty(bool a_NewPlayerStyle)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			CallAndroid("SetUseNewPlayerNotificationsFirstParty", a_NewPlayerStyle);
		}
	}

	public static void DoLoadGame()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			CallAndroid("DoLoadGame");
		}
	}

	public static string GetCurrentPlayerId()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			return CallAndroidWithReturn<string>("GetCurrentPlayerId", null, new object[0]);
		}
		return null;
	}

	public static string GetCurrentPlayerDisplayName()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			return CallAndroidWithReturn<string>("GetCurrentPlayerDisplayName", null, new object[0]);
		}
		return null;
	}

	public static long GetCurrentPlayerRetrievedTimestamp()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			return CallAndroidWithReturn("GetCurrentPlayerRetrievedTimestamp", -1L);
		}
		return -1L;
	}

	public static void DoLoadPlayer(string a_PlayerId)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			CallAndroid("DoLoadPlayer", a_PlayerId);
		}
	}

	public static void DoLoadLeaderboardMetadata()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			CallAndroid("DoLoadLeaderboardMetadata");
		}
	}

	public static void DoLoadLeaderboardMetadataSingle(string a_LeaderboardId)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			CallAndroid("DoLoadLeaderboardMetadataSingle", a_LeaderboardId);
		}
	}

	public static void DoSubmitScore(string a_LeaderboardId, long a_Score)
	{
		if (string.IsNullOrEmpty(a_LeaderboardId))
		{
			Debug.LogError("Cannot submit score with a null or empty leaderboard ID!");
		}
		else if (Application.platform == RuntimePlatform.Android)
		{
			CallAndroid("DoSubmitScore", a_LeaderboardId, a_Score);
		}
	}

	public static void DoSubmitScoreImmediate(string a_LeaderboardId, long a_Score)
	{
		if (string.IsNullOrEmpty(a_LeaderboardId))
		{
			Debug.LogError("Cannot submit score with a null or empty leaderboard ID!");
		}
		else if (Application.platform == RuntimePlatform.Android)
		{
			CallAndroid("DoSubmitScoreImmediate", a_LeaderboardId, a_Score);
		}
	}

	public static void DoLoadPlayerCenteredScores(string a_LeaderboardId, TimeSpan a_TimeSpan, LeaderboardCollection a_LeaderboardCollection, int a_MaxResults, bool a_ForceReload = false)
	{
		a_MaxResults = Mathf.Clamp(a_MaxResults, 1, 25);
		if (Application.platform == RuntimePlatform.Android)
		{
			CallAndroid("DoLoadPlayerCenteredScores", a_LeaderboardId, (int)a_TimeSpan, (int)a_LeaderboardCollection, a_MaxResults, a_ForceReload);
		}
	}

	public static void DoLoadTopScores(string a_LeaderboardId, TimeSpan a_TimeSpan, LeaderboardCollection a_LeaderboardCollection, int a_MaxResults, bool a_ForceReload = false)
	{
		a_MaxResults = Mathf.Clamp(a_MaxResults, 1, 25);
		if (Application.platform == RuntimePlatform.Android)
		{
			CallAndroid("DoLoadTopScores", a_LeaderboardId, (int)a_TimeSpan, (int)a_LeaderboardCollection, a_MaxResults, a_ForceReload);
		}
	}

	public static void DoLoadMoreScores(int a_ScoreIndex, int a_MaxResults, PageDirection a_PageDirection)
	{
		a_MaxResults = Mathf.Clamp(a_MaxResults, 1, 25);
		if (Application.platform == RuntimePlatform.Android)
		{
			CallAndroid("DoLoadMoreScores", a_ScoreIndex, a_MaxResults, (int)a_PageDirection);
		}
	}

	public static void DoLoadAchievements()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			CallAndroid("DoLoadAchievements");
		}
	}

	public static void DoIncrementAchievement(string a_AchievementId, int a_NumSteps)
	{
		if (string.IsNullOrEmpty(a_AchievementId))
		{
			Debug.LogError("Cannot increment achievement with a null or empty leaderboard ID!");
		}
		else if (Application.platform == RuntimePlatform.Android)
		{
			CallAndroid("DoIncrementAchievement", a_AchievementId, a_NumSteps);
		}
	}

	public static void DoIncrementAchievementImmediate(string a_AchievementId, int a_NumSteps)
	{
		if (string.IsNullOrEmpty(a_AchievementId))
		{
			Debug.LogError("Cannot increment achievement with a null or empty leaderboard ID!");
		}
		else if (Application.platform == RuntimePlatform.Android)
		{
			CallAndroid("DoIncrementAchievementImmediate", a_AchievementId, a_NumSteps);
		}
	}

	public static void DoUnlockAchievement(string a_AchievementId)
	{
		if (string.IsNullOrEmpty(a_AchievementId))
		{
			Debug.LogError("Cannot unlock achievement with a null or empty leaderboard ID!");
		}
		else if (Application.platform == RuntimePlatform.Android)
		{
			CallAndroid("DoUnlockAchievement", a_AchievementId);
		}
	}

	public static void DoUnlockAchievementImmediate(string a_AchievementId)
	{
		if (string.IsNullOrEmpty(a_AchievementId))
		{
			Debug.LogError("Cannot unlock achievement with a null or empty leaderboard ID!");
		}
		else if (Application.platform == RuntimePlatform.Android)
		{
			CallAndroid("DoUnlockAchievementImmediate", a_AchievementId);
		}
	}

	public static void DoRevealAchievement(string a_AchievementId)
	{
		if (string.IsNullOrEmpty(a_AchievementId))
		{
			Debug.LogError("Cannot reveal achievement with a null or empty leaderboard ID!");
		}
		else if (Application.platform == RuntimePlatform.Android)
		{
			CallAndroid("DoRevealAchievement", a_AchievementId);
		}
	}

	public static void DoRevealAchievementImmediate(string a_AchievementId)
	{
		if (string.IsNullOrEmpty(a_AchievementId))
		{
			Debug.LogError("Cannot reveal achievement with a null or empty leaderboard ID!");
		}
		else if (Application.platform == RuntimePlatform.Android)
		{
			CallAndroid("DoRevealAchievementImmediate", a_AchievementId);
		}
	}

	public static void DoShowAllLeaderboards()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			CallAndroid("DoShowAllLeaderboards");
		}
	}

	public static void DoShowSingleLeaderboard(string a_LeaderboardId)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			CallAndroid("DoShowSingleLeaderboard", a_LeaderboardId);
		}
	}

	public static void DoShowAllAchievements()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			CallAndroid("DoShowAllAchievements");
		}
	}

	public static void DoShowSettings()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			CallAndroid("DoShowSettings");
		}
	}

	private static void Log(string a_FunctionName, params object[] a_Args)
	{
		if (!m_Debug)
		{
			return;
		}
		string text = string.Empty;
		for (int i = 0; i < a_Args.Length; i++)
		{
			string text2 = text;
			text = string.Concat(text2, a_Args[i], "(", a_Args[i].GetType(), ")");
			if (i != a_Args.Length - 1)
			{
				text += ",";
			}
		}
		Debug.Log("GoogleGamesBinding :: " + a_FunctionName + " with arguments [" + text + "].");
	}

	private static void LogWarning(string a_FunctionName, string a_Warning)
	{
		if (m_Debug)
		{
			Debug.LogWarning("GoogleGamesBinding :: " + a_FunctionName + " - " + a_Warning);
		}
	}

	private static void CallAndroid(string a_FunctionName, params object[] a_Args)
	{
		if (m_AndroidGamePlugin != null)
		{
			Log(a_FunctionName, a_Args);
			m_AndroidGamePlugin.Call(a_FunctionName, a_Args);
		}
		else
		{
			LogWarning(a_FunctionName, "Failed because Android Game Plugin is null!");
		}
	}

	private static T CallAndroidWithReturn<T>(string a_FunctionName, T a_DefaultValue, params object[] a_Args)
	{
		if (m_AndroidGamePlugin != null)
		{
			Log(a_FunctionName, a_Args);
			return m_AndroidGamePlugin.Call<T>(a_FunctionName, a_Args);
		}
		LogWarning(a_FunctionName, "Failed because Android Game Plugin is null!");
		return a_DefaultValue;
	}
}
