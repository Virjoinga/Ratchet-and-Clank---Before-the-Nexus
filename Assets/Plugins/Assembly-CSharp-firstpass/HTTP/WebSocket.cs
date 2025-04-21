using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using UnityEngine;

namespace HTTP
{
	public class WebSocket
	{
		[Flags]
		public enum OpCode
		{
			OpCodeContinuation = 0,
			OpCodeText = 1,
			OpCodeBinary = 2,
			OpCodeClose = 8,
			OpCodePing = 9,
			OpCodePong = 0xA
		}

		private enum ParseFrameResult
		{
			FrameIncomplete = 0,
			FrameOK = 1,
			FrameError = 2
		}

		public enum CloseEventCode
		{
			CloseEventCodeNotSpecified = -1,
			CloseEventCodeNormalClosure = 1000,
			CloseEventCodeGoingAway = 1001,
			CloseEventCodeProtocolError = 1002,
			CloseEventCodeUnsupportedData = 1003,
			CloseEventCodeFrameTooLarge = 1004,
			CloseEventCodeNoStatusRcvd = 1005,
			CloseEventCodeAbnormalClosure = 1006,
			CloseEventCodeInvalidUTF8 = 1007,
			CloseEventCodeMinimumUserDefined = 3000,
			CloseEventCodeMaximumUserDefined = 4999
		}

		private class FrameData
		{
			public OpCode opCode;

			public bool final;

			public bool reserved1;

			public bool reserved2;

			public bool reserved3;

			public bool masked;

			public int payload;

			public int payloadLength;

			public int end;
		}

		private class SubArray : IEnumerable, IEnumerable<byte>
		{
			private List<byte> array;

			private int offset;

			private int length;

			public SubArray(List<byte> array, int offset, int length)
			{
				this.array = array;
				this.offset = offset;
				this.length = length;
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			public IEnumerator<byte> GetEnumerator()
			{
				return new SubArrayEnum(array, offset, length);
			}
		}

		private class SubArrayEnum : IEnumerator, IDisposable, IEnumerator<byte>
		{
			private List<byte> array;

			private int offset;

			private int length;

			private int position = -1;

			object IEnumerator.Current
			{
				get
				{
					return Current;
				}
			}

			public byte Current
			{
				get
				{
					try
					{
						return array[offset + position];
					}
					catch (IndexOutOfRangeException)
					{
						throw new InvalidOperationException();
					}
				}
			}

			public SubArrayEnum(List<byte> array, int offset, int length)
			{
				this.array = array;
				this.offset = offset;
				this.length = length;
			}

			public bool MoveNext()
			{
				position++;
				return position < length;
			}

			public void Reset()
			{
				position = -1;
			}

			public void Dispose()
			{
			}
		}

		public struct OutgoingMessage
		{
			public OpCode opCode;

			public byte[] data;

			public OutgoingMessage(OpCode opCode, byte[] data)
			{
				this.opCode = opCode;
				this.data = data;
			}
		}

		public delegate void OnTextMessageHandler(string message);

		public delegate void OnBinaryMessageHandler(byte[] message);

		private const byte FINALBIT = 128;

		private const byte RESERVEDBIT1 = 64;

		private const byte RESERVEDBIT2 = 32;

		private const byte RESERVEDBIT3 = 16;

		private const byte OP_CODE_MASK = 15;

		private const byte MASKBIT = 128;

		private const byte PAYLOAD_LENGTH_MASK = 127;

		private const int MASKING_KEY_WIDTH_IN_BYTES = 4;

		private const int MAX_PAYLOAD_WITHOUT_EXTENDED_LENGTH_FIELD = 125;

		private const int PAYLOAD_WITH_TWO_BYTE_EXTENDED_FIELD = 126;

		private const int PAYLOAD_WITH_EIGHT_BYTE_EXTENDED_FIELD = 127;

		public int niceness = 100;

		public Exception exception;

		public bool isDone;

		public bool connected;

		private Thread outgoingWorkerThread;

		private Thread incomingWorkerThread;

		private HttpConnection connection;

		private List<string> incomingText = new List<string>();

		private List<byte[]> incomingBinary = new List<byte[]>();

		private List<OutgoingMessage> outgoing = new List<OutgoingMessage>();

		private bool hasContinuousFrame;

		private OpCode continuousFrameOpCode;

		private List<byte> continuousFrameData = new List<byte>();

		private bool receivedClosingHandshake;

		private CloseEventCode closeEventCode;

		private string closeEventReason = string.Empty;

