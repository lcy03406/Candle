using System;
using System.Collections.Generic;
using CandleLib.Common;
using CandleLib.Network;

namespace CandleLib.Echo {
	using System.Diagnostics;
	using System.Threading;
	using SID = ID<Session>;
	public class Server : Manager {
		Statistic<string> stat = new Statistic<string>();
		static State InitState = new State();
		static Server() {
			InitState.Register((Protocol.Ping p, Server m, SID sid) => {
				m.Send(sid, p);
				lock (m.stat) {
					m.stat.Add("echo", 1);
				}
			});
		}

		public Server() : base(InitState) {
		}

		public void Run() {
			Logger.SetDefaultLevel(LogLevel.Info);
			PacketHelper.RegisterPacketTypes(typeof(Protocol));
			Listen("127.0.0.1", 24678);
			Stopwatch stopWatch = new Stopwatch();
			stopWatch.Start();
			while (true) {
				Thread.Sleep(10000);
				lock (stat) {
					stopWatch.Stop();
					stopWatch.Start();
					TimeSpan ts = stopWatch.Elapsed;
					Logger.Info("stat", "!!!!!{0:00}:{1:00}:{2:00}.{3:000}!!!!!",
							ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
					stat.Print();
				}
			}
		}
	}
}
