using System;
using System.Net;
using System.Net.Sockets;
using CandleLib.Common;

namespace CandleLib.Network {
	sealed class Connector {
		IManagerCallback manager;
		State state;
		Socket socket;
		IPEndPoint remote;

		public Connector(IManagerCallback manager, State state) {
			this.manager = manager;
			this.state = state;
		}

		public void Connect(string ip, int port) {
			if (socket != null)
				return;
			IPAddress ipAddress = IPAddress.Parse(ip);
			remote = new IPEndPoint(ipAddress, port);
			socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			socket.BeginConnect(remote, ConnectCallback, this);
		}

		public void Connect(IPEndPoint remote) {
			if (socket != null)
				return;
			this.remote = remote;
			socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			socket.BeginConnect(remote, ConnectCallback, this);
		}

		private void ConnectCallback(IAsyncResult ar) {
			try {
				socket.EndConnect(ar);
			} catch (SocketException e) {
				Logger.Debug("network", "connect {0} error {1}.", remote, e.ErrorCode);
				//TODO delay
				socket.BeginConnect(remote, ConnectCallback, this);
				return;
			} catch (ObjectDisposedException) {
				Logger.Debug("network", "connect {0} cancel.", remote);
				return;
			}
			Logger.Debug("network", "connect {0} ok.", socket.RemoteEndPoint.ToString());
			IConnection conn = new Connection(socket);
			conn.reconn = remote;
			manager.OnConnect(this, conn);
			conn.InitRecv();
		}
	}
}
