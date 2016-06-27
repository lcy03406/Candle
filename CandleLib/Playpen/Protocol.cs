using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CandleLib.Network;
using ProtoBuf;

namespace CandleLib.Playpen {
	using UID = ID<User>;

	public static class Protocol {
		public abstract class Req : Packet {
			[ProtoMember(1)]
			public int iself { get { return (self as IID).id; } set { (self as IID).id = value; } }
			public UID self;
			protected const int T = 1;
		}
		public abstract class Rep : Packet {
			[ProtoMember(1)]
			public int iself { get { return (self as IID).id; } set { (self as IID).id = value; } }
			public UID self;
			[ProtoMember(2)]
			public int ret;
			protected const int T = 2;
		}

		[ProtoContract]
		public struct Pos {
			[ProtoMember(1)]
			public int x;
			[ProtoMember(2)]
			public int y;
		}
		[ProtoContract]
		public class Player {
			[ProtoMember(1)]
			public string name;
			[ProtoMember(2)]
			public Pos pos;
		}

		[PacketType(1), ProtoContract]
		public class Login : Req {
			[ProtoMember(T+1)]
			public string name;

		}
		[PacketType(2), ProtoContract]
		public class LoginRe : Rep {
			[ProtoMember(T+1)]
			public string name;
		}
		[PacketType(3), ProtoContract]
		public class Logout : Req {
		}
		[PacketType(4), ProtoContract]
		public class LogoutRe : Rep {
		}
		[PacketType(5), ProtoContract]
		public class Join : Req {
			[ProtoMember(T+1)]
			public int pen;
		}
		[PacketType(6), ProtoContract]
		public class JoinRe : Rep {
			[ProtoMember(T+1)]
			public int pen;
		}
		[PacketType(7), ProtoContract]
		public class Quit : Req {
		}
		[PacketType(8), ProtoContract]
		public class QuitRe : Rep {
		}
		[PacketType(9), ProtoContract]
		public class Pen : Packet {
			[ProtoMember(1)]
			public int time;
			[ProtoMember(2)]
			public Dictionary<int, Player> players;
		}
		[PacketType(10), ProtoContract]
		public class Tick : Packet {
			[ProtoMember(1)]
			public int time;
		}
		[PacketType(11), ProtoContract]
		public class Move : Req {
			[ProtoMember(T+1)]
			public Pos to;
		}
		[PacketType(12), ProtoContract]
		public class MoveRe : Req {
			[ProtoMember(T + 1)]
			public Pos to;
		}
	}
}