		private bool closing;

		private bool connectionBroken;

		private UTF8Encoding enc = new UTF8Encoding();

		[method: MethodImpl(32)]
		public event Action OnConnect;

		[method: MethodImpl(32)]
		public event Action OnDisconnect;

		[method: MethodImpl(32)]
		public event OnTextMessageHandler OnTextMessageRecv;

		[method: MethodImpl(32)]
		public event OnBinaryMessageHandler OnBinaryMessageRecv;

		private void OnTextMessage(string msg)
		{
			lock (incomingText)
			{
				incomingText.Add(msg);
			}
		}

		private void OnBinaryMessage(byte[] msg)
		{
			lock (incomingBinary)
			{
				incomingBinary.Add(msg);
			}
		}

		public IEnumerator Dispatcher()
		{
			do
			{
				yield return null;
				if (this.OnTextMessageRecv != null)
				{
					lock (incomingText)
					{
						if (incomingText.Count > 0)
						{
							foreach (string j in incomingText)
							{
								this.OnTextMessageRecv(j);
							}
							incomingText.Clear();
						}
					}
				}
				if (this.OnBinaryMessageRecv == null)
				{
					continue;
				}
				lock (incomingBinary)
				{
					if (incomingBinary.Count <= 0)
					{
						continue;
					}
					foreach (byte[] i in incomingBinary)
					{
						this.OnBinaryMessageRecv(i);
					}
					incomingBinary.Clear();
				}
			}
			while (!connectionBroken);
			if (this.OnDisconnect != null)
			{
				this.OnDisconnect();
			}
		}

		public void Connect(string uri)
		{
			Connect(new Uri(uri));
		}

		public Coroutine Wait()
		{
			return UniWeb.Instance.StartCoroutine(_Wait());
		}

		private IEnumerator _Wait()
		{
			while (!isDone)
			{
				yield return null;
			}
		}

		public void Connect(Uri uri)
		{
			UniWeb.Instance.StartCoroutine(_Connect(uri));
		}

