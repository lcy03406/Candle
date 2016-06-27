using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CandleLib.Network;
using ProtoBuf;

namespace CandleLib.Playpen {
	using SID = ID<Session>;
	using UID = ID<User>;

	public class User {
		public SID sid;
		public UID id;
		public string name;
		public int pen;
	}

	public class Lobby : Manager {
		Dictionary<SID, HashSet<UID>> sessions;
		Dictionary<UID, User> users;
		Dictionary<int, Pen> pens;
		UID maxuid = UID.None;

		public Lobby() : base(ServerState.stateLobby) {
		}

		static Lobby() {
			State state = ServerState.stateLobby;
			state.Register((Protocol.Login p, Lobby m, SID sid) => {
				User user = new User();
				user.id = UID.Alloc(m.maxuid, m.users.Keys);
				user.name = p.name;
				user.sid = sid;
				m.users.Add(user.id, user);
				m.sessions[sid].Add(user.id);
				Protocol.LoginRe re = new Protocol.LoginRe();
				re.ret = 0;
				re.self = user.id;
				m.Send(sid, re);
			});
			Register(state, (Protocol.Logout q, Protocol.LogoutRe r, User a, Lobby m, SID sid) => {
				m.users.Remove(q.self);
				return 0;
			});
			Register(state, (Protocol.Join q, Protocol.JoinRe r, User a, Lobby m, SID sid) => {
				Pen pen;
				if (!m.pens.TryGetValue(q.pen, out pen)) {
					pen = new Pen();
					m.pens.Add(q.pen, pen);
				}
				pen.AddUser(a);
				return 0;
			});
		}

		delegate int PlayerProc<Q, R>(Q q, R r, User a, Lobby m, SID sid) where Q : Protocol.Req where R: Protocol.Rep, new();
		static void Register<Q, R>(State state, PlayerProc<Q, R> proc) where Q : Protocol.Req where R : Protocol.Rep, new() {
			state.Register((Q q, Lobby m, SID sid) => {
				R r = new R();
				r.self = q.self;
				User a;
				if (!m.users.TryGetValue(q.self, out a)) {
					r.ret = 1;
					m.Send(sid, r);
					return;
				}
				if (a.sid != sid) {
					r.ret = 2;
					m.Send(sid, r);
					return;
				}
				int ret = proc(q, r, a, m, sid);
				r.ret = ret;
				m.Send(sid, r);
			});
		}

		public void OnDisconnect(SID sid) {
			HashSet<UID> ids;
			if (!sessions.TryGetValue(sid, out ids))
				return;
			foreach (int id in ids) {
				User player;
				if (!users.TryGetValue(id, out player))
					continue;
				int pen = 
			}
			throw new NotImplementedException();
		}
/*
		User GetUser(UID id, SID sid) {
			User user = null;
			if (!users.TryGetValue(id, out user)) {
				return null;
			}
			if (user.sid != sid) {
				return null;
			}
			return user;
		}
		*/
	}
	public static class ServerState {
		public static State stateLobby = new State();
		public static State statePen = new State();
	}
}
