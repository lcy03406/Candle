using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using ProtoBuf;

namespace CandleLib.Network {
	using SID = ID<Session>;

	interface IManagerCallback {
		void OnAccept(Listener lis, IConnection con);
		void OnConnect(Connector cor, IConnection con);
		void OnDisconnect(IConnection con);
		void OnRecvPacket(IConnection con, Packet p);
	}

	public struct Session {
	}

	public interface IManager {
		void Listen(string ip, int port);
		void Connect(string ip, int port);
		void Disconnect(SID sid);
		void Send(SID sid, Packet p);
		void Broadcast(State state, Packet p);
		void Broadcast(Packet p);
		void Transfer(SID sid, Manager new_manager);
		void SetState(SID sid, State state);
	}

	public class Manager : IManagerCallback, IManager {
		private List<Listener> listen = new List<Listener>();
		private List<Connector> connect = new List<Connector>();
		private Dictionary<SID, IConnection> conns = new Dictionary<SID, IConnection>();
		private SID maxsid = SID.None;
		protected State initState;

		public Manager(State initState) {
			this.initState = initState;
		}

		#region private
		private void AddConn(IConnection con) {
			lock (conns) {
				con.sid = SID.Alloc(maxsid, conns.Keys);
				conns.Add(con.sid, con);
			}
			con.manager = this;
			con.state = initState;
		}

		private IConnection FindConn(SID sid) {
			IConnection conn = null;
			lock (conns) {
				if (!conns.TryGetValue(sid, out conn))
					return null;
			}
			return conn;
		}

		private void Connect(IPEndPoint remote) {
			Connector c = new Connector(this, initState);
			connect.Add(c);
			c.Connect(remote);
		}
		#endregion

		#region IManagerCallback
		void IManagerCallback.OnAccept(Listener lis, IConnection con) {
			AddConn(con);
			con.state.OnConnEvent(State.ConnEvent.Accept, this, con.sid);
		}

		void IManagerCallback.OnConnect(Connector cor, IConnection con) {
			AddConn(con);
			connect.Remove(cor);
			con.state.OnConnEvent(State.ConnEvent.Connect, this, con.sid);
		}

		void IManagerCallback.OnDisconnect(IConnection con) {
			conns.Remove(con.sid);
			con.state.OnConnEvent(State.ConnEvent.Disconnect, this, con.sid);
			if (con.reconn != null) {
				Connect(con.reconn);
			}
		}

		void IManagerCallback.OnRecvPacket(IConnection con, Packet p) {
			con.state.OnRecvPacket(p, this, con.sid);
		}
		#endregion

		#region IManager
		public void Listen(string ip, int port) {
			Listener l = new Listener(this, initState);
			listen.Add(l);
			l.Listen(ip, port);
		}

		public void Connect(string ip, int port) {
			Connector c = new Connector(this, initState);
			connect.Add(c);
			c.Connect(ip, port);
		}

		public void Disconnect(SID sid) {
			IConnection conn = FindConn(sid);
			if (conn == null)
				return;
			lock (conns) {
				conns.Remove(sid);
			}
			conn.Disconnect();
		}

		public void Send(SID sid, Packet p) {
			IConnection conn = FindConn(sid);
			if (conn == null)
				return;
			byte[] byteData;
			using (MemoryStream stream = new MemoryStream()) {
				Serializer.SerializeWithLengthPrefix(stream, p, PrefixStyle.Base128, PacketHelper.GetPacketTypeId(p));
				byteData = stream.ToArray();
			}
			conn.SendRaw(byteData);
		}

		public void Broadcast(State state, Packet p) {
			List<IConnection> cons = new List<IConnection>();
			lock (conns) {
				foreach (IConnection conn in conns.Values) {
					if (state == null || conn.state == state) {
						cons.Add(conn);
					}
				}
			}
			byte[] byteData;
			using (MemoryStream stream = new MemoryStream()) {
				Serializer.SerializeWithLengthPrefix(stream, p, PrefixStyle.Base128, PacketHelper.GetPacketTypeId(p));
				byteData = stream.ToArray();
			}
			foreach (IConnection conn in cons) {
				conn.SendRaw(byteData);
			}
		}

		public void Broadcast(Packet p) {
			Broadcast(null, p);
		}

		public void Transfer(SID sid, Manager new_manager) {
			IConnection conn = FindConn(sid);
			if (conn == null)
				return;
			lock (conn) {
				new_manager.AddConn(conn);
			}
			lock (conns) {
				conns.Remove(sid);
			}
		}

		public void SetState(SID sid, State state) {
			IConnection conn = FindConn(sid);
			if (conn == null)
				return;
			lock (conn) {
				conn.state = state;
			}
		}
		#endregion
	}
}
