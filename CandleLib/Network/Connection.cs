using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using CandleLib.Common;

namespace CandleLib.Network {
	interface IConnection {
		IManagerCallback manager { get; set; }
		ID<Session> sid { get; set; }
		State state { get; set; }
		IPEndPoint reconn { get; set; }

		void InitRecv();
		void SendRaw(byte[] byteData, int size);
		void Disconnect();
	}

	sealed class Connection : IConnection {
		Socket socket;
		byte[] buffer;
		int dataEnd;
		int packetSize;
		IManagerCallback manager;
		IManagerCallback IConnection.manager { get { return manager; } set { manager = value; } }
		ID<Session> IConnection.sid { get; set; }
		State IConnection.state { get; set; }
		IPEndPoint IConnection.reconn { get; set; }

		void IConnection.InitRecv() {
			if (buffer == null) {
				buffer = new byte[4096];
				try {
					socket.BeginReceive(buffer, 0, buffer.Length, 0, new AsyncCallback(RecvCallback), this);
				} catch (SocketException e) {
					Logger.Debug("network", "BeginReceive error {0}.", e.ErrorCode);
					Disconnect();
					return;
				} catch (ObjectDisposedException) {
					Disconnect();
					return;
				}
			}
		}

		void IConnection.SendRaw(byte[] byteData, int size) {
			try {
				socket.BeginSend(byteData, 0, size, 0, new AsyncCallback(SendCallback), this);
			} catch (SocketException e) {
				Logger.Debug("network", "BeginReceive error {0}.", e.ErrorCode);
				Disconnect();
				return;
			} catch (ObjectDisposedException) {
				Disconnect();
				return;
			}

		}

		void IConnection.Disconnect() {
			try {
				socket.Shutdown(SocketShutdown.Both);
				socket.Close();
			} catch (SocketException e) {
				Logger.Debug("network", "Shutdown error {0}.", e.ErrorCode);
				return;
			} catch (ObjectDisposedException) {
				return;
			}
		}

		public Connection(Socket socket) {
			this.socket = socket;
		}

		private void Disconnect() {
			try {
				Logger.Debug("network", "socket {0} Shotdown.", socket);
				socket.Shutdown(SocketShutdown.Both);
				socket.Close();
				if (manager != null) {
					manager.OnDisconnect(this);
				}
			} catch (SocketException e) {
				Logger.Debug("network", "Shotdown error {0}.", e.ErrorCode);
				return;
			} catch (ObjectDisposedException) {
				return;
			}
		}

		private void RecvCallback(IAsyncResult ar) {
			int bytesRead;
			try {
				bytesRead = socket.EndReceive(ar);
			} catch (SocketException e) {
				Logger.Debug("network", "EndReceive error {0}.", e.ErrorCode);
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
				try {
					socket.BeginReceive(buffer, dataEnd, buffer.Length - dataEnd, 0, new AsyncCallback(RecvCallback), this);
				} catch (SocketException e) {
					Logger.Debug("network", "BeginReceive error {0}.", e.ErrorCode);
					Disconnect();
					return;
				} catch (ObjectDisposedException) {
					Disconnect();
					return;
				}
			}
		}
		private Packet TryReadPacket() {
			if (dataEnd < packetSize)
				return null;
			using (MemoryStream stream = new MemoryStream(buffer, 0, dataEnd, false)) {
				if (packetSize == 0) {
					int packetType, bytesRead;
					packetSize = PacketHelper.ReadLengthPrefix(stream, out packetType, out bytesRead);
					if (bytesRead > 0) {
						if (!PacketHelper.ValidRecv(packetType, packetSize)) {
							Logger.Debug("network", "ValidRecv error type={0} size={1}.", packetType, packetSize);
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
				Packet p = PacketHelper.Deserialize(stream);
				if (p == null) {
					return null;
				}
				Logger.Debug("network", "Recv type={0} size={1}.", p.GetType(), packetSize);
				int position = (int)stream.Position;
				if (position < dataEnd) {
					Buffer.BlockCopy(buffer, position, buffer, 0, dataEnd - position);
				}
				dataEnd -= position;
				return p;
			}
		}

		private void SendCallback(IAsyncResult ar) {
			try {
				int bytesSent = socket.EndSend(ar);
			} catch (SocketException e) {
				Logger.Debug("network", "send error {0}.", e.ErrorCode);
				Disconnect();
				return;
			} catch (ObjectDisposedException) {
				Disconnect();
				return;
			}
		}
	}
}
