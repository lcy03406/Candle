using System;
using System.Net;
using System.Net.Sockets;
using CandleLib.Common;

namespace CandleLib.Network {
	sealed class Listener {
		IManagerCallback manager;
		State state;
		Socket socket;

		public Listener(IManagerCallback manager, State state) {
			this.manager = manager;
			this.state = state;
		}

		public void Listen(string ip, int port) {
			if (socket != null)
				return;
			IPAddress ipAddress = IPAddress.Parse(ip);
			IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);
			socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			socket.Bind(localEndPoint);
			socket.Listen(100);
			Logger.Debug("network", "Listen on {0}.", localEndPoint);
			socket.BeginAccept(AcceptCallback, this);
		}
		private void AcceptCallback(IAsyncResult ar) {
			Socket handler;
			try {
				handler = socket.EndAccept(ar);
			} catch (SocketException e) {
				Logger.Debug("network", "accept error {0}.", e.ErrorCode);
				return;
			} catch (ObjectDisposedException) {
				return;
			}
			Logger.Debug("network", "accept ok.");
			socket.BeginAccept(AcceptCallback, this);
			IConnection conn = new Connection(handler);
			manager.OnAccept(this, conn);
			conn.InitRecv();
		}
	}
}
