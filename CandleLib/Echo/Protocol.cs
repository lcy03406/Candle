using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CandleLib.Network;
using ProtoBuf;

namespace CandleLib.Echo {
	using SID = ID<Session>;
	public static class Protocol {
		[PacketType(4), ProtoContract]
		public sealed class Ping : Packet {
			[ProtoMember(1)]
			public SID self;
			[ProtoMember(2)]
			public List<int> data = new List<int>();
		}
	}
}
