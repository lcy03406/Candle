using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CandleLib.Network;

namespace CandleLib.Echo {
	using SID = ID<Session>;
	public class Client : Manager {
		const int ConnCount = 100;
		const int SendCount = 1;
		const int EchoCount = 1;
		Statistic<string> stat = new Statistic<string>();
		static State InitState = new State();
		static Client() {
			InitState.Register((Protocol.Ping p, Server m, SID sid) => {
				for (int i = 0; i < EchoCount; ++i) {
					m.Send(sid, p);
				}
			});
			InitState.RegisterConn(State.ConnEvent.Connect, (Client m, SID sid) => {
				for (int i = 0; i < SendCount; ++i) {
					Protocol.Ping p = new Protocol.Ping();
					p.data.Add(sid.id);
					p.data.Add(i);
				}
			});
		}

		public Client() : base(InitState) {
		}

		public void Run() {
			for (int i = 0; i < ConnCount; ++i) {
				Connect("127.0.0.1", 24678);
			}
		}

	}
}
