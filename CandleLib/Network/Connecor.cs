using System;
using System.Net;
using System.Net.Sockets;

namespace CandleLib.Network {
	interface IConnectorCallback {
		void OnConnectSuccess(Connector cor, Connection con);
		void OnConnectFail(Connector cor, int error);
	}
	sealed class Connector {
		IConnectorCallback cb;
		State state;
		Socket socket;
		IPEndPoint remote;

		public Connector(IConnectorCallback cb, State state) {
			this.cb = cb;
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
				Console.WriteLine("connect {0} error {1}.", remote, e.ErrorCode);
				//TODO delay
				socket.BeginConnect(remote, ConnectCallback, this);
				return;
			} catch (ObjectDisposedException) {
				Console.WriteLine("connect {0} cancel.", remote);
				return;
			}
			Console.WriteLine("connect {0} ok.", socket.RemoteEndPoint.ToString());
			IConnection conn = new Connection(socket);
			conn.reconn = remote;
			cb.OnConnect(this, conn);
			conn.InitRecv();
		}
	}
}
