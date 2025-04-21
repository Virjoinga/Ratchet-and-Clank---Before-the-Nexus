using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace GameCenter
{
	public class Service
	{
		public delegate void ConnectionStateDelegate(bool _online);

		public delegate void ResponseScoresSuccessDelegate(string _playerID, long _scores, string _categoryID);

		public delegate void ResponseScoresFailedDelegate(string _playerID, string _error, string _categoryID);

		public delegate void ResponseAchievementsSuccessDelegate(Achievement[] _Achievements);

		public delegate void ResponseAchievementsFailedDelegate(string _Error);

		public static bool showAchievementNotification
		{
			set
			{
			}
		}

		public static bool IsAvailable
		{
			get
			{
				return false;
			}
		}

		public static bool IsAuthenticated
		{
			get
			{
				return false;
			}
		}

		[Obsolete("GameCenter.IsComplete has been deprecated. Use GameCenter.IsAuthenticated")]
		public static bool IsComplete
		{
			get
			{
				return IsAuthenticated;
			}
		}

		public static string PlayerID
		{
			get
			{
				return string.Empty;
			}
		}

		public static string PlayerAlias
		{
			get
			{
				return string.Empty;
			}
		}

		[Obsolete("GameCenter.Orientation has been deprecated. The rotation occurs with the status bar.")]
		public static ScreenOrientation Orientation
		{
			set
			{
			}
		}

		[method: MethodImpl(32)]
		public static event ConnectionStateDelegate ConnectionState;

		[method: MethodImpl(32)]
		public static event ResponseScoresSuccessDelegate ResponseScoresSuccess;

		[method: MethodImpl(32)]
		public static event ResponseScoresFailedDelegate ResponseScoresFailed;

		[method: MethodImpl(32)]
		public static event ResponseAchievementsSuccessDelegate ResponseAchievementsSuccess;

		[method: MethodImpl(32)]
		public static event ResponseAchievementsFailedDelegate ResponseAchievementsFailed;

		[DllImport("__Internal")]
		private static extern bool GameCenter_IsAvailable();

		[DllImport("__Internal")]
		private static extern bool GameCenter_Authenticate();

		[DllImport("__Internal")]
		private static extern bool GameCenter_IsAuthenticated();

		[DllImport("__Internal")]
		private static extern void GameCenter_ShowAchievements();

		[DllImport("__Internal")]
		private static extern void GameCenter_ReportAchievement(string _achievementID, float _percentage);

		[DllImport("__Internal")]
		private static extern void GameCenter_ResetAchievements();

		[DllImport("__Internal")]
		private static extern void GameCenter_ShowLeaderboard_Default();

		[DllImport("__Internal")]
		private static extern void GameCenter_ShowLeaderboard(string _categoryID);

		[DllImport("__Internal")]
		private static extern void GameCenter_ReportScore(long _score, string _categoryID);

		[DllImport("__Internal")]
		private static extern IntPtr GameCenter_PlayerAlias();

		[DllImport("__Internal")]
		private static extern IntPtr GameCenter_PlayerID();

		[DllImport("__Internal")]
		private static extern void GameCenter_RequestScores(string _playerID, string _categoryID);

		[DllImport("__Internal")]
		private static extern void GameCenter_RequestAchievements();

		[DllImport("__Internal")]
		private static extern void GameCenter_EnableAchievenentNotification(bool _Enable);

		[Obsolete("GameCenter.Authenticate(bool,bool) has been deprecated. Use GameCenter.Authenticate()")]
		public static void Authenticate(bool _useLeaderboards, bool _useAchievements)
		{
			Authenticate();
		}

		public static void Authenticate()
		{
		}

		public static void ShowAchievements()
		{
		}

		public static void ReportAchievement(string _achievementID)
		{
		}

		public static void ReportAchievement(string _achievementID, float _percentage)
		{
		}

		public static void ResetAchievements()
		{
		}

		public static void ShowLeaderboard()
		{
		}

		public static void ShowLeaderboard(string _categoryID)
		{
		}

		public static void ReportScore(long _score, string _categoryID)
		{
		}

		[Obsolete("GameCenter.GetScore has been deprecated. Use GameCenter.RequestScores")]
		public static int GetScore(string _categoryID)
		{
			return 0;
		}

		public static void RequestScores(string _playerID, string _categoryID)
		{
		}

		public static void RequestAchievements()
		{
		}

		private static void OnChangeState(bool _online)
		{
			if (Service.ConnectionState != null)
			{
				Service.ConnectionState(_online);
			}
		}

		private static void OnResponseScoresSuccess(string _playerID, long _scores, string _categoryID)
		{
			if (Service.ResponseScoresSuccess != null)
			{
				Service.ResponseScoresSuccess(_playerID, _scores, _categoryID);
			}
		}

		private static void OnResponseScoresFailed(string _playerID, string _error, string _categoryID)
		{
			if (Service.ResponseScoresFailed != null)
			{
				Service.ResponseScoresFailed(_playerID, _error, _categoryID);
			}
		}

		private static void OnResponseAchievementsSuccess(Achievement[] _Achievements)
		{
			if (Service.ResponseAchievementsSuccess != null)
			{
				Service.ResponseAchievementsSuccess(_Achievements);
			}
		}

		private static void OnResponseAchievementsFailed(string _Error)
		{
			if (Service.ResponseAchievementsFailed != null)
			{
				Service.ResponseAchievementsFailed(_Error);
			}
		}

		private static string PtrToString(IntPtr p)
		{
			if (p == IntPtr.Zero)
			{
				return string.Empty;
			}
			return Marshal.PtrToStringAnsi(p);
		}
	}
}
