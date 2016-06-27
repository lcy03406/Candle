using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using ProtoBuf;

namespace CandleLib.Network {
	public interface IID {
		int id { get; set; }
	}

	public struct ID<T> : IID {
		//int IID.id { get { return id; } set { id = value; } }
		//private int id;
		
		public int id {	get; set; }

		private ID(int id) {
			this.id = id;
		}

		public static ID<T> None = new ID<T>(0);

		public static ID<T> Alloc(ID<T> last, ICollection<ID<T>> list) {
			ID<T> i = new ID<T>(last.id + 1);
			while (i.id == 0 || list.Contains(i)) {
				i = new ID<T>(i.id + 1);
			}
			return i;
		}

		public override bool Equals(object obj) {
			if (obj == null)
				return false;
			if (!(obj is ID<T>))
				return false;
			ID<T> b = (ID<T>)obj;
			return id == b.id;
		}

		public override int GetHashCode() {
			return id;
		}

		public override string ToString() {
			return id.ToString();
		}

		public static bool operator ==(ID<T> a, ID<T> b) {
			return a.id == b.id;
		}

		public static bool operator !=(ID<T> a, ID<T> b) {
			return a.id != b.id;
		}
	}
}
