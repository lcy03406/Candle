using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CandleLib.Network {
	using SID = ID<Session>;

	public class State {
		public delegate void RecvProc<P, M>(P packet, M manager, SID sid) where P : Packet where M : Manager ;
		class Handler {
			public RecvProc<Packet, Manager> action;
		};

		Dictionary<Type, Handler> handlers = new Dictionary<Type, Handler>();

		public void Register<P, M>(RecvProc<P, M> func) where P : Packet where M : Manager {
			handlers.Add(typeof(P), new Handler() {
				action = (Packet p, Manager manager, SID sid) => {
					P t = (P)p;
					M m = (M)manager;
					func(t, m, sid);
				}
			});
		}

		internal void OnRecvPacket(Packet p, Manager manager, SID sid) {
			Handler h = null;
			for (Type type = p.GetType(); type != null; type = type.BaseType) {
				if (handlers.TryGetValue(p.GetType(), out h)) {
					h.action(p, manager, sid);
					return;
				}
			}
		}

		public enum ConnEvent {
			Accept,
			Connect,
			Disconnect,
		}
		public delegate void ConnProc<M>(M manager, SID sid) where M : Manager;
		ConnProc<Manager>[] conn = new ConnProc<Manager>[3];
		public void RegisterConn<M>(ConnEvent i, ConnProc<M> func) where M : Manager {
			conn[(int)i] = (Manager manager, SID sid) => {
				M m = (M)manager;
				func(m, sid);
			};
		}
		internal void OnConnEvent(ConnEvent i, Manager manager, SID sid) {
			ConnProc<Manager> func = conn[(int)i];
			if (func != null) {
				func(manager, sid);
			}
		}
	}
}
