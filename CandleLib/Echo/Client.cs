using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CandleLib.Common;
using CandleLib.Network;
using System.Diagnostics;
using System.Threading;
using System.IO;
using ProtoBuf;

namespace CandleLib.Echo {
	using SID = ID<Session>;
	public class Client : Manager {
		const int ConnCount = 1;
		const int SendCount = 1;
		const int EchoCount = 1;
		Statistic<string> stat = new Statistic<string>();
		Stopwatch stopWatch = new Stopwatch();
		static State InitState = new State();
		static Client() {
			InitState.Register((Protocol.Ping p, Client m, SID sid) => {
				lock (m.stat) {
					m.stat.Add("recv", 1);
				}
				for (int i = 0; i < EchoCount; ++i) {
					m.Send(sid, p);
					lock (m.stat) {
						m.stat.Add("echo", 1);
						m.stat.Add("send", 1);
					}
				}
			});
			InitState.RegisterConn(State.ConnEvent.Connect, (Client m, SID sid) => {
				for (int i = 0; i < SendCount; ++i) {
					Protocol.Ping p = new Protocol.Ping();
					p.self = sid;
					p.data.Add(i);
					m.Send(sid, p);
					lock (m.stat) {
						m.stat.Add("send", 1);
					}
				}
			});
		}

		public Client() : base(InitState) {
		}

		public void Run() {
			Logger.SetDefaultLevel(LogLevel.Info);
			PacketHelper.RegisterPacketTypes(typeof(Protocol));
			for (int i = 0; i < ConnCount; ++i) {
				Connect("127.0.0.1", 24678);
			}
			stopWatch.Start();
			while (true) {
				Thread.Sleep(10000);
				lock (stat) {
					TimeSpan ts = stopWatch.Elapsed;
					Logger.Info("stat", "!!!!!{0:00}:{1:00}:{2:00}.{3:000}!!!!!",
							ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
					stat.Print();
				}
			}
		}
	}
}
