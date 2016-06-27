using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CandleLib.Network {
	public interface Packet {

	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface,
	AllowMultiple = false, Inherited = false)]
	public sealed class PacketTypeAttribute : Attribute {
		public PacketTypeAttribute(int id) { Id = id; }
		public int Id { get; set; }
		public int Size { get; set; }
	}

	public static class PacketHelper {
		public static int GetPacketTypeId(this Packet packet) {
			int id, size;
			GetPacketTypeInfo(packet.GetType(), out id, out size);
			return id;
		}

		public static bool GetPacketTypeInfo(Type type, out int id, out int size) {
			object[] attrs = type.GetCustomAttributes(typeof(PacketTypeAttribute), false);
			if (attrs == null || attrs.Length == 0) {
				id = size = 0;
				return false;
			}
			PacketTypeAttribute attr = (PacketTypeAttribute)attrs[0];
			id = attr.Id;
			size = attr.Size;
			return id > 0;
		}

		public struct TypeInfo {
			public Type type;
			public PacketTypeAttribute attr;
		}

		private static Dictionary<int, TypeInfo> types = new Dictionary<int, TypeInfo>();

		public static Type GetPacketType(int id) {
			TypeInfo ti;
			types.TryGetValue(id, out ti);
			return ti.type;
		}

		public static bool ValidRecv(int id, int size) {
			TypeInfo ti;
			if (!types.TryGetValue(id, out ti))
				return false;
			return size <= ti.attr.Size;
		}

		public static void RegisterPacketTypes(Type group) {
			Type[] inner = group.GetNestedTypes(System.Reflection.BindingFlags.Public);
			foreach (Type type in inner) {
				RegisterPacketType(type);
			}
		}

		public static bool RegisterPacketType(Type type) {
			if (!type.IsSealed)
				return false;
			if (!typeof(Packet).IsAssignableFrom(type))
				return false;
			object[] attrs = type.GetCustomAttributes(typeof(PacketTypeAttribute), false);
			if (attrs == null || attrs.Length == 0)
				return false;
			PacketTypeAttribute attr = (PacketTypeAttribute)attrs[0];
			types.Add(attr.Id, new TypeInfo() { type = type, attr = attr });
			return true;
		}
	}
}
