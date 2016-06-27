using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using ProtoBuf;
using ProtoBuf.Meta;

namespace CandleLib.Network {
	interface IConnection {
		IManagerCallback manager { get; set; }
		ID<Session> sid { get; set; }
		State state { get; set; }
		IPEndPoint reconn { get; set; }

		void InitRecv();
		void SendRaw(byte[] byteData);
		void Disconnect();
	}

	sealed class Connection : IConnection {
		Socket socket;
		byte[] buffer;
		int dataEnd;
		int packetSize;
		IManagerCallback manager;
		IManagerCallback IConnection.manager { get { return manager; } set { value = manager; } }
		ID<Session> IConnection.sid { get; set; }
		State IConnection.state { get; set; }
		IPEndPoint IConnection.reconn { get; set; }

		void IConnection.InitRecv() {
			if (buffer == null) {
				buffer = new byte[4096];
				socket.BeginReceive(buffer, 0, buffer.Length, 0, new AsyncCallback(RecvCallback), this);
			}
		}

		void IConnection.SendRaw(byte[] byteData) {
			socket.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), this);
		}

		void IConnection.Disconnect() {
			manager = null;
			socket.Shutdown(SocketShutdown.Both);
			socket.Close();
		}

		public Connection(Socket socket) {
			this.socket = socket;
		}

		private void Disconnect() {
			socket.Shutdown(SocketShutdown.Both);
			socket.Close();
			IManagerCallback manager = this.manager;
			this.manager = null;
			if (manager != null) {
				manager.OnDisconnect(this);
			}
		}

		private void RecvCallback(IAsyncResult ar) {
			int bytesRead;
			try {
				bytesRead = socket.EndReceive(ar);
			} catch (SocketException e) {
				Console.WriteLine("recv error {0}.", e.ErrorCode);
				Disconnect();
				return;
			} catch (ObjectDisposedException) {
				Disconnect();
				return;
			}
			if (bytesRead == 0) {
				Disconnect();
			}
			if (bytesRead > 0) {
				dataEnd += bytesRead;
				while (true) {
					Packet p = TryReadPacket();
					if (p == null)
						break;
					manager.OnRecvPacket(this, p);
				}
				socket.BeginReceive(buffer, dataEnd, buffer.Length - dataEnd, 0, new AsyncCallback(RecvCallback), this);
			}
		}
		private Packet TryReadPacket() {
			if (dataEnd < packetSize)
				return null;
			using (MemoryStream stream = new MemoryStream(buffer, 0, dataEnd, false)) {
				if (packetSize == 0) {
					int packetType, bytesRead;
					packetSize = ProtoReader.ReadLengthPrefix(stream, false, PrefixStyle.Base128, out packetType, out bytesRead);
					if (bytesRead > 0) {
						if (!PacketHelper.ValidRecv(packetType, packetSize)) {
							Disconnect();
							return null;
						}
						packetSize += bytesRead;
						if (buffer.Length < packetSize) {
							byte[] old = buffer;
							int newSize = (packetSize + 8191) / 4096;
							buffer = new byte[newSize];
							Buffer.BlockCopy(old, 0, buffer, 0, dataEnd);
							return null;
						}
						if (dataEnd < packetSize)
							return null;
						stream.Position = 0;
					}
				}
				object buf = RuntimeTypeModel.Default.DeserializeWithLengthPrefix(stream, null, null, PrefixStyle.Base128, 0, PacketHelper.GetPacketType); ;
				if (buf == null) {
					return null;
				}
				int position = (int)stream.Position;
				if (position < dataEnd) {
					Buffer.BlockCopy(buffer, position, buffer, 0, dataEnd - position);
				}
				dataEnd -= position;
				Packet p = buf as Packet;
				return p;
			}
		}

		private void SendCallback(IAsyncResult ar) {
			try {
				int bytesSent = socket.EndSend(ar);
			} catch (SocketException e) {
				Console.WriteLine("send error {0}.", e.ErrorCode);
				Disconnect();
				return;
			} catch (ObjectDisposedException) {
				Disconnect();
				return;
			}
		}
	}
}
