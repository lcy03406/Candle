using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CandleLib.Network;

namespace CandleLib.Echo {
	using SID = ID<Session>;
	public class Server : Manager {
		static State InitState = new State();
		static Server() {
			InitState.Register((Protocol.Ping p, Server m, SID sid) => {
				m.Send(sid, p);
			});
		}

		public Server() : base(InitState) {
		}
	}
}
