using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CandleLib.Network;
using ProtoBuf;

namespace CandleLib.Echo {
	public static class Protocol {
		[PacketType(1), ProtoContract]
		public class Ping : Packet {
			[ProtoMember(1)]
			public List<int> data = new List<int>();
		}
	}
}
