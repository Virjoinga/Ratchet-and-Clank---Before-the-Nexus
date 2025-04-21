using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

public class ServerCommand
{
	public float lastRetry = float.NegativeInfinity;

	public object userData;

	public string url;

	public string userId;

	public string onlineId;

	public string accessToken;

	public string refreshToken;

	public Dictionary<string, string> paramFields;

	public Dictionary<string, string> textFields;

	public Dictionary<string, byte[]> binFields;

	public HttpWebMethod method;

	[method: MethodImpl(32)]
	public event OnHttpWebFailDelegate onFail;

	[method: MethodImpl(32)]
	public event OnHttpWebPassDelegate onPass;

	public void command(OnHttpWebFailDelegate failCallback, OnHttpWebPassDelegate passCallback)
	{
		string text = userId;
		if (text == string.Empty)
		{
			text = ServerConnection.GetUserId();
		}
		string pSNName = onlineId;
		if (pSNName == string.Empty)
		{
			pSNName = ServerConnection.GetPSNName();
		}
		string text2 = accessToken;
		if (text2 == string.Empty)
		{
			text2 = ServerConnection.GetAccessToken();
		}
		string text3 = refreshToken;
		if (text3 == string.Empty)
		{
			text3 = ServerConnection.GetRefreshToken();
		}
		switch (method)
		{
		case HttpWebMethod.GET:
			paramFields.Add("UserID", pSNName);
			HttpWeb.GET(url, paramFields, passCallback, failCallback, this);
			break;
		case HttpWebMethod.POST:
			textFields.Add("UserID", pSNName);
			HttpWeb.POST(url, textFields, binFields, passCallback, failCallback, this);
			break;
		}
	}

	public void dispatchError(HttpWeb target)
	{
		if (this.onFail != null)
		{
			this.onFail(target, userData);
		}
	}

	public void dispatchSuccess(HttpWeb target)
	{
		if (this.onPass != null)
		{
			this.onPass(target, userData);
		}
	}

	public void Serialize(BinaryWriter stream)
	{
		stream.Write(url);
		stream.Write(userId);
		stream.Write(onlineId);
		stream.Write(accessToken);
		stream.Write(refreshToken);
		stream.Write((byte)method);
		int num = ((paramFields != null) ? paramFields.Count : 0);
		stream.Write(num);
		if (num > 0)
		{
			foreach (KeyValuePair<string, string> paramField in paramFields)
			{
				stream.Write(paramField.Key);
				stream.Write(paramField.Value);
			}
		}
		int num2 = ((textFields != null) ? textFields.Count : 0);
		stream.Write(num2);
		if (num2 > 0)
		{
			foreach (KeyValuePair<string, string> textField in textFields)
			{
				stream.Write(textField.Key);
				stream.Write(textField.Value);
			}
		}
		int num3 = ((binFields != null) ? binFields.Count : 0);
		stream.Write(num3);
		if (num3 <= 0)
		{
			return;
		}
		foreach (KeyValuePair<string, byte[]> binField in binFields)
		{
			stream.Write(binField.Key);
			stream.Write(binField.Value.Length);
			stream.Write(binField.Value);
		}
	}

	public void Deserialize(BinaryReader stream)
	{
		url = stream.ReadString();
		userId = stream.ReadString();
		onlineId = stream.ReadString();
		accessToken = stream.ReadString();
		refreshToken = stream.ReadString();
		method = (HttpWebMethod)stream.ReadByte();
		int num = stream.ReadInt32();
		paramFields = new Dictionary<string, string>();
		for (int i = 0; i < num; i++)
		{
			string key = stream.ReadString();
			string value = stream.ReadString();
			paramFields.Add(key, value);
		}
		int num2 = stream.ReadInt32();
		textFields = new Dictionary<string, string>();
		for (int j = 0; j < num2; j++)
		{
			string key2 = stream.ReadString();
			string value2 = stream.ReadString();
			textFields.Add(key2, value2);
		}
		int num3 = stream.ReadInt32();
		binFields = new Dictionary<string, byte[]>();
		for (int k = 0; k < num3; k++)
		{
			string key3 = stream.ReadString();
			int count = stream.ReadInt32();
			byte[] value3 = stream.ReadBytes(count);
			binFields.Add(key3, value3);
		}
	}
}
