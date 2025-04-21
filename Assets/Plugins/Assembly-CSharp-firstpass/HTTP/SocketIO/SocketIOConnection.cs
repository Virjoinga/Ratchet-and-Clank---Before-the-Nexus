using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HTTP.SocketIO
{
	public class SocketIOConnection : MonoBehaviour
	{
		public delegate void EventHandler(SocketIOConnection socket, SocketIOMessage msg);

		public string url;

		public bool reconnect;

		public string sid;

		public float heartbeatTimeout;

		public float closingTimeout;

		public string[] transports;

		private Dictionary<string, EventHandler> handlers = new Dictionary<string, EventHandler>();

		private WebSocket socket;

		private int msgUid;

		public bool Ready
		{
			get
			{
				return socket != null;
			}
		}

		private void OnApplicationQuit()
		{
			socket.Close(WebSocket.CloseEventCode.CloseEventCodeGoingAway, "Bye.");
		}

		public void On(string eventName, EventHandler fn)
		{
			handlers[eventName] = fn;
		}

		public int Send(SocketIOMessage msg)
		{
			msg.id = msgUid++;
			if (socket == null)
			{
				Debug.LogError("Socket.IO is not initialised yet!");
				return -1;
			}
			socket.Send(msg.ToString());
			return msg.id.Value;
		}

		public int Send(object payload)
		{
			SocketIOMessage socketIOMessage = new SocketIOMessage();
			socketIOMessage.type = SocketIOMessage.FrameType.JSONMESSAGE;
			socketIOMessage.data = JsonSerializer.Encode(payload);
			return Send(socketIOMessage);
		}

		public int Emit(string eventName, params object[] args)
		{
			SocketIOMessage socketIOMessage = new SocketIOMessage();
			socketIOMessage.type = SocketIOMessage.FrameType.EVENT;
			Hashtable hashtable = new Hashtable();
			hashtable["name"] = eventName;
			hashtable["args"] = args;
			socketIOMessage.data = JsonSerializer.Encode(hashtable);
			return Send(socketIOMessage);
		}

		private void Start()
		{
			Application.runInBackground = true;
			if (!url.EndsWith("/"))
			{
				url += "/";
			}
			Dispatch("connecting", null);
			StartCoroutine(EstablishConnection(delegate
			{
				Dispatch("connect", null);
			}, delegate
			{
				Dispatch("connect_failed", null);
			}));
		}

		private void Reconnect()
		{
			Dispatch("reconnecting", null);
			StartCoroutine(EstablishConnection(delegate
			{
				Dispatch("reconnect", null);
			}, delegate
			{
				Dispatch("reconnect_failed", null);
			}));
		}

		private IEnumerator EstablishConnection(Action success, Action failed)
		{
			Request req = new Request("POST", url + "socket.io/1/");
			yield return req.Send();
			if (req.exception == null)
			{
				if (req.response.status == 200)
				{
					string[] parts = (from i in req.response.Text.Split(':')
						select i.Trim()).ToArray();
					sid = parts[0];
					float.TryParse(parts[1], out heartbeatTimeout);
					float.TryParse(parts[2], out closingTimeout);
					transports = (from i in parts[3].Split(',')
						select i.Trim().ToLower()).ToArray();
				}
				if (transports.Contains("websocket"))
				{
					socket = new WebSocket();
					StartCoroutine(socket.Dispatcher());
					socket.Connect(url + "socket.io/1/websocket/" + sid);
					socket.OnTextMessageRecv += HandleSocketOnTextMessageRecv;
					socket.OnDisconnect += delegate
					{
						Dispatch("disconnect", null);
					};
					success();
				}
				else
				{
					failed();
					Debug.LogError("Websocket is not supported with this server.");
				}
			}
			else
			{
				failed();
			}
		}

		private void Dispatch(string eventName, SocketIOMessage msg)
		{
			EventHandler value = null;
			if (handlers.TryGetValue(eventName, out value))
			{
				value(this, msg);
			}
		}

		private void HandleSocketOnTextMessageRecv(string message)
		{
			SocketIOMessage socketIOMessage = SocketIOMessage.FromString(message);
			socketIOMessage.socket = this;
			switch (socketIOMessage.type)
			{
			case SocketIOMessage.FrameType.DISCONNECT:
				StopCoroutine("Hearbeat");
				Dispatch("disconnect", null);
				if (reconnect)
				{
					Reconnect();
				}
				break;
			case SocketIOMessage.FrameType.CONNECT:
				if (socketIOMessage.endPoint == null)
				{
					StartCoroutine("Heartbeat");
				}
				break;
			case SocketIOMessage.FrameType.HEARTBEAT:
				break;
			case SocketIOMessage.FrameType.MESSAGE:
				Dispatch("message", socketIOMessage);
				break;
			case SocketIOMessage.FrameType.JSONMESSAGE:
				Dispatch("json_message", socketIOMessage);
				break;
			case SocketIOMessage.FrameType.EVENT:
				Dispatch(socketIOMessage.eventName, socketIOMessage);
				break;
			case SocketIOMessage.FrameType.ACK:
				break;
			case SocketIOMessage.FrameType.ERROR:
				Dispatch("error", socketIOMessage);
				break;
			case SocketIOMessage.FrameType.NOOP:
				break;
			}
		}

		private IEnumerator Heartbeat()
		{
			SocketIOMessage beat = new SocketIOMessage
			{
				type = SocketIOMessage.FrameType.HEARTBEAT
			};
			WaitForSeconds delay = new WaitForSeconds(heartbeatTimeout * 0.8f);
			while (socket.connected)
			{
				socket.Send(beat.ToString());
				yield return delay;
			}
		}
	}
}
