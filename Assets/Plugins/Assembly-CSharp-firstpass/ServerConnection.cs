using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public class ServerConnection : MonoBehaviour
{
	protected const uint MAGIC = 3133059083u;

	public string serverUrl = "https://nexus-dev.fullmoongames.net";

	public float retryInterval = 3f;

	protected static string verificationCode = string.Empty;

	protected static string accessToken = string.Empty;

	protected static string refreshToken = string.Empty;

	protected static string userId = string.Empty;

	protected static string onlineId = string.Empty;

	protected static bool authorized = false;

	protected static ServerConnection instance = null;

	protected static Queue<ServerCommand> commandQueue = null;

	protected static byte[] KEY = Encoding.UTF8.GetBytes("sdf$lkdf9994wnlsrg0!.!3452nj@3fs");

	protected static byte[] IV = Encoding.UTF8.GetBytes("1fds$$7s.^s41x7#");

	private string clientId = "fd23e588-bbd2-4204-a2fe-cb1c9c11abcc";

	private string clientSecret = "oJzXy3gmIA8L8E1j";

	private string redirectUri = "com.darkside.rcbtn.scecompcall://redirect";

	[method: MethodImpl(32)]
	public static event OnLoginSuccessDelegate onLoginFlowSuccess;

	[method: MethodImpl(32)]
	public static event OnLoginFailDelegate onLoginFlowFail;

	private void Start()
	{
		if ((bool)instance)
		{
			Debug.LogError("Only one instance of ServerConnection is allowed");
			return;
		}
		verificationCode = string.Empty;
		accessToken = string.Empty;
		refreshToken = string.Empty;
		userId = string.Empty;
		onlineId = string.Empty;
		authorized = false;
		instance = this;
		commandQueue = new Queue<ServerCommand>();
		PSNAuthorizationWindow.onLoginSuccess += onLoginSuccessPath;
		PSNAuthorizationWindow.onLoginFail += onLoginFailPath;
	}

	protected void onLoginSuccessPath(string _verificationCode)
	{
		verificationCode = _verificationCode;
		instance.GetAccessTokenApiCall();
	}

	protected void onLoginFailPath(string _returnUrl)
	{
		verificationCode = string.Empty;
		accessToken = string.Empty;
		refreshToken = string.Empty;
		userId = string.Empty;
		onlineId = string.Empty;
		authorized = false;
		if (ServerConnection.onLoginFlowFail != null)
		{
			ServerConnection.onLoginFlowFail();
		}
		Debug.Log("ServerConnection: Authorization Failed");
	}

	public static void LoginWithCredentials(string login, string password)
	{
		PSNAuthorizationWindow.LoginWithCredentials(login, password);
	}

	public static bool IsAuthorized()
	{
		return authorized;
	}

	protected static void ErrorCallback(HttpWeb target, object userData = null)
	{
		ServerCommand serverCommand = userData as ServerCommand;
		serverCommand.lastRetry = Time.realtimeSinceStartup;
		serverCommand.dispatchError(target);
		lock (commandQueue)
		{
			commandQueue.Enqueue(serverCommand);
		}
	}

	protected static void SuccessCallback(HttpWeb target, object userData = null)
	{
		ServerCommand serverCommand = userData as ServerCommand;
		serverCommand.dispatchSuccess(target);
	}

	public static void GetRaritanium(OnHttpWebPassDelegate successCallback, OnHttpWebFailDelegate errorCallback = null, object userData = null)
	{
		ServerCommand serverCommand = new ServerCommand();
		serverCommand.onPass += successCallback;
		serverCommand.onFail += errorCallback;
		serverCommand.userData = userData;
		serverCommand.method = HttpWebMethod.GET;
		serverCommand.lastRetry = float.NegativeInfinity;
		serverCommand.binFields = null;
		serverCommand.textFields = null;
		serverCommand.paramFields = new Dictionary<string, string>();
		serverCommand.url = instance.serverUrl + "/shareddata/get_raritanium/";
		serverCommand.userId = userId;
		serverCommand.onlineId = onlineId;
		serverCommand.accessToken = accessToken;
		serverCommand.refreshToken = refreshToken;
		lock (commandQueue)
		{
			commandQueue.Enqueue(serverCommand);
		}
	}

	public static void PostRaritanium(int amount, OnHttpWebPassDelegate successCallback = null, OnHttpWebFailDelegate errorCallback = null, object userData = null)
	{
		ServerCommand serverCommand = new ServerCommand();
		serverCommand.onPass += successCallback;
		serverCommand.onFail += errorCallback;
		serverCommand.userData = userData;
		serverCommand.method = HttpWebMethod.POST;
		serverCommand.lastRetry = float.NegativeInfinity;
		serverCommand.binFields = null;
		serverCommand.textFields = new Dictionary<string, string> { 
		{
			"Raritanium",
			amount.ToString()
		} };
		serverCommand.paramFields = null;
		serverCommand.url = instance.serverUrl + "/shareddata/post_raritanium";
		serverCommand.userId = userId;
		serverCommand.onlineId = onlineId;
		serverCommand.accessToken = accessToken;
		serverCommand.refreshToken = refreshToken;
		lock (commandQueue)
		{
			commandQueue.Enqueue(serverCommand);
		}
	}

	public static void ProcessCommandQueue()
	{
		lock (commandQueue)
		{
			if (commandQueue.Count < 1 || !IsAuthorized())
			{
				return;
			}
			ServerCommand serverCommand = commandQueue.Peek();
			if (serverCommand != null && Time.realtimeSinceStartup - serverCommand.lastRetry >= instance.retryInterval)
			{
				if (!authorized)
				{
					serverCommand.lastRetry = Time.realtimeSinceStartup;
					Debug.Log("Unable to send request. Authorize first");
				}
				else
				{
					serverCommand = commandQueue.Dequeue();
					serverCommand.command(ErrorCallback, SuccessCallback);
				}
			}
		}
	}

	protected static void Encode(MemoryStream src, Stream dst)
	{
		byte[] buffer = src.GetBuffer();
		RijndaelManaged rijndaelManaged = new RijndaelManaged();
		rijndaelManaged.Key = KEY;
		rijndaelManaged.IV = IV;
		rijndaelManaged.Mode = CipherMode.CBC;
		rijndaelManaged.Padding = PaddingMode.ISO10126;
		ICryptoTransform cryptoTransform = rijndaelManaged.CreateEncryptor(KEY, IV);
		byte[] buffer2 = cryptoTransform.TransformFinalBlock(buffer, 0, buffer.Length);
		BinaryWriter binaryWriter = new BinaryWriter(dst);
		binaryWriter.Write(buffer2);
	}

	protected static void Decode(byte[] src, MemoryStream dst)
	{
		RijndaelManaged rijndaelManaged = new RijndaelManaged();
		rijndaelManaged.Key = KEY;
		rijndaelManaged.IV = IV;
		rijndaelManaged.Mode = CipherMode.CBC;
		rijndaelManaged.Padding = PaddingMode.ISO10126;
		ICryptoTransform cryptoTransform = rijndaelManaged.CreateDecryptor(KEY, IV);
		byte[] buffer = cryptoTransform.TransformFinalBlock(src, 0, src.Length);
		BinaryWriter binaryWriter = new BinaryWriter(dst);
		binaryWriter.Write(buffer);
	}

	public static void SaveQueue(bool dontSaveGETCommands = false)
	{
		lock (commandQueue)
		{
			try
			{
				Stream stream = File.Open(Application.persistentDataPath + "/requestCache.dat", FileMode.Create);
				MemoryStream memoryStream = new MemoryStream();
				BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
				binaryWriter.Write(3133059083u);
				binaryWriter.Write(verificationCode);
				binaryWriter.Write(accessToken);
				binaryWriter.Write(refreshToken);
				binaryWriter.Write(userId);
				binaryWriter.Write(onlineId);
				int num = commandQueue.Count;
				if (dontSaveGETCommands)
				{
					num = 0;
					foreach (ServerCommand item in commandQueue)
					{
						if (item.method == HttpWebMethod.POST)
						{
							num++;
						}
					}
				}
				binaryWriter.Write(num);
				foreach (ServerCommand item2 in commandQueue)
				{
					if (!dontSaveGETCommands || item2.method != 0)
					{
						item2.Serialize(binaryWriter);
					}
				}
				binaryWriter.Flush();
				Encode(memoryStream, stream);
				stream.Flush();
				stream.Close();
			}
			catch (Exception ex)
			{
				Debug.LogError("Was unable to save request cache file: " + ex.Message);
			}
		}
	}

	protected static void cachedErrorCallback(HttpWeb target, object userData)
	{
		Debug.Log("CACHED REQUEST \"" + target.url + "\" TRANSFER FAILURE: " + target.error);
	}

	protected static void cachedSuccessCallback(HttpWeb target, object userData)
	{
		Debug.Log("CACHED REQUEST \"" + target.url + "\" TRANSFER SUCCESS: " + target.text);
	}

	public static void LoadQueue()
	{
		lock (commandQueue)
		{
			try
			{
				byte[] src = File.ReadAllBytes(Application.persistentDataPath + "/requestCache.dat");
				MemoryStream memoryStream = new MemoryStream();
				Decode(src, memoryStream);
				memoryStream.Seek(0L, SeekOrigin.Begin);
				BinaryReader binaryReader = new BinaryReader(memoryStream);
				uint num = binaryReader.ReadUInt32();
				if (num != 3133059083u)
				{
					throw new Exception("Wrong cache file signature");
				}
				string text = binaryReader.ReadString();
				string text2 = binaryReader.ReadString();
				string text3 = binaryReader.ReadString();
				string text4 = binaryReader.ReadString();
				string text5 = binaryReader.ReadString();
				if (!IsAuthorized())
				{
					verificationCode = text;
					accessToken = text2;
					refreshToken = text3;
					userId = text4;
					onlineId = text5;
					authorized = onlineId != string.Empty && accessToken != string.Empty && refreshToken != string.Empty;
				}
				commandQueue.Clear();
				int num2 = binaryReader.ReadInt32();
				for (int i = 0; i < num2; i++)
				{
					ServerCommand serverCommand = new ServerCommand();
					serverCommand.onFail += cachedErrorCallback;
					serverCommand.onPass += cachedSuccessCallback;
					serverCommand.Deserialize(binaryReader);
					commandQueue.Enqueue(serverCommand);
				}
				memoryStream.Close();
			}
			catch (Exception ex)
			{
				Debug.Log("Was unable to open request cache file: " + ex.Message);
			}
		}
	}

	protected void LogoutCurrentUser()
	{
		verificationCode = string.Empty;
		accessToken = string.Empty;
		refreshToken = string.Empty;
		userId = string.Empty;
		onlineId = string.Empty;
		authorized = false;
	}

	public static void Logout()
	{
		instance.LogoutCurrentUser();
	}

	public static string GetUserId()
	{
		return userId;
	}

	public static string GetPSNName()
	{
		return onlineId;
	}

	public static string GetAccessToken()
	{
		return accessToken;
	}

	public static string GetRefreshToken()
	{
		return refreshToken;
	}

	private void Update()
	{
		ProcessCommandQueue();
	}

	public void OnDisable()
	{
		instance = null;
	}

	private static string ParseJson(string rawString, string regexString)
	{
		Regex regex = new Regex(regexString);
		Match match = regex.Match(rawString);
		if (match.Success)
		{
			return match.Groups[1].Value;
		}
		return string.Empty;
	}

	private void onGetAccessTokenSuccess(HttpWeb target, object userData = null)
	{
		accessToken = ParseJson(target.text, "\"access_token\":\\s*\"(\\S*?)\"");
		refreshToken = ParseJson(target.text, "\"refresh_token\":\\s*\"(\\S*?)\"");
		instance.ValidateAccessTokenApiCall();
	}

	private void onGetAccessTokenFail(HttpWeb target, object userData = null)
	{
		Debug.LogError("GetAccessTokenFail: " + target.error);
		if (ServerConnection.onLoginFlowFail != null)
		{
			ServerConnection.onLoginFlowFail();
		}
	}

	private void GetAccessTokenApiCall()
	{
		string url = "https://auth.api.sonyentertainmentnetwork.com/2.0/oauth/token";
		Hashtable hashtable = new Hashtable();
		hashtable.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(clientId + ":" + clientSecret)));
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("grant_type", "authorization_code");
		dictionary.Add("redirect_uri", redirectUri);
		dictionary.Add("code", verificationCode);
		HttpWeb.POST(url, dictionary, null, onGetAccessTokenSuccess, onGetAccessTokenFail, null, hashtable);
	}

	private void onValidateAccessTokenSuccess(HttpWeb target, object userData = null)
	{
		userId = ParseJson(target.text, "\"user_id\":\\s*\"(\\S*?)\"");
		instance.GetAccountInformationApiCall();
	}

	private void onValidateAccessTokenFail(HttpWeb target, object userData = null)
	{
		Debug.LogError("ValidateAccessTokenFail: " + target.error);
		if (ServerConnection.onLoginFlowFail != null)
		{
			ServerConnection.onLoginFlowFail();
		}
	}

	private void ValidateAccessTokenApiCall()
	{
		string url = "https://auth.api.sonyentertainmentnetwork.com/2.0/oauth/token/" + accessToken;
		Hashtable hashtable = new Hashtable();
		hashtable.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(clientId + ":" + clientSecret)));
		HttpWeb.GET(url, null, onValidateAccessTokenSuccess, onValidateAccessTokenFail, null, hashtable);
	}

	private void onGetAccountInformationSuccess(HttpWeb target, object userData = null)
	{
		onlineId = ParseJson(target.text, "\"onlineId\":\\s*\"(\\S*?)\"");
		authorized = true;
		if (ServerConnection.onLoginFlowSuccess != null)
		{
			ServerConnection.onLoginFlowSuccess();
		}
		Debug.Log("ServerConnection: Authorization Succeeded");
	}

	private void onGetAccountInformationFail(HttpWeb target, object userData = null)
	{
		Debug.LogError("GetAccountInformationFail: " + target.error);
		if (ServerConnection.onLoginFlowFail != null)
		{
			ServerConnection.onLoginFlowFail();
		}
	}

	private void GetAccountInformationApiCall()
	{
		string url = "https://vl.api.np.ac.playstation.net/vl/api/v1/s2s/users/me/info";
		Hashtable hashtable = new Hashtable();
		hashtable.Add("Authorization", "Bearer " + accessToken);
		HttpWeb.GET(url, null, onGetAccountInformationSuccess, onGetAccountInformationFail, null, hashtable);
	}
}
