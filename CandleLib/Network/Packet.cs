using System;
using System.IO;
using System.Collections.Generic;
using CandleLib.Common;
using ProtoReader = ProtoBuf.ProtoReader;
using RuntimeTypeModel = ProtoBuf.Meta.RuntimeTypeModel;

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
		const ProtoBuf.PrefixStyle style = ProtoBuf.PrefixStyle.Base128;
		public static void Serialize(this Packet packet, Stream stream) {
			int id = GetPacketTypeId(packet);
			RuntimeTypeModel model = RuntimeTypeModel.Default;
			Type type = packet.GetType();
			model.SerializeWithLengthPrefix(stream, packet, /*model.MapType*/type, style, id);
		}
		public static Packet Deserialize(Stream stream) {
			object buf = RuntimeTypeModel.Default.DeserializeWithLengthPrefix(stream, null, null, style, 0, GetPacketType);
			return buf as Packet;
		}
		public static int ReadLengthPrefix(Stream stream, out int packetType, out int bytesRead) {
			return ProtoReader.ReadLengthPrefix(stream, true, style, out packetType, out bytesRead);
		}
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
			return ti.attr.Size <= 0 || size <= ti.attr.Size;
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
			int id = attr.Id;
			if (type == GetPacketType(id))
				return true;
			Logger.Debug("network", "Register packet type={0} id={1}.", type, id);
			types.Add(id, new TypeInfo() { type = type, attr = attr });
			return true;
		}
	}
}
