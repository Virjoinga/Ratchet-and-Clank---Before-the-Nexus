using System.Collections;

namespace HTTP.SocketIO
{
	public class SocketIOMessage
	{
		public enum FrameType
		{
			DISCONNECT = 0,
			CONNECT = 1,
			HEARTBEAT = 2,
			MESSAGE = 3,
			JSONMESSAGE = 4,
			EVENT = 5,
			ACK = 6,
			ERROR = 7,
			NOOP = 8
		}

		public SocketIOConnection socket;

		public FrameType type;

		public int? id;

		public bool isUser;

		public string endPoint;

		public string data;

		public string eventName;

		public object[] args;

		public static SocketIOMessage FromString(string msg)
		{
			SocketIOMessage socketIOMessage = new SocketIOMessage();
			int result = 0;
			if (int.TryParse(NextPart(msg, out msg), out result))
			{
				socketIOMessage.type = (FrameType)result;
			}
			string text = NextPart(msg, out msg);
			if (text == null)
			{
				socketIOMessage.id = null;
				socketIOMessage.isUser = false;
			}
			else
			{
				if (text.EndsWith("+"))
				{
					socketIOMessage.isUser = true;
					text = text.Substring(0, text.Length - 1);
				}
				int result2;
				if (int.TryParse(text, out result2))
				{
					socketIOMessage.id = result2;
				}
			}
			socketIOMessage.endPoint = NextPart(msg, out msg);
			if (msg.Length > 0)
			{
				socketIOMessage.data = msg.Substring(1);
			}
			if (socketIOMessage.type == FrameType.EVENT)
			{
				Hashtable hashtable = JsonSerializer.Decode(socketIOMessage.data) as Hashtable;
				socketIOMessage.eventName = hashtable["name"] as string;
				socketIOMessage.args = ((ArrayList)hashtable["args"]).ToArray();
			}
			return socketIOMessage;
		}

		private static string NextPart(string parts, out string remainder)
		{
			if (parts[0] == ':')
			{
				remainder = parts.Substring(1);
				return null;
			}
			int num = parts.IndexOf(':');
			string result = parts.Substring(0, num);
			remainder = parts.Substring(num);
			return result;
		}

		public override string ToString()
		{
			return string.Format("{0}:{1}:{2}:{3}", (int)type, (!isUser) ? id.ToString() : (id + "+"), endPoint, data);
		}
	}
}