		private IEnumerator _Connect(Uri uri)
		{
			isDone = false;
			connected = false;
			exception = null;
			Request req = new Request("GET", uri.ToString());
			req.headers.Set("Upgrade", "websocket");
			req.headers.Set("Connection", "Upgrade");
			string key = WebSocketKey();
			req.headers.Set("Sec-WebSocket-Key", key);
			req.headers.Add("Sec-WebSocket-Protocol", "chat, superchat");
			req.headers.Set("Sec-WebSocket-Version", "13");
			req.headers.Set("Origin", "null");
			req.acceptGzip = false;
			yield return req.Send();
			if (req.exception != null)
			{
				exception = req.exception;
			}
			else if (req.response.headers.Get("Upgrade").ToLower() == "websocket" && req.response.headers.Get("Connection").ToLower() == "upgrade")
			{
				string receivedKey = req.response.headers.Get("Sec-Websocket-Accept").ToLower();
				SHA1CryptoServiceProvider sha = new SHA1CryptoServiceProvider();
				sha.ComputeHash(enc.GetBytes(key + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"));
				string computedKey = Convert.ToBase64String(sha.Hash).ToLower();
				if (computedKey == receivedKey)
				{
					connected = true;
					connection = req.upgradedConnection;
					outgoingWorkerThread = new Thread(OutgoingWorker);
					outgoingWorkerThread.Start();
					incomingWorkerThread = new Thread(IncomingWorker);
					incomingWorkerThread.Start();
					UniWeb.Instance.StartCoroutine(Dispatcher());
					UniWeb.Instance.OnQuit(delegate
					{
						Close(CloseEventCode.CloseEventCodeGoingAway, "Quit");
						req.upgradedConnection.Dispose();
					});
					if (this.OnConnect != null)
					{
						this.OnConnect();
					}
				}
				else
				{
					connected = false;
				}
			}
			isDone = true;
		}

		public void Send(string data)
		{
			outgoing.Add(new OutgoingMessage(OpCode.OpCodeText, enc.GetBytes(data)));
		}

		public void Send(byte[] data)
		{
			outgoing.Add(new OutgoingMessage(OpCode.OpCodeBinary, data));
		}

		public void Close(CloseEventCode code, string reason)
		{
			StartClosingHandshake(code, reason);
		}

		private void OutgoingWorker()
		{
			while (true)
			{
				Thread.Sleep(niceness);
				lock (outgoing)
				{
					while (connection.stream.CanWrite && outgoing.Count > 0)
					{
						OutgoingMessage outgoingMessage = outgoing[0];
						byte[] array = BuildFrame(outgoingMessage.opCode, outgoingMessage.data);
						connection.stream.Write(array, 0, array.Length);
						outgoing.RemoveAt(0);
					}
				}
			}
		}

		private void IncomingWorker()
		{
			connectionBroken = false;
			List<byte> list = new List<byte>();
			while (connection.client.Client.Connected)
			{
				int num = connection.stream.ReadByte();
				if (num == -1)
				{
					break;
				}
				list.Add((byte)num);
				ProcessBuffer(list);
			}
			connectionBroken = true;
		}

		private bool ProcessBuffer(List<byte> buffer)
		{
			return ProcessFrame(buffer);
		}

		private bool ProcessFrame(List<byte> buffer)
		{
			FrameData frame;
			if (ParseFrame(buffer, out frame) != ParseFrameResult.FrameOK)
			{
				return false;
			}
			switch (frame.opCode)
			{
			case OpCode.OpCodeContinuation:
				if (!hasContinuousFrame)
				{
					Debug.LogWarning("Received unexpected continuation frame.");
					return false;
				}
				continuousFrameData.AddRange(new SubArray(buffer, frame.payload, frame.payloadLength));
				RemoveProcessed(buffer, frame.end);
				if (!frame.final)
				{
					break;
				}
				continuousFrameData = new List<byte>();
				hasContinuousFrame = false;
				if (continuousFrameOpCode == OpCode.OpCodeText)
				{
					string msg = string.Empty;
					if (continuousFrameData.Count > 0)
					{
						byte[] array3 = continuousFrameData.ToArray();
						msg = enc.GetString(array3, 0, array3.Length);
					}
					OnTextMessage(msg);
				}
				else if (continuousFrameOpCode == OpCode.OpCodeBinary)
				{
					OnBinaryMessage(continuousFrameData.ToArray());
				}
				break;
			case OpCode.OpCodeText:
				if (frame.final)
				{
					string msg2 = string.Empty;
					if (frame.payloadLength > 0)
					{
						byte[] array5 = new byte[frame.payloadLength];
						buffer.CopyTo(frame.payload, array5, 0, frame.payloadLength);
						msg2 = enc.GetString(array5, 0, array5.Length);
					}
					OnTextMessage(msg2);
					RemoveProcessed(buffer, frame.end);
				}
				else
				{
					hasContinuousFrame = true;
					continuousFrameOpCode = OpCode.OpCodeText;
					continuousFrameData.AddRange(new SubArray(buffer, frame.payload, frame.payloadLength));
					RemoveProcessed(buffer, frame.end);
				}
				break;
			case OpCode.OpCodeBinary:
				if (frame.final)
				{
					byte[] array4 = new byte[frame.payloadLength];
					buffer.CopyTo(frame.payload, array4, 0, frame.payloadLength);
					OnBinaryMessage(array4);
					RemoveProcessed(buffer, frame.end);
				}
				else
				{
					hasContinuousFrame = true;
					continuousFrameOpCode = OpCode.OpCodeBinary;
					continuousFrameData.AddRange(new SubArray(buffer, frame.payload, frame.payloadLength));
					RemoveProcessed(buffer, frame.end);
				}
				break;
			case OpCode.OpCodeClose:
				if (frame.payloadLength >= 2)
				{
					byte b = buffer[frame.payload];
					byte b2 = buffer[frame.payload + 1];
					closeEventCode = (CloseEventCode)((b << 8) | b2);
				}
				else
				{
					closeEventCode = CloseEventCode.CloseEventCodeNoStatusRcvd;
				}
				if (frame.payloadLength >= 3)
				{
					byte[] array2 = new byte[frame.payloadLength - 2];
					buffer.CopyTo(2, array2, 0, frame.payloadLength - 2);
					closeEventReason = enc.GetString(array2, 0, array2.Length);
				}
				else
				{
					closeEventReason = string.Empty;
				}
				RemoveProcessed(buffer, frame.end);
				receivedClosingHandshake = true;
				StartClosingHandshake(closeEventCode, closeEventReason);
				break;
			case OpCode.OpCodePing:
			{
				byte[] array = new byte[frame.payloadLength];
				buffer.CopyTo(frame.payload, array, 0, frame.payloadLength);
				RemoveProcessed(buffer, frame.end);
				lock (outgoing)
				{
					outgoing.Add(new OutgoingMessage(OpCode.OpCodePong, array));
				}
				break;
			}
			case OpCode.OpCodePong:
				RemoveProcessed(buffer, frame.end);
				break;
			default:
				Debug.LogError("SHOULD NOT REACH HERE");
				break;
			}
			return buffer.Count != 0;
		}

		private ParseFrameResult ParseFrame(List<byte> buffer, out FrameData frame)
		{
			frame = null;
			if (buffer.Count < 2)
			{
				return ParseFrameResult.FrameIncomplete;
			}
			int num = 0;
			byte b = buffer[num++];
			byte b2 = buffer[num++];
			bool final = (b & 0x80) > 0;
			bool reserved = (b & 0x40) > 0;
			bool reserved2 = (b & 0x20) > 0;
			bool reserved3 = (b & 0x10) > 0;
			OpCode opCode = (OpCode)(b & 0xF);
			bool flag = (b2 & 0x80) > 0;
			long num2 = b2 & 0x7F;
			if (num2 > 125)
			{
				int num3 = ((num2 != 126) ? 8 : 2);
				if (buffer.Count - num < num3)
				{
					return ParseFrameResult.FrameIncomplete;
				}
				num2 = 0L;
				for (int i = 0; i < num3; i++)
				{
					num2 <<= 8;
					num2 |= (int)buffer[num++];
				}
			}
			int num4 = (flag ? 4 : 0);
			if (num2 > long.MaxValue || num2 + num4 > int.MaxValue)
			{
				Debug.LogError(string.Format("WebSocket frame length too large: {0} bytes", num2));
				return ParseFrameResult.FrameError;
			}
			int num5 = (int)num2;
			if (buffer.Count - num < num4 + num5)
			{
				return ParseFrameResult.FrameIncomplete;
			}
			if (flag)
			{
				int num6 = num;
				int num7 = num + 4;
				for (int j = 0; j < num5; j++)
				{
					List<byte> list;
					List<byte> list2 = (list = buffer);
					int index;
					int index2 = (index = num7 + j);
					byte b3 = list[index];
					list2[index2] = (byte)(b3 ^ buffer[num6 + j % 4]);
				}
			}
			frame = new FrameData();
			frame.opCode = opCode;
			frame.final = final;
			frame.reserved1 = reserved;
			frame.reserved2 = reserved2;
			frame.reserved3 = reserved3;
			frame.masked = flag;
			frame.payload = num + num4;
			frame.payloadLength = num5;
			frame.end = num + num4 + num5;
			return ParseFrameResult.FrameOK;
		}

		private byte[] BuildFrame(OpCode opCode, byte[] data)
		{
			List<byte> list = new List<byte>();
			list.Add((byte)(0x80u | (byte)opCode));
			if (data.Length <= 125)
			{
				list.Add((byte)(0x80u | ((uint)data.Length & 0xFFu)));
			}
			else if (data.Length <= 65535)
			{
				list.Add(254);
				list.Add((byte)((data.Length & 0xFF00) >> 8));
				list.Add((byte)((uint)data.Length & 0xFFu));
			}
			else
			{
				list.Add(byte.MaxValue);
				byte[] array = new byte[8];
				int num = data.Length;
				for (int i = 0; i < 8; i++)
				{
					array[7 - i] = (byte)((uint)num & 0xFFu);
					num >>= 8;
				}
				list.AddRange(array);
			}
			int count = list.Count;
			list.AddRange(new byte[4]);
			int count2 = list.Count;
			list.AddRange(data);
			Arc4RandomNumberGenerator.CryptographicallyRandomValues(list, count, 4);
			for (int j = 0; j < data.Length; j++)
			{
				List<byte> list2;
				List<byte> list3 = (list2 = list);
				int index;
				int index2 = (index = count2 + j);
				byte b = list2[index];
				list3[index2] = (byte)(b ^ list[count + j % 4]);
			}
			return list.ToArray();
		}

		private void RemoveProcessed(List<byte> buffer, int length)
		{
			buffer.RemoveRange(0, length);
		}

		private void StartClosingHandshake(CloseEventCode code, string reason)
		{
			if (!closing)
			{
				List<byte> list = new List<byte>();
				if (!receivedClosingHandshake)
				{
					byte item = (byte)((int)code >> 8);
					byte item2 = (byte)code;
					list.Add(item);
					list.Add(item2);
					list.AddRange(enc.GetBytes(reason));
					outgoing.Add(new OutgoingMessage(OpCode.OpCodeClose, list.ToArray()));
				}
				closing = true;
			}
		}

		private string WebSocketKey()
		{
			return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
		}
	}
}
